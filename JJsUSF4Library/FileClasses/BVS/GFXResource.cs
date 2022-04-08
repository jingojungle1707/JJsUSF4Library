using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class GFXResource
    {
        public string Name { get; set; }

        //This is like a tree path inside the specified ResourceType folder in SourceFile, highest-order byte is the first index
        public byte Index2 { get; set; }
        public byte Index1 { get; set; }
        public byte Index0 { get; set; }

        //Byte at 0x23 stores both of these.
        //byte && 0x03 == ResourceType
        //byte && 0x30 == SourceFile
        public SourceFile Source { get; set; }
        public ResourceType Type { get; set; }
        public int UnkLong0x24 {get; set; }
        public int UnkShort0x28 { get; set; }
        public int UnkShort0x2A { get; set; }
        //Long pointer 0x2C


        public GFXResource(BinaryReader br, int offset = 0)
        {
            Name = br.ReadZString();
            Index2 = br.ReadByte();
            Index1 = br.ReadByte();
            Index0 = br.ReadByte();

            int sourceFlag = br.ReadByte();

            Source = (SourceFile)(sourceFlag & 0x30);
            Type = (ResourceType)(sourceFlag & 0x03);

            UnkLong0x24 = br.ReadInt32();
            UnkShort0x28 = br.ReadInt16();
            UnkShort0x2A = br.ReadInt16();

            int mysteryPointer = br.ReadInt16();
        }

        public enum SourceFile
        {
            COMMON = 0x00,
            STAGE = 0x10,
            CHARACTER = 0x20,
        }
        public enum ResourceType
        {
            OBJECT = 0x00,
            PARTICLE = 0x01,
            TRACER = 0x02,
        }

    }
}
