using Odin.Networking;
using Odin.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Odin
{
    public class RealmClient
    {
        private ClientManager _manager;
        private NetworkConnection _connection;

        private bool _disconnected = false;

        public RealmClient(Socket socket, ClientManager manager)
        {
            _manager = manager;

            _connection = new NetworkConnection(socket);
            _connection.OnDisconnect = Disconnected;
            _connection.HandlePacket = HandlePacket;
            _connection.ParsePacket = Packet.Parse;
            _connection.ConnectionName = "Realm Client";

            _connection.StartReceive();
        }

        #region Public Methods        

        #endregion

        #region Private Methods

        public void Disconnected()
        {
            if (_disconnected) return;

            Dispose();
        }

        private void Dispose()
        {
            _manager.OnDisconnect(this);
            _connection.Dispose();
        }

        private void HandlePacket(Packet packet)
            => packet.Handle(this);

        #endregion

    }
}
