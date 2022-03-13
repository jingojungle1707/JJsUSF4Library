using System.Collections.Generic;
using System.IO;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class Hitbox
    {
        public List<HitboxDatablock> Datablocks;

        public Hitbox(BinaryReader br, int offset = 0)
        {
            Datablocks = new List<HitboxDatablock>();
            for (int i = 0; i < 0x0C; i++) Datablocks.Add(new HitboxDatablock(br, offset + i * 0x50));
        }

        public Hitbox()
        {

        }

        public byte[] GenerateBytes()
        {
            List<byte> Data = new List<byte>();

            foreach (HitboxDatablock hdb in Datablocks) Data.AddRange(hdb.GenerateBytes());

            return Data.ToArray();
        }
    }
}