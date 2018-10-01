using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odin.Networking.Packets.Incoming
{
    public class SetDetails : Packet
    {
        public string Details { get; set; }

        public override PacketType Type => PacketType.SET_DETAILS;

        public override void Handle(RealmClient client)
        {
            Discord.Details = Details;
        }

        protected override void Read(PacketReader rdr)
        {
            Details = rdr.ReadUTF();
        }        
    }
}
