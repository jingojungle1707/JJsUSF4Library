using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.ScriptClasses
{
    class USF4BCMCancel
    {
        public string Name;

        public List<string> CancelsInto;

        public USF4BCMCancel(BinaryReader br, string name, List<string> moveNames, int offset = 0)
        {
            CancelsInto = new List<string>();

            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            Name = name;

            int cancellableMoveCount = br.ReadInt32();
            int cancellableMovesPointer = br.ReadInt32();

            br.BaseStream.Seek(offset + cancellableMovesPointer, SeekOrigin.Begin);

            for (int i = 0; i < cancellableMoveCount; i++) CancelsInto.Add(moveNames[br.ReadInt16()]);   
        }
    }
}
