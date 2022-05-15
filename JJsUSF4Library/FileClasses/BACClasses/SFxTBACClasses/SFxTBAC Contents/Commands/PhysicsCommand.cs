using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BACClasses.SFxTBACClasses
{
    public class PhysicsCommand : CommandBase
    {
        public float Force { get; set; }
        public PhysicsFlags Flags { get; set; }
        public int UnkShort2_0x06 { get; set; }
        public int UnkShort3_0x08 { get; set; }
        public int UnkShort4_0x0A { get; set; }

        [Flags]
        public enum PhysicsFlags
        {
            NONE = 0x0000,
            XVELOCITY = 0x0001,
            UNK0x02 = 0x0002,
            UNK0x04 = 0x0004,
            UNK0x08 = 0x0008,
            YVELOCITY = 0x0010,
            UNK0x20 = 0x0020,
            UNK0x40 = 0x0040,
            UNK0x80 = 0x0080,
            XACCELERATION = 0x0100,
            UNK0x0200 = 0x0200,
            UNK0x0400 = 0x0400,
            UNK0x0800 = 0x0800,
            YACCELERATION = 0x1000,
            UNK0x2000 = 0x2000,
            UNK0x4000 = 0x4000,
            UNK0x8000 = 0x8000
        }

        public PhysicsCommand() { }
        public PhysicsCommand(byte[] Data, int startTick, int endTick)
        {
            StartTick = startTick;
            EndTick = endTick;

            Force = USF4Utils.ReadFloat(0x00, Data);
            Flags = (PhysicsFlags)USF4Utils.ReadInt(false, 0x04, Data);
            UnkShort2_0x06 = USF4Utils.ReadInt(false, 0x06, Data);
            UnkShort3_0x08 = USF4Utils.ReadInt(false, 0x08, Data);
            UnkShort4_0x0A = USF4Utils.ReadInt(false, 0x0A, Data);
        }
        public override void ReadCommandDataBlock(BinaryReader br, int startTick, int endTick)
        {
            StartTick = startTick;
            EndTick = endTick;
            Force = br.ReadSingle();
            Flags = (PhysicsFlags)br.ReadUInt16();
            UnkShort2_0x06 = br.ReadInt16();
            UnkShort3_0x08 = br.ReadInt16();
            UnkShort4_0x0A = br.ReadInt16();
        }

        public override byte[] GenerateDataBlockBytes()
        {
            List<byte> Data = new List<byte>();

            USF4Utils.AddFloatAsBytes(Data, Force);
            USF4Utils.AddIntAsBytes(Data, (int)Flags, false);
            USF4Utils.AddIntAsBytes(Data, UnkShort2_0x06, false);
            USF4Utils.AddIntAsBytes(Data, UnkShort3_0x08, false);
            USF4Utils.AddIntAsBytes(Data, UnkShort4_0x0A, false);

            return Data.ToArray();
        }
    }
}
