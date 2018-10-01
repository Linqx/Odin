using Odin.Networking.Packets;
using Odin.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Odin.Networking
{
    public class NetworkConnection : IDisposable
    {
        private const int PORT = 14006;

        /// <summary>
        /// The name of the connection
        /// </summary>
        public virtual string Name => "Network Connection";

        /// <summary>
        /// The remote host this <see cref="NetworkConnection"/> is connected to
        /// </summary>
        public readonly string Host;

        /// <summary>
        /// The <see cref="Socket"/> used to communicate with the remote program
        /// </summary>
        private Socket _socket;

        /// <summary>
        /// Called on socket disconnection
        /// </summary>
        public Action OnDisconnect;

        /// <summary>
        /// Name of the connection
        /// </summary>
        public string ConnectionName { get; set; }

        /// <summary>
        /// Socket connected.
        /// </summary>
        public bool Connected = false;

        /// <summary>
        /// Initializes with a previously established <see cref="Socket"/>
        /// </summary>
        /// <param name="socket"></param>
        public NetworkConnection(Socket socket)
        {
            Connected = true;
            SetupSocket(socket);
            _socket = socket;

            IPEndPoint endPoint = (IPEndPoint)socket.RemoteEndPoint;

            Host = endPoint.Address.ToString();
        }

        /// <summary>
        /// Starts the receiving process
        /// </summary>
        public void StartReceive()
        {
            if (_socket.Connected)
            {
                BeginRead(_receivePosition, _receiveSize);
            }
        }

        /// <summary>
        /// Sets up the socket state
        /// </summary>
        /// <param name="socket"></param>
        private void SetupSocket(Socket socket)
        {
            socket.NoDelay = true;
            socket.ReceiveTimeout = 5000;
            socket.SendTimeout = 5000;
        }

        /// <summary>
        /// Connects to the Host and Port specified in class variables
        /// </summary>
        /// <param name="callback"></param>
        public void Connect(Action<bool> callback) => Connect(Host, PORT, callback);

        /// <summary>
        /// Connects to a specific Host and Port
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="callback"></param>
        public void Connect(string host, int port, Action<bool> callback) => _socket.BeginConnect(host, port, ConnectCallback, callback);

        /// <summary>
        /// Called once the connect call finishes
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                _socket.EndConnect(ar);
                Connected = true;
            }
            catch (Exception e)
            {
                Logger.Log(ConnectionName, e, ConsoleColor.Red);
                Disconnect();
            }
            ((Action<bool>)ar.AsyncState).Invoke(Connected);

            StartReceive();
        }

        private bool _disconnected = false;
        private readonly object _disconnectLock = new object();

        /// <summary>
        /// Disconnects from the remote socket
        /// </summary>
        public void Disconnect()
        {
            lock (_disconnectLock)
            {
                if (_disconnected) return;
                _disconnected = true;
            }

            _socket.Dispose();

            if (Connected)
                OnDisconnect?.Invoke();
            Connected = false;
        }

        /// <summary>
        /// Disposes any resources held by this object
        /// </summary>
        public void Dispose()
        {
            Disconnect();
        }

        public void Reset()
        {
            Disconnect();

            _disconnected = false;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SetupSocket(_socket);

            MaxReceivedSize = int.MaxValue;
            _receiveBuffer = new byte[4];
            _receiveSize = 4;
            _receivePosition = 0;
            _receivedSize = false;

            _sending = false;
            _sendQueue.Clear();
        }

        #region Reading

        public int MaxReceivedSize = int.MaxValue;

        /// <summary>
        /// Receive buffer for data
        /// </summary>
        private byte[] _receiveBuffer = new byte[4];

        /// <summary>
        /// The current receive position in the buffer
        /// </summary>
        private int _receivePosition = 0;

        /// <summary>
        /// The current size awaiting to be received
        /// </summary>
        private int _receiveSize = 4;

        /// <summary>
        /// Determines if the size information has been received
        /// </summary>
        private bool _receivedSize = false;

        /// <summary>
        /// Dispatches when a packet is received
        /// </summary>
        public Action<Packet> HandlePacket;

        /// <summary>
        /// Parses a packet from byte data
        /// </summary>
        public Func<byte[], Packet> ParsePacket;

        /// <summary>
        /// Starts the read operation
        /// </summary>
        /// <param name="offset">The position to start writing data into the receive buffer</param>
        /// <param name="amount">The amount of data to write into the receive buffer</param>
        private void BeginRead(int offset, int amount)
        {
            if (!_socket.Connected) return;

            try
            {
                _socket.BeginReceive(_receiveBuffer, offset, amount, SocketFlags.None, ReceiveCallback, null);
            }
            catch (ObjectDisposedException)
            {

            }
            catch (SocketException)
            {
                Disconnect();
            }
            catch (Exception e)
            {
                Logger.Log(ConnectionName, e, ConsoleColor.Red);
                Disconnect();
            }
        }

        /// <summary>
        /// Called when data is received, or when a socket error occurs
        /// </summary>
        /// <param name="ar"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int length = _socket.EndReceive(ar);

                _receivePosition += length;
                int bytesRemaining = _receiveSize - _receivePosition;

                if (length <= 0) // Disconnect
                {
                    Disconnect();
                    return;
                }
                else if (bytesRemaining > 0) // Still needs more data to arrive
                {
                    BeginRead(_receivePosition, bytesRemaining);
                }
                else if (!_receivedSize) // Received packet size
                {
                    int size = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(_receiveBuffer, 0)) - 4; // Remove 4 bytes because already have size
                    if (size > MaxReceivedSize)
                    {
                        throw new InvalidOperationException(
                            "The received size " + size + " is larger than the max size " + MaxReceivedSize);
                    }
                    else
                    {
                        _receiveBuffer = new byte[size];
                        _receiveSize = size;
                        _receivePosition = 0;
                        _receivedSize = true;

                        BeginRead(_receivePosition, _receiveSize);
                    }
                }
                else // Received packet
                {
                    byte[] data = new byte[_receiveSize];
                    Array.Copy(_receiveBuffer, 0, data, 0, _receiveSize);

                    // Parse packet / Signal data received
                    if (ParsePacket != null)
                    {
                        try
                        {
                            var packet = ParsePacket.Invoke(data);
                            ReceivedPacket(packet);
                        }
                        catch (Exception e)
                        {
                            Logger.Log("Network Connection", e, ConsoleColor.Red);
                            if (_socket.Connected)
                            {
                                Disconnect();
                            }
                            return;
                        }
                    }

                    _receiveBuffer = new byte[4];
                    _receiveSize = 4;
                    _receivePosition = 0;
                    _receivedSize = false;

                    BeginRead(_receivePosition, _receiveSize);
                }
            }
            catch (ObjectDisposedException)
            {

            }
            catch (SocketException)
            {
                Disconnect();
            }
            catch (Exception e)
            {
                Logger.Log(ConnectionName, e, ConsoleColor.Red);
                if (_socket.Connected)
                {
                    Disconnect();
                }
            }
        }

        /// <summary>
        /// Called when data has been received and a packet was parsed from it
        /// </summary>
        /// <param name="packet"></param>
        protected virtual void ReceivedPacket(Packet packet)
        {
            HandlePacket?.Invoke(packet);
        }

        #endregion

        #region Sending

        /// <summary>
        /// Detemines if a packet is currently being sent
        /// </summary>
        private bool _sending = false;

        /// <summary>
        /// The queue that hold pending packets
        /// </summary>
        private readonly List<Packet> _sendQueue = new List<Packet>();

        /// <summary>
        /// An object used to synchronize send operations
        /// </summary>
        private readonly object _sendLock = new object();

        /// <summary>
        /// If available, sends a packet to the remote socket, queues the packet if already sending
        /// </summary>
        /// <param name="packet">The packet to send</param>
        public void Send(Packet packet, Action callback = null)
        {
            packet.State = callback;
            lock (_sendLock)
            {
                if (_sending) // If already sending, queues the packet to be sent when the current packet finishes being sent
                {
                    _sendQueue.Add(packet);
                    return;
                }
                else
                {
                    _sending = true;
                }
            }

            SendPacket(packet);
        }

        /// <summary>
        /// Sends a packet to the remote socket
        /// </summary>
        /// <param name="packet">The packet to send</param>
        private void SendPacket(Packet packet)
        {
            try
            {
                byte[] data = packet.GetData();

                _socket.BeginSend(data, 0, data.Length, SocketFlags.None, SendCallback, packet.State);
            }
            catch (ObjectDisposedException)
            {

            }
            catch (SocketException)
            {
                Disconnect();
            }
            catch (Exception e) // Catches any send errors
            {
                Logger.Log(ConnectionName, e, ConsoleColor.Red);
                Disconnect();
            }
        }

        /// <summary>
        /// Called when a send function finishes, also checks the packet queue
        /// </summary>
        /// <param name="ar"></param>
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                int sent = _socket.EndSend(ar);

                ((Action)ar.AsyncState)?.Invoke();

                lock (_sendLock) // Checks the packet queue
                {
                    if (_sendQueue.Count > 0)
                    {
                        var packet = _sendQueue[0];
                        _sendQueue.RemoveAt(0);
                        SendPacket(packet);
                    }
                    else
                    {
                        _sending = false;
                    }
                }
            }
            catch (ObjectDisposedException)
            {

            }
            catch (SocketException)
            {
                Disconnect();
            }
            catch (Exception e)
            {
                Logger.Log(ConnectionName, e, ConsoleColor.Red);
                Disconnect();
            }
        }

        #endregion
    }
}
