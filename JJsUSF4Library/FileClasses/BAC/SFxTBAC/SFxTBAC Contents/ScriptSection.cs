using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace JJsUSF4Library.FileClasses.ScriptClasses
{
    public class ScriptSection
    {
        public Command.COMMANDTYPE Type;
        public byte
            UnkByte1_0x01;

        public List<Command> Commands;

        public ScriptSection()
        {

        }

        public ScriptSection(BinaryReader br, int offset = 0)
        {
            Commands = new List<Command>();

            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            Type = (Command.COMMANDTYPE)br.ReadByte();
            UnkByte1_0x01 = br.ReadByte();
            int commandCount = br.ReadInt16();
            int commandTicksPointer = br.ReadInt32();
            int commandDatasPointer = br.ReadInt32();

            //Read start and end tick data
            List<int> startTicks = new List<int>();
            List<int> endTicks = new List<int>();
            br.BaseStream.Seek(offset + commandTicksPointer, SeekOrigin.Begin);
            for (int i = 0; i < commandCount; i++)
            {
                startTicks.Add(br.ReadInt16());
                endTicks.Add(br.ReadInt16());
            }
            //Read command data
            br.BaseStream.Seek(offset + commandDatasPointer, SeekOrigin.Begin);
            for (int i = 0; i < commandCount; i++)
            {
                Commands.Add(new Command(br, Type, startTicks[i], endTicks[i]));
            }
        }

        public ScriptSection(byte[] Data)
        {
            Type = (Command.COMMANDTYPE)Data[0];
            UnkByte1_0x01 = Data[1];
            int commandCount = USF4Utils.ReadInt(false, 0x02, Data);
            int commandHeadersPointer = USF4Utils.ReadInt(true, 0x04, Data);
            int commandDatasPointer = USF4Utils.ReadInt(true, 0x08, Data);

            Commands = new List<Command>();

            for (int i = 0; i < commandCount; i++)
            {
                Commands.Add(new Command(
                    Data.Slice(commandDatasPointer + i * Command.CommandLengths[Type], 0),
                    Type,
                    USF4Utils.ReadInt(false, commandHeadersPointer + i * 0x04, Data),
                    USF4Utils.ReadInt(false, commandHeadersPointer + 0x02 + i * 0x04, Data)
                ));
            }
        }

        public byte[] GenerateScriptSectionBytes()
        {
            List<byte> Data = new List<byte>();

            for (int i = 0; i < Commands.Count; i++)
            {
                Data.AddRange(Commands[i].CommandData.GenerateDataBlockBytes());
            }
            //Check for types that contain params
            if (new Command.COMMANDTYPE[] { 
                Command.COMMANDTYPE.Flow, 
                Command.COMMANDTYPE.ETC, 
                Command.COMMANDTYPE.Unk6, 
                Command.COMMANDTYPE.SFX, 
                Command.COMMANDTYPE.GFX }
                .Contains(Type))
            {
                int commandLength = (Type == Command.COMMANDTYPE.Flow) ? 0x0C : 0x08;
                int commandOffset = (Type == Command.COMMANDTYPE.Flow) ? 0x08 : 0x04;

                for (int i = 0; i < Commands.Count; i++)
                {
                    if (Commands[i].CommandData.Params != null && Commands[i].CommandData.Params.Count > 0)
                    {
                        USF4Utils.UpdateIntAtPosition(Data, i * commandLength + commandOffset, Data.Count - i * commandLength);
                        Data.AddRange(Commands[i].CommandData.GenerateParamBytes());
                    }
                }
            }

            return Data.ToArray();
        }

        public class Command
        {
            public enum COMMANDTYPE
            {
                UNKNOWN = 0xFF,
                Flow = 0x00,
                Speed = 0x01,
                Status = 0x02,
                Physics = 0x03,
                Cancel = 0x04,
                ETC = 0x05,
                Unk6 = 0x06,
                Hitbox = 0x07,
                Hurtbox = 0x08,
                Pushbox = 0x09,
                Animation = 0x0A,
                AnimationMod = 0x0B,
                SFX = 0x0C,
                GFX = 0x0D,
            }
            public enum AnimationFile
            {
                COMMON = 0x00,
                UNK1 = 0x01, //Could be CMN.cam.ema from resource\battle\chara\CMN? Appears in unused cody ex knifethrow?
                OBJ = 0x02,
                CAM = 0x03,
                FCE = 0x04,
                INTRO = 0x05,
                APPEAL = 0x06,
                UNK7 = 0x07,
                UNK8 = 0x08,
                UNK9 = 0x09,
                UNKA = 0x0A
            }
            public enum SoundFile
            {
                COMMON = 0x00,
                UNK1 = 0x01,
                CV = 0x02,
                UNK3 = 0x03,
                UNK4 = 0x04,
                UNK5 = 0x05,
                UNK6 = 0x06,
                UNK7 = 0x07,
                UNK8 = 0x08,
                UNK9 = 0x09,
                UNKA = 0x0A
            }

            public static Dictionary<COMMANDTYPE, int> CommandLengths = new Dictionary<COMMANDTYPE, int>()
            {
                { COMMANDTYPE.Flow, 0x0C},
                { COMMANDTYPE.Speed, 0x04},
                { COMMANDTYPE.Status, 0x08},
                { COMMANDTYPE.Physics, 0x0C},
                { COMMANDTYPE.Cancel, 0x04},
                { COMMANDTYPE.ETC, 0x08},
                { COMMANDTYPE.Unk6, 0x08},
                { COMMANDTYPE.Hitbox, 0x30},
                { COMMANDTYPE.Hurtbox, 0x2C},
                { COMMANDTYPE.Pushbox, 0x18},
                { COMMANDTYPE.Animation, 0x08},
                { COMMANDTYPE.AnimationMod, 0x08},
                { COMMANDTYPE.SFX, 0x08},
                { COMMANDTYPE.GFX, 0x08}
            };

            [XmlElement]
            public CommandDataBlock CommandData;

            public Command()
            {

            }

            [XmlInclude(typeof(CommandDataBlock.FlowCommand))]
            [XmlInclude(typeof(CommandDataBlock.SpeedCommand))]
            [XmlInclude(typeof(CommandDataBlock.StatusCommand))]
            [XmlInclude(typeof(CommandDataBlock.PhysicsCommand))]
            [XmlInclude(typeof(CommandDataBlock.CancelCommand))]
            [XmlInclude(typeof(CommandDataBlock.ETCCommand))]
            [XmlInclude(typeof(CommandDataBlock.Unk6Command))]
            [XmlInclude(typeof(CommandDataBlock.HitboxCommand))]
            [XmlInclude(typeof(CommandDataBlock.HurtboxCommand))]
            [XmlInclude(typeof(CommandDataBlock.PushboxCommand))]
            [XmlInclude(typeof(CommandDataBlock.AnimationCommand))]
            [XmlInclude(typeof(CommandDataBlock.AnimationModCommand))]
            [XmlInclude(typeof(CommandDataBlock.SFXCommand))]
            [XmlInclude(typeof(CommandDataBlock.GFXCommand))]
            public static CommandDataBlock FetchCommandDataBlockType(COMMANDTYPE type)
            {
                return type switch
                {
                    COMMANDTYPE.Flow => new CommandDataBlock.FlowCommand(),
                    COMMANDTYPE.Speed => new CommandDataBlock.SpeedCommand(),
                    COMMANDTYPE.Status => new CommandDataBlock.StatusCommand(),
                    COMMANDTYPE.Physics => new CommandDataBlock.PhysicsCommand(),
                    COMMANDTYPE.Cancel => new CommandDataBlock.CancelCommand(),
                    COMMANDTYPE.ETC => new CommandDataBlock.ETCCommand(),
                    COMMANDTYPE.Unk6 => new CommandDataBlock.Unk6Command(),
                    COMMANDTYPE.Hitbox => new CommandDataBlock.HitboxCommand(),
                    COMMANDTYPE.Hurtbox => new CommandDataBlock.HurtboxCommand(),
                    COMMANDTYPE.Pushbox => new CommandDataBlock.PushboxCommand(),
                    COMMANDTYPE.Animation => new CommandDataBlock.AnimationCommand(),
                    COMMANDTYPE.AnimationMod => new CommandDataBlock.AnimationModCommand(),
                    COMMANDTYPE.SFX => new CommandDataBlock.SFXCommand(),
                    COMMANDTYPE.GFX => new CommandDataBlock.GFXCommand(),
                    COMMANDTYPE.UNKNOWN => new CommandDataBlock(),
                    _ => new CommandDataBlock(),
                };
            }

            public Command(BinaryReader br, COMMANDTYPE type, int startTick, int endTick)
            {
                CommandData = FetchCommandDataBlockType(type);
                CommandData.ReadCommandDataBlock(br, startTick, endTick);
            }

            public Command(byte[] Data, COMMANDTYPE type, int startTick, int endTick)
            {
                switch (type)
                {
                    case COMMANDTYPE.Flow:
                        {
                            CommandData = new CommandDataBlock.FlowCommand(Data, startTick, endTick);
                            break;
                        }
                    case COMMANDTYPE.Speed:
                        {
                            CommandData = new CommandDataBlock.SpeedCommand(Data, startTick, endTick);
                            break;
                        }
                    case COMMANDTYPE.Status:
                        {
                            CommandData = new CommandDataBlock.StatusCommand(Data, startTick, endTick);
                            break;
                        }
                    case COMMANDTYPE.Physics:
                        {
                            CommandData = new CommandDataBlock.PhysicsCommand(Data, startTick, endTick);
                            break;
                        }
                    case COMMANDTYPE.Cancel:
                        {
                            CommandData = new CommandDataBlock.CancelCommand(Data, startTick, endTick);
                            break;
                        }
                    case COMMANDTYPE.ETC:
                        {
                            CommandData = new CommandDataBlock.ETCCommand(Data, startTick, endTick);
                            break;
                        }
                    case COMMANDTYPE.Unk6:
                        {
                            CommandData = new CommandDataBlock.Unk6Command(Data, startTick, endTick);
                            break;
                        }
                    case COMMANDTYPE.Hitbox:
                        {
                            CommandData = new CommandDataBlock.HitboxCommand(Data, startTick, endTick);
                            break;
                        }
                    case COMMANDTYPE.Hurtbox:
                        {
                            CommandData = new CommandDataBlock.HurtboxCommand(Data, startTick, endTick);
                            break;
                        }
                    case COMMANDTYPE.Pushbox:
                        {
                            CommandData = new CommandDataBlock.PushboxCommand(Data, startTick, endTick);
                            break;
                        }
                    case COMMANDTYPE.Animation:
                        {
                            CommandData = new CommandDataBlock.AnimationCommand(Data, startTick, endTick);
                            break;
                        }
                    case COMMANDTYPE.AnimationMod:
                        {
                            CommandData = new CommandDataBlock.AnimationModCommand(Data, startTick, endTick);
                            break;
                        }
                    case COMMANDTYPE.SFX:
                        {
                            CommandData = new CommandDataBlock.SFXCommand(Data, startTick, endTick);
                            break;
                        }
                    case COMMANDTYPE.GFX:
                        {
                            CommandData = new CommandDataBlock.GFXCommand(Data, startTick, endTick);
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("UNKNOWN COMMAND TYPE");
                            break;
                        }
                }
            }

            public class CommandDataBlock
            {
                public int StartTick { get; set; }
                public int EndTick {get; set; }

                public List<int> Params { get; set; }

                public virtual void ReadCommandDataBlock(BinaryReader br, int startTick, int endTick)
                {

                }

                public virtual void ReadParamData(BinaryReader br, int paramsCount, int globalPointer)
                {
                    int startOffset = (int)br.BaseStream.Position;
                    Params = new List<int>();
                    br.BaseStream.Seek(globalPointer, SeekOrigin.Begin);
                    for (int i = 0; i < paramsCount; i++)
                    {
                        Params.Add(br.ReadInt32());
                    }
                    br.BaseStream.Seek(startOffset, SeekOrigin.Begin);
                }

                public virtual byte[] GenerateDataBlockBytes()
                {
                    List<byte> Data = new List<byte>();
                    return Data.ToArray();
                }
                public virtual byte[] GenerateParamBytes()
                {
                    List<byte> Data = new List<byte>();

                    if (Params != null && Params.Count > 0)
                    {
                        for (int i = 0; i < Params.Count; i++) USF4Utils.AddIntAsBytes(Data, Params[i], true);
                    }

                    return Data.ToArray();
                }

                public class FlowCommand : CommandDataBlock
                {
                    public FlowType Type;
                    public int
                        UnkByte1_0x01,
                        UnkByte3_0x03,
                        Script,
                        UnkShort4_0x06;
                    public FlowCommand() { }
                    public override void ReadCommandDataBlock(BinaryReader br, int startTick, int endTick)
                    {
                        int startOffset = (int)br.BaseStream.Position;
                        StartTick = startTick;
                        EndTick = endTick;
                        Type = (FlowType)br.ReadByte();

                        if (Type.ToString().Contains("UNK")) 
                        {

                        }
                        UnkByte1_0x01 = br.ReadByte();
                        int paramsCount = br.ReadByte();
                        UnkByte3_0x03 = br.ReadByte();
                        Script = br.ReadInt16();
                        UnkShort4_0x06 = br.ReadInt16();
                        int paramsPointer = br.ReadInt32();

                        //Read params
                        if (paramsCount > 0) ReadParamData(br, paramsCount, paramsPointer + startOffset);
                        
                    }
                    public FlowCommand(byte[] Data, int startTick, int endTick)
                    {
                        Params = new List<int>();

                        StartTick = startTick;
                        EndTick = endTick;

                        Type = (FlowType)Data[0];
                        UnkByte1_0x01 = Data[1];
                        int paramsCount = Data[2];
                        UnkByte3_0x03 = Data[3];
                        Script = USF4Utils.ReadInt(false, 0x04, Data);
                        UnkShort4_0x06 = USF4Utils.ReadInt(false, 0x06, Data);
                        int paramsPointer = USF4Utils.ReadInt(true, 0x0A, Data);

                        for (int i = 0; i < paramsCount; i++)
                        {
                            Params.Add(USF4Utils.ReadInt(true, paramsPointer + i * 0x04, Data));
                        }
                    }

                    public override byte[] GenerateDataBlockBytes()
                    {
                        List<byte> Data = new List<byte>();

                        Data.Add((byte)Type); //1
                        Data.Add((byte)UnkByte1_0x01); //2
                        Data.Add((byte)(Params == null ? 0 : Params.Count)); //3
                        Data.Add((byte)UnkByte3_0x03); //4
                        USF4Utils.AddIntAsBytes(Data, Script, false); //5-6
                        USF4Utils.AddIntAsBytes(Data, UnkShort4_0x06, false); //7-8
                        USF4Utils.AddIntAsBytes(Data, 0, true); //9-A-B-C Params pointer, need to overwrite it later

                        return Data.ToArray();
                    }

                    public enum FlowType
                    {
                        ALWAYS = 0x00,
                        UNK0x01 = 0x01,
                        HIT = 0x02,
                        UNK0x03 = 0x03, //UNK
                        UNK0x04 = 0x04, //UNK
                        HIT2 = 0x05,
                        UNK0x06 = 0x06,
                        UNK0x07 = 0x07,
                        UNK0x08 = 0x08,
                        LAND = 0x09,
                        WALL = 0x0A,
                        UNK0x0B = 0x0B,
                        INPUT = 0x0C,
                        UNK0x0D = 0x0D,
                        UNK0x0E = 0x0E,
                        UNK0x0F = 0x0F,
                        UNK0x10 = 0x10 //CDY KNIFE_PICK_UP
                    }
                }
                public class SpeedCommand : CommandDataBlock
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
                public class StatusCommand : CommandDataBlock
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
                public class PhysicsCommand : CommandDataBlock
                {
                    public float Force;
                    public int
                        Flags,
                        UnkShort2_0x06,
                        UnkShort3_0x08,
                        UnkShort4_0x0A;

                    public PhysicsCommand() { }
                    public PhysicsCommand(byte[] Data, int startTick, int endTick)
                    {
                        StartTick = startTick;
                        EndTick = endTick;

                        Force = USF4Utils.ReadFloat(0x00, Data);
                        Flags = USF4Utils.ReadInt(false, 0x04, Data);
                        UnkShort2_0x06 = USF4Utils.ReadInt(false, 0x06, Data);
                        UnkShort3_0x08 = USF4Utils.ReadInt(false, 0x08, Data);
                        UnkShort4_0x0A = USF4Utils.ReadInt(false, 0x0A, Data);
                    }
                    public override void ReadCommandDataBlock(BinaryReader br, int startTick, int endTick)
                    {
                        StartTick = startTick;
                        EndTick = endTick;
                        Force = br.ReadSingle();
                        Flags = br.ReadInt16();
                        UnkShort2_0x06 = br.ReadInt16();
                        UnkShort3_0x08 = br.ReadInt16();
                        UnkShort4_0x0A = br.ReadInt16();
                    }

                    public override byte[] GenerateDataBlockBytes()
                    {
                        List<byte> Data = new List<byte>();

                        USF4Utils.AddFloatAsBytes(Data, Force);
                        USF4Utils.AddIntAsBytes(Data, Flags, false);
                        USF4Utils.AddIntAsBytes(Data, UnkShort2_0x06, false);
                        USF4Utils.AddIntAsBytes(Data, UnkShort3_0x08, false);
                        USF4Utils.AddIntAsBytes(Data, UnkShort4_0x0A, false);

                        return Data.ToArray();
                    }
                }
                public class CancelCommand : CommandDataBlock
                {
                    [Flags]
                    public enum Conditions
                    {
                        ON_HIT = 0x01,
                        ON_BLOCK = 0x02,
                        UNK0x04 = 0x04,
                        END = 0x08
                    }

                    public int
                        CancelList;
                    public Conditions Condition;

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
                public class ETCCommand : CommandDataBlock
                {
                    public byte Type;
                    public int UnkShort1_0x01;

                    public ETCCommand() { }
                    public ETCCommand(byte[] Data, int startTick, int endTick)
                    {
                        Params = new List<int>();

                        StartTick = startTick;
                        EndTick = endTick;
                        Type = Data[0x00];
                        UnkShort1_0x01 = USF4Utils.ReadInt(false, 0x01, Data);
                        int paramsCount = Data[0x03];
                        int paramsPointer = USF4Utils.ReadInt(true, 0x04, Data);

                        for (int i = 0; i < paramsCount; i++)
                        {
                            Params.Add(USF4Utils.ReadInt(true, paramsPointer + i * 0x04, Data));
                        }
                    }
                    public override void ReadCommandDataBlock(BinaryReader br, int startTick, int endTick)
                    {
                        int startOffset = (int)br.BaseStream.Position;
                        StartTick = startTick;
                        EndTick = endTick;
                        Type = br.ReadByte();
                        UnkShort1_0x01 = br.ReadInt16();
                        int paramsCount = br.ReadByte();
                        int paramsPointer = br.ReadInt32();

                        if (paramsCount > 0) ReadParamData(br, paramsCount, paramsPointer + startOffset);
                    }
                    public override byte[] GenerateDataBlockBytes()
                    {
                        List<byte> Data = new List<byte>();
                        Data.Add(Type);
                        USF4Utils.AddIntAsBytes(Data, UnkShort1_0x01, false);
                        Data.Add((byte)(Params == null ? 0 : Params.Count));
                        USF4Utils.AddIntAsBytes(Data, 0, true); //Params pointer, need to overwrite it later

                        return Data.ToArray();
                    }

                }
                public class Unk6Command : CommandDataBlock
                {
                    public byte UnkByte0_0x00;
                    public int UnkShort1_0x01;

                    public Unk6Command() { }
                    public Unk6Command(byte[] Data, int startTick, int endTick)
                    {
                        Params = new List<int>();

                        StartTick = startTick;
                        EndTick = endTick;
                        UnkByte0_0x00 = Data[0x00];
                        UnkShort1_0x01 = USF4Utils.ReadInt(false, 0x01, Data);
                        int paramsCount = Data[0x03];
                        int paramsPointer = USF4Utils.ReadInt(true, 0x04, Data);

                        for (int i = 0; i < paramsCount; i++)
                        {
                            Params.Add(USF4Utils.ReadInt(true, paramsPointer + i * 0x04, Data));
                        }
                    }
                    public override void ReadCommandDataBlock(BinaryReader br, int startTick, int endTick)
                    {
                        int startOffset = (int)br.BaseStream.Position;
                        StartTick = startTick;
                        EndTick = endTick;
                        UnkByte0_0x00 = br.ReadByte();
                        UnkShort1_0x01 = br.ReadInt16();
                        int paramsCount = br.ReadByte();
                        int paramsPointer = br.ReadInt32();

                        if (paramsCount > 0) ReadParamData(br, paramsCount, paramsPointer + startOffset);
                    }
                    public override byte[] GenerateDataBlockBytes()
                    {
                        List<byte> Data = new List<byte>();
                        Data.Add(UnkByte0_0x00);
                        USF4Utils.AddIntAsBytes(Data, UnkShort1_0x01, false);
                        Data.Add((byte)(Params == null ? 0 : Params.Count));
                        USF4Utils.AddIntAsBytes(Data, 0, true); //Params pointer, need to overwrite it later

                        return Data.ToArray();
                    }
                }
                public class HitboxCommand : CommandDataBlock
                {
                    public float
                        X, Y, Width, Height;
                    //0x10
                    public int
                        UnkShort4_0x10,
                        UnkShort5_0x12,
                        UnkLong6_0x14;
                    public byte
                        ID,
                        Properties,
                        Type,
                        HitLevel,
                        UnkByte11_0x1C,
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

                    public HitboxCommand() { }
                    public HitboxCommand(byte[] Data, int startTick, int endTick)
                    {
                        StartTick = startTick;
                        EndTick = endTick;
                        X = USF4Utils.ReadFloat(0x00, Data);
                        Y = USF4Utils.ReadFloat(0x04, Data);
                        Width = USF4Utils.ReadFloat(0x08, Data);
                        Height = USF4Utils.ReadFloat(0x0C, Data);
                        UnkShort4_0x10 = USF4Utils.ReadInt(false, 0x10, Data);
                        UnkShort5_0x12 = USF4Utils.ReadInt(false, 0x12, Data);
                        UnkLong6_0x14 = USF4Utils.ReadInt(true, 0x14, Data);
                        ID = Data[0x18];
                        Properties = Data[0x19];
                        Type = Data[0x1A];
                        HitLevel = Data[0x1B];
                        UnkByte11_0x1C = Data[0x1C];
                        Juggle = Data[0x1D];
                        JuggleAdd = Data[0x1E];
                        UnkByte14_0x1F = Data[0x1F];
                        UnkByte15_0x20 = Data[0x20];
                        UnkByte16_0x21 = Data[0x21];
                        UnkByte17_0x22 = Data[0x22];
                        UnkByte18_0x23 = Data[0x23];
                        HitEffect = USF4Utils.ReadInt(false, 0x24, Data);
                        UnkByte20_0x26 = Data[0x26];
                        UnkByte21_0x27 = Data[0x27];
                        UnkFloat22_0x28 = USF4Utils.ReadFloat(0x28, Data);
                        UnkFloat23_0x2C = USF4Utils.ReadFloat(0x2C, Data);
                    }
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
                        UnkLong6_0x14 = br.ReadInt32();
                        ID = br.ReadByte();
                        Properties = br.ReadByte();
                        Type = br.ReadByte();
                        HitLevel = br.ReadByte();
                        UnkByte11_0x1C = br.ReadByte();
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
                        USF4Utils.AddIntAsBytes(Data, UnkLong6_0x14, true);
                        Data.Add(ID);
                        Data.Add(Properties);
                        Data.Add(Type);
                        Data.Add(HitLevel);
                        Data.Add(UnkByte11_0x1C);
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
                public class HurtboxCommand : CommandDataBlock
                {
                    public float
                        X, Y, Width, Height;
                    public int
                        UnkShort4_0x10,
                        UnkShort5_0x12,
                        Flags, //Short
                        UnkShort7_0x16,
                        Counter; //Short
                    public byte
                        UnkByte9_0x1A,
                        UnkByte10_0x1B;
                    public int Vul; //Short
                    public byte Armour;
                    public int UnkShort13_0x1F; //Short
                    public byte
                        UnkByte14_0x21,
                        UnkByte15_0x22,
                        UnkByte16_0x23,
                        UnkByte17_0x24,
                        UnkByte18_0x25;
                    public int UnkShort19_0x26;
                    public float UnkFloat20_0x28;

                    public HurtboxCommand() { }
                    public HurtboxCommand(byte[] Data, int startTick, int endTick)
                    {
                        StartTick = startTick;
                        EndTick = endTick;
                        X = USF4Utils.ReadFloat(0x00, Data);
                        Y = USF4Utils.ReadFloat(0x04, Data);
                        Width = USF4Utils.ReadFloat(0x08, Data);
                        Height = USF4Utils.ReadFloat(0x0C, Data);
                        //0x10
                        UnkShort4_0x10 = USF4Utils.ReadInt(false, 0x10, Data);
                        UnkShort5_0x12 = USF4Utils.ReadInt(false, 0x12, Data);
                        Flags = USF4Utils.ReadInt(false, 0x14, Data);
                        UnkShort7_0x16 = USF4Utils.ReadInt(false, 0x16, Data);
                        Counter = USF4Utils.ReadInt(false, 0x18, Data);
                        UnkByte9_0x1A = Data[0x1A];
                        UnkByte10_0x1B = Data[0x1B];
                        Vul = USF4Utils.ReadInt(false, 0x1C, Data);
                        Armour = Data[0x1E];
                        UnkShort13_0x1F = USF4Utils.ReadInt(false, 0x1F, Data);
                        UnkByte14_0x21 = Data[0x21];
                        UnkByte15_0x22 = Data[0x22];
                        UnkByte16_0x23 = Data[0x23];
                        UnkByte17_0x24 = Data[0x24];
                        UnkByte18_0x25 = Data[0x25];
                        UnkShort19_0x26 = USF4Utils.ReadInt(false, 0x26, Data);
                        UnkFloat20_0x28 = USF4Utils.ReadFloat(0x28, Data);
                    }
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
                        Flags = br.ReadInt16();
                        UnkShort7_0x16 = br.ReadInt16();
                        Counter = br.ReadInt16();
                        UnkByte9_0x1A = br.ReadByte();
                        UnkByte10_0x1B = br.ReadByte();
                        Vul = br.ReadInt16();
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
                        USF4Utils.AddIntAsBytes(Data, Vul, false);
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
                public class PushboxCommand : CommandDataBlock
                {
                    public float
                        X, Y, Width, Height;
                    public int
                        UnkShort4_0x10,
                        UnkShort5_0x12,
                        UnkShort6_0x14,
                        UnkShort7_0x16;

                    public PushboxCommand() { }
                    public PushboxCommand(byte[] Data, int startTick, int endTick)
                    {
                        StartTick = startTick;
                        EndTick = endTick;
                        X = USF4Utils.ReadFloat(0x00, Data);
                        Y = USF4Utils.ReadFloat(0x04, Data);
                        Width = USF4Utils.ReadFloat(0x08, Data);
                        Height = USF4Utils.ReadFloat(0x0C, Data);
                        //0x10
                        UnkShort4_0x10 = USF4Utils.ReadInt(false, 0x10, Data);
                        UnkShort5_0x12 = USF4Utils.ReadInt(false, 0x12, Data);
                        UnkShort6_0x14 = USF4Utils.ReadInt(false, 0x14, Data);
                        UnkShort7_0x16 = USF4Utils.ReadInt(false, 0x16, Data);
                    }
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
                        UnkShort6_0x14 = br.ReadInt16();
                        UnkShort7_0x16 = br.ReadInt16();
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
                        USF4Utils.AddIntAsBytes(Data, UnkShort6_0x14, false);
                        USF4Utils.AddIntAsBytes(Data, UnkShort7_0x16, false);

                        return Data.ToArray();
                    }
                }
                public class AnimationCommand : CommandDataBlock
                {
                    public int
                        ID; //Short
                    public AnimationFile File;
                    public byte
                        Flags;
                    public int
                        Start, //Short
                        End; //Short

                    public AnimationCommand() { }
                    public AnimationCommand(byte[] Data, int startTick, int endTick)
                    {
                        StartTick = startTick;
                        EndTick = endTick;
                        ID = USF4Utils.ReadInt(false, 0x00, Data);
                        File = (AnimationFile)Data[0x02];
                        Flags = Data[0x03];
                        Start = USF4Utils.ReadInt(false, 0x04, Data);
                        End = USF4Utils.ReadInt(false, 0x06, Data);
                    }
                    public override void ReadCommandDataBlock(BinaryReader br, int startTick, int endTick)
                    {
                        StartTick = startTick;
                        EndTick = endTick;
                        ID = br.ReadInt16();
                        File = (AnimationFile)br.ReadByte();
                        Flags = br.ReadByte();
                        Start = br.ReadInt16();
                        End = br.ReadInt16();
                    }
                    public override byte[] GenerateDataBlockBytes()
                    {
                        List<byte> Data = new List<byte>();

                        USF4Utils.AddIntAsBytes(Data, ID, false);
                        Data.Add((byte)File);
                        Data.Add(Flags);
                        USF4Utils.AddIntAsBytes(Data, Start, false);
                        USF4Utils.AddIntAsBytes(Data, End, false);

                        return Data.ToArray();
                    }
                }
                public class AnimationModCommand : CommandDataBlock
                {
                    public int
                        UnkShort0_0x00,
                        UnkShort1_0x02;
                    public float UnkFloat2_0x04;

                    public AnimationModCommand() { }
                    public AnimationModCommand(byte[] Data, int startTick, int endTick)
                    {
                        StartTick = startTick;
                        EndTick = endTick;
                        UnkShort0_0x00 = USF4Utils.ReadInt(false, 0x00, Data);
                        UnkShort1_0x02 = USF4Utils.ReadInt(false, 0x02, Data);
                        UnkFloat2_0x04 = USF4Utils.ReadFloat(0x04, Data);
                    }
                    public override void ReadCommandDataBlock(BinaryReader br, int startTick, int endTick)
                    {
                        StartTick = startTick;
                        EndTick = endTick;
                        UnkShort0_0x00 = br.ReadInt16();
                        UnkShort1_0x02 = br.ReadInt16();
                        UnkFloat2_0x04 = br.ReadSingle();
                    }
                    public override byte[] GenerateDataBlockBytes()
                    {
                        List<byte> Data = new List<byte>();

                        USF4Utils.AddIntAsBytes(Data, UnkShort0_0x00, false);
                        USF4Utils.AddIntAsBytes(Data, UnkShort1_0x02, false);
                        USF4Utils.AddFloatAsBytes(Data, UnkFloat2_0x04);

                        return Data.ToArray();
                    }
                }
                public class SFXCommand : CommandDataBlock
                {
                    public int
                        ID, //short
                        File; //byte

                    public SFXCommand() { }
                    public SFXCommand(byte[] Data, int startTick, int endTick)
                    {
                        Params = new List<int>();

                        StartTick = startTick;
                        EndTick = endTick;
                        ID = USF4Utils.ReadInt(false, 0x00, Data);
                        File = Data[2];
                        int paramsCount = Data[3];
                        int paramsPointer = USF4Utils.ReadInt(true, 0x04, Data);

                        for (int i = 0; i < paramsCount; i++)
                        {
                            Params.Add(USF4Utils.ReadInt(true, paramsPointer + i * 0x04, Data));
                        }
                    }
                    public override void ReadCommandDataBlock(BinaryReader br, int startTick, int endTick)
                    {
                        int startOffset = (int)br.BaseStream.Position;
                        StartTick = startTick;
                        EndTick = endTick;
                        ID = br.ReadInt16();
                        File = br.ReadByte();
                        int paramsCount = br.ReadByte();
                        int paramsPointer = br.ReadInt32();

                        if (paramsCount > 0) ReadParamData(br, paramsCount, paramsPointer + startOffset);
                    }
                    public override byte[] GenerateDataBlockBytes()
                    {
                        List<byte> Data = new List<byte>();

                        USF4Utils.AddIntAsBytes(Data, ID, false);
                        Data.Add((byte)File);
                        Data.Add((byte)(Params == null ? 0 : Params.Count));
                        USF4Utils.AddIntAsBytes(Data, 0, true); //params pointer

                        return Data.ToArray();
                    }
                }
                public class GFXCommand : CommandDataBlock
                {
                    public int ID; //Short
                    public byte File;

                    public GFXCommand() { }
                    public GFXCommand(byte[] Data, int startTick, int endTick)
                    {
                        Params = new List<int>();

                        StartTick = startTick;
                        EndTick = endTick;
                        ID = USF4Utils.ReadInt(false, 0x00, Data);
                        File = Data[0x02];
                        int paramsCount = Data[0x03];
                        int paramsPointer = USF4Utils.ReadInt(true, 0x04, Data);

                        for (int i = 0; i < paramsCount; i++)
                        {
                            Params.Add(USF4Utils.ReadInt(true, paramsPointer + i * 0x04, Data));
                        }
                    }
                    public override void ReadCommandDataBlock(BinaryReader br, int startTick, int endTick)
                    {
                        int startOffset = (int)br.BaseStream.Position;
                        StartTick = startTick;
                        EndTick = endTick;
                        ID = br.ReadInt16();
                        File = br.ReadByte();
                        int paramsCount = br.ReadByte();
                        int paramsPointer = br.ReadInt32();

                        if (paramsCount > 0) ReadParamData(br, paramsCount, paramsPointer + startOffset);
                    }
                    public override byte[] GenerateDataBlockBytes()
                    {
                        List<byte> Data = new List<byte>();

                        USF4Utils.AddIntAsBytes(Data, ID, false);
                        Data.Add(File);
                        Data.Add((byte)(Params == null ? 0 : Params.Count));
                        USF4Utils.AddIntAsBytes(Data, 0, true); //Params pointer, need to overwrite it later

                        return Data.ToArray();
                    }
                }
            }
        }
    }
}