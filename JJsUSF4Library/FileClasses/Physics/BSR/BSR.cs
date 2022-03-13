using JJsUSF4Library.FileClasses.SubfileClasses;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JJsUSF4Library.FileClasses
{
    public class BSR : USF4File
    {
        public List<Physic> Physics;
        public List<string> NodeNames;
        public List<string> InfluencingNodeNames;

        public BSR()
        {

        }
        public BSR(BinaryReader br, string name, int offset = 0)
        {
            Name = name;
            ReadFromStream(br, offset);
        }
        public override void ReadFromStream(BinaryReader br, int offset = 0, int fileLength = 0)
        {
            br.BaseStream.Seek(offset + 0x0C, SeekOrigin.Begin);

            Physics = new List<Physic>();
            NodeNames = new List<string>();
            InfluencingNodeNames = new List<string>();
            List<int> physicsPointerList = new List<int>();

            //0x0C
            int physicsCount = br.ReadInt32();
            //0x10
            int physicsIndexPointer = br.ReadInt32();
            int nodeCount = br.ReadInt32();
            int nodeNamesIndexPointer = br.ReadInt32();
            int influencingNodeCount = br.ReadInt32();
            //0x20
            int influencingNodeNameIndexPointer = br.ReadInt32();

            //Read physics
            for (int i = 0; i < physicsCount; i++) physicsPointerList.Add(br.ReadInt32());
            for (int i = 0; i < physicsCount; i++) Physics.Add(new Physic(br, physicsPointerList[i]));

            //Read node names
            br.BaseStream.Seek(nodeNamesIndexPointer, SeekOrigin.Begin);
            br.BaseStream.Seek(br.ReadInt32(), SeekOrigin.Begin);
            for (int i = 0; i < nodeCount; i++) NodeNames.Add(USF4Utils.ReadZString(br));

            //Read influencing nodes
            br.BaseStream.Seek(influencingNodeNameIndexPointer, SeekOrigin.Begin);
            br.BaseStream.Seek(br.ReadInt32(), SeekOrigin.Begin);
            for (int i = 0; i < influencingNodeCount; i++) InfluencingNodeNames.Add(USF4Utils.ReadZString(br));

        }
        public BSR(byte[] Data, string name)
        {
            Name = name;
            ReadFile(Data);
        }

        //public override void ReadFile(byte[] Data)
        //{
        //    HEXBytes = Data;
        //    int physicsCount = USF4Utils.ReadInt(true, 0x0C, Data);
        //    int physicsIndexPointer = USF4Utils.ReadInt(true, 0x10, Data);
        //    int nodeCount = USF4Utils.ReadInt(true, 0x14, Data); //Number of bones with physics calculations
        //    int nodeNamesIndexPointer = USF4Utils.ReadInt(true, 0x18, Data);
        //    int influencingNodeCount = USF4Utils.ReadInt(true, 0x1C, Data); //List of bones which input motion into physics chains??
        //    int influencingNodeNameIndexPointer = USF4Utils.ReadInt(true, 0x20, Data);

        //    List<int> physicsPointerList = new List<int>();
        //    Physics = new List<Physic>();
        //    List<int> nodeNamesPointerList = new List<int>();
        //    NodeNames = new List<string>();
        //    List<int> influencingNodeNamesPointerList = new List<int>();
        //    InfluencingNodeNames = new List<string>();

        //    for (int i = 0; i < physicsCount; i++)
        //    {
        //        physicsPointerList.Add(USF4Utils.ReadInt(true, physicsIndexPointer + i * 4, Data));
        //        Physics.Add(new Physic(Data.Slice(physicsPointerList[i], 0)));
        //    }

        //    for (int i = 0; i < nodeCount; i++)
        //    {
        //        nodeNamesPointerList.Add(USF4Utils.ReadInt(true, nodeNamesIndexPointer + i * 4, Data));
        //        NodeNames.Add(Encoding.ASCII.GetString(USF4Utils.ReadZeroTermStringToArray(nodeNamesPointerList[i], Data, Data.Length)));
        //    }

        //    for (int i = 0; i < influencingNodeCount; i++)
        //    {
        //        influencingNodeNamesPointerList.Add(USF4Utils.ReadInt(true, influencingNodeNameIndexPointer + i * 4, Data));
        //        InfluencingNodeNames.Add(Encoding.ASCII.GetString(USF4Utils.ReadZeroTermStringToArray(influencingNodeNamesPointerList[i], Data, Data.Length)));
        //    }
        //}

        public override byte[] GenerateBytes()
        {
            List<byte> Data = new List<byte>();

            Data.AddRange(new List<byte> { 0x23, 0x42, 0x53, 0x52, 0xFE, 0xFF, 0x24, 0x00, 0x01, 0x00, 0x02, 0x00 });
            USF4Utils.AddIntAsBytes(Data, Physics.Count, true);
            //0x10
            int physicsIndexPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            USF4Utils.AddIntAsBytes(Data, NodeNames.Count, true);
            int nodeNamesIndexPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            USF4Utils.AddIntAsBytes(Data, InfluencingNodeNames.Count, true);
            //0x20
            int influencingNodeNameIndexPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            List<int> physicsPointerListPositions = new List<int>();
            USF4Utils.UpdateIntAtPosition(Data, physicsIndexPointerPosition, Data.Count);
            for (int i = 0; i < Physics.Count; i++)
            {
                physicsPointerListPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, -1, true);
            }
            for (int i = 0; i < Physics.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(Data, physicsPointerListPositions[i], Data.Count);
                Data.AddRange(Physics[i].GenerateBytes());
            }
            List<int> nodeNamesPointerListPositions = new List<int>();
            USF4Utils.UpdateIntAtPosition(Data, nodeNamesIndexPointerPosition, Data.Count);
            for (int i = 0; i < NodeNames.Count; i++)
            {
                nodeNamesPointerListPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, -1, true);
            }
            for (int i = 0; i < NodeNames.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(Data, nodeNamesPointerListPositions[i], Data.Count);
                Data.AddRange(Encoding.ASCII.GetBytes(NodeNames[i]));
                Data.Add(0x00);
            }
            USF4Utils.AddZeroToLineEnd(Data);
            List<int> influencingNodeNamesPointerListPositions = new List<int>();
            USF4Utils.UpdateIntAtPosition(Data, influencingNodeNameIndexPointerPosition, Data.Count);
            for (int i = 0; i < InfluencingNodeNames.Count; i++)
            {
                influencingNodeNamesPointerListPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, -1, true);
            }
            for (int i = 0; i < InfluencingNodeNames.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(Data, influencingNodeNamesPointerListPositions[i], Data.Count);
                Data.AddRange(Encoding.ASCII.GetBytes(InfluencingNodeNames[i]));
                Data.Add(0x00);
            }

            return Data.ToArray();
        }
    }
}