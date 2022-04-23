using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BVSClasses
{
    public class ResourceInstance
    {
        public List<ResourceInstanceParamType1> ParamsType1 { get; set; } = new List<ResourceInstanceParamType1>();
        public List<ResourceInstanceParamType2> ParamsType2 { get; set; } = new List<ResourceInstanceParamType2>();
        public byte UnkByte0x00 { get; set; }
        public byte UnkByte0x01 { get; set; }
        public byte UnkByte0x02 { get; set; }
        public byte UnkByte0x03 { get; set; }
        public byte UnkByte0x04 { get; set; }
        public byte UnkByte0x05 { get; set; }
        public byte UnkByte0x06 { get; set; }
        public byte UnkByte0x07 { get; set; }
        public int BitFlag { get; set; }
        public int UnkShort0x0A { get; set; }
        public int UnkShort0x0C { get; set; }
        public int UnkShort0x0E { get; set; } //0x00 or 0x01
        public float UnkFloat0x10 { get; set; } //seen in SKR.vfx.bvs
        public int UnkLong0x14 { get; set; }
        public int UnkLong0x18 { get; set; }
        public int UnkLong0x1C { get; set; }
        public int UnkLong0x20 { get; set; }
        public ResourceInstance()
        {

        }
        public ResourceInstance(BinaryReader br, int offset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            UnkByte0x00 = br.ReadByte();
            UnkByte0x01 = br.ReadByte();
            UnkByte0x02 = br.ReadByte();
            UnkByte0x03 = br.ReadByte();
            UnkByte0x04 = br.ReadByte();
            UnkByte0x05 = br.ReadByte();
            UnkByte0x06 = br.ReadByte();
            UnkByte0x07 = br.ReadByte();
            BitFlag = br.ReadInt16();
            UnkShort0x0A = br.ReadInt16();
            UnkShort0x0C = br.ReadInt16();
            UnkShort0x0E = br.ReadInt16();
            //0x10
            UnkFloat0x10 = br.ReadSingle();
            UnkLong0x14 = br.ReadInt32();
            UnkLong0x18 = br.ReadInt32();
            UnkLong0x1C = br.ReadInt32();
            //0x20
            UnkLong0x20 = br.ReadInt32();
            int paramsType1Count = br.ReadInt16();
            int paramsType2Count = br.ReadInt16();
            int paramsType1Pointer = br.ReadInt32();
            int paramsType2Pointer = br.ReadInt32();

            if (paramsType1Count > 0)
            {
                for (int i = 0; i < paramsType1Count; i++)
                {
                    ParamsType1.Add(new ResourceInstanceParamType1(br, offset + paramsType1Pointer + i * 0x20));
                }
            }
            if (paramsType2Count > 0)
            {
                for (int i = 0; i < paramsType2Count; i++)
                {
                    ParamsType2.Add(new ResourceInstanceParamType2(br, offset + paramsType2Pointer + i * 0x20));
                }
            }
        }
        public List<byte>GenerateHeaderBytes()
        {
            List<byte> data = new List<byte>();

            data.Add(UnkByte0x00);
            data.Add(UnkByte0x01);
            data.Add(UnkByte0x02);
            data.Add(UnkByte0x03);
            data.Add(UnkByte0x04);
            data.Add(UnkByte0x05);
            data.Add(UnkByte0x06);
            data.Add(UnkByte0x07);
            USF4Utils.AddIntAsBytes(data, BitFlag, false);
            USF4Utils.AddIntAsBytes(data, UnkShort0x0A, false);
            USF4Utils.AddIntAsBytes(data, UnkShort0x0C, false);
            USF4Utils.AddIntAsBytes(data, UnkShort0x0E, false);
            USF4Utils.AddFloatAsBytes(data, UnkFloat0x10);
            USF4Utils.AddIntAsBytes(data, UnkLong0x14, true);
            USF4Utils.AddIntAsBytes(data, UnkLong0x18, true);
            USF4Utils.AddIntAsBytes(data, UnkLong0x1C, true);
            USF4Utils.AddIntAsBytes(data, UnkLong0x20, true);
            USF4Utils.AddIntAsBytes(data, ParamsType1.Count, false);
            USF4Utils.AddIntAsBytes(data, ParamsType2.Count, false);
            USF4Utils.AddIntAsBytes(data, 0, true); //Params type1 offset
            USF4Utils.AddIntAsBytes(data, 0, true); //Params type2 offset

            return data;
        }

    }
}
