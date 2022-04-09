using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class ResourceInstanceParamType2
    {
        public float UnkFloat0x00 { get; set; } 
        public int UnkShort0x04 { get; set; } //Script ID? Another effect ID?
        public int UnkShort0x06 { get; set; } //Unused, or previous field should be Long
        public int UnkShort0x08 { get; set; } // = 01
        public int UnkShort0x0A { get; set; } //Unused
        public int UnkLong0x0C { get; set; } //Unused
        public int UnkLong0x10 { get; set; } //Unused
        public int UnkLong0x14 { get; set; } //Unused
        public int UnkLong0x18 { get; set; } //Unused
        public int UnkLong0x1C { get; set; } //Unused

        public ResourceInstanceParamType2(BinaryReader br, int offset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            UnkFloat0x00 = br.ReadSingle();
            UnkShort0x04 = br.ReadInt16();
            UnkShort0x06 = br.ReadInt16();
            UnkShort0x08 = br.ReadInt16();
            UnkShort0x0A = br.ReadInt16();
            UnkLong0x0C = br.ReadInt32();
            UnkLong0x10 = br.ReadInt32();
            UnkLong0x14 = br.ReadInt32();
            UnkLong0x18 = br.ReadInt32();
            UnkLong0x1C = br.ReadInt32();
        }
        public List<byte> GenerateBytes()
        {
            List<byte> data = new List<byte>();

            USF4Utils.AddFloatAsBytes(data, UnkFloat0x00);
            USF4Utils.AddIntAsBytes(data, UnkShort0x04, false);
            USF4Utils.AddIntAsBytes(data, UnkShort0x06, false);
            USF4Utils.AddIntAsBytes(data, UnkShort0x08, false);
            USF4Utils.AddIntAsBytes(data, UnkShort0x0A, false);
            USF4Utils.AddIntAsBytes(data, UnkLong0x0C, true);
            USF4Utils.AddIntAsBytes(data, UnkLong0x10, true);
            USF4Utils.AddIntAsBytes(data, UnkLong0x14, true);
            USF4Utils.AddIntAsBytes(data, UnkLong0x18, true);
            USF4Utils.AddIntAsBytes(data, UnkLong0x1C, true);

            return data;
        }
    }
}
