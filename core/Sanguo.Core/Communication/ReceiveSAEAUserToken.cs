using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Sanguo.Core.Communication
{
    /// <summary>
    /// Represents the user token of a SocketAsyncEventArgs of a receive socket.
    /// </summary>
    public class ReceiveSAEAUToken
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
}
