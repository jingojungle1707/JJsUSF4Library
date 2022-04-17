using JJsUSF4Library.FileClasses.SubfileClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses
{
    public class BTR : USF4File
    {
        public List<Tracer> Tracers { get; set; } = new List<Tracer>();
        public List<SpriteSheet> SpriteSheets { get; set; } = new List<SpriteSheet>();

        //0x0C Tracer Count (long)
        //0x10 Tracer Pointer (long)
        //0x14 Sprite sheet count (long)
        //0x18 Sprite sheet pointer (long)
        //0x1C (Optional?) BTR name, length unknown but can't be more than 0x30 or it would collide with Tracer list
        public BTR()
        {

        }
        public BTR(BinaryReader br, string name, int offset = 0)
        {
            Name = name;
            ReadFromStream(br);
        }

        public override void ReadFromStream(BinaryReader br, int offset = 0, int fileLength = 0)
        {
            br.BaseStream.Seek(offset + 0x0C, SeekOrigin.Begin);

            int tracerCount = br.ReadInt32();
            //0x10
            int tracerPointer = br.ReadInt32();
            int spriteSheetCount = br.ReadInt32();
            int spriteSheetPointer = br.ReadInt32();

            if (spriteSheetCount > 2)
            {
                //throw new Exception($"MULTIPLE SPRITE SHEETS IN .btr FILE \"{Name}\"");
            }

            for (int i = 0; i < tracerCount; i++)
            {
                Tracers.Add(new Tracer(br, offset + tracerPointer + i * 0xC0));
            }
            for (int i = 0; i < spriteSheetCount; i++)
            {
                SpriteSheets.Add(new SpriteSheet(br, offset + spriteSheetPointer + i * 0x1C));
            }
        }

        public void UpdateTextureIndices(int valueToAdd)
        {
            foreach (SpriteSheet item in SpriteSheets)
            {
                item.TextureIndex += (byte)valueToAdd;
            }
        }
    }
}
