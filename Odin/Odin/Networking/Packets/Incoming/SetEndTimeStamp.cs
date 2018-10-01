using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odin.Networking.Packets.Incoming
{
    public class SetEndTimeStamp : Packet
    {
        public long EndTimestamp { get; set; }

        public override PacketType Type => PacketType.SET_END_TIME;

        public override void Handle(RealmClient client)
        {
            Discord.EndTimestamp = EndTimestamp;
        }

        protected override void Read(PacketReader rdr)
        {
            EndTimestamp = rdr.ReadInt64();
        }
    }
}
