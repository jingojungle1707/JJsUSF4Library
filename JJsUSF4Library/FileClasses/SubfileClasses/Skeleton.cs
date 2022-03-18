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

        public List<byte> GenerateBytes()
        {
            List<byte> data = new List<byte>();
            //0x00
            USF4Utils.AddIntAsBytes(data, Nodes.Count, false);
            USF4Utils.AddIntAsBytes(data, IKNodes.Count, false);
            USF4Utils.AddIntAsBytes(data, IKDataBlocks.Count, true);
            int nodeListPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, -1, true);
            int nameIndexPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, -1, true);
            //0x10
            int iKBoneListPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, -1, true);
            int iKOBjectNameIndexPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, -1, true);
            int registerPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, -1, true);
            int secondaryMatrixPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, -1, true);
            //0x20
            int iKDataPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, -1, true);
            USF4Utils.AddZeroToLineEnd(data);
            //0x30
            USF4Utils.AddIntAsBytes(data, 0x00, true); //Padding
            USF4Utils.AddIntAsBytes(data, 0x00, false); //Padding
            USF4Utils.AddIntAsBytes(data, MysteryShort, false);
            USF4Utils.AddFloatAsBytes(data, MysteryFloat1);
            USF4Utils.AddFloatAsBytes(data, MysteryFloat2);
            //0x40 - Node relationships and main matrices
            USF4Utils.UpdateIntAtPosition(data, nodeListPointerPosition, data.Count);
            for (int i = 0; i < Nodes.Count; i++)
            {
                USF4Utils.AddSignedShortAsBytes(data, Nodes[i].Parent);
                USF4Utils.AddSignedShortAsBytes(data, Nodes[i].Child1);
                USF4Utils.AddSignedShortAsBytes(data, Nodes[i].Sibling);
                USF4Utils.AddSignedShortAsBytes(data, Nodes[i].Child3);
                USF4Utils.AddSignedShortAsBytes(data, Nodes[i].Child4);
                USF4Utils.AddSignedShortAsBytes(data, Nodes[i].BitFlag);
                USF4Utils.AddFloatAsBytes(data, Nodes[i].PreMatrixFloat);

                //Check if we need NodeMatrix or TransformMatrix
                Matrix4x4 m = (Type == SkeletonType.EMO) ? Nodes[i].NodeMatrix : Nodes[i].TransformMatrix;
                USF4Utils.AddFloatAsBytes(data, m.M11);
                USF4Utils.AddFloatAsBytes(data, m.M12);
                USF4Utils.AddFloatAsBytes(data, m.M13);
                USF4Utils.AddFloatAsBytes(data, m.M14);
                USF4Utils.AddFloatAsBytes(data, m.M21);
                USF4Utils.AddFloatAsBytes(data, m.M22);
                USF4Utils.AddFloatAsBytes(data, m.M23);
                USF4Utils.AddFloatAsBytes(data, m.M24);
                USF4Utils.AddFloatAsBytes(data, m.M31);
                USF4Utils.AddFloatAsBytes(data, m.M32);
                USF4Utils.AddFloatAsBytes(data, m.M33);
                USF4Utils.AddFloatAsBytes(data, m.M34);
                USF4Utils.AddFloatAsBytes(data, m.M41);
                USF4Utils.AddFloatAsBytes(data, m.M42);
                USF4Utils.AddFloatAsBytes(data, m.M43);
                USF4Utils.AddFloatAsBytes(data, m.M44);
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
            for (int i = 0; i < NodeNames.Count; i++)
            {
                nodeNameIndexPointerPositions.Add(data.Count);
                USF4Utils.AddIntAsBytes(data, -1, true);
            }
            for (int i = 0; i < NodeNames.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(data, nodeNameIndexPointerPositions[i], data.Count);
                data.AddRange(Encoding.ASCII.GetBytes(NodeNames[i]));
                data.Add(0x00);
            }
            USF4Utils.AddZeroToLineEnd(data);

            //Secondary Matrix List TODO Check the secondary matrix position - not sure where it appears
            //when there's both secondary matrices AND IK data
            if (Type == SkeletonType.EMO)
            {
                USF4Utils.UpdateIntAtPosition(data, secondaryMatrixPointerPosition, data.Count);
                for (int i = 0; i < Nodes.Count; i++)
                {
                    USF4Utils.AddFloatAsBytes(data, Nodes[i].SkinBindPoseMatrix.M11);
                    USF4Utils.AddFloatAsBytes(data, Nodes[i].SkinBindPoseMatrix.M12);
                    USF4Utils.AddFloatAsBytes(data, Nodes[i].SkinBindPoseMatrix.M13);
                    USF4Utils.AddFloatAsBytes(data, Nodes[i].SkinBindPoseMatrix.M14);
                    USF4Utils.AddFloatAsBytes(data, Nodes[i].SkinBindPoseMatrix.M21);
                    USF4Utils.AddFloatAsBytes(data, Nodes[i].SkinBindPoseMatrix.M22);
                    USF4Utils.AddFloatAsBytes(data, Nodes[i].SkinBindPoseMatrix.M23);
                    USF4Utils.AddFloatAsBytes(data, Nodes[i].SkinBindPoseMatrix.M24);
                    USF4Utils.AddFloatAsBytes(data, Nodes[i].SkinBindPoseMatrix.M31);
                    USF4Utils.AddFloatAsBytes(data, Nodes[i].SkinBindPoseMatrix.M32);
                    USF4Utils.AddFloatAsBytes(data, Nodes[i].SkinBindPoseMatrix.M33);
                    USF4Utils.AddFloatAsBytes(data, Nodes[i].SkinBindPoseMatrix.M34);
                    USF4Utils.AddFloatAsBytes(data, Nodes[i].SkinBindPoseMatrix.M41);
                    USF4Utils.AddFloatAsBytes(data, Nodes[i].SkinBindPoseMatrix.M42);
                    USF4Utils.AddFloatAsBytes(data, Nodes[i].SkinBindPoseMatrix.M43);
                    USF4Utils.AddFloatAsBytes(data, Nodes[i].SkinBindPoseMatrix.M44);
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

            return data;
        }
    }
}