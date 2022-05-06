using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BACClasses.SFxTBACClasses
{
    public class SpeedCommand : CommandBase
    {
        public float Multiplier;
        public SpeedCommand() { }
        public SpeedCommand(byte[] Data, int startTick, int endTick)
        {
            StartTick = startTick;
            EndTick = endTick;
            Multiplier = USF4Utils.ReadFloat(0, Data);
        }
        public override void ReadCommandDataBlock(BinaryReader br, int startTick, int endTick)
        {
            StartTick = startTick;
            EndTick = endTick;
            Multiplier = br.ReadSingle();
        }
        public override byte[] GenerateDataBlockBytes()
        {
            List<byte> Data = new List<byte>();

            USF4Utils.AddFloatAsBytes(Data, Multiplier);

            return Data.ToArray();
        }
    }
}
