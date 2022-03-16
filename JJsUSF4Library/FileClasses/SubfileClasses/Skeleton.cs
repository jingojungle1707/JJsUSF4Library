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
        public int MysteryShort; //I've seen 0x00, 0x01, 0x02, 0x03. EMA and EMO don't always match.
        public float MysteryFloat1; //These two "floats" might be a checksum to ensure the EMO and EMA skeletons match?
        public float MysteryFloat2;

        public List<Node> Nodes;
        public List<byte[]> FFList;
        public List<IKNode> IKNodes;
        public List<IKDataBlock> IKDataBlocks;

        public List<string> NodeNames
        {
            get { return Nodes.Select(o => o.Name).ToList(); }
        }

        public List<string> IKNodeNames
        {
            get { return IKNodes.Select(o => o.Name).ToList(); }
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
            MysteryFloat1 = br.ReadSingle();    //2		Are these some kind of checksum to make sure EMA and EMO skels match?
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
                Nodes.Add(new Node(br, type, offset + nodeListPointer + i * 0x50, offset + secondaryMatrixPointer + i * 0x40));
                //Add the name data in
                Nodes.Last().Name = nodeNames[i];
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

        public byte[] GenerateBytes()
        {
            List<byte> Data = new List<byte>();
            //0x00
            USF4Utils.AddIntAsBytes(Data, Nodes.Count, false);
            USF4Utils.AddIntAsBytes(Data, IKNodes.Count, false);
            USF4Utils.AddIntAsBytes(Data, IKDataBlocks.Count, true);
            int nodeListPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            int nameIndexPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            //0x10
            int iKBoneListPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            int iKOBjectNameIndexPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            int registerPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            int secondaryMatrixPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            //0x20
            int iKDataPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            USF4Utils.AddZeroToLineEnd(Data);
            //0x30
            USF4Utils.AddIntAsBytes(Data, 0x00, true); //Padding
            USF4Utils.AddIntAsBytes(Data, 0x00, false); //Padding
            USF4Utils.AddIntAsBytes(Data, MysteryShort, false);
            USF4Utils.AddFloatAsBytes(Data, MysteryFloat1);
            USF4Utils.AddFloatAsBytes(Data, MysteryFloat2);
            //0x40 - Node relationships and main matrices
            USF4Utils.UpdateIntAtPosition(Data, nodeListPointerPosition, Data.Count);
            for (int i = 0; i < Nodes.Count; i++)
            {
                USF4Utils.AddSignedShortAsBytes(Data, Nodes[i].Parent);
                USF4Utils.AddSignedShortAsBytes(Data, Nodes[i].Child1);
                USF4Utils.AddSignedShortAsBytes(Data, Nodes[i].Sibling);
                USF4Utils.AddSignedShortAsBytes(Data, Nodes[i].Child3);
                USF4Utils.AddSignedShortAsBytes(Data, Nodes[i].Child4);
                USF4Utils.AddSignedShortAsBytes(Data, Nodes[i].BitFlag);
                USF4Utils.AddFloatAsBytes(Data, Nodes[i].PreMatrixFloat);

                //Check if we need NodeMatrix or TransformMatrix
                Matrix4x4 m = (Type == SkeletonType.EMO) ? Nodes[i].NodeMatrix : Nodes[i].TransformMatrix;
                USF4Utils.AddFloatAsBytes(Data, m.M11);
                USF4Utils.AddFloatAsBytes(Data, m.M12);
                USF4Utils.AddFloatAsBytes(Data, m.M13);
                USF4Utils.AddFloatAsBytes(Data, m.M14);
                USF4Utils.AddFloatAsBytes(Data, m.M21);
                USF4Utils.AddFloatAsBytes(Data, m.M22);
                USF4Utils.AddFloatAsBytes(Data, m.M23);
                USF4Utils.AddFloatAsBytes(Data, m.M24);
                USF4Utils.AddFloatAsBytes(Data, m.M31);
                USF4Utils.AddFloatAsBytes(Data, m.M32);
                USF4Utils.AddFloatAsBytes(Data, m.M33);
                USF4Utils.AddFloatAsBytes(Data, m.M34);
                USF4Utils.AddFloatAsBytes(Data, m.M41);
                USF4Utils.AddFloatAsBytes(Data, m.M42);
                USF4Utils.AddFloatAsBytes(Data, m.M43);
                USF4Utils.AddFloatAsBytes(Data, m.M44);
            }
            //FF Register
            USF4Utils.UpdateIntAtPosition(Data, registerPointerPosition, Data.Count);
            for (int i = 0; i < FFList.Count; i++)
            {
                Data.AddRange(FFList[i]);
            }
            //Node Name Index
            USF4Utils.UpdateIntAtPosition(Data, nameIndexPointerPosition, Data.Count);
            List<int> nodeNameIndexPointerPositions = new List<int>();
            for (int i = 0; i < NodeNames.Count; i++)
            {
                nodeNameIndexPointerPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, -1, true);
            }
            for (int i = 0; i < NodeNames.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(Data, nodeNameIndexPointerPositions[i], Data.Count);
                Data.AddRange(Encoding.ASCII.GetBytes(NodeNames[i]));
                Data.Add(0x00);
            }
            USF4Utils.AddZeroToLineEnd(Data);

            //Secondary Matrix List TODO Check the secondary matrix position - not sure where it appears
            //when there's both secondary matrices AND IK data
            if (Type == SkeletonType.EMO)
            {
                USF4Utils.UpdateIntAtPosition(Data, secondaryMatrixPointerPosition, Data.Count);
                for (int i = 0; i < Nodes.Count; i++)
                {
                    USF4Utils.AddFloatAsBytes(Data, Nodes[i].SkinBindPoseMatrix.M11);
                    USF4Utils.AddFloatAsBytes(Data, Nodes[i].SkinBindPoseMatrix.M12);
                    USF4Utils.AddFloatAsBytes(Data, Nodes[i].SkinBindPoseMatrix.M13);
                    USF4Utils.AddFloatAsBytes(Data, Nodes[i].SkinBindPoseMatrix.M14);
                    USF4Utils.AddFloatAsBytes(Data, Nodes[i].SkinBindPoseMatrix.M21);
                    USF4Utils.AddFloatAsBytes(Data, Nodes[i].SkinBindPoseMatrix.M22);
                    USF4Utils.AddFloatAsBytes(Data, Nodes[i].SkinBindPoseMatrix.M23);
                    USF4Utils.AddFloatAsBytes(Data, Nodes[i].SkinBindPoseMatrix.M24);
                    USF4Utils.AddFloatAsBytes(Data, Nodes[i].SkinBindPoseMatrix.M31);
                    USF4Utils.AddFloatAsBytes(Data, Nodes[i].SkinBindPoseMatrix.M32);
                    USF4Utils.AddFloatAsBytes(Data, Nodes[i].SkinBindPoseMatrix.M33);
                    USF4Utils.AddFloatAsBytes(Data, Nodes[i].SkinBindPoseMatrix.M34);
                    USF4Utils.AddFloatAsBytes(Data, Nodes[i].SkinBindPoseMatrix.M41);
                    USF4Utils.AddFloatAsBytes(Data, Nodes[i].SkinBindPoseMatrix.M42);
                    USF4Utils.AddFloatAsBytes(Data, Nodes[i].SkinBindPoseMatrix.M43);
                    USF4Utils.AddFloatAsBytes(Data, Nodes[i].SkinBindPoseMatrix.M44);
                }
            }

            if (IKDataBlocks.Count != 0)
            {
                //IKData Blocks
                USF4Utils.UpdateIntAtPosition(Data, iKDataPointerPosition, Data.Count);
                for (int i = 0; i < IKDataBlocks.Count; i++)
                {
                    USF4Utils.AddIntAsBytes(Data, IKDataBlocks[i].Method, false);
                    USF4Utils.AddIntAsBytes(Data, IKDataBlocks[i].Length, false);
                    Data.Add((byte)IKDataBlocks[i].Flag0x00);
                    Data.Add((byte)IKDataBlocks[i].Flag0x01);
                    for (int j = 0; j < IKDataBlocks[i].BoneIDs.Count; j++)
                    {
                        USF4Utils.AddIntAsBytes(Data, IKDataBlocks[i].BoneIDs[j], false);
                    }
                    for (int j = 0; j < IKDataBlocks[i].IKFloats.Count; j++)
                    {
                        USF4Utils.AddFloatAsBytes(Data, IKDataBlocks[i].IKFloats[j]);
                    }
                }
                //IK Bone Lists Index
                USF4Utils.UpdateIntAtPosition(Data, iKBoneListPointerPosition, Data.Count);
                List<int> iKNodeBoneListPointerPositions = new List<int>();

                for (int i = 0; i < IKNodes.Count; i++)
                {
                    USF4Utils.AddIntAsBytes(Data, IKNodes[i].BoneCount, true);
                    iKNodeBoneListPointerPositions.Add(Data.Count);
                    USF4Utils.AddIntAsBytes(Data, IKNodes[i].BoneListPointer, true);
                }
                //IK Bone Lists
                for (int i = 0; i < IKNodes.Count; i++)
                {
                    USF4Utils.UpdateIntAtPosition(Data, iKNodeBoneListPointerPositions[i], Data.Count - iKNodeBoneListPointerPositions[i]);
                    for (int j = 0; j < IKNodes[i].BoneList.Count; j++)
                    {
                        USF4Utils.AddIntAsBytes(Data, IKNodes[i].BoneList[j], false);
                    }
                }
                //IKNameIndex
                USF4Utils.UpdateIntAtPosition(Data, iKOBjectNameIndexPointerPosition, Data.Count);
                List<int> iKObjectNamePointers = new List<int>();
                for (int i = 0; i < IKNodes.Count; i++)
                {
                    iKObjectNamePointers.Add(Data.Count);
                    USF4Utils.AddIntAsBytes(Data, -1, true);
                }
                //IK Names
                for (int i = 0; i < IKNodes.Count; i++)
                {
                    USF4Utils.UpdateIntAtPosition(Data, iKObjectNamePointers[i], Data.Count);
                    Data.AddRange(Encoding.ASCII.GetBytes(IKNodeNames[i]));
                    Data.Add(0x00);
                }
            }
            Data.Add(0x00);

            return Data.ToArray();
        }
    }
}