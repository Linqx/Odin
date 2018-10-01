using Odin.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Odin.Networking.Packets
{
    public abstract class Packet
    {
        public object State;
        public byte Id => (byte)Type;
        public abstract PacketType Type { get; }

        public abstract void Handle(RealmClient client);

        public byte[] GetData()
        {
            using (var stream = new MemoryStream())
            {
                var wtr = new PacketWriter(stream);
                wtr.Write(0);  // Reserve 4 bytes for packet size
                wtr.Write(Id); // Write packet id
                Write(wtr);    // Write packet

                byte[] data = stream.ToArray();
                byte[] sizeBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data.Length));

                data[0] = sizeBytes[0];
                data[1] = sizeBytes[1];
                data[2] = sizeBytes[2];
                data[3] = sizeBytes[3];

                return data;
            }
        }

        protected virtual void Read(PacketReader rdr)
        {
        }

        protected virtual void Write(PacketWriter wtr)
        {
        }

        public static Packet Parse(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                var r = new PacketReader(stream);
                byte id = r.ReadByte();

                Packet packet = null;
                if (_packetTypes.Value.TryGetValue(id, out Type type))
                    packet = (Packet)Activator.CreateInstance(type);
                else
                    Logger.Log("Packet", $"Unkown packet id received! id: {id}", ConsoleColor.Red);

                packet.Read(r);
                r.Dispose();

                return packet;
            }
        }

        private static readonly ThreadLocal<Dictionary<byte, Type>> _packetTypes = new ThreadLocal<Dictionary<byte, Type>>(() =>
        {
            var type = typeof(Packet);
            return type.Assembly.GetTypes().Where(_ => _.IsSubclassOf(type))
                .ToDictionary(_ => ((Packet)Activator.CreateInstance(_)).Id);
        });

        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            ret.Append("{ ");
            PropertyInfo[] arr = GetType().GetProperties();
            for (int i = 0; i < arr.Length; i++)
            {
                if (i != 0) ret.Append(", ");
                ret.AppendFormat("{0}: {1}", arr[i].Name, arr[i].GetValue(this, null));
            }
            ret.Append(" }");
            return ret.ToString();
        }
    }

    public enum PacketType : byte
    {
        SET_LARGE_IMAGE_KEY = 1,
        SET_LARGE_IMAGE_TEXT = 2,
        SET_SMALL_IMAGE_KEY = 3,
        SET_SMALL_IMAGE_TEXT = 4,
        SET_STATE = 5,
        SET_DETAILS = 6,
        SET_START_TIME = 7,
        SET_END_TIME = 8
    }
}
