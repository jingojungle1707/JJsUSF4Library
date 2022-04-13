using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class SpriteSheet
    {
        public byte UnkByte0x00 { get; set; }
        public byte TextureIndex { get; set; }
        public byte UnkByte0x02 { get; set; }   //both always 0x22?
        public byte UnkByte0x03 { get; set; }   //both always 0x22?
        public int UnkLong0x04 { get; set; }
        public int UnkShort0x08 { get; set; }
        public int BitFlag0x0A { get; set; } // SHORT 0x00 for still image, 0x02 for animated
        public List<ISpriteFrame> Frames { get; set; } = new List<ISpriteFrame>();
        public SpriteSheet()
        {

        }
        public SpriteSheet(BinaryReader br, int offset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            UnkByte0x00 = br.ReadByte();
            TextureIndex = br.ReadByte();
            UnkByte0x02 = br.ReadByte();
            UnkByte0x03 = br.ReadByte();
            UnkLong0x04 = br.ReadInt32();
            UnkShort0x08 = br.ReadInt16();
            BitFlag0x0A = br.ReadInt16();

            if (BitFlag0x0A == 0)
            {
                Frames.Add(new StaticSpriteFrame() 
                { 
                    XOffset = br.ReadSingle(), 
                    YOffset = br.ReadSingle(),
                    Width = br.ReadSingle(),
                    Height = br.ReadSingle(),
                });
            }
            else if (BitFlag0x0A == 2)
            {
                //Padding bytes?
                br.ReadInt32();
                br.ReadInt32();
                br.ReadInt16();

                int frameCount = br.ReadInt16();
                int frameDataPointer = br.ReadInt32(); //Counts from immediately after BitFlag

                for (int i = 0; i < frameCount; i++)
                {
                    Frames.Add(new AnimatedSpriteFrame()
                    {
                        Frame = br.ReadSingle(),
                        XOffset = br.ReadSingle(),
                        YOffset = br.ReadSingle(),
                        Width = br.ReadSingle(),
                        Height = br.ReadSingle(),
                    });
                }
            }
        }
    }
}
