using JJsUSF4Library.FileClasses.ScriptClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JJsUSF4Library.FileClasses.BACClasses.SFxTBACClasses
{
    public class ScriptSection
    {
        public CommandBase.COMMANDTYPE Type;
        public byte
            UnkByte1_0x01;

        public List<CommandBase> Commands;

        public ScriptSection()
        {

        }

        public ScriptSection(BinaryReader br, int offset = 0)
        {
            Commands = new List<CommandBase>();

            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            Type = (CommandBase.COMMANDTYPE)br.ReadByte();
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
                //Commands.Add(new Command(br, Type, startTicks[i], endTicks[i]));
                Commands.Add(CommandFactory.ReadCommandBlock(br, Type, startTicks[i], endTicks[i]));
            }
        }

        public byte[] GenerateScriptSectionBytes()
        {
            List<byte> Data = new List<byte>();

            for (int i = 0; i < Commands.Count; i++)
            {
                Data.AddRange(Commands[i].GenerateDataBlockBytes());
            }

            int commandLength = (Type == CommandBase.COMMANDTYPE.Flow) ? 0x0C : 0x08;
            int commandOffset = (Type == CommandBase.COMMANDTYPE.Flow) ? 0x08 : 0x04;

            for (int i = 0; i < Commands.Count; i++)
            {
                CommandHasParamsBase paramCommand = Commands[i] as CommandHasParamsBase;

                if (paramCommand != null && paramCommand.Params.Count > 0)
                {
                    USF4Utils.UpdateIntAtPosition(Data, i * commandLength + commandOffset, Data.Count - i * commandLength);
                    Data.AddRange(paramCommand.GenerateParamBytes());
                }
            }
            
            return Data.ToArray();
        }
    }
}