using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JJsUSF4Library.FileClasses.BACClasses.SFxTBACClasses
{
    public class HitboxCommand : CommandBase
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        //0x10
        public short UnkShort4_0x10 { get; set; }
        public short UnkShort5_0x12 { get; set; }
        public HitFlags HitFlag { get; set; }
        public short UnkShort6_0x16 { get; set; }

        [XmlElement("ID_SByte")]
        public sbyte ID { get; set; }
        [XmlElement("Properties_SByte")]
        public sbyte Properties { get; set; }
        [XmlElement("NumberofHits_SByte")]
        public sbyte NumberOfHits { get; set; }
        [XmlElement("HitLevel_SByte")]
        public sbyte HitLevel { get; set; }
        public HitboxType Type { get; set; }
        [XmlElement("Juggle_SByte")]
        public sbyte Juggle { get; set; }
        [XmlElement("JuggleAdd_SByte")]
        public sbyte JuggleAdd { get; set; }
        public sbyte UnkByte14_0x1F { get; set; }
        //0x20
        public sbyte UnkByte15_0x20 { get; set; }
        public sbyte UnkByte16_0x21 { get; set; }
        [XmlElement("DamageMultiplier_SByte")]
        public sbyte DamageMultiplier { get; set; }
        [XmlElement("RecoverablePercentage_SByte")]
        public sbyte RecoverablePercentage { get; set; }
        [XmlElement("HitEffect_Short")] //TODO check - short? ushort?
        public short HitEffect { get; set; }
        public sbyte UnkByte20_0x26 { get; set; }
        public sbyte UnkByte21_0x27 { get; set; }
        public float UnkFloat22_0x28 { get; set; }
        public float UnkFloat23_0x2C { get; set; }

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
            DONT_HIT_WEAPON = 0x400,
            DONT_HIT_NO_WEAPON = 0x800,
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
            ID = br.ReadSByte();
            Properties = br.ReadSByte();
            NumberOfHits = br.ReadSByte();
            HitLevel = br.ReadSByte();
            Type = (HitboxType)br.ReadByte();
            Juggle = br.ReadSByte();
            JuggleAdd = br.ReadSByte();
            UnkByte14_0x1F = br.ReadSByte();
            UnkByte15_0x20 = br.ReadSByte();
            UnkByte16_0x21 = br.ReadSByte();
            DamageMultiplier = br.ReadSByte();
            RecoverablePercentage = br.ReadSByte();
            HitEffect = br.ReadInt16();
            UnkByte20_0x26 = br.ReadSByte();
            UnkByte21_0x27 = br.ReadSByte();
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
            Data.Add((byte)ID);
            Data.Add((byte)Properties);
            Data.Add((byte)NumberOfHits);
            Data.Add((byte)HitLevel);
            Data.Add((byte)Type);
            Data.Add((byte)Juggle);
            Data.Add((byte)JuggleAdd);
            Data.Add((byte)UnkByte14_0x1F);
            Data.Add((byte)UnkByte15_0x20);
            Data.Add((byte)UnkByte16_0x21);
            Data.Add((byte)DamageMultiplier);
            Data.Add((byte)RecoverablePercentage);
            USF4Utils.AddIntAsBytes(Data, HitEffect, false);
            Data.Add((byte)UnkByte20_0x26);
            Data.Add((byte)UnkByte21_0x27);
            USF4Utils.AddFloatAsBytes(Data, UnkFloat22_0x28);
            USF4Utils.AddFloatAsBytes(Data, UnkFloat23_0x2C);

            return Data.ToArray();
        }
    }
}
