using System.Collections.Generic;
using System.IO;

namespace JJsUSF4Library.FileClasses.ScriptClasses
{
    public class SFxTBCMInputMotion
    {
        public string Name;

        public List<InputDetail> InputDetails;

        public SFxTBCMInputMotion()
        {

        }
        public SFxTBCMInputMotion(BinaryReader br, string name, int offset = 0)
        {
            Name = name;
            InputDetails = new List<InputDetail>();

            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            List<int> inputDetailPointers = new List<int>()
            {
                br.ReadInt32(),
                br.ReadInt32(),
                br.ReadInt32(),
                br.ReadInt32()
            };

            for (int i = 0; i < inputDetailPointers.Count; i++)
                InputDetails.Add(new InputDetail(br, offset + inputDetailPointers[i]));
        }
        public SFxTBCMInputMotion(byte[] Data, string name)
        {
            Name = name;

            InputDetails = new List<InputDetail>();
            List<int> inputDetailPointers = new List<int>()
            {
                USF4Utils.ReadInt(true, 0x00,Data),
                USF4Utils.ReadInt(true, 0x04,Data),
                USF4Utils.ReadInt(true, 0x08,Data),
                USF4Utils.ReadInt(true, 0x0C,Data),
            };

            for (int i = 0; i < inputDetailPointers.Count; i++)
                InputDetails.Add(new InputDetail(Data.Slice(inputDetailPointers[i], 0x104)));
        }

        public class InputDetail
        {
            public int
                InputCount; //Short

            //Each input is length 0x10
            public List<int>
                Type, //Long
                Buffer, //Short
                Input, //Short
                MoveFlags, //Short
                Flags, //Long
                Requirement; //Short

            public InputDetail()
            {

            }
            public InputDetail(BinaryReader br, int offset = 0)
            {
                InputCount = br.ReadInt16();

                Type = new List<int>();
                Buffer = new List<int>();
                Input = new List<int>();
                MoveFlags = new List<int>();
                Flags = new List<int>();
                Requirement = new List<int>();

                for (int i = 0; i < InputCount; i++)
                {
                    Type.Add(br.ReadInt32());
                    Buffer.Add(br.ReadInt16());
                    Input.Add(br.ReadInt16());
                    MoveFlags.Add(br.ReadInt16());
                    Flags.Add(br.ReadInt32());
                    Requirement.Add(br.ReadInt16());
                }
            }
            public InputDetail(byte[] Data)
            {
                InputCount = USF4Utils.ReadInt(false, 0x00, Data);

                Type = new List<int>();
                Buffer = new List<int>();
                Input = new List<int>();
                MoveFlags = new List<int>();
                Flags = new List<int>();
                Requirement = new List<int>();

                for (int i = 0; i < InputCount; i++)
                {
                    Type.Add(USF4Utils.ReadInt(true, 0x02 + i * 0x10, Data));
                    Buffer.Add(USF4Utils.ReadInt(false, 0x06 + i * 0x10, Data));
                    Input.Add(USF4Utils.ReadInt(false, 0x08 + i * 0x10, Data));
                    MoveFlags.Add(USF4Utils.ReadInt(false, 0x0A + i * 0x10, Data));
                    Flags.Add(USF4Utils.ReadInt(true, 0x0C + i * 0x10, Data));
                    Requirement.Add(USF4Utils.ReadInt(false, 0x10 + i * 0x10, Data));
                }
            }
        }
    }
}
