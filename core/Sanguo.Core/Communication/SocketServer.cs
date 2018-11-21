using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Sanguo.Core.Communication
{
    /// <summary>
    /// Represents a server that can handle a lot of socket requests simultaneously.
    /// </summary>
    public class SocketServer
    {
        #region Common

        /// <summary>
        /// The port to which the server should listen.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The maximum number of clients can connect to the server simultaneously.
        /// </summary>
        public int MaxClientCount { get; private set; }

        /// <summary>
        /// The size of the each buffer.
        /// </summary>
        public int BufferSize { get; set; }

        /// <summary>
        /// The maximum interval of two client heartbeats.
        /// </summary>
        public int HeartbeatInterval { get; set; } = 10 * 1000;

        /// <summary>
        /// Initialize a new instance of the SocketServer class, using the specified port.
        /// </summary>
        /// <param name="port">The port to which the server should listen.</param>
        public SocketServer(int port)
        {
            Port = port;
            MaxClientCount = 1000;
            BufferSize = 1024;
        }

        /// <summary>
        /// Initialize a new instance of the SocketServer class, using the specified port and maximum client count.
        /// </summary>
        /// <param name="port">The port to which the server should listen.</param>
        /// <param name="maxClientCount">The maximum number of clients can connect to the server simultaneously.</param>
        public SocketServer(int port, int maxClientCount)
        {
            Port = port;
            MaxClientCount = maxClientCount;
            BufferSize = 1024;
        }

        /// <summary>
        /// Initialize a new instance of the SocketServer class, using the specified port, maximum client count and buffer size.
        /// </summary>
        /// <param name="port">The port to which the server should listen.</param>
        /// <param name="maxClientCount">The maximum number of clients can connect to the server simultaneously.</param>
        /// <param name="bufferSize">The size of the each buffer.</param>
        public SocketServer(int port, int maxClientCount, int bufferSize)
        {
            Port = port;
            MaxClientCount = MaxClientCount;
            BufferSize = bufferSize;
        }

        #endregion

        #region Buffer Manage

        /// <summary>
        /// The pool that contains the available buffer indexes.
        /// </summary>
        private Stack<int> _availableBufferIndexPool;

        /// <summary>
        /// The buffer receiver socket uses.
        /// </summary>
        private byte[] _buffer;

        /// <summary>
        /// Initialize the buffer.
        /// </summary>
        private void InitializeBuffer()
        {
            //Directly initialize all buffers, which slowers the lanuch speed, but acclerates the running speed.
            _availableBufferIndexPool = new Stack<int>();
            _buffer = new byte[MaxClientCount * BufferSize];
            for (int i = 0; i < MaxClientCount; i++)
            {
                _availableBufferIndexPool.Push(i);
            }
        }

        /// <summary>
        /// Assign a buffer region for specified SocketAsyncEventArgs.
        /// </summary>
        /// <param name="args">The specified SocketAsyncEventArgs to be assigned with buffer.</param>
        private void AssignBuffer(SocketAsyncEventArgs args)
        {
            lock (_availableBufferIndexPool)
            {
                if (_availableBufferIndexPool.Count > 0)
                    args.SetBuffer(_buffer, _availableBufferIndexPool.Pop(), BufferSize);
            }
        }

        /// <summary>
        /// Free the buffer occupied by the specified SocketAsyncEventArgs.
        /// </summary>
        /// <param name="args">Which the buffer is assigned to.</param>
        private void FreeBuffer(SocketAsyncEventArgs args)
        {
            try
            {
                int index = ((Receive_SAEAUToken)args.UserToken).BufferIndex;
                if (index >= 0)
                    _availableBufferIndexPool.Push(index);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
        #endregion

        #region Receive SocketAsyncEventArgs UserToken

        /// <summary>
        /// Represents the user token of a SocketAsyncEventArgs of a receive socket.
        /// </summary>
        class Receive_SAEAUToken
        {
            /// <summary>
            /// The assigned buffer region index.
            /// </summary>
            public int BufferIndex { get; set; } = -1;

            /// <summary>
            /// Universial Unique IDentity.
            /// </summary>
            public string UUID { get; } = Guid.NewGuid().ToString();

            /// <summary>
            /// The time that the client sent heartbeat package.
            /// </summary>
            public DateTime HeartbeatTime { get; set; }

            /// <summary>
            /// The socket which receives the message.
            /// </summary>
            public Socket Socket { get; set; }

            /// <summary>
            /// The correspond message sender's SocketAsyncEventArgs.
            /// </summary>
            public SocketAsyncEventArgs SenderSAEA { get; set; }
        }
        #endregion

        #region SocketAsyncEventArgs Pool

        /// <summary>
        /// The availabe receiver SocketAsyncEventArgs.
        /// </summary>
        private List<SocketAsyncEventArgs> _availableSAEAPool;

        /// <summary>
        /// The receiver SocketAsyncEventArgs that have been occupied.
        /// </summary>
        private List<SocketAsyncEventArgs> _occupiedSAEAPool;

        /// <summary>
        /// Initialize the pool of the receiver SocketAsyncEventArgs.
        /// </summary>
        private void InitializeSAEAPool()
        {
            _availableSAEAPool = new List<SocketAsyncEventArgs>();
            _occupiedSAEAPool = new List<SocketAsyncEventArgs>();
            for (int i = 0; i < MaxClientCount; i++)
            {
                SocketAsyncEventArgs saea = new SocketAsyncEventArgs();
                saea.Completed += new EventHandler<SocketAsyncEventArgs>(OnIOCompleted);
                AssignBuffer(saea);
                Receive_SAEAUToken userToken = new Receive_SAEAUToken();
                SocketAsyncEventArgs senderSAEA = new SocketAsyncEventArgs();
                senderSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(OnIOCompleted);
                userToken.SenderSAEA = senderSAEA;
                userToken.HeartbeatTime = DateTime.Now;
                saea.UserToken = userToken;
                _availableSAEAPool.Add(saea);
            }
        }

        #endregion

        #region Core Code

        /// <summary>
        /// The socket instance of this SocketServer.
        /// </summary>
        Socket ServerSocket;

        /// <summary>
        /// Starts the listen operation.
        /// </summary>
        public void StartListen()
        {
            try
            {
                ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ServerSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
                ServerSocket.Listen(MaxClientCount);
                InitializeBuffer();
                InitializeSAEAPool();
                Thread thread_HeartbeatChecker = new Thread(CheckClientHeartbeat);
                thread_HeartbeatChecker.IsBackground = true;
                thread_HeartbeatChecker.Start();
                StartAccept(null);
            }
            catch { }
        }

        /// <summary>
        /// Accept the connection request from the client.
        /// </summary>
        private void StartAccept(SocketAsyncEventArgs saea_Accept)
        {
            if (saea_Accept == null)
            {
                saea_Accept = new SocketAsyncEventArgs();
                saea_Accept.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            }
            else
                saea_Accept.AcceptSocket = null;  //Clean the object before reuse it.

            if (!ServerSocket.AcceptAsync(saea_Accept))
                ProcessAccept(saea_Accept);
        }

        /// <summary>
        /// The callback after the accept completed.
        /// </summary>
        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        /// <summary>
        /// The callback after a send or receive operation completed.
        /// </summary>
        private void OnIOCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
            }
        }

        /// <summary>
        /// After the asynchronous connecton operation completed, this method will be called.
        /// </summary>
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            Socket s = e.AcceptSocket;
            if (s != null && s.Connected)
            {
                try
                {
                    SocketAsyncEventArgs saea_Receive = _availableSAEAPool[0];
                    _availableSAEAPool.Remove(saea_Receive);
                    _occupiedSAEAPool.Add(saea_Receive);
                    if (saea_Receive != null)
                    {
                        Receive_SAEAUToken userToken = (Receive_SAEAUToken)saea_Receive.UserToken;
                        userToken.Socket = s;

                        if (!userToken.Socket.ReceiveAsync(saea_Receive))
                            ProcessReceive(saea_Receive);
                    }
                    else
                    {
                        s.Close();
                    }
                }
                catch { }
            }
            StartAccept(e);
        }

        /// <summary>
        /// After the asynchronous receive operation completed, this method will be called.
        /// </summary>
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            try
            {
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    Receive_SAEAUToken userToken = (Receive_SAEAUToken)e.UserToken;
                    userToken.HeartbeatTime = DateTime.Now;

                    if (DataReceived != null)
                    {
                        byte[] receivedData = new byte[e.BytesTransferred];
                        Array.Copy(e.Buffer, e.Offset, receivedData, 0, e.BytesTransferred);
                        DataReceived(receivedData);
                    }
                    if (!userToken.Socket.ReceiveAsync(e))
                        ProcessReceive(e);
                }
                else CloseClientSocket(e);
            }
            catch { }
        }

        /// <summary>
        /// After the asynchronous send operation completed, this method will be called.
        /// </summary>
        private void ProcessSend(SocketAsyncEventArgs e)
        {

        }

        /// <summary>
        /// Socket close operation.
        /// </summary>
        private void CloseClientSocket(SocketAsyncEventArgs saea)
        {
            try
            {
                Receive_SAEAUToken userToken = (Receive_SAEAUToken)saea.UserToken;
                _occupiedSAEAPool.Remove(saea);
                _availableSAEAPool.Add(saea);
                if (userToken.Socket != null)
                {
                    if (userToken.Socket.Connected)
                        userToken.Socket.Shutdown(SocketShutdown.Both);
                    userToken.Socket.Close();
                }
            }
            catch { }
        }

        /// <summary>
        /// Cheak for the client's heartbeat.
        /// </summary>
        private void CheckClientHeartbeat()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(HeartbeatInterval);
                    if (_occupiedSAEAPool != null && _occupiedSAEAPool.Count > 0)
                    {
                        foreach (SocketAsyncEventArgs saea in _occupiedSAEAPool)
                        {
                            Receive_SAEAUToken userToken = (Receive_SAEAUToken)saea.UserToken;
                            if (((Receive_SAEAUToken)saea.UserToken).HeartbeatTime.
                                AddMilliseconds(HeartbeatInterval).CompareTo(DateTime.Now) < 0)
                                userToken.Socket?.Close();
                        }
                    }
                }
                catch { }
            }
        }
        #endregion

        #region Callback
        public delegate void DataReceivedHandler(byte[] data);
        public event DataReceivedHandler DataReceived;
        #endregion
    }
}
