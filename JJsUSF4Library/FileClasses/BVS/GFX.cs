using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class GFX
    {
        public List<ResourceItem> ResourceItems { get; set; } = new List<ResourceItem>();
        public string Name { get; set; }
        public int GFXID { get; set; }
        public int UnkLong0x22 { get; set; }
        //0x26 long resource count
        public int UnkShort0x2A { get; set; } //probably unused
        //0x2C long resource pointer

        public GFX(BinaryReader br, int offset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            Name = Encoding.ASCII.GetString(br.ReadBytes(0x20)).Split('\0')[0];
            GFXID = br.ReadInt16();
            UnkLong0x22 = br.ReadInt32();
            int resourceItemsCount = br.ReadInt32();
            UnkShort0x2A = br.ReadInt16();
            int resourceItemsPointer = br.ReadInt32();

            for (int i = 0; i < resourceItemsCount; i++)
            {
                ResourceItems.Add(new ResourceItem(br, offset + resourceItemsPointer + i * 0x10));
            }
        }

        public List<byte> GenerateHeaderBytes()
        {
            List<byte> data = new List<byte>();

            data.AddRange(USF4Utils.StringToNullPaddedBytes(Name, 0x20, out _));
            USF4Utils.AddIntAsBytes(data, GFXID, false);
            USF4Utils.AddIntAsBytes(data, UnkLong0x22, true);
            USF4Utils.AddIntAsBytes(data, ResourceItems.Count, true);
            USF4Utils.AddIntAsBytes(data, UnkShort0x2A, false);
            USF4Utils.AddIntAsBytes(data, 0, true); //ResourceItem pointer - to be updated later

            return data;
        }

        public List<byte> GenerateBytes()
        {
            throw new NotImplementedException();
        }
    }
}
