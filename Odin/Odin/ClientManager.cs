using Odin.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Odin
{
    public class ClientManager
    {
        #region Public

        #endregion

        #region Private

        private Socket _socket;
        private int _port;

        private RealmClient _client;

        #endregion

        #region Constructor

        public ClientManager()
        {
            _port = Program.Config.Port;

            Logger.Log("Client Manager", $"Listening for Clients on port: {_port}...", ConsoleColor.White);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IPAddress.Any, _port));
            _socket.Listen(0xff);
            _socket.BeginAccept(Listen, null);
        }

        #endregion

        #region Public Methods

        public void OnDisconnect(RealmClient client)
        {
            _client = null;
            _socket.BeginAccept(Listen, null);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Listens for a socket until a RealmClient has connected. Will not Listen for another connection.
        /// </summary>
        /// <param name="ar"></param>
        private void Listen(IAsyncResult ar)
        {           
            try
            {
                var socket = _socket.EndAccept(ar);
                if (socket != null)
                    _client = new RealmClient(socket, this);
            }
            catch (Exception e)
            {
                Logger.Log("Client Manager", e, ConsoleColor.Red);
            }
            finally
            {
                if (_client == null)
                {
                    try
                    {
                        _socket.BeginAccept(Listen, null);
                    }
                    catch (Exception e)
                    {
                        Logger.Log("Client Manager", e);
                    }
                }
            }
        }

        #endregion
    }
}
