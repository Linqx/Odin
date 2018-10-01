using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odin.Networking.Packets.Incoming
{
    public class SetSmallImageText : Packet
    {
        public string SmallImageText { get; set; }

        public override PacketType Type => PacketType.SET_SMALL_IMAGE_TEXT;

        public override void Handle(RealmClient client)
        {
            Discord.SmallImageText = SmallImageText;
        }

        protected override void Read(PacketReader rdr)
        {
            SmallImageText = rdr.ReadUTF();
        }
    }
}
