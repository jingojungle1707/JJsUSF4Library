using JJsUSF4Library.FileClasses.SubfileClasses;
using System.Collections.Generic;
using System.IO;

namespace JJsUSF4Library.FileClasses
{
    public partial class USF4Script
    {
        public string Name;

        public List<Command> Commands;

        //0x00
        public int
            FirstHitboxFrame, //one of these is short, one is long?
            LastHitboxFrame,
            TotalDuration, //Short
            PhysicsFlags, //Long
            OperationFlags, //Long
        //0x10
            FinishAnimation; //Short

        public USF4Script()
        {

        }

        public USF4Script(BinaryReader br, string name, int offset = 0)
        {
            Name = name;
            ReadFromStream(br, offset);
        }
        public void ReadFromStream(BinaryReader br, int offset = 0)
        {
            Commands = new List<Command>();

            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            FirstHitboxFrame = br.ReadInt16();
            LastHitboxFrame = br.ReadInt32();
            TotalDuration = br.ReadInt16();
            PhysicsFlags = br.ReadInt32();
            OperationFlags = br.ReadInt32();
            //0x10
            FinishAnimation = br.ReadInt16();
            int commandCount = br.ReadInt16();
            int commandIndexPointer = br.ReadInt32();

            br.BaseStream.Seek(commandIndexPointer + offset, SeekOrigin.Begin);
            for (int i = 0; i < commandCount; i++)
            {
                Commands.Add(new Command(br, offset + commandIndexPointer + i * 0x0C));
            }
        }
        public byte[] GenerateBytes()
        {
            List<byte> Data = new List<byte>();

            USF4Utils.AddIntAsBytes(Data, FirstHitboxFrame, false);
            USF4Utils.AddIntAsBytes(Data, LastHitboxFrame, true);
            USF4Utils.AddIntAsBytes(Data, TotalDuration, false);
            USF4Utils.AddIntAsBytes(Data, PhysicsFlags, true);
            USF4Utils.AddIntAsBytes(Data, OperationFlags, true);
            USF4Utils.AddIntAsBytes(Data, FinishAnimation, false);
            USF4Utils.AddIntAsBytes(Data, Commands.Count, false);
            int commandIndexPointer = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            USF4Utils.UpdateIntAtPosition(Data, commandIndexPointer, Data.Count);
            //Slightly unusual because we need to update the tick pointer at commandHeader + 4 and the datablock pointer at commandHeader + 8
            List<int> commandHeaderPositions = new List<int>();
            for (int i = 0; i < Commands.Count; i++)
            {
                commandHeaderPositions.Add(Data.Count);
                Data.AddRange(Commands[i].GenerateHeaderBytes());
            }
            for (int i = 0; i < Commands.Count; i++)
            {
                //tick pointer is relative to start of the command index entry
                int tickPointer = Data.Count - commandHeaderPositions[i];
                USF4Utils.UpdateIntAtPosition(Data, commandHeaderPositions[i] + 0x04 , tickPointer);
                Data.AddRange(Commands[i].GenerateTickBytes());
                //Same for datablock pointer
                int datablockPointer = Data.Count - commandHeaderPositions[i];
                USF4Utils.UpdateIntAtPosition(Data, commandHeaderPositions[i] + 0x08, datablockPointer);
                Data.AddRange(Commands[i].GenerateDatablockBytes());
            }

            return Data.ToArray();
        }
    }
}