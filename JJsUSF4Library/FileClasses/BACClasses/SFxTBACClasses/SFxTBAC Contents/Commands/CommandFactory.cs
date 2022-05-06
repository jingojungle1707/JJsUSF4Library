using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BACClasses.SFxTBACClasses
{
    internal static class CommandFactory
    {
        public static CommandBase ReadCommandBlock(BinaryReader br, CommandBase.COMMANDTYPE type, int startTick, int endTick)
        {
            CommandBase command = FetchCommandDataBlockType(type);

            command.ReadCommandDataBlock(br, startTick, endTick);
            
            return command;
        }
        public static CommandBase FetchCommandDataBlockType(CommandBase.COMMANDTYPE type)
        {
            return type switch
            {
                CommandBase.COMMANDTYPE.Flow => new FlowCommand(),
                CommandBase.COMMANDTYPE.Speed => new SpeedCommand(),
                CommandBase.COMMANDTYPE.Status => new StatusCommand(),
                CommandBase.COMMANDTYPE.Physics => new PhysicsCommand(),
                CommandBase.COMMANDTYPE.Cancel => new CancelCommand(),
                CommandBase.COMMANDTYPE.ETC => new ETCCommand(),
                CommandBase.COMMANDTYPE.Unk6 => new Unk6Command(),
                CommandBase.COMMANDTYPE.Hitbox => new HitboxCommand(),
                CommandBase.COMMANDTYPE.Hurtbox => new HurtboxCommand(),
                CommandBase.COMMANDTYPE.Pushbox => new PushboxCommand(),
                CommandBase.COMMANDTYPE.Animation => new AnimationCommand(),
                CommandBase.COMMANDTYPE.AnimationMod => new AnimationModCommand(),
                CommandBase.COMMANDTYPE.SFX => new SFXCommand(),
                CommandBase.COMMANDTYPE.GFX => new GFXCommand(),
                CommandBase.COMMANDTYPE.UNKNOWN => throw new ArgumentException(),
                _ => throw new ArgumentException(),
            };
        }
    }
}
