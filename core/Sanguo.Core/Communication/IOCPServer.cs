using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Sanguo.Core.Communication
{
    /// <summary>
    /// IOCP SOCKET Server
    /// </summary>
    public class IOCPServer : IDisposable
    {
        /// <summary>
        /// Pre-alloc buffers for send and receive.
        /// </summary>
        const int opsToPreAlloc = 2;

        #region Fields
        /// <summary>
        /// The maximum numbers of client that can simultaneously connect to this server.
        /// </summary>
        private int _maxClient;

        /// <summary>
        /// The listening socket, which is used to process the connection request.
        /// </summary>
        private Socket _serverSock;

        /// <summary>
        /// The number of the clients that is connected to this server.
        /// </summary>
        private int _clientCount;

        /// <summary>
        /// The size of the buffer of each I/O socket.
        /// </summary>
        private int _bufferSize = 1024;

        /// <summary>
        /// The max count of accepted clients.
        /// </summary>
        Semaphore _maxAcceptedClients;

        /// <summary>
        /// Buffer management
        /// </summary>
        BufferManager _bufferManager;

        /// <summary>
        /// Free SASE object pool
        /// </summary>
        SocketAsyncEventArgsPool _objectPool;

        Dictionary<SocketAsyncEventArgs, DateTime> heartbeatDictionary;

        List<SocketAsyncEventArgs> removingList;

        private bool disposed = false;

        #endregion

        #region Properties

        /// <summary>
        /// Is the server still running.
        /// </summary>
        public bool IsRunning { get; private set; }
        /// <summary>
        /// The IP addresses to be listened.
        /// </summary>
        public IPAddress Address { get; private set; }
        /// <summary>
        /// The port to be listened.
        /// </summary>
        public int Port { get; private set; }
        /// <summary>
        /// The encoding which the communication uses.
        /// </summary>
        public Encoding Encoding { get; set; }

        public int ClientCount { get { return _clientCount; } }
        #endregion

        #region Ctors

        /// <summary>
        /// Create a new instance of a async IOCP socket server.
        /// </summary>
        /// <param name="listenPort">The port to be listened.</param>
        /// <param name="maxClient">The maximum numbers of clients can simultaneously connectted to this IOCP server.</param>
        public IOCPServer(int listenPort, int maxClient) : this(IPAddress.Any, listenPort, maxClient){}

        /// <summary>
        /// Create a new instance of a async IOCP socket server.
        /// </summary>
        /// <param name="localEP">The local EndPoint to listen.</param>
        /// <param name="maxClient">The maximum numbers of clients can simultaneously connectted to this IOCP server.</param>
        public IOCPServer(IPEndPoint localEP, int maxClient) : this(localEP.Address, localEP.Port, maxClient){}

        /// <summary>
        /// Create a new instance of a async IOCP socket server.
        /// </summary>
        /// <param name="localIPAddress">The IP addresses to be listened.</param>
        /// <param name="listenPort">The port to be listened.</param>
        /// <param name="maxClient">The maximum numbers of clients can simultaneously connectted to this IOCP server.</param>
        public IOCPServer(IPAddress localIPAddress, int listenPort, int maxClient)
        {
            this.Address = localIPAddress;
            this.Port = listenPort;
            this.Encoding = Encoding.Default;

            _maxClient = maxClient;

            _serverSock = new Socket(localIPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            _bufferManager = new BufferManager(_bufferSize * _maxClient * opsToPreAlloc, _bufferSize);

            _objectPool = new SocketAsyncEventArgsPool(_maxClient);

            _maxAcceptedClients = new Semaphore(_maxClient, _maxClient);
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializer method.
        /// </summary>
        public void Init()
        {
            // Allocates one large byte buffer which all I/O operations use a piece of.  This gaurds against memory fragmentation
            _bufferManager.InitBuffer();

            // Heartbeat test
            heartbeatDictionary = new Dictionary<SocketAsyncEventArgs, DateTime>();
            removingList = new List<SocketAsyncEventArgs>();
            Thread t_heartbeat = new Thread(() =>
             {
                 while (!disposed)
                 {
                     lock (heartbeatDictionary)
                     {
                         foreach (var sase in heartbeatDictionary.Keys)
                         {
                             if ((DateTime.Now - heartbeatDictionary[sase]) > TimeSpan.FromSeconds(5))
                                 removingList.Add(sase);
                         }
                         foreach (var e in removingList)
                         {
                             lock (e)
                             {
                                 Socket s = e.UserToken as Socket;
                                 ClientDisconnected?.Invoke(this, e);
                                 s.Shutdown(SocketShutdown.Send);
                                 s.Close();
                                 Interlocked.Decrement(ref _clientCount);
                                 _maxAcceptedClients.Release();
                                 _objectPool.Push(e);
                                 heartbeatDictionary.Remove(e);
                             }
                         }
                         removingList.Clear();
                     }
                     Thread.Sleep(1000);
                 }
             });
            t_heartbeat.Start();
            // preallocate pool of SocketAsyncEventArgs objects
            SocketAsyncEventArgs readWriteEventArg;
            
            for (int i = 0; i < _maxClient; i++)
            {
                //Pre-allocate a set of reusable SocketAsyncEventArgs
                readWriteEventArg = new SocketAsyncEventArgs();
                readWriteEventArg.DisconnectReuseSocket = true;
                readWriteEventArg.Completed += (sender, e) =>
                {
                    // Determine which type of operation just completed and call the associated handler.
                    switch (e.LastOperation)
                    {
                        case SocketAsyncOperation.Accept:
                            ProcessAccept(e);
                            break;
                        case SocketAsyncOperation.Receive:
                            ProcessReceive(e);
                            break;
                        default:
                            throw new ArgumentException("The last operation completed on the socket was not a receive or send");
                    }
                };
                readWriteEventArg.UserToken = null;

                // assign a byte buffer from the buffer pool to the SocketAsyncEventArg object
                _bufferManager.SetBuffer(readWriteEventArg);

                // add SocketAsyncEventArg to the pool
                _objectPool.Push(readWriteEventArg);

            }
        }


        #endregion

        #region Start
        /// <summary>
        /// Start the IOCP server.
        /// </summary>
        public void Start()
        {
            if (!IsRunning)
            {
                Init();
                IsRunning = true;
                IPEndPoint localEndPoint = new IPEndPoint(Address, Port);
                // Create the listening socket.
                _serverSock = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                if (localEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    // Listening dual-mode (IPv4 & IPv6) 
                    _serverSock.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, false);
                    _serverSock.Bind(new IPEndPoint(IPAddress.IPv6Any, localEndPoint.Port));
                }
                else
                {
                    _serverSock.Bind(localEndPoint);
                }
                // Start listening.
                _serverSock.Listen(_maxClient);
                // Post a null request.
                StartAccept(null);
            }
        }
        #endregion

        #region Stop

        /// <summary>
        /// Stop the IOCP server.
        /// </summary>
        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                _serverSock.Close();
            }
        }

        #endregion
        
        #region Accept

        /// <summary>
        /// Start to accept socket request.
        /// </summary>
        private void StartAccept(SocketAsyncEventArgs args)
        {
            if (args == null)
            {
                args = new SocketAsyncEventArgs();
                args.Completed += (sender, e) => { ProcessAccept(e); };
            }
            else
            {
                //socket must be cleared since the context object is being reused
                args.AcceptSocket = null;
            }
            _maxAcceptedClients.WaitOne();
            if (!_serverSock.AcceptAsync(args))
            {
                ProcessAccept(args);
            }
        }

        /// <summary>
        /// Watching socket accept process.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed accept operation.</param>
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                Socket s = e.AcceptSocket;
                if (s.Connected)
                {
                    try
                    {
                        Interlocked.Increment(ref _clientCount);
                        SocketAsyncEventArgs asyniar = _objectPool.Pop();
                        asyniar.UserToken = s;
                        ClientConnected?.Invoke(this, e);
                        if (!s.ReceiveAsync(asyniar))
                        {
                            ProcessReceive(asyniar);
                        }
                    }
                    catch (ObjectDisposedException)
                    {

                    }
                    StartAccept(e);
                }
            }
        }

        #endregion

        #region Send data

        /// <summary>
        /// Send data asynchronously.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="data"></param>
        public void Send(SocketAsyncEventArgs e, byte[] data)
        {
            e.SetBuffer(data, 0, data.Length);
            ((Socket)e.UserToken).SendAsync(e);
        }

        /// <summary>
        /// Send string asynchronously
        /// </summary>
        /// <param name="e"></param>
        /// <param name="data"></param>
        public void Send(SocketAsyncEventArgs e, string data) => 
            Send(e, Encoding.Default.GetBytes(data));
        #endregion

        #region Receive data
        
        /// <summary>
        /// Process the data received.
        /// </summary>
        /// <param name="e"></param>
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            Socket s = (Socket)e.UserToken;
            if (e.SocketError == SocketError.Success&&e.BytesTransferred > 0)
            {
                if (s.Available == 0)
                {
                    DataReceived?.Invoke(this, e);
                    lock (heartbeatDictionary)
                    {
                        if (heartbeatDictionary.ContainsKey(e))
                            heartbeatDictionary[e] = DateTime.Now;
                        else
                            heartbeatDictionary.Add(e, DateTime.Now);
                    }
                }
                if (!s.ReceiveAsync(e))
                    ProcessReceive(e);
            }
        }

        #endregion

        #region Dispose
        /// <summary>
        /// Performs application-defined tasks associated with freeing, 
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release 
        /// both managed and unmanaged resources; <c>false</c> 
        /// to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    try
                    {
                        Stop();
                        if (_serverSock != null)
                        {
                            _serverSock = null;
                        }
                    }
                    catch (SocketException)
                    {
                    }
                }
                disposed = true;
            }
        }
        #endregion
        
        public event EventHandler<SocketAsyncEventArgs> DataReceived;
        public event EventHandler<SocketAsyncEventArgs> DataSent;
        public event EventHandler<SocketAsyncEventArgs> ClientConnected;
        public event EventHandler<SocketAsyncEventArgs> ClientDisconnected;
    }
}
