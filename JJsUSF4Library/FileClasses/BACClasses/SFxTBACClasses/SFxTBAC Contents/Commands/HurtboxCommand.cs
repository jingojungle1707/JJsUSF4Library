using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BACClasses.SFxTBACClasses
{
    public class HurtboxCommand : CommandBase
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public short UnkShort4_0x10 { get; set; }
        public short UnkShort5_0x12 { get; set; }
        public ushort Flags { get; set; } //Short 
        public short UnkShort7_0x16 { get; set; }
        public short Counter { get; set; } //Short
        public byte UnkByte9_0x1A { get; set; }
        public byte UnkByte10_0x1B { get; set; }
        public Vulnerability VulFlags { get; set; }
        public byte Armour { get; set; }
        public short UnkShort13_0x1F { get; set; } //Short
        public byte UnkByte14_0x21 { get; set; }
        public byte UnkByte15_0x22 { get; set; }
        public byte UnkByte16_0x23 { get; set; }
        public byte UnkByte17_0x24 { get; set; }
        public byte UnkByte18_0x25 { get; set; }
        public short UnkShort19_0x26 { get; set; }
        public float UnkFloat20_0x28 { get; set; }

        [Flags]
        public enum Vulnerability
        {
            NONE = 0x00,
            STRIKE = 0x01,
            PROJECTILE = 0x02,
            THROW = 0x04,
            UNK0x08 = 0x08,
            UNK0x10 = 0x10,
            UNK0x20 = 0x20,
            UNK0x40 = 0x40,
            UNK0x80 = 0x80,
            UNK0x0100 = 0x0100,
            UNK0x0200 = 0x0200,
            UNK0x0400 = 0x0400,
            UNK0x0800 = 0x0800,
            UNK0x1000 = 0x1000,
            UNK0x2000 = 0x2000,
            UNK0x4000 = 0x4000,
            UNK0x8000 = 0x8000,
        }

        public HurtboxCommand() { }
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
            Flags = br.ReadUInt16();
            UnkShort7_0x16 = br.ReadInt16();
            Counter = br.ReadInt16();
            UnkByte9_0x1A = br.ReadByte();
            UnkByte10_0x1B = br.ReadByte();
            VulFlags = (Vulnerability)br.ReadInt16();

            if (VulFlags.ToString().Contains("UNK"))
            {

            }

            Armour = br.ReadByte();
            UnkShort13_0x1F = br.ReadInt16();
            UnkByte14_0x21 = br.ReadByte();
            UnkByte15_0x22 = br.ReadByte();
            UnkByte16_0x23 = br.ReadByte();
            UnkByte17_0x24 = br.ReadByte();
            UnkByte18_0x25 = br.ReadByte();
            UnkShort19_0x26 = br.ReadInt16();
            UnkFloat20_0x28 = br.ReadSingle();
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
            USF4Utils.AddIntAsBytes(Data, Flags, false);
            USF4Utils.AddIntAsBytes(Data, UnkShort7_0x16, false);
            USF4Utils.AddIntAsBytes(Data, Counter, false);
            Data.Add(UnkByte9_0x1A);
            Data.Add(UnkByte10_0x1B);
            USF4Utils.AddIntAsBytes(Data, (int)VulFlags, false);
            Data.Add(Armour);
            USF4Utils.AddIntAsBytes(Data, UnkShort13_0x1F, false);
            Data.Add(UnkByte14_0x21);
            Data.Add(UnkByte15_0x22);
            Data.Add(UnkByte16_0x23);
            Data.Add(UnkByte17_0x24);
            Data.Add(UnkByte18_0x25);
            USF4Utils.AddIntAsBytes(Data, UnkShort19_0x26, false);
            USF4Utils.AddFloatAsBytes(Data, UnkFloat20_0x28);

            return Data.ToArray();
        }
    }
}
