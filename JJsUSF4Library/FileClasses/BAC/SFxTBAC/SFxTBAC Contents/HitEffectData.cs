using System.Collections.Generic;
using System.IO;

namespace JJsUSF4Library.FileClasses.ScriptClasses
{
    public class HitEffectData
    {
        public string Name;
        public int
            Damage,
            Effect, //Effect & script might actually be the same value, just listed in the JSON twice for clarity?
            Script,
            AHitstop,
            AShaking,
            VHitstop,
            VShaking,
            //0x10
            UnkLong7_0x10,
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

        public HitEffectData()
        {

        }

        public HitEffectData(BinaryReader br, int offset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            OffsetCommands = new List<OffsetCommand>();
            HitEffectParams = new List<HitEffectParam>();

            Damage = br.ReadInt32();
            Effect = br.ReadInt16();
            Script = br.ReadInt16();
            AHitstop = br.ReadInt16();
            AShaking = br.ReadInt16();
            VHitstop = br.ReadInt16();
            VShaking = br.ReadInt16();
            //0x10
            UnkLong7_0x10 = br.ReadInt32();
            HitStun = br.ReadInt16();
            HitStun2 = br.ReadInt16();
            UnkShort10_0x18 = br.ReadInt16();
            int offsetCommandCount = br.ReadByte();
            int hitEffectParamCount = br.ReadByte();
            AMeter = br.ReadInt16();
            VMeter = br.ReadInt16();
            //0x20
            ForceX = br.ReadSingle();
            ForceY = br.ReadSingle();
            int offsetCommandPointer = br.ReadInt32();
            int hitEffectParamPointer = br.ReadInt32();

            br.BaseStream.Seek(offset + offsetCommandPointer, SeekOrigin.Begin);
            for (int i = 0; i < offsetCommandCount; i++)
            {
                //Each offset command is length 0x08
                OffsetCommands.Add(new OffsetCommand()
                {
                    UnkShort0_0x00 = br.ReadInt16(),
                    UnkByte1_0x02 = br.ReadByte(),
                    ParamCount = br.ReadByte(),
                    ParamPointer = br.ReadInt32(),
                    Params = new List<int>()
                });

                //Populate offsetCommand parameters
                br.BaseStream.Seek(offset + offsetCommandPointer + OffsetCommands[i].ParamPointer, SeekOrigin.Begin);
                for (int j = 0; j < OffsetCommands[i].ParamCount; j++)
                {
                    OffsetCommands[i].Params.Add(br.ReadInt32());
                }
            }
            br.BaseStream.Seek(offset + hitEffectParamPointer, SeekOrigin.Begin);
            for (int i = 0; i < hitEffectParamCount; i++)
            {
                HitEffectParams.Add(new HitEffectParam()
                {
                    UnkShort0_0x00 = br.ReadInt16(),
                    UnkShort1_0x02 = br.ReadInt16(),
                    UnkShort2_0x04 = br.ReadInt16(),
                    UnkShort3_0x06 = br.ReadInt16(),
                });
            }
        }

        public HitEffectData(byte[] Data)
        {
            OffsetCommands = new List<OffsetCommand>();
            HitEffectParams = new List<HitEffectParam>();

            Damage = USF4Utils.ReadInt(true, 0x00, Data);
            Effect = USF4Utils.ReadInt(false, 0x04, Data);
            Script = USF4Utils.ReadInt(false, 0x06, Data);
            AHitstop = USF4Utils.ReadInt(false, 0x08, Data);
            AShaking = USF4Utils.ReadInt(false, 0x0A, Data);
            VHitstop = USF4Utils.ReadInt(false, 0x0C, Data);
            VShaking = USF4Utils.ReadInt(false, 0x0E, Data);
            //0x10
            UnkLong7_0x10 = USF4Utils.ReadInt(true, 0x10, Data);
            HitStun = USF4Utils.ReadInt(false, 0x14, Data);
            HitStun2 = USF4Utils.ReadInt(false, 0x16, Data);
            UnkShort10_0x18 = USF4Utils.ReadInt(false, 0x18, Data);
            int offsetCommandCount = Data[0x1A];
            int hitEffectParamCount = Data[0x1B];
            AMeter = USF4Utils.ReadInt(false, 0x1C, Data);
            VMeter = USF4Utils.ReadInt(false, 0x1E, Data);
            //0x20
            ForceX = USF4Utils.ReadFloat(0x20, Data);
            ForceY = USF4Utils.ReadFloat(0x24, Data);
            int offsetCommandPointer = USF4Utils.ReadInt(true, 0x28, Data);
            int hitEffectParamPointer = USF4Utils.ReadInt(true, 0x2C, Data);

            for (int i = 0; i < offsetCommandCount; i++)
            {
                //Each offset command is length 0x08
                OffsetCommand offsetCommand = new OffsetCommand()
                {
                    UnkShort0_0x00 = USF4Utils.ReadInt(false, offsetCommandPointer + i * 0x08, Data),
                    UnkByte1_0x02 = Data[offsetCommandPointer + 2 + i * 0x08],
                    ParamCount = Data[offsetCommandPointer + 3 + i * 0x08],
                    ParamPointer = USF4Utils.ReadInt(true, offsetCommandPointer + 0x04 + i * 0x08, Data),
                    Params = new List<int>()
                };
                //Populate parameters
                for (int j = 0; j < offsetCommand.ParamCount; j++) offsetCommand.Params.Add(USF4Utils.ReadInt(true, offsetCommandPointer + i * 0x08 + offsetCommand.ParamPointer + j * 0x04, Data));
                //Push offsetCommand to list
                OffsetCommands.Add(offsetCommand);
            }
            for (int i = 0; i < hitEffectParamCount; i++)
            {
                HitEffectParams.Add(new HitEffectParam()
                {
                    UnkShort0_0x00 = USF4Utils.ReadInt(false, hitEffectParamPointer + 0x00 + i * 0x08, Data),
                    UnkShort1_0x02 = USF4Utils.ReadInt(false, hitEffectParamPointer + 0x02 + i * 0x08, Data),
                    UnkShort2_0x04 = USF4Utils.ReadInt(false, hitEffectParamPointer + 0x04 + i * 0x08, Data),
                    UnkShort3_0x06 = USF4Utils.ReadInt(false, hitEffectParamPointer + 0x06 + i * 0x08, Data),
                });
            }
        }        

        public class OffsetCommand
        {
            public int
                UnkShort0_0x00;
            public byte
                UnkByte1_0x02,
                ParamCount;
            public int
                ParamPointer;
            public List<int> Params;
        }
        public class HitEffectParam
        {
            public int
                UnkShort0_0x00,
                UnkShort1_0x02,
                UnkShort2_0x04,
                UnkShort3_0x06;
        }
    }
}
