using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BACClasses.SFxTBACClasses
{
    public class CancelCommand : CommandBase
    {
        [Flags]
        public enum Conditions
        {
            START = 0x00,
            ON_HIT = 0x01,
            ON_BLOCK = 0x02,
            UNK0x04 = 0x04,
            END = 0x08
        }
        public int CancelList { get; set; }
        public Conditions Condition { get; set; }

        //Cancel "Type"? Is this equivalent to ONO's cancel "Condition"?
        //Defines on hit/on block settings?

        public CancelCommand() { }
        public CancelCommand(byte[] Data, int startTick, int endTick)
        {
            StartTick = startTick;
            EndTick = endTick;
            CancelList = USF4Utils.ReadInt(false, 0x00, Data);
            Condition = (Conditions)USF4Utils.ReadInt(false, 0x02, Data);
        }
        public override void ReadCommandDataBlock(BinaryReader br, int startTick, int endTick)
        {
            StartTick = startTick;
            EndTick = endTick;
            CancelList = br.ReadInt16();
            Condition = (Conditions)br.ReadInt16();
        }
        public override byte[] GenerateDataBlockBytes()
        {
            List<byte> Data = new List<byte>();

            USF4Utils.AddIntAsBytes(Data, CancelList, false);
            USF4Utils.AddIntAsBytes(Data, (int)Condition, false);

            return Data.ToArray();
        }
    }
}
