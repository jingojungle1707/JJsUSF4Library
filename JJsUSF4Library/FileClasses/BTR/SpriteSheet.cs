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
        public byte SpriteSheetID { get; set; } //Maybe? Seems to increment when there's multiple sheets
        public byte TextureIndex { get; set; }
        public byte UnkByte0x02 { get; set; }   //0x22, 0x21
        public byte UnkByte0x03 { get; set; }   //both always 0x22?
        public int UnkLong0x04 { get; set; }
        public int UnkShort0x08 { get; set; }
        public int BitFlag0x0A { get; set; } // SHORT 0x00 for still image, 0x02 for animated
        public ISpriteInstance SpriteInstance { get; set; }

        public SpriteSheet()
        {

        }
        public SpriteSheet(BinaryReader br, int offset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            SpriteSheetID = br.ReadByte();
            TextureIndex = br.ReadByte();
            UnkByte0x02 = br.ReadByte();
            UnkByte0x03 = br.ReadByte();
            UnkLong0x04 = br.ReadInt32();
            UnkShort0x08 = br.ReadInt16();
            BitFlag0x0A = br.ReadInt16();

            //TODO WORK OUT BITFLAG == 1! It appears in PSN_KISS.et.btr.
            //It appears to be 4 floats long but its 0.005 / 0.005 / 0.00 / 0.00 so it obviously can't be X/Y/W/H!
            if (BitFlag0x0A == 0 || BitFlag0x0A == 1)
            {
                //TODO tidy this up
                SpriteInstance = new StaticSpriteInstance()
                {
                    Frames = new List<ISpriteFrame>()
                    {
                        new StaticSpriteFrame()
                        {
                            XOffset = br.ReadSingle(),
                            YOffset = br.ReadSingle(),
                            Width = br.ReadSingle(),
                            Height = br.ReadSingle(),
                        }
                    }
                };
            }
            else if (BitFlag0x0A == 2)
            {
                SpriteInstance = new AnimatedSpriteInstance(br, offset + 0x0C);
            }
            else
            {
                throw new Exception("UNKNOWN BITFLAG IN SPRITESHEET");
            }
        }

        public List<byte> GenerateHeaderBytes(out int spriteInstancePointer)
        {
            List<byte> data = new List<byte>();

            data.Add(SpriteSheetID);
            data.Add(TextureIndex);
            data.Add(UnkByte0x02);
            data.Add(UnkByte0x03);

            USF4Utils.AddIntAsBytes(data, UnkLong0x04, true);
            USF4Utils.AddIntAsBytes(data, UnkShort0x08, false);
            USF4Utils.AddIntAsBytes(data, BitFlag0x0A, false);

            return data;
        }
    }
}
