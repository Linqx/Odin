using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odin.Networking.Packets.Incoming
{
    public class SetLargeImageText : Packet
    {
        public string LargeImageText { get; set; }

        public override PacketType Type => PacketType.SET_LARGE_IMAGE_TEXT;

        public override void Handle(RealmClient client)
        {
            Discord.LargeImageText = LargeImageText;
        }

        protected override void Read(PacketReader rdr)
        {
            LargeImageText = rdr.ReadUTF();
        }
    }
}
