using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class Skeleton
    {
        public int MysteryShort { get; set; } //I've seen 0x00, 0x01, 0x02, 0x03. EMA and EMO don't always match.
        public float MysteryFloat1 { get; set; } //These two "floats" might be a checksum to ensure the EMO and EMA skeletons match?
        public float MysteryFloat2 { get; set; }

        public List<Node> Nodes { get; set; }

        public List<byte[]> FFList { get; set; }
        public List<IKNode> IKNodes { get; set; }
        public List<IKDataBlock> IKDataBlocks { get; set; }

        public List<string> NodeNames
        {
            get { return Nodes.Select(o => o.Name).ToList(); }
        }

        public List<string> IKNodeNames
        {
            get { return IKNodes.Select(o => o.Name).ToList(); }
        }

        public Node NodeByName(string nodeName)
        {
            return Nodes.Where(x => x.Name == nodeName).FirstOrDefault();
        }

        public SkeletonType Type;

        public enum SkeletonType
        {
            EMO,
            EMA
        }

        public Skeleton()
        {

        }

        public Skeleton(BinaryReader br, SkeletonType type, int offset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            Type = type;

            #region Initialise Lists
            Nodes = new List<Node>();
            FFList = new List<byte[]>();
            IKNodes = new List<IKNode>();
            IKDataBlocks = new List<IKDataBlock>();

            List<string> nodeNames = new List<string>();
            List<string> iKNodeNames = new List<string>();
            #endregion

            #region Read Header
            int nodeCount = br.ReadInt16();
            int iKObjectCount = br.ReadInt16();
            int iKDataCount = br.ReadInt32();
            int nodeListPointer = br.ReadInt32();
            int nameIndexPointer = br.ReadInt32();
            //0x10
            int iKBoneListPointer = br.ReadInt32();
            int iKObjectNameIndexPointer = br.ReadInt32();
            int registerPointer = br.ReadInt32();
            int secondaryMatrixPointer = br.ReadInt32();
            //0x20
            int iKDataPointer = br.ReadInt32();
            //Skip ahead to 0x36 from start of skeleton
            br.BaseStream.Seek(offset + 0x36, SeekOrigin.Begin);
            //0x36
            MysteryShort = br.ReadInt16();      //1 REALLY no idea what these are
            MysteryFloat1 = br.ReadSingle();    //2		Are these some kind of checksum
            MysteryFloat2 = br.ReadSingle();    //3

            #endregion

            #region Read Nodes
            //Read names - jump to start of name index, read the first pointer
            //Then we can just jump to the first name, and from there keep reading Z-strings without worrying about later pointers
            br.BaseStream.Seek(offset + nameIndexPointer, SeekOrigin.Begin);
            br.BaseStream.Seek(offset + br.ReadInt32(), SeekOrigin.Begin);
            for (int i = 0; i < nodeCount; i++)
            {
                nodeNames.Add(USF4Utils.ReadZString(br));
            }
            //Read nodes
            for (int i = 0; i < nodeCount; i++)
            {
                Nodes.Add(new Node(br, type, nodeNames[i], nodeNames, offset + nodeListPointer + i * 0x50, offset + secondaryMatrixPointer + i * 0x40));
            }
            //Read animation mirroring register
            br.BaseStream.Seek(offset + registerPointer, SeekOrigin.Begin);
            for (int i = 0; i < nodeCount; i++) FFList.Add(br.ReadBytes(0x08));

            #endregion

            #region Read IK Data
            if (iKDataCount != 0)
            {
                //There's no pointers to individual blocks, but there's also no gaps between either
                //So we just set it at the start of the region and let it run without further seeks
                br.BaseStream.Seek(offset + iKDataPointer, SeekOrigin.Begin);
                for (int i = 0; i < iKDataCount; i++) IKDataBlocks.Add(new IKDataBlock(br));
            }
            #endregion

            #region Read IK Chains & Names
            if (iKObjectCount != 0)
            {
                //Same as node names, just read the first pointer and then trust the z-strings
                br.BaseStream.Seek(offset + iKObjectNameIndexPointer, SeekOrigin.Begin);
                br.BaseStream.Seek(offset + br.ReadInt32(), SeekOrigin.Begin);
                for (int i = 0; i < iKObjectCount; i++) iKNodeNames.Add(USF4Utils.ReadZString(br));

                for (int i = 0; i < iKObjectCount; i++)
                {
                    br.BaseStream.Seek(offset + iKBoneListPointer + i * 0x08, SeekOrigin.Begin);
                    IKNodes.Add(new IKNode(br));
                    IKNodes.Last().Name = iKNodeNames[i];
                }
            }
            #endregion
        }

        private List<byte> GenerateHeader()
        {
            List<byte> data = new List<byte>();

            USF4Utils.AddIntAsBytes(data, Nodes.Count, false);
            USF4Utils.AddIntAsBytes(data, IKNodes.Count, false);
            USF4Utils.AddIntAsBytes(data, IKDataBlocks.Count, true);
            USF4Utils.AddIntAsBytes(data, 0, true);
            USF4Utils.AddIntAsBytes(data, 0, true);
            //0x10
            USF4Utils.AddIntAsBytes(data, 0, true);
            USF4Utils.AddIntAsBytes(data, 0, true);
            USF4Utils.AddIntAsBytes(data, 0, true);
            USF4Utils.AddIntAsBytes(data, 0, true);
            //0x20
            USF4Utils.AddIntAsBytes(data, 0, true);
            USF4Utils.AddZeroToLineEnd(data);
            //0x30
            USF4Utils.AddIntAsBytes(data, 0x00, true); //Padding
            USF4Utils.AddIntAsBytes(data, 0x00, false); //Padding
            USF4Utils.AddIntAsBytes(data, MysteryShort, false);
            USF4Utils.AddFloatAsBytes(data, MysteryFloat1);
            USF4Utils.AddFloatAsBytes(data, MysteryFloat2);
            USF4Utils.AddZeroToLineEnd(data);

            return data;
        }

        public byte[] GenerateBytes()
        {
            List<byte> data = GenerateHeader();

            //All the pointer positions are fixed within the header, so no need to calculate them at runtime
            int nodeListPointerPosition = 0x08;
            int nameIndexPointerPosition = 0x0C;
            //0x10
            int iKBoneListPointerPosition = 0x10;
            int iKOBjectNameIndexPointerPosition = 0x14;
            int registerPointerPosition = 0x18;
            int secondaryMatrixPointerPosition = 0x1C;
            int iKDataPointerPosition = 0x20;

            //0x40 - Node relationships and main matrices
            USF4Utils.UpdateIntAtPosition(data, nodeListPointerPosition, data.Count);
            //Fetch node names and store so we don't have to repeatedly access the property
            List<string> nodeNames = NodeNames;
            for (int i = 0; i < Nodes.Count; i++)
            {
                data.AddRange(Nodes[i].GenerateMainBytes(Type, nodeNames));
            }
            //FF Register
            USF4Utils.UpdateIntAtPosition(data, registerPointerPosition, data.Count);
            for (int i = 0; i < FFList.Count; i++)
            {
                data.AddRange(FFList[i]);
            }
            //Node Name Index
            USF4Utils.UpdateIntAtPosition(data, nameIndexPointerPosition, data.Count);
            List<int> nodeNameIndexPointerPositions = new List<int>();
            for (int i = 0; i < nodeNames.Count; i++)
            {
                nodeNameIndexPointerPositions.Add(data.Count);
                USF4Utils.AddIntAsBytes(data, -1, true);
            }
            for (int i = 0; i < nodeNames.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(data, nodeNameIndexPointerPositions[i], data.Count);
                data.AddRange(Encoding.ASCII.GetBytes(nodeNames[i]));
                data.Add(0x00);
            }
            USF4Utils.AddZeroToLineEnd(data);

            //Inverse skin bind pose matrices
            if (Type == SkeletonType.EMO)
            {
                USF4Utils.UpdateIntAtPosition(data, secondaryMatrixPointerPosition, data.Count);
                for (int i = 0; i < Nodes.Count; i++)
                {
                    USF4Utils.AddMatrix4x4AsBytes(data, Nodes[i].InverseSkinBindPoseMatrix);
                }
            }

            if (IKDataBlocks.Count != 0)
            {
                //IKData Blocks
                USF4Utils.UpdateIntAtPosition(data, iKDataPointerPosition, data.Count);
                for (int i = 0; i < IKDataBlocks.Count; i++)
                {
                    USF4Utils.AddIntAsBytes(data, IKDataBlocks[i].Method, false);
                    USF4Utils.AddIntAsBytes(data, IKDataBlocks[i].Length, false);
                    data.Add((byte)IKDataBlocks[i].Flag0x00);
                    data.Add((byte)IKDataBlocks[i].Flag0x01);
                    for (int j = 0; j < IKDataBlocks[i].BoneIDs.Count; j++)
                    {
                        USF4Utils.AddIntAsBytes(data, IKDataBlocks[i].BoneIDs[j], false);
                    }
                    for (int j = 0; j < IKDataBlocks[i].IKFloats.Count; j++)
                    {
                        USF4Utils.AddFloatAsBytes(data, IKDataBlocks[i].IKFloats[j]);
                    }
                }
            }

            if (IKNodes.Count > 0)
            {


                //IK Bone Lists Index
                USF4Utils.UpdateIntAtPosition(data, iKBoneListPointerPosition, data.Count);
                List<int> iKNodeBoneListPointerPositions = new List<int>();

                for (int i = 0; i < IKNodes.Count; i++)
                {
                    USF4Utils.AddIntAsBytes(data, IKNodes[i].BoneCount, true);
                    iKNodeBoneListPointerPositions.Add(data.Count);
                    USF4Utils.AddIntAsBytes(data, IKNodes[i].BoneListPointer, true);
                }
                //IK Bone Lists
                for (int i = 0; i < IKNodes.Count; i++)
                {
                    USF4Utils.UpdateIntAtPosition(data, iKNodeBoneListPointerPositions[i], data.Count - iKNodeBoneListPointerPositions[i]);
                    for (int j = 0; j < IKNodes[i].BoneList.Count; j++)
                    {
                        USF4Utils.AddIntAsBytes(data, IKNodes[i].BoneList[j], false);
                    }
                }
                //IKNameIndex
                USF4Utils.UpdateIntAtPosition(data, iKOBjectNameIndexPointerPosition, data.Count);
                List<int> iKObjectNamePointers = new List<int>();
                for (int i = 0; i < IKNodes.Count; i++)
                {
                    iKObjectNamePointers.Add(data.Count);
                    USF4Utils.AddIntAsBytes(data, -1, true);
                }
                //IK Names
                for (int i = 0; i < IKNodes.Count; i++)
                {
                    USF4Utils.UpdateIntAtPosition(data, iKObjectNamePointers[i], data.Count);
                    data.AddRange(Encoding.ASCII.GetBytes(IKNodeNames[i]));
                    data.Add(0x00);
                }
            }
            data.Add(0x00);

            return data.ToArray();
        }
    }
}