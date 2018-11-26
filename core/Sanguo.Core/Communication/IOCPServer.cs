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
        /// 信号量
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
        /// 服务器是否正在运行
        /// </summary>
        public bool IsRunning { get; private set; }
        /// <summary>
        /// 监听的IP地址
        /// </summary>
        public IPAddress Address { get; private set; }
        /// <summary>
        /// 监听的端口
        /// </summary>
        public int Port { get; private set; }
        /// <summary>
        /// 通信使用的编码
        /// </summary>
        public Encoding Encoding { get; set; }

        public int ClientCount { get { return _clientCount; } }
        #endregion

        #region Ctors

        /// <summary>
        /// 异步IOCP SOCKET服务器
        /// </summary>
        /// <param name="listenPort">监听的端口</param>
        /// <param name="maxClient">最大的客户端数量</param>
        public IOCPServer(int listenPort, int maxClient)
            : this(IPAddress.Any, listenPort, maxClient)
        {
        }

        /// <summary>
        /// 异步Socket TCP服务器
        /// </summary>
        /// <param name="localEP">监听的终结点</param>
        /// <param name="maxClient">最大客户端数量</param>
        public IOCPServer(IPEndPoint localEP, int maxClient)
            : this(localEP.Address, localEP.Port, maxClient)
        {
        }

        /// <summary>
        /// 异步Socket TCP服务器
        /// </summary>
        /// <param name="localIPAddress">监听的IP地址</param>
        /// <param name="listenPort">监听的端口</param>
        /// <param name="maxClient">最大客户端数量</param>
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

        #region 初始化

        /// <summary>
        /// Initializer method.
        /// </summary>
        public void Init()
        {
            // Allocates one large byte buffer which all I/O operations use a piece of.  This gaurds 
            // against memory fragmentation
            _bufferManager.InitBuffer();

            // Heartbeat 
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
        /// 启动
        /// </summary>
        public void Start()
        {
            if (!IsRunning)
            {
                Init();
                IsRunning = true;
                IPEndPoint localEndPoint = new IPEndPoint(Address, Port);
                // 创建监听socket
                _serverSock = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                //_serverSock.ReceiveBufferSize = _bufferSize;
                //_serverSock.SendBufferSize = _bufferSize;
                if (localEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    // 配置监听socket为 dual-mode (IPv4 & IPv6) 
                    // 27 is equivalent to IPV6_V6ONLY socket option in the winsock snippet below,
                    _serverSock.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, false);
                    _serverSock.Bind(new IPEndPoint(IPAddress.IPv6Any, localEndPoint.Port));
                }
                else
                {
                    _serverSock.Bind(localEndPoint);
                }
                // 开始监听
                _serverSock.Listen(this._maxClient);
                // 在监听Socket上投递一个接受请求。
                StartAccept(null);
            }
        }
        #endregion

        #region Stop

        /// <summary>
        /// 停止服务
        /// </summary>
        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                _serverSock.Close();
                //TODO 关闭对所有客户端的连接

            }
        }

        #endregion
        
        #region Accept

        /// <summary>
        /// 从客户端开始接受一个连接操作
        /// </summary>
        private void StartAccept(SocketAsyncEventArgs asyniar)
        {
            if (asyniar == null)
            {
                asyniar = new SocketAsyncEventArgs();
                asyniar.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            }
            else
            {
                //socket must be cleared since the context object is being reused
                asyniar.AcceptSocket = null;
            }
            _maxAcceptedClients.WaitOne();
            if (!_serverSock.AcceptAsync(asyniar))
            {
                ProcessAccept(asyniar);
                //如果I/O挂起等待异步则触发AcceptAsyn_Asyn_Completed事件
                //此时I/O操作同步完成，不会触发Asyn_Completed事件，所以指定BeginAccept()方法
            }
        }

        /// <summary>
        /// accept 操作完成时回调函数
        /// </summary>
        /// <param name="sender">Object who raised the event.</param>
        /// <param name="e">SocketAsyncEventArg associated with the completed accept operation.</param>
        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        /// <summary>
        /// 监听Socket接受处理
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed accept operation.</param>
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {

                Socket s = e.AcceptSocket;//和客户端关联的socket
                if (s.Connected)
                {
                    try
                    {
                        Interlocked.Increment(ref _clientCount);//原子操作加1
                        SocketAsyncEventArgs asyniar = _objectPool.Pop();
                        asyniar.UserToken = s;
                        ClientConnected?.Invoke(this, e);
                        if (!s.ReceiveAsync(asyniar))//投递接收请求
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

        #region 发送数据

        /// <summary>
        /// 异步的发送数据
        /// </summary>
        /// <param name="e"></param>
        /// <param name="data"></param>
        public void Send(SocketAsyncEventArgs e, byte[] data)
        {
            
            e.SetBuffer(data, 0, data.Length);
            e.Completed += DataSent;
            ((Socket)e.UserToken).SendAsync(e);
        }

        /// <summary>
        /// 同步的使用socket发送数据
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="timeout"></param>
        public void Send(Socket socket, byte[] buffer, int offset, int size, int timeout)
        {
            socket.SendTimeout = 0;
            int startTickCount = Environment.TickCount;
            int sent = 0; // how many bytes is already sent
            do
            {
                if (Environment.TickCount > startTickCount + timeout)
                {
                    throw new Exception("Timeout.");
                }
                try
                {
                    sent += socket.Send(buffer, offset + sent, size - sent, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                    ex.SocketErrorCode == SocketError.IOPending ||
                    ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably full, wait and try again
                        Thread.Sleep(30);
                    }
                    else
                    {
                        throw ex; // any serious error occurr
                    }
                }
            } while (sent < size);
        }


        #endregion

        #region 接收数据
        
        /// <summary>
        ///接收完成时处理函数
        /// </summary>
        /// <param name="e">与接收完成操作相关联的SocketAsyncEventArg对象</param>
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            Socket s = (Socket)e.UserToken;
            if (e.SocketError == SocketError.Success)//if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                // 检查远程主机是否关闭连接
                if (e.BytesTransferred > 0)
                {
                    //判断所有需接收的数据是否已经完成
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
                    if (!s.ReceiveAsync(e))//为接收下一段数据，投递接收请求，这个函数有可能同步完成，这时返回false，并且不会引发SocketAsyncEventArgs.Completed事件
                        //同步接收时处理接收完成事件
                        ProcessReceive(e);
                }
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
