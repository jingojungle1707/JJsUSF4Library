using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BVSClasses
{
    public class SFxTBVS : BVS
    {
        public SFxTBVS()
        {

        }
        public SFxTBVS(BinaryReader br, string name, int offset = 0)
        {
            Name = name;
            ReadFromStream(br, offset);
        }
        public override void ReadFromStream(BinaryReader br, int offset = 0, int fileLength = 0)
        {
            br.BaseStream.Seek(offset + 0x0C, SeekOrigin.Begin);

            int gfxCount = br.ReadInt16();
            int doubleMysteryResourceCount = br.ReadInt16(); //Always unused?
            //0x10
            int gfxResourceCount = br.ReadInt16();
            int mysteryResourceCount = br.ReadInt16();
            int sfxtResource0Count = br.ReadInt16();
            int sfxtResource1Count = br.ReadInt16();
            int sfxtResource2Count = br.ReadInt16();
            int sfxtResource3Count = br.ReadInt16();
            int sfxtResource4Count = br.ReadInt16();
            int sfxtResource5Count = br.ReadInt16();

            //0x20
            //gfx pointer is to an INDEX, the rest are directly to whatever they're pointing at
            int gfxIndexPointer = br.ReadInt32();
            int doubleMysteryResourcePointer = br.ReadInt32(); //Always unused?
            int gfxResourcePointer = br.ReadInt32();
            int mysteryResourcePointer = br.ReadInt32();
            //0x30
            int sfxtResource0Pointer = br.ReadInt32();
            int sfxtResource1Pointer = br.ReadInt32();
            int sfxtResource2Pointer = br.ReadInt32();
            int sfxtResource3Pointer = br.ReadInt32();
            //0x40
            int sfxtResource4Pointer = br.ReadInt32();
            int sfxtResource5Pointer = br.ReadInt32();

            //Read gfxResources first so we can pass by ref
            Dictionary<int, GFXResource> gfxResourcesByOffset = new Dictionary<int, GFXResource>();
            Dictionary<int, MysteryResource> mysteryResourcesByOffset = new Dictionary<int, MysteryResource>();
            //These GFXResources/MysteryResources appear at end of file, but we read them first so we can pass them to the GFX by reference
            for (int i = 0; i < gfxResourceCount; i++)
            {
                GFXResource resource = new GFXResource(br, offset + gfxResourcePointer + i * 0x30);
                gfxResourcesByOffset.Add(gfxResourcePointer + i * 0x30, resource);
                Resources.Add(resource);
            }
            for (int i = 0; i < mysteryResourceCount; i++)
            {
                MysteryResource mysteryResource = new MysteryResource(br, mysteryResourcePointer + i * 0x38);
                mysteryResourcesByOffset.Add(mysteryResourcePointer + i * 0x38, mysteryResource);
                MysteryResources.Add(mysteryResource);
            }

            br.BaseStream.Seek(offset + gfxIndexPointer, SeekOrigin.Begin);
            List<int> gfxPointers = new List<int>();
            for (int i = 0; i < gfxCount; i++) gfxPointers.Add(br.ReadInt32());

            for (int i = 0; i < gfxCount; i++)
            {
                br.BaseStream.Seek(offset + gfxPointers[i], SeekOrigin.Begin);
                string name = Encoding.ASCII.GetString(br.ReadBytes(0x20)).Split('\0')[0];
                br.ReadBytes(0x16);
                int resourceItemsCount = br.ReadInt16();
                int resourceItemsPointer = br.ReadInt32();

                List<ResourceItem> gfXResourceItems = new List<ResourceItem>();
                for (int j = 0; j < resourceItemsCount; j++)
                {
                    gfXResourceItems.Add(new ResourceItem(br, gfxResourcesByOffset, offset + gfxPointers[i] + resourceItemsPointer + i * 0x10));
                }

                GFXs.Add(new GFX()
                {
                    Name = name,
                    ResourceItems = gfXResourceItems,
                });
            }
        }
    }
}
