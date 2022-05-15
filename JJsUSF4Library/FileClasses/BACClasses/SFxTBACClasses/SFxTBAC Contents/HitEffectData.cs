using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace JJsUSF4Library.FileClasses.ScriptClasses
{
    public class HitEffectData
    {
        public HitEffectType Type;
        public int Damage;
        public EffectType Effect;
        public int
            Script,
            AHitstop,
            AShaking,
            VHitstop,
            VShaking;
        //0x10
        public HitEffectFlag
            Flags;

        public int
            HitStun, //0x14
            HitStun2,
            UnkShort10_0x18;
        //public byte
            //OffsetCommandCount, //0x1A
            //HitEffectParamCount; //0x1B
        public int
            AMeter, //0x1C
            VMeter;
        //0x20
        public float
            ForceX,
            ForceY;
        //public int
            //OffsetCommandPointer, //From start of HitEffectData
            //HitEffectParamPointer;
        public List<OffsetCommand> OffsetCommands;
        public List<HitEffectParam> HitEffectParams;

        public enum HitEffectType
        {
            STAND_HIT = 0x00,
            CROUCH_HIT = 0x01,
            PREJUMP_HIT = 0x02,
            AIR_HIT = 0x03,
            POSTBOUND_HIT = 0x04,
            STAND_GUARD = 0x05,
            CROUCH_GUARD = 0x06,
            PREJUMP_GUARD = 0x07,
            AIR_GUARD = 0x08,
            POSTBOUND_GUARD = 0x09,
            STAND_COUNTERHIT = 0x0A,
            CROUCH_COUNTERHIT = 0x0B,
            PREJUMP_COUNTERHIT = 0x0C,
            AIR_COUNTERHIT = 0x0D,
            POSTBOUND_COUNTERHIT = 0x0E,
            UNUSED_1 = 0x0F,
            UNUSED_2 = 0x10,
            UNUSED_3 = 0x11,
            UNUSED_4 = 0x12,
            UNUSED_5 = 0x13,
        }

        public enum EffectType
        {
            UNK = -0x01,
            NONE = 0x00,
            HIT = 0x01,
            BLOCK = 0x02,
            BLOW = 0x03,
            BOUND = 0x04,
        }

        [Flags]
        public enum HitEffectFlag
        {
            UNKOTHER = -1,
            NONE = 0x0000,
            USE_LOCAL_SCRIPT = 0x0001,
            DONT_KO = 0x0002,
            UNK0x0004 = 0x0004, //No turn?
            HARD_KNOCKDOWN = 0x0008,
            UNK0x0010 = 0x0010,
            UNK0x0020 = 0x0020,
            UNK0x0040 = 0x0040,
            UNK0x0080 = 0x0080,
            UNK0x0100 = 0x0100,
            UNK0x0200 = 0x0200,
            UNK0x0400 = 0x0400,
            UNK0x0800 = 0x0800,
            UNK0x1000 = 0x1000,
            UNK0x2000 = 0x2000,
            UNK0x4000 = 0x4000,
            UNK0x8000 = 0x8000,
        }

        public HitEffectData()
        {

        }

        public HitEffectData(BinaryReader br, HitEffectType type, int offset = 0)
        {
            Type = type;

            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            OffsetCommands = new List<OffsetCommand>();
            HitEffectParams = new List<HitEffectParam>();

            Damage = br.ReadInt32();
            Effect = (EffectType)br.ReadInt16();

            if (Effect == EffectType.UNK)
            {

            }

            Script = br.ReadInt16();

            if (Script != 0)
            {

            }

            AHitstop = br.ReadInt16();
            AShaking = br.ReadInt16();
            VHitstop = br.ReadInt16();
            VShaking = br.ReadInt16();
            //0x10
            Flags = (HitEffectFlag)br.ReadInt32();

            HitStun = br.ReadInt16();
            HitStun2 = br.ReadInt16();
            UnkShort10_0x18 = br.ReadInt16();
//#if DEBUG
//            Debug.WriteLine($"Type: {Type}; EffectType: {Effect}; ScriptID: {Script}; Flags: {Flags}; Short0x18: {UnkShort10_0x18}");
//#endif

            int offsetCommandCount = br.ReadByte();
            int hitEffectParamCount = br.ReadByte();
            AMeter = br.ReadInt16();
            VMeter = br.ReadInt16();
            //0x20
            ForceX = br.ReadSingle();
            ForceY = br.ReadSingle();
            int offsetCommandPointer = br.ReadInt32();
            int hitEffectParamPointer = br.ReadInt32();

            for (int i = 0; i < offsetCommandCount; i++)
            {
                //Each offset command is length 0x08
                OffsetCommands.Add(new OffsetCommand(br, offset + offsetCommandPointer + i * 0x08));
            }
            for (int i = 0; i < hitEffectParamCount; i++)
            {
                HitEffectParams.Add(new HitEffectParam(br, offset + hitEffectParamPointer + i * 0x08));
            }
        }

        public class OffsetCommand
        {
            public int
                UnkShort0_0x00;
            public byte
                UnkByte1_0x02;
            public List<int> Params;

            public OffsetCommand()
            {
                Params = new List<int>();
            }
            public OffsetCommand(BinaryReader br, int offset = 0)
            {
                br.BaseStream.Seek(offset, SeekOrigin.Begin);

                UnkShort0_0x00 = br.ReadInt16();
                UnkByte1_0x02 = br.ReadByte();
                int paramCount = br.ReadByte();
                int paramPointer = br.ReadInt32();

                Params = new List<int>();
                br.BaseStream.Seek(offset + paramPointer, SeekOrigin.Begin);
                for (int i = 0; i < paramCount; i++) Params.Add(br.ReadInt32());
            }

        }
        public class HitEffectParam
        {
            public int
                UnkShort0_0x00,
                UnkByte1_0x02;

            public List<int> Params;

            public HitEffectParam()
            {
                Params = new List<int>();
            }

            public HitEffectParam(BinaryReader br, int offset = 0)
            {
                Params = new List<int>();
                br.BaseStream.Seek(offset, SeekOrigin.Begin);

                UnkShort0_0x00 = br.ReadInt16();
                UnkByte1_0x02 = br.ReadByte();
                int paramsCount = br.ReadByte();
                int paramsPointer = br.ReadInt32();

                br.BaseStream.Seek(paramsPointer + offset, SeekOrigin.Begin);
                for (int i = 0; i < paramsCount; i++) Params.Add(br.ReadInt32());
            }
        }
    }
}
