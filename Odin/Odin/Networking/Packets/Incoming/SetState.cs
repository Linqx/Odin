using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odin.Networking.Packets.Incoming
{
    public class SetState : Packet
    {
        public new string State { get; set; }

        public override PacketType Type => PacketType.SET_STATE;

        public override void Handle(RealmClient client)
        {
            Discord.State = State;
        }

        protected override void Read(PacketReader rdr)
        {
            State = rdr.ReadUTF();
        }        
    }
}
