using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odin.Networking.Packets.Incoming
{
    public class SetStartTimestamp : Packet
    {
        public long StartTimeStamp { get; set; }

        public override PacketType Type => PacketType.SET_START_TIME;

        public override void Handle(RealmClient client)
        {
            Discord.StartTimestamp = StartTimeStamp;
        }

        protected override void Read(PacketReader rdr)
        {
            StartTimeStamp = rdr.ReadInt64();
        }
    }
}
