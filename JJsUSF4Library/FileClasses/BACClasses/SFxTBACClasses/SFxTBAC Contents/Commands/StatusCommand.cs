using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BACClasses.SFxTBACClasses
{
    public class StatusCommand : CommandBase
    {
        public Status StatusFlags;
        public int UnkLong1_0x04;

        [Flags]
        public enum Status
        {
            STAND = 0x01, //LEI.bac -> GUARD_CANCEL. *forced* stand?
            CROUCH = 0x02,
            AIRBORNE = 0x04,
            UNUSED0x0008 = 0x08,

            COUNTERHIT = 0x10,
            UNK0x0020 = 0x20, //PacMan HIT_TURN_L
            UNK0x0040 = 0x40, //PacMan HIT_BODY_UPPER
            UNK0x0080 = 0x80, //Always set when crouch?

            UNK0x0100 = 0x0100,//Always set when airborne?
            UNK0x0200 = 0x0200, //LEI.bac -> DRUNKSTANCE -> SUIHOTAI
            UNK0x0400 = 0x0400, //SC_L
            UNK0x0800 = 0x0800, //CA_FINISH, GUARD_CANCEL, PSYCHOCRUSHER

            UNK0x1000 = 0x1000, //CROSS_ARTS, CROSS_ARTS_DMG
            UNK0x2000 = 0x2000, //RISE_D, CROSS_ARTS, CROSS_ARTS_DMG
            UNK0x4000 = 0x4000, //5LP, 5LK frames 0-5? Something to do with chain cancels? WARP
            UNK0x8000 = 0x8000, //TC_ASSIST, PANDORA_METAMORPHOSE, CROSS_ARTS

            UNK0x010000 = 0x10000, //CROSS_ARTS, CROSS_ARTS_HIT
            TURN = 0x20000, //THROW_F_DAMAGE, THROW_B_DAMAGE
            UNK0x040000 = 0x40000, //40,000 in RINGOUT_LOOP, CC_RINGOUT_START, TC_ATTACK, CROSS_ARTS_HIT, WARP, TC_5LP
            UNUSED0x080000 = 0x80000,

            UNK0x100000 = 0x100000, //RINGOUT_START, RINGIN_START, RINGIN_LOOP
            UNUSED0x200000 = 0x200000,
            UNK0x400000 = 0x400000, //RINGOUT_START, RINGIN_START, RINGIN_LOOP, CROSS_ARTS_HIT
            UNK0x800000 = 0x800000, //fdash, bdash

            UNUSED0x01000000 = 0x1000000,
            UNK0x02000000 = 0x2000000, //THROW_F, THROW_B
            UNUSED0x04000000 = 0x4000000,
            UNUSED0x08000000 = 0x8000000,

            UNUSED0x10000000 = 0x10000000,
            UNK0x20000000 = 0x20000000, //LEI.bac -> SLEEPSTANCE -> SLEEP_STAND
            UNUSED0x40000000 = 0x40000000,
            //UNK0x80000000 = 0x80000000, //hoping this isn't used because it's out of range for an int xD
        }

        public StatusCommand() { }
        public StatusCommand(byte[] Data, int startTick, int endTick)
        {
            StartTick = startTick;
            EndTick = endTick;
            StatusFlags = (Status)USF4Utils.ReadInt(true, 0x00, Data);
            UnkLong1_0x04 = USF4Utils.ReadInt(true, 0x04, Data);
        }
        public override void ReadCommandDataBlock(BinaryReader br, int startTick, int endTick)
        {
            StartTick = startTick;
            EndTick = endTick;
            StatusFlags = (Status)br.ReadInt32();
            UnkLong1_0x04 = br.ReadInt32();

            if (StatusFlags.ToString().Contains("UNUSED"))
            {

            }
        }
        public override byte[] GenerateDataBlockBytes()
        {
            List<byte> Data = new List<byte>();

            USF4Utils.AddIntAsBytes(Data, (int)StatusFlags, true);
            USF4Utils.AddIntAsBytes(Data, UnkLong1_0x04, true);

            return Data.ToArray();
        }
    }
}
