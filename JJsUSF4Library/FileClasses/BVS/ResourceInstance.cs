using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class ResourceInstance
    {
        public List<ResourceInstanceParam> Params { get; set; } = new List<ResourceInstanceParam>();
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
        public int UnkLong0x0C { get; set; }
        public int UnkLong0x10 { get; set; }
        public int UnkLong0x14 { get; set; }
        public int UnkLong0x18 { get; set; }
        public int UnkLong0x1C { get; set; }
        public int UnkLong0x20 { get; set; }

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
            UnkLong0x0C = br.ReadInt32();
            UnkLong0x10 = br.ReadInt32();
            UnkLong0x14 = br.ReadInt32();
            UnkLong0x18 = br.ReadInt32();
            UnkLong0x1C = br.ReadInt32();
            UnkLong0x20 = br.ReadInt32();
            int paramsCount = br.ReadInt32();
            int paramsStartOffset = br.ReadInt32();
            int paramsEndOffset = br.ReadInt32();

            if (paramsCount > 0)
            {
                for (int i = 0; i < paramsCount; i++)
                {
                    Params.Add(new ResourceInstanceParam(br, offset + paramsStartOffset));
                }
            }
        }

    }
}
