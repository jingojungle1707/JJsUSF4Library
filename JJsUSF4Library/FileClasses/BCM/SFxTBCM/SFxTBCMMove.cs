using System.IO;

namespace JJsUSF4Library.FileClasses.ScriptClasses
{
    public class SFxTBCMMove
    {
        public string Name;

        //0x00
        public int
            Input,
            InputFlags,
            PositionRestriction,
            UnkShort3_0x06,
            UnkShort4_0x08,
            UnkShort5_0x0A;
        public float
            UnknownFloat_0x0C;
        //0x10
        public int
            PositionRestrictionDistance,
            Restriction,
            UnkShort9_0x14,
            UnkShort10_0x16,
            MeterReq,
            MeterLoss,
            InputMotion,
            Script;
        //0x20
        public byte
            UnkByte15_0x20,
            UnkByte16_0x21;
        public int
            UnkShort17_0x22,
            UnkShort18_0x24,
            AIMinimumDistance;
        public float
            AIMaxDistance,
            UnknownFloat_0x2C,
            //0x30
            UnknownFloat_0x30;
        public int
            UnkShort23_0x34,
            UnkShort24_0x36,
            UnkShort25_0x38;
        public byte
            UnkByte26_0x3A,
            UnkByte27_0x3B,
            UnkByte28_0x3C,
            UnkByte29_0x3D,
            AIFar,
            AIVeryFar;

        public SFxTBCMMove()
        {

        }
        public SFxTBCMMove(BinaryReader br, string name, int offset = 0)
        {
            Name = name;

            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            Input = br.ReadInt16();
            InputFlags = br.ReadInt16();
            PositionRestriction = br.ReadInt16();
            UnkShort3_0x06 = br.ReadInt16();
            UnkShort4_0x08 = br.ReadInt16();
            UnkShort5_0x0A = br.ReadInt16();
            UnknownFloat_0x0C = br.ReadSingle();
            //0x10
            PositionRestrictionDistance = br.ReadInt16();
            Restriction = br.ReadInt16();
            UnkShort9_0x14 = br.ReadInt16();
            UnkShort10_0x16 = br.ReadInt16();
            MeterReq = br.ReadInt16();
            MeterLoss = br.ReadInt16();
            InputMotion = br.ReadInt16();
            Script = br.ReadInt16();
            //0x20
            UnkByte15_0x20 = br.ReadByte();
            UnkByte16_0x21 = br.ReadByte();
            UnkShort17_0x22 = br.ReadInt16();
            UnkShort18_0x24 = br.ReadInt16();
            AIMinimumDistance = br.ReadInt16();
            AIMaxDistance = br.ReadSingle();
            UnknownFloat_0x2C = br.ReadSingle();
            //0x30
            UnknownFloat_0x30 = br.ReadSingle();
            UnkShort23_0x34 = br.ReadInt16();
            UnkShort24_0x36 = br.ReadInt16();
            UnkShort25_0x38 = br.ReadInt16();
            UnkByte26_0x3A = br.ReadByte();
            UnkByte27_0x3B = br.ReadByte();
            UnkByte28_0x3C = br.ReadByte();
            UnkByte29_0x3D = br.ReadByte();
            AIFar = br.ReadByte();
            AIVeryFar = br.ReadByte();
        }

        public SFxTBCMMove(byte[] Data, string name)
        {
            Name = name;

            Input = USF4Utils.ReadInt(false, 0x00, Data);
            InputFlags = USF4Utils.ReadInt(false, 0x02, Data);
            PositionRestriction = USF4Utils.ReadInt(false, 0x04, Data);
            UnkShort3_0x06 = USF4Utils.ReadInt(false, 0x06, Data);
            UnkShort4_0x08 = USF4Utils.ReadInt(false, 0x08, Data);
            UnkShort5_0x0A = USF4Utils.ReadInt(false, 0x0A, Data);
            UnknownFloat_0x0C = USF4Utils.ReadFloat(0x0C, Data);
            //0x10
            PositionRestrictionDistance = USF4Utils.ReadInt(false, 0x10, Data);
            Restriction = USF4Utils.ReadInt(false, 0x12, Data);
            UnkShort9_0x14 = USF4Utils.ReadInt(false, 0x14, Data);
            UnkShort10_0x16 = USF4Utils.ReadInt(false, 0x16, Data);
            MeterReq = USF4Utils.ReadInt(false, 0x18, Data);
            MeterLoss = USF4Utils.ReadInt(false, 0x1A, Data);
            InputMotion = USF4Utils.ReadInt(false, 0x1C, Data);
            Script = USF4Utils.ReadInt(false, 0x1E, Data);
            //0x20
            UnkByte15_0x20 = Data[0x20];
            UnkByte16_0x21 = Data[0x21];
            UnkShort17_0x22 = USF4Utils.ReadInt(false, 0x22, Data);
            UnkShort18_0x24 = USF4Utils.ReadInt(false, 0x24, Data);
            AIMinimumDistance = USF4Utils.ReadInt(false, 0x26, Data);
            AIMaxDistance = USF4Utils.ReadFloat(0x28, Data);
            UnknownFloat_0x2C = USF4Utils.ReadFloat(0x2C, Data);
            //0x30
            UnknownFloat_0x30 = USF4Utils.ReadFloat(0x30, Data);
            UnkShort23_0x34 = USF4Utils.ReadInt(false, 0x34, Data);
            UnkShort24_0x36 = USF4Utils.ReadInt(false, 0x36, Data);
            UnkShort25_0x38 = USF4Utils.ReadInt(false, 0x38, Data);
            UnkByte26_0x3A = Data[0x3A];
            UnkByte27_0x3B = Data[0x3B];
            UnkByte28_0x3C = Data[0x3C];
            UnkByte29_0x3D = Data[0x3D];
            AIFar = Data[0x3E];
            AIVeryFar = Data[0x3F];
        }
    }
}
