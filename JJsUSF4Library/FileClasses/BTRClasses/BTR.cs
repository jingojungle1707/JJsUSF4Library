﻿using JJsUSF4Library.FileClasses.SubfileClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BTRClasses
{
    public class BTR : USF4File
    {
        public List<Tracer> Tracers { get; set; } = new List<Tracer>();
        public List<SpriteSheet> SpriteSheets { get; set; } = new List<SpriteSheet>();

        public string InternalName { get; set; }

        public int UnkLong0x08 { get; set; }

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
            ReadFromStream(br, offset);
        }

        public override void ReadFromStream(BinaryReader br, int offset = 0, int fileLength = 0)
        {
            Dictionary<int, SpriteSheet> spriteSheetsByOffset = new Dictionary<int, SpriteSheet>();

            br.BaseStream.Seek(offset + 0x08, SeekOrigin.Begin);

            UnkLong0x08 = br.ReadInt32();
            int tracerCount = br.ReadInt32();
            //0x10
            int tracerPointer = br.ReadInt32();
            int spriteSheetCount = br.ReadInt32();
            int spriteSheetPointer = br.ReadInt32();

            InternalName = Encoding.ASCII.GetString(br.ReadBytes(0x30));

            //Sprite sheets appear after tracers, but we read them first so we can pass a reference to the correct sprite sheet to the tracers
            for (int i = 0; i < spriteSheetCount; i++)
            {
                SpriteSheet spriteSheet = new SpriteSheet(br, offset + spriteSheetPointer + i * 0x1C);
                spriteSheetsByOffset.Add(spriteSheetPointer + i * 0x1C, spriteSheet);

                SpriteSheets.Add(spriteSheet);
            }

            for (int i = 0; i < tracerCount; i++)
            {
                int tracerStartOffset = tracerPointer + i * 0xC0;
                Tracer tracer = new Tracer(br, out int tracerSpriteSheetPointer, tracerStartOffset + offset);
                tracer.Sprites = spriteSheetsByOffset[tracerSpriteSheetPointer + tracerStartOffset];

                Tracers.Add(tracer);
            }
        }

        public void UpdateTextureIndices(int valueToAdd)
        {
            foreach (SpriteSheet item in SpriteSheets)
            {
                item.TextureIndex += (byte)valueToAdd;
            }
        }

        public override byte[] GenerateBytes()
        {
            List<byte> data = new List<byte>() { 0x23, 0x42, 0x54, 0x52, 0xFE, 0xFF, 0x4C, 0x00};

            USF4Utils.AddIntAsBytes(data, UnkLong0x08, true);
            USF4Utils.AddIntAsBytes(data, Tracers.Count, true);
            int tracersPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, 0, true);
            USF4Utils.AddIntAsBytes(data, SpriteSheets.Count, true);
            int spriteSheetPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, 0, true);

            data.AddRange(Encoding.ASCII.GetBytes(InternalName));
            //Pad out
            USF4Utils.AddPaddingZeros(data, 0x4C, data.Count);


            //We need to store the tracer start position, AND the sprite pointer position, because the pointer is relative to the start offset
            List<int> tracerStartPositions = new List<int>();
            List<int> tracerSpritePointerPositions = new List<int>();

            USF4Utils.UpdateIntAtPosition(data, tracersPointerPosition, data.Count);
            foreach (Tracer tracer in Tracers)
            {
                tracerStartPositions.Add(data.Count);
                int tracerStartOffset = data.Count;
                data.AddRange(tracer.GenerateBytes(out int tracerSpritePointerPosition));
                tracerSpritePointerPositions.Add(tracerStartOffset + tracerSpritePointerPosition);
            }

            List<int> frameDataPointerPositions = new List<int>();
            USF4Utils.UpdateIntAtPosition(data, spriteSheetPointerPosition, data.Count);
            foreach (SpriteSheet spriteSheet in SpriteSheets)
            {
                //Update tracerSpriteSheet pointers
                for (int i = 0; i < Tracers.Count; i++)
                {
                    if (Tracers[i].Sprites == spriteSheet)
                    {
                        USF4Utils.UpdateIntAtPosition(data, tracerSpritePointerPositions[i], data.Count - tracerStartPositions[i]);
                    }
                }

                int headerStartOffset = data.Count;
                data.AddRange(spriteSheet.GenerateHeaderBytes(out int frameDataPointerPosition));
                if (frameDataPointerPosition != -1) frameDataPointerPositions.Add(frameDataPointerPosition + headerStartOffset);
                else frameDataPointerPositions.Add(-1);
            }
            for (int i = 0; i < SpriteSheets.Count; i++)
            {
                if (frameDataPointerPositions[i] == -1) continue;

                SpriteSheet spriteSheet = SpriteSheets[i];

                USF4Utils.UpdateIntAtPosition(data, frameDataPointerPositions[i], data.Count - (frameDataPointerPositions[i] - 0x0C));

                data.AddRange(spriteSheet.GenerateFrameBytes());
            }

            return data.ToArray();
        }
    }
}
