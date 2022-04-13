using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class ResourceInstanceParamType1
    {
        public float UnkFloat0x00 { get; set; } 
        public float UnkFloat0x04 { get; set; } 
        public float UnkFloat0x08 { get; set; }
        public float UnkFloat0x0C { get; set; }
        public float UnkFloat0x10 { get; set; }
        public float UnkFloat0x14 { get; set; }
        public float UnkFloat0x18 { get; set; }
        public float UnkFloat0x1C { get; set; }
        public ResourceInstanceParamType1()
        {

        }
        public ResourceInstanceParamType1(BinaryReader br, int offset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            UnkFloat0x00 = br.ReadSingle();
            UnkFloat0x04 = br.ReadSingle();
            UnkFloat0x08 = br.ReadSingle();
            UnkFloat0x0C = br.ReadSingle();
            UnkFloat0x10 = br.ReadSingle();
            UnkFloat0x14 = br.ReadSingle();
            UnkFloat0x18 = br.ReadSingle();
            UnkFloat0x1C = br.ReadSingle();
        }
        public List<byte> GenerateBytes()
        {
            List<byte> data = new List<byte>();

            USF4Utils.AddFloatAsBytes(data, UnkFloat0x00);
            USF4Utils.AddFloatAsBytes(data, UnkFloat0x04);
            USF4Utils.AddFloatAsBytes(data, UnkFloat0x08);
            USF4Utils.AddFloatAsBytes(data, UnkFloat0x0C);
            USF4Utils.AddFloatAsBytes(data, UnkFloat0x10);
            USF4Utils.AddFloatAsBytes(data, UnkFloat0x14);
            USF4Utils.AddFloatAsBytes(data, UnkFloat0x18);
            USF4Utils.AddFloatAsBytes(data, UnkFloat0x1C);

            return data;
        }
    }
}
