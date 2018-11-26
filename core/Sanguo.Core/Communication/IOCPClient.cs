using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sanguo.Core.Communication
{
    public class IOCPClient
    {
        public int BufferSize { get; set; } = 2048;
        private Socket _socket;
        private SocketAsyncEventArgs _receiveArgs;
        private SocketAsyncEventArgs _sendArgs;
        private IPAddress _ip;
        private int _port;
        private byte[] _buffer;
        public IOCPClient(IPAddress ip, int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _ip = ip;
            _port = port;
        }
        public void Connect()
        {
            _socket.Connect(_ip, _port);
        }
        public void Connect(int c)
        {
            _socket.Connect(_ip, _port);
        }
        public void Listen()
        {
            if (_receiveArgs == null)
            {
                _receiveArgs = new SocketAsyncEventArgs();
                _receiveArgs.Completed += (sender, e) =>
                {
                    // Determine which type of operation just completed and call the associated handler.
                    switch (e.LastOperation)
                    {
                        case SocketAsyncOperation.Receive:
                            ProcessReceive(e);
                            break;
                        default:
                            throw new ArgumentException("The last operation completed on the socket was not a receive or send");
                    }
                };
                _buffer = new byte[BufferSize];
                _receiveArgs.SetBuffer(_buffer, 0, BufferSize);
                _socket.ReceiveAsync(_receiveArgs);
            }
        }
        public void Close() => _socket.Dispose();

        #region 接收数据

        /// <summary>
        ///接收完成时处理函数
        /// </summary>
        /// <param name="e">与接收完成操作相关联的SocketAsyncEventArg对象</param>
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)//if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                // 检查远程主机是否关闭连接
                if (e.BytesTransferred > 0)
                {
                    if (_socket.Available == 0)
                        DataReceived?.Invoke(this, e);
                    if (_socket.Connected&&!_socket.ReceiveAsync(e))//为接收下一段数据，投递接收请求，这个函数有可能同步完成，这时返回false，并且不会引发SocketAsyncEventArgs.Completed事件
                            //同步接收时处理接收完成事件
                        ProcessReceive(e);
                }
            }
            else
            {
                CloseClientSocket(e);
            }
        }

        #endregion

        #region 发送数据

        /// <summary>
        /// 异步的发送数据
        /// </summary>
        /// <param name="e"></param>
        public void Send(byte[] data) {
            if (_sendArgs == null)
                _sendArgs = new SocketAsyncEventArgs();
            _sendArgs.SetBuffer(data, 0, data.Length);
            _sendArgs.Completed += DataSent;
            _socket.SendAsync(_sendArgs);
        }

        public void Send(string str) => Send(Encoding.Default.GetBytes(str));

        #endregion

        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            Socket s = e.UserToken as Socket;
            try
            {
                s.Shutdown(SocketShutdown.Send);
            }
            catch (Exception)
            {
                // Throw if client has closed, so it is not necessary to catch.
            }
            finally
            {
                s.Close();
            }
        }
    
        public event EventHandler<SocketAsyncEventArgs> DataReceived;
        public event EventHandler<SocketAsyncEventArgs> DataSent;
    }
}
