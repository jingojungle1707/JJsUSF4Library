using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BACClasses.SFxTBACClasses
{
    public class PushboxCommand : CommandBase
    {
        public float
            X, Y, Width, Height;
        public int
            UnkShort4_0x10,
            UnkShort5_0x12,
            UnkShort6_0x14,
            UnkShort7_0x16;

        public PushboxCommand() { }
        public override void ReadCommandDataBlock(BinaryReader br, int startTick, int endTick)
        {
            StartTick = startTick;
            EndTick = endTick;
            X = br.ReadSingle();
            Y = br.ReadSingle();
            Width = br.ReadSingle();
            Height = br.ReadSingle();
            UnkShort4_0x10 = br.ReadInt16();
            UnkShort5_0x12 = br.ReadInt16();
            UnkShort6_0x14 = br.ReadInt16();
            UnkShort7_0x16 = br.ReadInt16();
        }
        public override byte[] GenerateDataBlockBytes()
        {
            List<byte> Data = new List<byte>();

            USF4Utils.AddFloatAsBytes(Data, X);
            USF4Utils.AddFloatAsBytes(Data, Y);
            USF4Utils.AddFloatAsBytes(Data, Width);
            USF4Utils.AddFloatAsBytes(Data, Height);
            USF4Utils.AddIntAsBytes(Data, UnkShort4_0x10, false);
            USF4Utils.AddIntAsBytes(Data, UnkShort5_0x12, false);
            USF4Utils.AddIntAsBytes(Data, UnkShort6_0x14, false);
            USF4Utils.AddIntAsBytes(Data, UnkShort7_0x16, false);

            return Data.ToArray();
        }
    }
}
