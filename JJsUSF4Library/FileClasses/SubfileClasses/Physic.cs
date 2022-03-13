using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class Physic
    {
        public byte[] HEXBytes;
        public float Gravity;
        public float AirResistance;
        public float MysteryFloat0x08; //Often these two floats seem to be zero
        public float MysteryFloat0x0C; //non-zero if there's a mystery data block?

        public List<int> ChainLengths;
        public List<Vector2> ChainFloats; //Still not sure about these
        public List<PhysNode> NodeDataBlocks;
        public List<LimitData> LimitDataBlocks;
        public List<MysteryData> MysteryDataBlocks;

        public Physic(BinaryReader br, int offset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            ChainLengths = new List<int>();
            ChainFloats = new List<Vector2>();
            NodeDataBlocks = new List<PhysNode>();
            LimitDataBlocks = new List<LimitData>();
            MysteryDataBlocks = new List<MysteryData>();

            Gravity = br.ReadSingle();
            AirResistance = br.ReadSingle();
            MysteryFloat0x08 = br.ReadSingle();
            MysteryFloat0x0C = br.ReadSingle();
            //0x10
            int chainLengthsCount = br.ReadInt32();
            int chainLengthsPointer = br.ReadInt32();
            int nodeDataCount = br.ReadInt32();
            int nodeDataPointer = br.ReadInt32();
            //0x20
            int limitDataCount = br.ReadInt32();
            int limitDataPointer = br.ReadInt32();
            int mysteryDataCount = br.ReadInt32();
            int mysteryDataPointer = br.ReadInt32();
            //0x30
            //Read ChainLengths/Floats
            br.BaseStream.Seek(chainLengthsPointer + offset, SeekOrigin.Begin);
            for (int i = 0; i < chainLengthsCount; i++)
            {
                ChainLengths.Add(br.ReadInt32());
                ChainFloats.Add(new Vector2(br.ReadSingle(), br.ReadSingle()));
            }
            //Read PhysNode blocks
            br.BaseStream.Seek(nodeDataPointer + offset, SeekOrigin.Begin);
            for (int i = 0; i < nodeDataCount; i++)
            {
                NodeDataBlocks.Add(new PhysNode()
                {
                    ID = br.ReadInt32(),
                    LimitValues = new List<float>()
                    {
                        br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                        br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                    }
                });
            }
            //Read LimitData block
            br.BaseStream.Seek(limitDataPointer + offset, SeekOrigin.Begin);
            for (int i = 0; i < limitDataCount; i++)
            {
                LimitDataBlocks.Add(new LimitData()
                {
                    bitflag = br.ReadInt16(),
                    ID1 = br.ReadByte(),
                    ID2 = br.ReadByte(),
                    LimitValues = new List<float>()
                    {
                        br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                        br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                    }
                });
            }
            //Read MysteryData block
            br.BaseStream.Seek(mysteryDataPointer + offset, SeekOrigin.Begin);
            for (int i = 0; i < mysteryDataCount; i++)
            {
                MysteryDataBlocks.Add(new MysteryData()
                {
                    bitflag = br.ReadInt16(),
                    ID1 = br.ReadByte(),
                    ID2 = br.ReadByte(),
                    Floats = new List<float>()
                    {
                        br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                        br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                        br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                    }
                });
            }
        }

        public Physic(byte[] Data)
        {
            Gravity = USF4Utils.ReadFloat(0x00, Data);
            AirResistance = USF4Utils.ReadFloat(0x04, Data);
            MysteryFloat0x08 = USF4Utils.ReadFloat(0x08, Data);
            MysteryFloat0x0C = USF4Utils.ReadFloat(0x0C, Data);
            int chainLengthsCount = USF4Utils.ReadInt(true, 0x10, Data);
            int chainLengthsPointer = USF4Utils.ReadInt(true, 0x14, Data);
            int nodeDataCount = USF4Utils.ReadInt(true, 0x18, Data);
            int nodeDataPointer = USF4Utils.ReadInt(true, 0x1C, Data);
            int limitDataCount = USF4Utils.ReadInt(true, 0x20, Data);
            int limitDataPointer = USF4Utils.ReadInt(true, 0x24, Data);
            int mysteryDataCount = USF4Utils.ReadInt(true, 0x28, Data);
            int mysteryDataPointer = USF4Utils.ReadInt(true, 0x2C, Data);

            ChainLengths = new List<int>();
            ChainFloats = new List<Vector2>();
            for (int i = 0; i < chainLengthsCount; i++)
            {
                ChainLengths.Add(USF4Utils.ReadInt(true, chainLengthsPointer + i * 12, Data));
                ChainFloats.Add(new Vector2(USF4Utils.ReadFloat(chainLengthsPointer + i * 12 + 4, Data),
                                                USF4Utils.ReadFloat(chainLengthsPointer + i * 12 + 8, Data)));
                if (ChainFloats[i].Y != 0)
                {
                    Console.WriteLine($"Prenodefloat {i} {ChainFloats[i].Y}");
                }
            }
            NodeDataBlocks = new List<PhysNode>();
            for (int i = 0; i < nodeDataCount; i++)
            {
                byte[] NodeData = Data.Slice(nodeDataPointer + i * 0x24, 0x24);
                NodeDataBlocks.Add(new PhysNode()
                {
                    ID = USF4Utils.ReadInt(true, 0x00, NodeData),
                    LimitValues = new List<float>() { USF4Utils.ReadFloat(0x04, NodeData), USF4Utils.ReadFloat(0x08, NodeData), USF4Utils.ReadFloat(0x0C, NodeData),
                                                      USF4Utils.ReadFloat(0x10, NodeData), USF4Utils.ReadFloat(0x14, NodeData), USF4Utils.ReadFloat(0x18, NodeData) }
                });
            }
            LimitDataBlocks = new List<LimitData>();
            for (int i = 0; i < limitDataCount; i++)
            {
                byte[] LimitData = Data.Slice(limitDataPointer + i * 0x1C, 0x1C);
                LimitDataBlocks.Add(new LimitData()
                {
                    bitflag = USF4Utils.ReadInt(false, 0x00, LimitData),
                    ID1 = LimitData[0x02],
                    ID2 = LimitData[0x03],
                    LimitValues = new List<float>()
                    {
                        USF4Utils.ReadFloat(0x04, LimitData), USF4Utils.ReadFloat(0x08, LimitData), USF4Utils.ReadFloat(0x0C, LimitData),
                        USF4Utils.ReadFloat(0x10, LimitData), USF4Utils.ReadFloat(0x14, LimitData), USF4Utils.ReadFloat(0x18, LimitData)
                    }
                });
            }
            MysteryDataBlocks = new List<MysteryData>();
            for (int i = 0; i < mysteryDataCount; i++)
            {
                byte[] MysteryData = Data.Slice(mysteryDataPointer + i * 0x28, 0x28);
                MysteryDataBlocks.Add(new MysteryData()
                {
                    bitflag = USF4Utils.ReadInt(false, 0x00, MysteryData),
                    ID1 = MysteryData[0x02],
                    ID2 = MysteryData[0x03],
                    Floats = new List<float>()
                    {
                        USF4Utils.ReadFloat(0x04, MysteryData), USF4Utils.ReadFloat(0x08, MysteryData), USF4Utils.ReadFloat(0x0C, MysteryData),
                        USF4Utils.ReadFloat(0x10, MysteryData), USF4Utils.ReadFloat(0x14, MysteryData), USF4Utils.ReadFloat(0x18, MysteryData),
                        USF4Utils.ReadFloat(0x1C, MysteryData), USF4Utils.ReadFloat(0x20, MysteryData), USF4Utils.ReadFloat(0x24, MysteryData)
                    }
                });
            }

        }

        public byte[] GenerateBytes()
        {
            List<byte> Data = new List<byte>();

            USF4Utils.AddFloatAsBytes(Data, Gravity);
            USF4Utils.AddFloatAsBytes(Data, AirResistance);
            USF4Utils.AddFloatAsBytes(Data, MysteryFloat0x08);
            USF4Utils.AddFloatAsBytes(Data, MysteryFloat0x0C);
            //0x10
            USF4Utils.AddIntAsBytes(Data, ChainLengths.Count, true);
            int PreNodePointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            USF4Utils.AddIntAsBytes(Data, NodeDataBlocks.Count, true);
            int NodeDataPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            //0x20
            USF4Utils.AddIntAsBytes(Data, LimitDataBlocks.Count, true);
            int LimitDataPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            USF4Utils.AddIntAsBytes(Data, MysteryDataBlocks.Count, true);
            int MysteryDataPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            //0x30
            USF4Utils.UpdateIntAtPosition(Data, PreNodePointerPosition, Data.Count);
            for (int i = 0; i < ChainLengths.Count; i++)
            {
                USF4Utils.AddIntAsBytes(Data, ChainLengths[i], true);
                USF4Utils.AddFloatAsBytes(Data, ChainFloats[i].X);
                USF4Utils.AddFloatAsBytes(Data, ChainFloats[i].Y);
            }
            USF4Utils.UpdateIntAtPosition(Data, NodeDataPointerPosition, Data.Count);
            for (int i = 0; i < NodeDataBlocks.Count; i++)
            {
                USF4Utils.AddIntAsBytes(Data, NodeDataBlocks[i].ID, true);
                USF4Utils.AddFloatAsBytes(Data, NodeDataBlocks[i].LimitValues[0]);
                USF4Utils.AddFloatAsBytes(Data, NodeDataBlocks[i].LimitValues[1]);
                USF4Utils.AddFloatAsBytes(Data, NodeDataBlocks[i].LimitValues[2]);
                USF4Utils.AddFloatAsBytes(Data, NodeDataBlocks[i].LimitValues[3]);
                USF4Utils.AddFloatAsBytes(Data, NodeDataBlocks[i].LimitValues[4]);
                USF4Utils.AddFloatAsBytes(Data, NodeDataBlocks[i].LimitValues[5]);
                Data.AddRange(new byte[] { 0x00, 0x6F, 0x64, 0x65, 0x5F, 0x6C, 0x69, 0x73 });
            }
            USF4Utils.UpdateIntAtPosition(Data, LimitDataPointerPosition, Data.Count);
            for (int i = 0; i < LimitDataBlocks.Count; i++)
            {
                USF4Utils.AddIntAsBytes(Data, LimitDataBlocks[i].bitflag, false);
                Data.Add((byte)LimitDataBlocks[i].ID1);
                Data.Add((byte)LimitDataBlocks[i].ID2);

                //if(LimitDataBlocks[i].bitflag == 0)
                //            {
                //	Utils.AddFloatAsBytes(Data, 10f);
                //	Utils.AddFloatAsBytes(Data, 0f);
                //	Utils.AddFloatAsBytes(Data, 0f);
                //	Utils.AddFloatAsBytes(Data, LimitDataBlocks[i].LimitValues[3]);
                //	Utils.AddFloatAsBytes(Data, LimitDataBlocks[i].LimitValues[4]);
                //	Utils.AddFloatAsBytes(Data, LimitDataBlocks[i].LimitValues[5]);
                //}
                //else
                //            {
                //	Utils.AddFloatAsBytes(Data, 0f);
                //	Utils.AddFloatAsBytes(Data, 0f);
                //	Utils.AddFloatAsBytes(Data, 0f);
                //	Utils.AddFloatAsBytes(Data, LimitDataBlocks[i].LimitValues[3]);
                //	Utils.AddFloatAsBytes(Data, LimitDataBlocks[i].LimitValues[4]);
                //	Utils.AddFloatAsBytes(Data, LimitDataBlocks[i].LimitValues[5]);
                //}
                USF4Utils.AddFloatAsBytes(Data, LimitDataBlocks[i].LimitValues[0]);
                USF4Utils.AddFloatAsBytes(Data, LimitDataBlocks[i].LimitValues[1]);
                USF4Utils.AddFloatAsBytes(Data, LimitDataBlocks[i].LimitValues[2]);
                USF4Utils.AddFloatAsBytes(Data, LimitDataBlocks[i].LimitValues[3]);
                USF4Utils.AddFloatAsBytes(Data, LimitDataBlocks[i].LimitValues[4]);
                USF4Utils.AddFloatAsBytes(Data, LimitDataBlocks[i].LimitValues[5]);
            }
            USF4Utils.UpdateIntAtPosition(Data, MysteryDataPointerPosition, Data.Count);
            for (int i = 0; i < MysteryDataBlocks.Count; i++)
            {
                USF4Utils.AddIntAsBytes(Data, MysteryDataBlocks[i].bitflag, false);
                Data.Add((byte)MysteryDataBlocks[i].ID1);
                Data.Add((byte)MysteryDataBlocks[i].ID2);
                USF4Utils.AddFloatAsBytes(Data, MysteryDataBlocks[i].Floats[0]);
                USF4Utils.AddFloatAsBytes(Data, MysteryDataBlocks[i].Floats[1]);
                USF4Utils.AddFloatAsBytes(Data, MysteryDataBlocks[i].Floats[2]);
                USF4Utils.AddFloatAsBytes(Data, MysteryDataBlocks[i].Floats[3]);
                USF4Utils.AddFloatAsBytes(Data, MysteryDataBlocks[i].Floats[4]);
                USF4Utils.AddFloatAsBytes(Data, MysteryDataBlocks[i].Floats[5]);
                USF4Utils.AddFloatAsBytes(Data, MysteryDataBlocks[i].Floats[6]);
                USF4Utils.AddFloatAsBytes(Data, MysteryDataBlocks[i].Floats[7]);
                USF4Utils.AddFloatAsBytes(Data, MysteryDataBlocks[i].Floats[8]);
            }
            return Data.ToArray();
        }
    }
}