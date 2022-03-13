using System;
using System.Collections.Generic;
using System.IO;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class Command
    {
        public List<CommandDatablock> Datablocks;

        public COMMANDTYPE
            Type; //short
        //Pointers both count from start of command header

        public enum COMMANDTYPE
        {
            UNKNOWN = 0xFF,
            Flow = 0x00,
            Animation = 0x01,
            Transition = 0x02,
            State = 0x03,
            Speed = 0x04,
            Physics = 0x05,
            Cancels = 0x06,
            Hitbox = 0x07,
            Invinc = 0x08,
            Hurtbox = 0x09,
            ETC = 0x0A,
            Targetlock = 0x0B,
            SFX = 0x0C,
        }

        public Command()
        {

        }

        public Command(BinaryReader br, int offset = 0)
        {
            Datablocks = new List<CommandDatablock>();

            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            Type = (COMMANDTYPE)br.ReadInt16();
            int datablockCount = br.ReadInt16();
            int timingPointer = br.ReadInt32();
            int datablockPointer = br.ReadInt32();

            //Read start and end timings to pass to the commanddatablocks
            List<int> startTicks = new List<int>();
            List<int> endTicks = new List<int>();
            br.BaseStream.Seek(offset + timingPointer, SeekOrigin.Begin);
            for (int i = 0; i < datablockCount; i++)
            {
                startTicks.Add(br.ReadInt16());
                endTicks.Add(br.ReadInt16());
            }
            //Read datablocks
            br.BaseStream.Seek(offset + datablockPointer, SeekOrigin.Begin);
            for (int i = 0; i < datablockCount; i++)
            {
                CommandDatablock cdb = FetchDataBlock(Type);
                cdb.StartTick = startTicks[i];
                cdb.EndTick = endTicks[i];
                cdb.ReadCommandDataBlock(br);

                Datablocks.Add(cdb);
            }
        }

        public byte[] GenerateHeaderBytes()
        {
            List<byte> Data = new List<byte>();

            USF4Utils.AddIntAsBytes(Data, (int)Type, false);
            USF4Utils.AddIntAsBytes(Data, Datablocks.Count, false);
            USF4Utils.AddIntAsBytes(Data, -1, true);
            USF4Utils.AddIntAsBytes(Data, -1, true);
            return Data.ToArray();
        }
        public byte[] GenerateTickBytes()
        {
            List<byte> Data = new List<byte>();

            for (int i = 0; i < Datablocks.Count; i++)
            {
                USF4Utils.AddIntAsBytes(Data, Datablocks[i].StartTick, false);
                USF4Utils.AddIntAsBytes(Data, Datablocks[i].EndTick, false);
            }

            return Data.ToArray();
        }
        public byte[] GenerateDatablockBytes()
        {
            List<byte> Data = new List<byte>();

            for (int i = 0; i < Datablocks.Count; i++) Data.AddRange(Datablocks[i].GenerateBytes());

            return Data.ToArray();
        }



        public CommandDatablock FetchDataBlock(COMMANDTYPE type)
        {
            switch (type)
            {
                case COMMANDTYPE.Flow: return new CommandDatablock.FlowCommand();
                case COMMANDTYPE.Animation: return new CommandDatablock.AnimationCommand();
                case COMMANDTYPE.Transition: return new CommandDatablock.TransitionCommand();
                case COMMANDTYPE.State: return new CommandDatablock.StateCommand();
                case COMMANDTYPE.Speed: return new CommandDatablock.SpeedCommand();
                case COMMANDTYPE.Physics: return new CommandDatablock.PhysicsCommand();
                case COMMANDTYPE.Cancels: return new CommandDatablock.CancelCommand();
                case COMMANDTYPE.Hitbox: return new CommandDatablock.HitboxCommand();
                case COMMANDTYPE.Invinc: return new CommandDatablock.InvincCommand();
                case COMMANDTYPE.Hurtbox: return new CommandDatablock.HurtboxCommand();
                case COMMANDTYPE.ETC: return new CommandDatablock.ETCCommand();
                case COMMANDTYPE.Targetlock: return new CommandDatablock.TargetlockCommand();
                case COMMANDTYPE.SFX: return new CommandDatablock.SFXCommand();
                default: return new CommandDatablock();
            }
        }
    }
}