using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
    }
}