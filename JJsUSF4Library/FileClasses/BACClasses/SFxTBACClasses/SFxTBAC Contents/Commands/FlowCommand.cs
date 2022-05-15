using System.Collections.Generic;
using System.IO;

namespace JJsUSF4Library.FileClasses.BACClasses.SFxTBACClasses
{
    public class FlowCommand : CommandHasParamsBase
    {
        public FlowType Type { get; set; }
        
        
        public byte UnkByte1_0x01 { get; set; }
        public byte UnkByte3_0x03 { get; set; }
        public short Script { get; set; }
        public short UnkShort4_0x06 { get; set; }
        public FlowCommand() { }

        public override void ReadCommandDataBlock(BinaryReader br, int startTick, int endTick)
        {
            int startOffset = (int)br.BaseStream.Position;
            StartTick = startTick;
            EndTick = endTick;
            Type = (FlowType)br.ReadByte();

            if (Type.ToString().Contains("UNK")) 
            {

            }
            UnkByte1_0x01 = br.ReadByte();
            int paramsCount = br.ReadByte();
            UnkByte3_0x03 = br.ReadByte();
            Script = br.ReadInt16();
            UnkShort4_0x06 = br.ReadInt16();
            int paramsPointer = br.ReadInt32();

            //Read params
            if (paramsCount > 0) ReadParamData(br, paramsCount, paramsPointer + startOffset);
        }

        public override byte[] GenerateDataBlockBytes()
        {
            List<byte> Data = new List<byte>();

            Data.Add((byte)Type); //1
            Data.Add((byte)UnkByte1_0x01); //2
            Data.Add((byte)(Params == null ? 0 : Params.Count)); //3
            Data.Add((byte)UnkByte3_0x03); //4
            USF4Utils.AddIntAsBytes(Data, Script, false); //5-6
            USF4Utils.AddIntAsBytes(Data, UnkShort4_0x06, false); //7-8
            USF4Utils.AddIntAsBytes(Data, 0, true); //9-A-B-C Params pointer, need to overwrite it later

            return Data.ToArray();
        }
        public enum FlowType
        {
            ALWAYS = 0x00,
            UNK0x01 = 0x01,
            HIT = 0x02,
            UNK0x03 = 0x03, //UNK
            UNK0x04 = 0x04, //UNK
            HIT2 = 0x05,
            UNK0x06 = 0x06,
            UNK0x07 = 0x07,
            UNK0x08 = 0x08,
            LAND = 0x09,
            WALL = 0x0A,
            UNK0x0B = 0x0B,
            INPUT = 0x0C,
            UNK0x0D = 0x0D,
            UNK0x0E = 0x0E,
            UNK0x0F = 0x0F,
            UNK0x10 = 0x10 //CDY KNIFE_PICK_UP
        }
    }
   
}