using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BACClasses.SFxTBACClasses
{
    public class AnimationModCommand : CommandBase
    {
        public int UnkShort0_0x00 { get; set; }
        public int UnkShort1_0x02 { get; set; }
        public float UnkFloat2_0x04 { get; set; }

        public AnimationModCommand() { }
        public override void ReadCommandDataBlock(BinaryReader br, int startTick, int endTick)
        {
            StartTick = startTick;
            EndTick = endTick;
            UnkShort0_0x00 = br.ReadInt16();
            UnkShort1_0x02 = br.ReadInt16();
            UnkFloat2_0x04 = br.ReadSingle();
        }
        public override byte[] GenerateDataBlockBytes()
        {
            List<byte> Data = new List<byte>();

            USF4Utils.AddIntAsBytes(Data, UnkShort0_0x00, false);
            USF4Utils.AddIntAsBytes(Data, UnkShort1_0x02, false);
            USF4Utils.AddFloatAsBytes(Data, UnkFloat2_0x04);

            return Data.ToArray();
        }
    }
}
