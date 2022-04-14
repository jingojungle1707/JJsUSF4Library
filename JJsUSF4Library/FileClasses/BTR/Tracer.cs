using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class Tracer
    {
        public int UnkLong0x00 { get; set; }
        public int BoneReference { get; set; }
        public int BoneReference2 { get; set; }
        public int UnkLong0x08 { get; set; }
        public int UnkLong0x0C { get; set; }
        //Floats x 0x24, total length 0x90
        public List<float> FloatIndex0x24 { get; set; } = new List<float>();
        public int UnkShort0xA0 { get; set; }
        public int UnkShort0xA2 { get; set; }
        //0xA4 SpriteSheet pointer (long)
        public float UnkFloat0xA8 { get; set; }
        public float UnkFloat0xAC { get; set; }
        public float UnkFloat0xB0 { get; set; }
        public float UnkFloat0xB4 { get; set; }
        public float UnkFloat0xB8 { get; set; }
        public float UnkFloat0xBC { get; set; }

        public Tracer()
        {

        }
        public Tracer(BinaryReader br, int offset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            UnkLong0x00 = br.ReadInt32();
            BoneReference = br.ReadInt16();
            BoneReference2 = br.ReadInt16();
            UnkLong0x08 = br.ReadInt32();
            UnkLong0x0C = br.ReadInt32();

            for (int i = 0; i < 0x24; i++) FloatIndex0x24.Add(br.ReadSingle());

            UnkShort0xA0 = br.ReadInt16();
            UnkShort0xA2 = br.ReadInt16();

            int spriteSheetPointer = br.ReadInt32();

            UnkFloat0xA8 = br.ReadSingle();
            UnkFloat0xAC = br.ReadSingle();
            UnkFloat0xB0 = br.ReadSingle();
            UnkFloat0xB4 = br.ReadSingle();
            UnkFloat0xB8 = br.ReadSingle();
            UnkFloat0xBC = br.ReadSingle();
        }


    }
}
