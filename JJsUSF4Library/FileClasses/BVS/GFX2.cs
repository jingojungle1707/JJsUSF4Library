using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class GFX2
    {
        public List<ResourceItem> ResourceItems { get; set; } = new List<ResourceItem>();
        public string Name { get; set; }
        public int GFX2ID { get; set; }
        public int UnkLong0x22 { get; set; }
        public int UnkLong0x26 { get; set; }
        //public List<GFXResource> Resources { get; set; } = new List<GFXResource>();

        //0x2A short resource count
        public int UnkShort0x2A { get; set; } //probably unused
        //0x2C long resource pointer

        public GFX2(BinaryReader br, int offset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            Name = br.ReadZString();
            GFX2ID = br.ReadInt16();
            UnkLong0x22 = br.ReadInt32();
            UnkLong0x26 = br.ReadInt32();
            int resourceCount = br.ReadInt16();
            int resourcePointer = br.ReadInt32();

            for (int i = 0; i < resourceCount; i++)
            {
                ResourceItems.Add(new ResourceItem(br, offset + resourcePointer + i * 0x10));
            }
        }

        public List<byte> GenerateBytes()
        {
            throw new NotImplementedException();
        }
    }
}
