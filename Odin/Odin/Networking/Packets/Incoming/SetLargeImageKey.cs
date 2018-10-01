using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odin.Networking.Packets.Incoming
{
    public class SetLargeImageKey : Packet
    {
        public string LargeImageKey { get; set; }

        public override PacketType Type => PacketType.SET_LARGE_IMAGE_KEY;

        public override void Handle(RealmClient client)
        {
            Discord.LargeImageKey = LargeImageKey;
        }

        protected override void Read(PacketReader rdr)
        {
            LargeImageKey = rdr.ReadUTF();
        }
    }
}
