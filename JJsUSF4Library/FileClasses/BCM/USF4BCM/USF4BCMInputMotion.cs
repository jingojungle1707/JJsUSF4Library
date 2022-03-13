using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.ScriptClasses
{
    class USF4BCMInputMotion
    {
        public string Name;

        public List<int>
                Type, //Long
                Buffer, //Short
                Input, //Short
                MoveFlags, //Short
                Flags, //Long
                Requirement; //Short


        public USF4BCMInputMotion(BinaryReader br, string name, int offset = 0)
        {
            Name = name;

            Type = new List<int>();
            Buffer = new List<int>();
            Input = new List<int>();
            MoveFlags = new List<int>();
            Flags = new List<int>();
            Requirement = new List<int>();

            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            int inputCount = br.ReadInt32();
            
            for (int i = 0; i < inputCount; i++)
            {
                Type.Add(br.ReadInt16());
                Buffer.Add(br.ReadInt16());
                Input.Add(br.ReadInt16());
                MoveFlags.Add(br.ReadInt16());
                Flags.Add(br.ReadInt16());
                Requirement.Add(br.ReadInt16());
            }

        }
    }
}
