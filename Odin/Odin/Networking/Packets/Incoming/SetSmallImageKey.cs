using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odin.Networking.Packets.Incoming
{
    public class SetSmallImageKey : Packet
    {
        public string SmallImageKey { get; set; }

        public override PacketType Type => PacketType.SET_SMALL_IMAGE_KEY;

        public override void Handle(RealmClient client)
        {
            Discord.SmallImageKey = SmallImageKey;
        }

        protected override void Read(PacketReader rdr)
        {
            SmallImageKey = rdr.ReadUTF();
        }
    }
}
