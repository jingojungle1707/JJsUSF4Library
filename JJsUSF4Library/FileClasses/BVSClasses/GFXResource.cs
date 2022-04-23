using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BVSClasses
{
    public class GFXResource
    {
        public List<ResourceInstance> ResourceInstances { get; set; } = new List<ResourceInstance>();
        public string Name { get; set; }

        //This is like a tree path inside the specified ResourceType folder in SourceFile, highest-order byte is the first index
        public byte Index2 { get; set; }
        public byte Index1 { get; set; }
        public byte Index0 { get; set; }

        //Byte at 0x23 stores both of sourcefile and resourcetype.
        //byte && 0x03 == ResourceType
        //byte && 0x30 == SourceFile
        public SourceFile Source { get; set; }
        public ResourceType Type { get; set; }
        public int UnkLong0x24 {get; set; }
        public int UnkShort0x28 { get; set; }
        //Short 0x2A resourceInstanceCount
        //Long pointer 0x2C
        public GFXResource()
        {

        }
        public GFXResource(BinaryReader br, int offset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            Name = Encoding.ASCII.GetString(br.ReadBytes(0x20)).Split('\0')[0];
            Index2 = br.ReadByte();
            Index1 = br.ReadByte();
            Index0 = br.ReadByte();

            int sourceFlag = br.ReadByte();

            Source = (SourceFile)(sourceFlag & 0x30);
            Type = (ResourceType)(sourceFlag & 0x03);

            UnkLong0x24 = br.ReadInt32();
            UnkShort0x28 = br.ReadInt16();
            int resourceInstanceCount = br.ReadInt16();
            int resourceInstancePointer = br.ReadInt32();

            for (int i = 0; i < resourceInstanceCount; i++)
            {
                ResourceInstances.Add(new ResourceInstance(br, offset + resourceInstancePointer + i * 0x30));
            }
        }

        public List<byte> GenerateHeaderBytes()
        {
            List<byte> data = new List<byte>();

            data.AddRange(USF4Utils.StringToNullPaddedBytes(Name, 0x20, out _));
            data.Add(Index2);
            data.Add(Index1);
            data.Add(Index0);
            data.Add((byte)((int)Source | (int)Type));
            USF4Utils.AddIntAsBytes(data, UnkLong0x24, true);
            USF4Utils.AddIntAsBytes(data, UnkShort0x28, false);
            USF4Utils.AddIntAsBytes(data, ResourceInstances.Count, false);
            USF4Utils.AddIntAsBytes(data, 0, true); //resourceinstance pointer, to be updated later

            return data;
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
