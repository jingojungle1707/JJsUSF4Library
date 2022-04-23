using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BVSClasses
{
    public class ResourceItem
    {
        public GFXResource Resource { get; set; }
        public int UnkShort0x04 { get; set; } //Used a bunch in RYX
        public int UnkShort0x06 { get; set; }
        public float UnkFloat0x08 { get; set; } //Always 1.0?
        public float UnkFloat0x0C { get; set; } //Always 0/unused?
        public ResourceItem()
        {

        }
        public ResourceItem(BinaryReader br, Dictionary<int, GFXResource> gfxResourcesByOffset, int offset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            int resourcePointer = br.ReadInt32();
            UnkShort0x04 = br.ReadInt16();
            UnkShort0x06 = br.ReadInt16();
            UnkFloat0x08 = br.ReadSingle();
            UnkFloat0x0C = br.ReadSingle();

            Resource = gfxResourcesByOffset[offset + resourcePointer];
        }
        public List<byte> GenerateBytes()
        {
            List<byte> data = new List<byte>();

            USF4Utils.AddIntAsBytes(data, 0, true); //resource pointer to be updated later
            USF4Utils.AddIntAsBytes(data, UnkShort0x04, false);
            USF4Utils.AddIntAsBytes(data, UnkShort0x06, false);
            USF4Utils.AddFloatAsBytes(data, UnkFloat0x08);
            USF4Utils.AddFloatAsBytes(data, UnkFloat0x0C);

            return data;
        }
    }
}
