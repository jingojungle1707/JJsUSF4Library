using JJsUSF4Library.FileClasses.SubfileClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JJsUSF4Library.FileClasses
{
    public class BVS : USF4File
    {
        //TODO there must be indexes into the ptex.emz and ttex.emz files somewhere in here?

        // Each GFX/GFX2 has a count&pointer into the "resource item index". Each resource index entry contains a bitflag (maybe?), a float (always 1.0?) and a resource pointer.
        // Res pointer           Bitflag?    Float      Float2/unused?
        // 10 08 00 00 || 00 00 || 06 00 || 00 00 80 3F || 00 00 00 00
        // For GFX2 the bitflag is always -1 (0xFFFF) but otherwise identical

        //Each resource points to an .emb in a .vfx.emz file, and an index path to the specific .eo/.ep/.et
        //It has a count&pointer to YET ANOTHER similar looking datablock, which we're going to call a ResourceInstance for now?

        //ResourceInstance has a bunch of values/flags, a bitflag (which is usually -1/0xFFFF), a parameters count (?), and a START and END
        //pointer into the parameters table. Each parameter is length 0x20, and it's all floats (?). If paramscount == 0, both start and end
        //point to the same position in the table, wherever the last set of params we read ended.

        // Defining a full GFX script requires:
        // GFX[2] ->
        //           List<ResourceIndex> ->
        //                                  Resource ->
        //                                              ResourceInstance ->
        //                                                                  ResourceInstanceParameters

        public List<GFX> GFXs { get; set; } = new List<GFX>();
        public List<GFX2> GFX2s { get; set; } = new List<GFX2>();
        //Do we actually want to store Resources at this level? Or just store them in GFX and reconstruct the resource index when we GenerateBytes()?
        public List<GFXResource> Resources { get; set; } = new List<GFXResource>();

        //public List<???> ??? dunno if these exist, there's space in the header for them to have a short count and a pointer but always 0 in files I've checked

        public BVS(BinaryReader br, int offset = 0)
        {
            br.BaseStream.Seek(offset + 0x0C, SeekOrigin.Begin);

            int gfxCount = br.ReadInt16();
            int gfx2Count = br.ReadInt16();
            int gfxResourceCount = br.ReadInt16();
            int mysteryEmptyCount = br.ReadInt16();
            int gfxPointer = br.ReadInt32();
            int gfx2Pointer = br.ReadInt32();
            int gfxResourcePointer = br.ReadInt32();
            int mysteryEmptyPointer = br.ReadInt32();

            //Reading resources first
            for (int i = 0; i < gfxResourceCount; i++)
            {
                Resources.Add(new GFXResource(br, offset + gfxResourcePointer + i * 0x30));
            }

            for (int i = 0; i < gfxCount; i++) GFXs.Add(new GFX(br, offset + gfxPointer + i * 0x30));
            for (int i = 0; i < gfx2Count; i++) GFX2s.Add(new GFX2(br, offset + gfx2Pointer + i * 0x30));
            //for (int i = 0; i < mysteryEmptyCount; i++) MysteryObjects.Add(new MysteryObject(br, mysteryEmptyPointer + i * 0x30));
        }

        public override byte[] GenerateBytes()
        {
            //#BVS, endian marker, version
            List<byte> data = new List<byte>() { 0x23, 0x42, 0x56, 0x53, 0xFE, 0xFF, 0x24, 0x00, 0x01, 0x00, 0x00, 0x00 };

            USF4Utils.AddIntAsBytes(data, GFXs.Count, false);
            USF4Utils.AddIntAsBytes(data, GFX2s.Count, false);
            //0x10
            USF4Utils.AddIntAsBytes(data, Resources.Count, false);
            USF4Utils.AddIntAsBytes(data, 0x00, false);
            int gfxPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, 0x00, true);
            int gfx2PointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, 0x00, true);
            int resourcePointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, 0x00, true);
            //0x20
            USF4Utils.AddIntAsBytes(data, 0x00, true);

            USF4Utils.UpdateIntAtPosition(data, gfxPointerPosition, data.Count);
            
            return data.ToArray();
        }

        private List<byte> GenerateGFXBytes(List<GFX> gFXs)
        {
            List<byte> data = new List<byte>();

            for (int i = 0; i < GFXs.Count; i++)
            {
                data.AddRange(GFXs[i].GenerateHeaderBytes());
            }

            return data;
        }
    }
}
