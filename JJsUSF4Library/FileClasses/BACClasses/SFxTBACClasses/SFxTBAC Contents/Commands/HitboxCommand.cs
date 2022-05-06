using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BACClasses.SFxTBACClasses
{
    public class HitboxCommand : CommandBase
    {
        public float
            X, Y, Width, Height;
        //0x10
        public int
            UnkShort4_0x10,
            UnkShort5_0x12;
        public HitFlags HitFlag;
        public int
            UnkShort6_0x16;
        public byte
            ID,
            Properties,
            UnkByte9_0x1A,
            HitLevel;
        public HitboxType Type;
        public byte
            Juggle,
            JuggleAdd,
            UnkByte14_0x1F,
            //0x20
            UnkByte15_0x20,
            UnkByte16_0x21,
            UnkByte17_0x22,
            UnkByte18_0x23;
        public int HitEffect; //short
        public byte
            UnkByte20_0x26,
            UnkByte21_0x27;
        public float
            UnkFloat22_0x28,
            UnkFloat23_0x2C;

        public enum HitboxType
        {
            STRIKE = 0x00,
            PROJECTILE = 0x01,
            THROW = 0x02,
            UNK0x03 = 0x03,
            PROXIMITY = 0x04,
            UNK0x05 = 0x05,
            UNK0x06 = 0x06,
            UNK0x07 = 0x07,
            UNK0x08 = 0x08,
        }
        [Flags]
        public enum HitFlags
        {
            NONE = 0x00,
            NO_HIT_STANDING = 0x01,
            NO_HIT_CROUCHING = 0x02,
            NO_HIT_AIR = 0x04,
            UNK0x08 = 0x08,
            UNK0x10 = 0x10,
            UNK0x20 = 0x20,
            UNK0x40 = 0x40,
            UNBLOCKABLE = 0x80,
            BREAK_ARMOR = 0x100,
            BREAK_COUNTER = 0x200,
            UNK0x0400 = 0x400,
            UNK0x0800 = 0x800,
            UNK0x1000 = 0x1000,
            UNK0x2000 = 0x2000,
            UNK0x4000 = 0x4000,
            UNK0x8000 = 0x8000,
        }
        public enum VulTypes
        {
            UNK0x00 = 0x00,
            UNK0x01 = 0x01,
            UNK0x02 = 0x02,
            UNK0x03 = 0x03,
            THROW = 0x04,
        }
        public HitboxCommand() { }
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
            HitFlag = (HitFlags)br.ReadInt16();
            UnkShort6_0x16 = br.ReadInt16();
            if (UnkShort6_0x16 != 0)
            {

            }
            ID = br.ReadByte();
            Properties = br.ReadByte();
            UnkByte9_0x1A = br.ReadByte();
            HitLevel = br.ReadByte();
            Type = (HitboxType)br.ReadByte();
            Juggle = br.ReadByte();
            JuggleAdd = br.ReadByte();
            UnkByte14_0x1F = br.ReadByte();
            UnkByte15_0x20 = br.ReadByte();
            UnkByte16_0x21 = br.ReadByte();
            UnkByte17_0x22 = br.ReadByte();
            UnkByte18_0x23 = br.ReadByte();
            HitEffect = br.ReadInt16();
            UnkByte20_0x26 = br.ReadByte();
            UnkByte21_0x27 = br.ReadByte();
            UnkFloat22_0x28 = br.ReadSingle();
            UnkFloat23_0x2C = br.ReadSingle();
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
            USF4Utils.AddIntAsBytes(Data, (int)HitFlag, false);
            USF4Utils.AddIntAsBytes(Data, UnkShort6_0x16, false);
            //USF4Utils.AddIntAsBytes(Data, UnkLong6_0x14, true);
            Data.Add(ID);
            Data.Add(Properties);
            Data.Add(UnkByte9_0x1A);
            Data.Add(HitLevel);
            Data.Add((byte)Type);
            Data.Add(Juggle);
            Data.Add(JuggleAdd);
            Data.Add(UnkByte14_0x1F);
            Data.Add(UnkByte15_0x20);
            Data.Add(UnkByte16_0x21);
            Data.Add(UnkByte17_0x22);
            Data.Add(UnkByte18_0x23);
            USF4Utils.AddIntAsBytes(Data, HitEffect, false);
            Data.Add(UnkByte20_0x26);
            Data.Add(UnkByte21_0x27);
            USF4Utils.AddFloatAsBytes(Data, UnkFloat22_0x28);
            USF4Utils.AddFloatAsBytes(Data, UnkFloat23_0x2C);

            return Data.ToArray();
        }
    }
}
