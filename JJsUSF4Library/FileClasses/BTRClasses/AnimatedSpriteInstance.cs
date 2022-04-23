using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BTRClasses
{
    public class AnimatedSpriteInstance : ISpriteInstance
    {
        public List<ISpriteFrame> Frames { get; set; } = new List<ISpriteFrame>();
        public float Duration { get; set; } //0x00
        public int UnkLong0x04 { get; set; }
        public int UnkShort0x08 { get; set; }
        //SHORT FramesCount 0x0A
        //LONG FramesPointer 0x0C
        public AnimatedSpriteInstance()
        {

        }

        public AnimatedSpriteInstance(BinaryReader br, int offset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            Duration = br.ReadSingle();
            UnkLong0x04 = br.ReadInt32();
            UnkShort0x08 = br.ReadInt16();

            int framesCount = br.ReadInt16();
            int framesPointer = br.ReadInt32();

            br.BaseStream.Seek(offset + framesPointer, SeekOrigin.Begin);
            for (int i = 0; i < framesCount; i++)
            {
                Frames.Add(new AnimatedSpriteFrame(br));
            }
        }

        public List<byte> GenerateHeaderBytes()
        {
            List<byte> data = new List<byte>();

            USF4Utils.AddFloatAsBytes(data, Duration);
            USF4Utils.AddIntAsBytes(data, UnkLong0x04, true);
            USF4Utils.AddIntAsBytes(data, UnkShort0x08, false);
            USF4Utils.AddIntAsBytes(data, Frames.Count, false);
            USF4Utils.AddIntAsBytes(data, 0, true); //frames pointer

            return data;
        }
    }
}
