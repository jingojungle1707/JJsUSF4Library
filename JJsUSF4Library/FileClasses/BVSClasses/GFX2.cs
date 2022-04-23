using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BVSClasses
{
    public class GFX2
    {
        public List<ResourceItem> ResourceItems { get; set; } = new List<ResourceItem>();
        public string Name { get; set; }
        public int GFX2ID { get; set; }
        public int UnkLong0x22 { get; set; }
        public int UnkLong0x26 { get; set; }

        //0x2A short resource count
        //0x2C long resource pointer
        public GFX2()
        {

        }
        public GFX2(BinaryReader br, Dictionary<int, GFXResource> gfxResourcesByOffset, int offset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            Name = Encoding.ASCII.GetString(br.ReadBytes(0x20)).Split('\0')[0];
            GFX2ID = br.ReadInt16();
            UnkLong0x22 = br.ReadInt32();
            UnkLong0x26 = br.ReadInt32();
            int resourceCount = br.ReadInt16();
            int resourcePointer = br.ReadInt32();

            for (int i = 0; i < resourceCount; i++)
            {
                ResourceItems.Add(new ResourceItem(br, gfxResourcesByOffset, offset + resourcePointer + i * 0x10));
            }
        }
        public List<byte> GenerateHeaderBytes()
        {
            List<byte> data = new List<byte>();

            data.AddRange(USF4Utils.StringToNullPaddedBytes(Name, 0x20, out _));
            USF4Utils.AddIntAsBytes(data, GFX2ID, false);
            USF4Utils.AddIntAsBytes(data, UnkLong0x22, true);
            USF4Utils.AddIntAsBytes(data, UnkLong0x26, true);
            USF4Utils.AddIntAsBytes(data, ResourceItems.Count, false);
            USF4Utils.AddIntAsBytes(data, 0, true); //ResourceItem pointer - to be updated later

            return data;
        }
    }
}
