using System.Collections.Generic;
using System.IO;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class CommandDatablock
    {
        public int
            StartTick,
            EndTick;

        public virtual void ReadCommandDataBlock(BinaryReader br)
        {

        }

        public virtual byte[] GenerateBytes()
        {
            List<byte> Data = new List<byte>();
            return Data.ToArray();
        }

        public class FlowCommand : CommandDatablock //COMMANDTYPE 0x00
        {
            public int Type, Input, Script, TargetFrame; //All short

            public override void ReadCommandDataBlock(BinaryReader br)
            {
                Type = br.ReadInt16();
                Input = br.ReadInt16();
                Script = br.ReadInt16();
                TargetFrame = br.ReadInt16();
            }

            public override byte[] GenerateBytes()
            {
                List<byte> Data = new List<byte>();

                USF4Utils.AddIntAsBytes(Data, Type, false);
                USF4Utils.AddIntAsBytes(Data, Input, false);
                USF4Utils.AddIntAsBytes(Data, Script, false);
                USF4Utils.AddIntAsBytes(Data, TargetFrame, false);

                return Data.ToArray();
            }
        }
        public class AnimationCommand : CommandDatablock //COMMANDTYPE 0x01
        {
            public byte Type, Flags;
            public int Animation, FromFrame, ToFrame; //all short

            public override void ReadCommandDataBlock(BinaryReader br)
            {
                Animation = br.ReadInt16();
                Type = br.ReadByte();
                Flags = br.ReadByte();
                FromFrame = br.ReadInt16();
                ToFrame = br.ReadInt16();
            }
            public override byte[] GenerateBytes()
            {
                List<byte> Data = new List<byte>();

                USF4Utils.AddIntAsBytes(Data, Animation, false);
                Data.Add(Type);
                Data.Add(Flags);
                USF4Utils.AddIntAsBytes(Data, FromFrame, false);
                USF4Utils.AddIntAsBytes(Data, ToFrame, false);

                return Data.ToArray();
            }
        }
        public class TransitionCommand : CommandDatablock //COMMANDTYPE 0x02
        {
            public int Flag1, Flag2; //both short
            public float
                Float1_0x04, Float2_0x08, Float3_0x0C, //0x10
                Float4_0x10, Float5_0x14, Float6_0x18;

            public override void ReadCommandDataBlock(BinaryReader br)
            {
                Flag1 = br.ReadInt16();
                Flag2 = br.ReadInt16();
                Float1_0x04 = br.ReadSingle();
                Float2_0x08 = br.ReadSingle();
                Float3_0x0C = br.ReadSingle();
                Float4_0x10 = br.ReadSingle();
                Float5_0x14 = br.ReadSingle();
                Float6_0x18 = br.ReadSingle();
            }
            public override byte[] GenerateBytes()
            {
                List<byte> Data = new List<byte>();

                USF4Utils.AddIntAsBytes(Data, Flag1, false);
                USF4Utils.AddIntAsBytes(Data, Flag2, false);
                USF4Utils.AddFloatAsBytes(Data, Float1_0x04);
                USF4Utils.AddFloatAsBytes(Data, Float2_0x08);
                USF4Utils.AddFloatAsBytes(Data, Float3_0x0C);
                USF4Utils.AddFloatAsBytes(Data, Float4_0x10);
                USF4Utils.AddFloatAsBytes(Data, Float5_0x14);
                USF4Utils.AddFloatAsBytes(Data, Float6_0x18);
                

                return Data.ToArray();
            }
        }
        public class StateCommand : CommandDatablock //COMMANDTYPE 0x03
        {
            public int Flags, UnkFlags2;

            public override void ReadCommandDataBlock(BinaryReader br)
            {
                Flags = br.ReadInt32();
                UnkFlags2 = br.ReadInt32();
            }
            public override byte[] GenerateBytes()
            {
                List<byte> Data = new List<byte>();

                USF4Utils.AddIntAsBytes(Data, Flags, true);
                USF4Utils.AddIntAsBytes(Data, UnkFlags2, true);

                return Data.ToArray();
            }
        }
        public class SpeedCommand : CommandDatablock //COMMANDTYPE 0x04
        {
            public float Multiplier;

            public override void ReadCommandDataBlock(BinaryReader br)
            {
                Multiplier = br.ReadSingle();
            }
            public override byte[] GenerateBytes()
            {
                List<byte> Data = new List<byte>();

                USF4Utils.AddFloatAsBytes(Data, Multiplier);

                return Data.ToArray();
            }
        }
        public class PhysicsCommand : CommandDatablock //COMMANDTYPE 0x05
        {
            public float VelX, VelY;
            public int UnkLong2_0x08, PhysicsFlags;
            public float AccX, AccY;
            public int UnkLong6_0x18, UnkLong7_0x1C;

            public override void ReadCommandDataBlock(BinaryReader br)
            {
                VelX = br.ReadSingle();
                VelY = br.ReadSingle();
                UnkLong2_0x08 = br.ReadInt32();
                PhysicsFlags = br.ReadInt32();
                AccX = br.ReadSingle();
                AccY = br.ReadSingle();
                UnkLong6_0x18 = br.ReadInt32();
                UnkLong7_0x1C = br.ReadInt32();
            }
            public override byte[] GenerateBytes()
            {
                List<byte> Data = new List<byte>();

                USF4Utils.AddFloatAsBytes(Data, VelX);
                USF4Utils.AddFloatAsBytes(Data, VelY);
                USF4Utils.AddIntAsBytes(Data, UnkLong2_0x08, true);
                USF4Utils.AddIntAsBytes(Data, PhysicsFlags, true);
                USF4Utils.AddFloatAsBytes(Data, AccX);
                USF4Utils.AddFloatAsBytes(Data, AccY);
                USF4Utils.AddIntAsBytes(Data, UnkLong6_0x18, true);
                USF4Utils.AddIntAsBytes(Data, UnkLong7_0x1C, true);

                return Data.ToArray();
            }
        }
        public class CancelCommand : CommandDatablock //COMMANDTYPE 0x06
        {
            public int Condition, Cancel; //both long

            public override void ReadCommandDataBlock(BinaryReader br)
            {
                Condition = br.ReadInt32();
                Cancel = br.ReadInt32();
            }
            public override byte[] GenerateBytes()
            {
                List<byte> Data = new List<byte>();

                USF4Utils.AddIntAsBytes(Data, Condition, true);
                USF4Utils.AddIntAsBytes(Data, Cancel, true);

                return Data.ToArray();
            }
        }
        public class HitboxCommand : CommandDatablock  //COMMANDTYPE 0x07 //Length 0x2C
        {
            public float
                X, Y, Rotation, Width, //0x10
                Height, UnkFloat5_0x14;

            public byte ID, Juggle, Type, HitLevel;
            public int HitFlags; //short
            public byte UnkByte11_0x1E, UnkByte12_0x1F,
                Hits, JugglePotential, JuggleIncrement, JuggleIncrementLimit,
                HitboxEffect, UnkByte17_0x25;
            public int UnkShort18_0x26, HitboxData; //Long

            public override void ReadCommandDataBlock(BinaryReader br)
            {
                X = br.ReadSingle();
                Y = br.ReadSingle();
                Rotation = br.ReadSingle();
                Width = br.ReadSingle();
                //0x10
                Height = br.ReadSingle();//0x10-0x13
                UnkFloat5_0x14 = br.ReadSingle();//0x14-0x17
                ID = br.ReadByte();//0x18
                Juggle = br.ReadByte();//0x19
                Type = br.ReadByte();//0x1A
                HitLevel = br.ReadByte();//0x1B
                HitFlags = br.ReadInt16();//0x1C-0x1D
                UnkByte11_0x1E = br.ReadByte();//0x1E
                UnkByte12_0x1F = br.ReadByte();//0x1F
                //0x20
                Hits = br.ReadByte();//0x20
                JugglePotential = br.ReadByte();//0x21
                JuggleIncrement = br.ReadByte();//0x22
                JuggleIncrementLimit = br.ReadByte();//0x23
                HitboxEffect = br.ReadByte();//0x24
                UnkByte17_0x25 = br.ReadByte();//0x25
                UnkShort18_0x26 = br.ReadInt16();//0x26-0x27
                HitboxData = br.ReadInt32();//0x28-0x2B
            }
            public override byte[] GenerateBytes()
            {
                List<byte> Data = new List<byte>();

                USF4Utils.AddFloatAsBytes(Data, X);
                USF4Utils.AddFloatAsBytes(Data, Y);
                USF4Utils.AddFloatAsBytes(Data, Rotation);
                USF4Utils.AddFloatAsBytes(Data, Width);
                USF4Utils.AddFloatAsBytes(Data, Height);
                USF4Utils.AddFloatAsBytes(Data, UnkFloat5_0x14);
                Data.Add(ID);
                Data.Add(Juggle);
                Data.Add(Type);
                Data.Add(HitLevel);
                USF4Utils.AddIntAsBytes(Data, HitFlags, false);
                Data.Add(UnkByte11_0x1E);
                Data.Add(UnkByte12_0x1F);
                Data.Add(Hits);
                Data.Add(JugglePotential);
                Data.Add(JuggleIncrement);
                Data.Add(JuggleIncrementLimit);
                Data.Add(HitboxEffect);
                Data.Add(UnkByte17_0x25);
                USF4Utils.AddIntAsBytes(Data, UnkShort18_0x26, false);
                USF4Utils.AddIntAsBytes(Data, HitboxData, true);
                
                return Data.ToArray();
            }
        }
        public class InvincCommand : CommandDatablock //COMMANDTYPE 0x08
        {
            public int
                Flags, Location, //both long
                UnkShort2_0x08, UnkShort3_0x0A, UnkShort4_0x0C, UnkShort5_0x0E;

            public override void ReadCommandDataBlock(BinaryReader br)
            {
                Flags = br.ReadInt32();
                Location = br.ReadInt32();
                UnkShort2_0x08 = br.ReadInt16();
                UnkShort3_0x0A = br.ReadInt16();
                UnkShort4_0x0C = br.ReadInt16();
                UnkShort5_0x0E = br.ReadInt16();
            }
            public override byte[] GenerateBytes()
            {
                List<byte> Data = new List<byte>();

                USF4Utils.AddIntAsBytes(Data, Flags, true);
                USF4Utils.AddIntAsBytes(Data, Location, true);
                USF4Utils.AddIntAsBytes(Data, UnkShort2_0x08, false);
                USF4Utils.AddIntAsBytes(Data, UnkShort3_0x0A, false);
                USF4Utils.AddIntAsBytes(Data, UnkShort4_0x0C, false);
                USF4Utils.AddIntAsBytes(Data, UnkShort5_0x0E, false);

                return Data.ToArray();
            }

        }
        public class HurtboxCommand : CommandDatablock //COMMANDTYPE 0x09
        {
            public float
                X, Y, Rotation, Width, //0x10
                Height, UnkFloat5_0x14;
            public int BoxType, Location; //both long

            public override void ReadCommandDataBlock(BinaryReader br)
            {
                X = br.ReadSingle();
                Y = br.ReadSingle();
                Rotation = br.ReadSingle();
                Width = br.ReadSingle();
                //0x10
                Height = br.ReadSingle();
                UnkFloat5_0x14 = br.ReadSingle();
                BoxType = br.ReadInt32();
                Location = br.ReadInt32();
            }
            public override byte[] GenerateBytes()
            {
                List<byte> Data = new List<byte>();

                USF4Utils.AddFloatAsBytes(Data, X);
                USF4Utils.AddFloatAsBytes(Data, Y);
                USF4Utils.AddFloatAsBytes(Data, Rotation);
                USF4Utils.AddFloatAsBytes(Data, Width);
                USF4Utils.AddFloatAsBytes(Data, Height);
                USF4Utils.AddFloatAsBytes(Data, UnkFloat5_0x14);
                USF4Utils.AddIntAsBytes(Data, BoxType, true);
                USF4Utils.AddIntAsBytes(Data, Location, true);

                return Data.ToArray();
            }
        }
        public class ETCCommand : CommandDatablock //COMMANDTYPE 0x0A
        {
            public int Type, ShortParam, //short
                UnkLong2_0x04, UnkLong3_0x08, UnkLong4_0x0C, UnkLong5_0x10,
                UnkLong6_0x14, UnkLong7_0x18, UnkLong8_0x1C;

            public override void ReadCommandDataBlock(BinaryReader br)
            {
                Type = br.ReadInt16();
                ShortParam = br.ReadInt16();
                UnkLong2_0x04 = br.ReadInt32();
                UnkLong3_0x08 = br.ReadInt32();
                UnkLong4_0x0C = br.ReadInt32();
                UnkLong5_0x10 = br.ReadInt32();
                UnkLong6_0x14 = br.ReadInt32();
                UnkLong7_0x18 = br.ReadInt32();
                UnkLong8_0x1C = br.ReadInt32();
            }
            public override byte[] GenerateBytes()
            {
                List<byte> Data = new List<byte>();

                USF4Utils.AddIntAsBytes(Data, Type, false);
                USF4Utils.AddIntAsBytes(Data, ShortParam, false);
                USF4Utils.AddIntAsBytes(Data, UnkLong2_0x04, true);
                USF4Utils.AddIntAsBytes(Data, UnkLong3_0x08, true);
                USF4Utils.AddIntAsBytes(Data, UnkLong4_0x0C, true);
                USF4Utils.AddIntAsBytes(Data, UnkLong5_0x10, true);
                USF4Utils.AddIntAsBytes(Data, UnkLong6_0x14, true);
                USF4Utils.AddIntAsBytes(Data, UnkLong7_0x18, true);
                USF4Utils.AddIntAsBytes(Data, UnkLong8_0x1C, true);

                return Data.ToArray();
            }
        }
        public class TargetlockCommand : CommandDatablock //COMMANDTYPE 0x0B
        {
            public int Type, Script, UnkLong2_0x08, UnkLong3_0x0C; //all long

            public override void ReadCommandDataBlock(BinaryReader br)
            {
                Type = br.ReadInt32();
                Script = br.ReadInt32();
                UnkLong2_0x08 = br.ReadInt32();
                UnkLong3_0x0C = br.ReadInt32();
            }
            public override byte[] GenerateBytes()
            {
                List<byte> Data = new List<byte>();

                USF4Utils.AddIntAsBytes(Data, Type, true);
                USF4Utils.AddIntAsBytes(Data, Script, true);
                USF4Utils.AddIntAsBytes(Data, UnkLong2_0x08, true);
                USF4Utils.AddIntAsBytes(Data, UnkLong3_0x0C, true);

                return Data.ToArray();
            }
        }
        public class SFXCommand : CommandDatablock //COMMANDTYPE 0x0C
        {
            public int Type, Sound, //Short 
                UnkLong2_0x04, UnkLong3_0x08, UnkLong4_0x0C;

            public override void ReadCommandDataBlock(BinaryReader br)
            {
                Type = br.ReadInt16();
                Sound = br.ReadInt16();
                UnkLong2_0x04 = br.ReadInt32();
                UnkLong3_0x08 = br.ReadInt32();
                UnkLong4_0x0C = br.ReadInt32();
            }
            public override byte[] GenerateBytes()
            {
                List<byte> Data = new List<byte>();

                USF4Utils.AddIntAsBytes(Data, Type, false);
                USF4Utils.AddIntAsBytes(Data, Sound, false);
                USF4Utils.AddIntAsBytes(Data, UnkLong2_0x04, true);
                USF4Utils.AddIntAsBytes(Data, UnkLong3_0x08, true);
                USF4Utils.AddIntAsBytes(Data, UnkLong4_0x0C, true);

                return Data.ToArray();
            }
        }
    }
}
