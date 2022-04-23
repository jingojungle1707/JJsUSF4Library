using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BVSClasses
{
    public class MysteryResource
    {
        public string Name { get; set; }
        public int MysteryResourceID { get; set; }
        public int UnkShort0x22 { get; set; }
        public int UnkLong0x24 { get; set; }
        public float UnkFloat0x28 { get; set; }
        public int UnkShort0x2C { get; set; }
        public int UnkShort0x2E { get; set; }
        public int UnkShort0x30 { get; set; }

        //0x32 short paramsCount
        //0x34 long paramsPointer
        public List<MysteryResourceParam> MysteryResourceParams { get; set; } = new List<MysteryResourceParam>();

        public MysteryResource()
        {

        }
        public MysteryResource(BinaryReader br, int offset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            Name = Encoding.ASCII.GetString(br.ReadBytes(0x20)).Split('\0')[0];
            MysteryResourceID = br.ReadInt16();
            UnkShort0x22 = br.ReadInt16();
            UnkLong0x24 = br.ReadInt32();
            UnkFloat0x28 = br.ReadSingle();
            UnkShort0x2C = br.ReadInt16();
            UnkShort0x2E = br.ReadInt16();
            UnkShort0x30 = br.ReadInt16();
            int paramsCount = br.ReadInt16();
            int paramsPointer = br.ReadInt32();

            br.BaseStream.Seek(offset + paramsPointer, SeekOrigin.Begin);
            for (int i = 0; i < paramsCount; i++) MysteryResourceParams.Add(new MysteryResourceParam(br));
        }

        public List<byte> GenerateHeaderBytes()
        {
            List<byte> data = new List<byte>(USF4Utils.StringToNullPaddedBytes(Name, 0x20, out _));
            USF4Utils.AddIntAsBytes(data, MysteryResourceID, false);
            USF4Utils.AddIntAsBytes(data, UnkShort0x22, false);
            USF4Utils.AddIntAsBytes(data, UnkLong0x24, true);
            USF4Utils.AddFloatAsBytes(data, UnkFloat0x28);
            USF4Utils.AddIntAsBytes(data, UnkShort0x2C, false);
            USF4Utils.AddIntAsBytes(data, UnkShort0x2E, false);
            USF4Utils.AddIntAsBytes(data, UnkShort0x30, false);
            USF4Utils.AddIntAsBytes(data, MysteryResourceParams.Count, false);
            USF4Utils.AddIntAsBytes(data, 0, true); //MysteryParamsPointer

            return data;
        }
    }
}
