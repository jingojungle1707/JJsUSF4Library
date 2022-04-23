using JJsUSF4Library.FileClasses.SubfileClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JJsUSF4Library.FileClasses.BVSClasses
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

        //ResourceInstance has a bunch of values/flags, a bitflag (which is usually -1/0xFFFF), and TWO params counts/pointers. Calling them type1/type2 for now
        //So far I've only seen one or the other used, but maybe in theory could use both at once?

        // Defining a full GFX script requires:
        // GFX[2] ->
        //           List<ResourceIndex> ->
        //                                  Resources ->
        //                                              ResourceInstances ->
        //                                                                  ResourceInstanceParameters

        public List<GFX> GFXs { get; set; } = new List<GFX>();
        public List<GFX2> GFX2s { get; set; } = new List<GFX2>();
        public List<GFXResource> Resources { get; set; } = new List<GFXResource>();
        public List<MysteryResource> MysteryResources { get; set; } = new List<MysteryResource>();
 
        public BVS()
        {

        }
        public BVS(BinaryReader br, string name, int offset = 0)
        {
            Name = name;
            ReadFromStream(br, offset);
        }

        public override void ReadFromStream(BinaryReader br, int offset = 0, int fileLength = 0)
        {
            br.BaseStream.Seek(offset + 0x0C, SeekOrigin.Begin);

            int gfxCount = br.ReadInt16();
            int gfx2Count = br.ReadInt16();
            int gfxResourceCount = br.ReadInt16();
            int mysteryResourceCount = br.ReadInt16();
            int gfxPointer = br.ReadInt32();
            int gfx2Pointer = br.ReadInt32();
            int gfxResourcePointer = br.ReadInt32();
            int mysteryResourcePointer = br.ReadInt32();

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

            for (int i = 0; i < gfxCount; i++)
            {
                int gfxStartOffset = offset + gfxPointer + i * 0x30;
                GFX gfx = new GFX(br, gfxResourcesByOffset, out int mysterResourcePointer, gfxStartOffset);
                if (mysterResourcePointer != 0)
                {
                    gfx.MysteryResourceData = mysteryResourcesByOffset[mysterResourcePointer + gfxStartOffset];
                }
                GFXs.Add(gfx);
            }
            for (int i = 0; i < gfx2Count; i++) GFX2s.Add(new GFX2(br, gfxResourcesByOffset, offset + gfx2Pointer + i * 0x30));
            
        }

        public override byte[] GenerateBytes()
        {
            //#BVS, endian marker, version
            List<byte> data = new List<byte>() { 0x23, 0x42, 0x56, 0x53, 0xFE, 0xFF, 0x24, 0x00, 0x01, 0x00, 0x00, 0x00 };

            USF4Utils.AddIntAsBytes(data, GFXs.Count, false);
            USF4Utils.AddIntAsBytes(data, GFX2s.Count, false);
            //0x10
            USF4Utils.AddIntAsBytes(data, Resources.Count, false);
            USF4Utils.AddIntAsBytes(data, MysteryResources.Count, false);
            int gfxPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, 0x00, true);
            int gfx2PointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, 0x00, true);
            int resourcePointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, 0x00, true);
            //0x20
            int mysteryResourcePointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, 0x00, true);

            List<(string, int)> resourceItemNameResourcePointerPositionPairs = new List<(string, int)>();

            if (GFXs.Count > 0) USF4Utils.UpdateIntAtPosition(data, gfxPointerPosition, data.Count);
            List<int> mysteryResourcePointerPositions = new List<int>();
            int resourceItemCount = 0;
            for (int i = 0; i < GFXs.Count; i++)
            {
                data.AddRange(GFXs[i].GenerateHeaderBytes());
                //Store mysteryresourcepointerposition
                mysteryResourcePointerPositions.Add(data.Count - 0x08);
                //Predict the position of the resource items and update
                int resourceItemPointer = (GFXs.Count - i) * 0x30 + resourceItemCount * 0x10;
                USF4Utils.UpdateIntAtPosition(data, data.Count - 0x04, resourceItemPointer);
                resourceItemCount += GFXs[i].ResourceItems.Count;
            }
            //Write GFX resource items
            List<List<int>> gfxResourceItemResourcePointerPositions = new List<List<int>>();
            for (int i = 0; i < GFXs.Count; i++)
            {
                gfxResourceItemResourcePointerPositions.Add(new List<int>());
                for (int j = 0; j < GFXs[i].ResourceItems.Count; j++)
                {
                    gfxResourceItemResourcePointerPositions.Last().Add(data.Count);
                    //resourceItemNameResourcePointerPositionPairs.Add(new (GFXs[i].ResourceItems[j].ResourceName, data.Count));
                    data.AddRange(GFXs[i].ResourceItems[j].GenerateBytes());
                }
            }
            if (GFX2s.Count > 0) USF4Utils.UpdateIntAtPosition(data, gfx2PointerPosition, data.Count);
            resourceItemCount = 0;
            for (int i = 0; i < GFX2s.Count; i++)
            {
                data.AddRange(GFX2s[i].GenerateHeaderBytes());
                int resourceItemPointer = (GFX2s.Count - i) * 0x30 + resourceItemCount * 0x10;
                USF4Utils.UpdateIntAtPosition(data, data.Count - 4, resourceItemPointer);
                resourceItemCount += GFX2s[i].ResourceItems.Count;
            }

            List<List<int>> gfx2ResourceItemResourcePointerPositions = new List<List<int>>();
            for (int i = 0; i < GFX2s.Count; i++)
            {
                gfx2ResourceItemResourcePointerPositions.Add(new List<int>());
                for (int j = 0; j < GFX2s[i].ResourceItems.Count; j++)
                {
                    gfx2ResourceItemResourcePointerPositions.Last().Add(data.Count);
                    //resourceItemNameResourcePointerPositionPairs.Add(new(GFX2s[i].ResourceItems[j].ResourceName, data.Count));
                    data.AddRange(GFX2s[i].ResourceItems[j].GenerateBytes());
                }
            }

            if (Resources.Count > 0) USF4Utils.UpdateIntAtPosition(data, resourcePointerPosition, data.Count);
            int resourceInstanceCount = 0;
            for (int i = 0; i < Resources.Count; i++)
            {
                //Find all resourceItems targetting this resource and update their pointers
                for (int j = 0; j < GFXs.Count; j++)
                {
                    GFX gfx = GFXs[j];
                    for (int k = 0; k < gfx.ResourceItems.Count; k++)
                    {
                        if (gfx.ResourceItems[k].Resource == Resources[i])
                        {
                            int position = gfxResourceItemResourcePointerPositions[j][k];
                            USF4Utils.UpdateIntAtPosition(data, position, data.Count - position);
                        }
                    }
                }
                //And again for GFX2s
                for (int j = 0; j < GFX2s.Count; j++)
                {
                    GFX2 gfx2 = GFX2s[j];
                    for (int k = 0; k < gfx2.ResourceItems.Count; k++)
                    {
                        if (gfx2.ResourceItems[k].Resource == Resources[i])
                        {
                            int position = gfx2ResourceItemResourcePointerPositions[j][k];
                            USF4Utils.UpdateIntAtPosition(data, position, data.Count - position);
                        }
                    }
                }
                //Write resource
                data.AddRange(Resources[i].GenerateHeaderBytes());
                int resourceInstancePointer = (Resources.Count - i) * 0x30 + resourceInstanceCount * 0x30;
                USF4Utils.UpdateIntAtPosition(data, data.Count - 4, resourceInstancePointer);
                resourceInstanceCount += Resources[i].ResourceInstances.Count;
            }
            //Write resource instances
            int paramsCurrentCount = 0;
            int resourceInstancesCurrentCount = 0;
            int resourceInstancesTotalCount = Resources.Select(x => x.ResourceInstances.Count).Sum();
            for (int i = 0; i < Resources.Count; i++)
            {
                for (int j = 0; j < Resources[i].ResourceInstances.Count; j++)
                {
                    data.AddRange(Resources[i].ResourceInstances[j].GenerateHeaderBytes());
                    int paramsType1Pointer = (resourceInstancesTotalCount - resourceInstancesCurrentCount) * 0x30 + (paramsCurrentCount * 0x20);
                    int paramsType2Pointer = paramsType1Pointer + Resources[i].ResourceInstances[j].ParamsType1.Count * 0x20;
                    USF4Utils.UpdateIntAtPosition(data, data.Count - 8, paramsType1Pointer);
                    USF4Utils.UpdateIntAtPosition(data, data.Count - 4, paramsType2Pointer);
                    resourceInstancesCurrentCount += 1;
                    paramsCurrentCount += Resources[i].ResourceInstances[j].ParamsType1.Count;
                    paramsCurrentCount += Resources[i].ResourceInstances[j].ParamsType2.Count;
                }
            }
            //Write resource params
            for (int i = 0; i < Resources.Count; i++)
            {
                for (int j = 0; j < Resources[i].ResourceInstances.Count; j++)
                {
                    for (int k = 0; k < Resources[i].ResourceInstances[j].ParamsType1.Count; k++)
                    {
                        data.AddRange(Resources[i].ResourceInstances[j].ParamsType1[k].GenerateBytes());
                    }
                    for (int k = 0; k < Resources[i].ResourceInstances[j].ParamsType2.Count; k++)
                    {
                        data.AddRange(Resources[i].ResourceInstances[j].ParamsType2[k].GenerateBytes());
                    }
                }
            }
            //Write mysteryResources & update any gfx mysteryResourcePointers as necessary
            if (MysteryResources.Count > 0) USF4Utils.UpdateIntAtPosition(data, mysteryResourcePointerPosition, data.Count);
            List<int> mysteryResourceParamsPointerPositions = new List<int>();
            for (int i = 0; i < MysteryResources.Count; i++)
            {
                MysteryResource mysteryResource = MysteryResources[i];
                //Find matching gfx and update pointers
                for (int j =0; j < GFXs.Count; j++)
                {
                    GFX gfx = GFXs[j];
                    if (gfx.MysteryResourceData != null && gfx.MysteryResourceData == mysteryResource)
                    {
                        USF4Utils.UpdateIntAtPosition(data, mysteryResourcePointerPositions[j], data.Count - (mysteryResourcePointerPositions[j] - 0x28));
                    }
                }

                //Write mysteryresource header data
                data.AddRange(mysteryResource.GenerateHeaderBytes());
                mysteryResourceParamsPointerPositions.Add(data.Count - 0x04);
            }
            for (int i = 0; i < MysteryResources.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(data, mysteryResourceParamsPointerPositions[i], data.Count - (mysteryResourceParamsPointerPositions[i] - 0x34));
                foreach (MysteryResourceParam mrp in MysteryResources[i].MysteryResourceParams)
                {
                    data.AddRange(mrp.GenerateBytes());
                }
            }

            return data.ToArray();
        }
    }
}
