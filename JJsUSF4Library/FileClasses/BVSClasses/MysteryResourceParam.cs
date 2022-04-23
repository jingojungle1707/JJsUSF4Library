using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BVSClasses
{
    public class MysteryResourceParam
    {
        public float UnkFloat0x00 { get; set; } //Int stored as float
        public float UnkFloat0x04 { get; set; }
        public float UnkFloat0x08 { get; set; }
        public float UnkFloat0x0C { get; set; }
        public float UnkFloat0x10 { get; set; }
        public float UnkFloat0x14 { get; set; }
        public float UnkFloat0x18 { get; set; }
        public float UnkFloat0x1C { get; set; }
        public float UnkFloat0x20 { get; set; }
        public float UnkFloat0x24 { get; set; }
        public float UnkFloat0x28 { get; set; }
        public float UnkFloat0x2C { get; set; }
        public float UnkFloat0x30 { get; set; }
        public float UnkFloat0x34 { get; set; }
        public int UnkLong0x38 { get; set; } //0x01
        public int UnkLong0x3C { get; set; } //0x00

        public MysteryResourceParam(BinaryReader br)
        {
            UnkFloat0x00 = br.ReadSingle();
            UnkFloat0x04 = br.ReadSingle();
            UnkFloat0x08 = br.ReadSingle();
            UnkFloat0x0C = br.ReadSingle();
            UnkFloat0x10 = br.ReadSingle();
            UnkFloat0x14 = br.ReadSingle();
            UnkFloat0x18 = br.ReadSingle();
            UnkFloat0x1C = br.ReadSingle();
            UnkFloat0x20 = br.ReadSingle();
            UnkFloat0x24 = br.ReadSingle();
            UnkFloat0x28 = br.ReadSingle();
            UnkFloat0x2C = br.ReadSingle();
            UnkFloat0x30 = br.ReadSingle();
            UnkFloat0x34 = br.ReadSingle();
            UnkLong0x38 = br.ReadInt32();
            UnkLong0x3C = br.ReadInt32();
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
            USF4Utils.AddFloatAsBytes(data, UnkFloat0x20);
            USF4Utils.AddFloatAsBytes(data, UnkFloat0x24);
            USF4Utils.AddFloatAsBytes(data, UnkFloat0x28);
            USF4Utils.AddFloatAsBytes(data, UnkFloat0x2C);
            USF4Utils.AddFloatAsBytes(data, UnkFloat0x30);
            USF4Utils.AddFloatAsBytes(data, UnkFloat0x34);
            USF4Utils.AddIntAsBytes(data, UnkLong0x38, true);
            USF4Utils.AddIntAsBytes(data, UnkLong0x3C, true);

            return data;
        }
    }
}
