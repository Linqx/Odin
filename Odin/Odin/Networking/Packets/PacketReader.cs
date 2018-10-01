using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Odin.Networking.Packets
{
    public class PacketReader : BinaryReader
    {
        public PacketReader(Stream str) : base(str, Encoding.UTF8)
        {
        }

        #region Read

        public override short ReadInt16() => IPAddress.NetworkToHostOrder(base.ReadInt16());
        public override int ReadInt32() => IPAddress.NetworkToHostOrder(base.ReadInt32());
        public override long ReadInt64() => IPAddress.NetworkToHostOrder(base.ReadInt64());
        public override ushort ReadUInt16() => (ushort)IPAddress.NetworkToHostOrder((short)base.ReadUInt16());
        public override uint ReadUInt32() => (uint)IPAddress.NetworkToHostOrder((int)base.ReadUInt32());
        public override ulong ReadUInt64() => (ulong)IPAddress.NetworkToHostOrder((long)base.ReadUInt64());
        public string ReadUTF() => Encoding.UTF8.GetString(ReadBytes(ReadInt16()));
        public string Read32UTF() => Encoding.UTF8.GetString(ReadBytes(ReadInt32()));

        public override float ReadSingle()
        {
            byte[] arr = base.ReadBytes(4);
            Array.Reverse(arr);
            return BitConverter.ToSingle(arr, 0);
        }

        public override double ReadDouble()
        {
            byte[] arr = base.ReadBytes(8);
            Array.Reverse(arr);
            return BitConverter.ToDouble(arr, 0);
        }

        public string ReadNullTerminatedString()
        {
            StringBuilder ret = new StringBuilder();
            byte b = ReadByte();
            while (b != 0)
            {
                ret.Append((char)b);
                b = ReadByte();
            }
            return ret.ToString();
        }

        #endregion
    }
}
