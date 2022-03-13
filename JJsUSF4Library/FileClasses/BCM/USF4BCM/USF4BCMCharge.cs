using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.ScriptClasses
{
    class USF4BCMCharge
    {
        public string Name;
        public int
            Input,
            UnkShort1_0x02,
            MoveFlags,
            Frames,
            UnkShort4_0x08,
            UnkShort5_0x0A,
            StorageIndex; //long

        public USF4BCMCharge(BinaryReader br, string name)
        {
            Name = name;

            Input = br.ReadInt16();
            UnkShort1_0x02 = br.ReadInt16();
            MoveFlags = br.ReadInt16();
            Frames = br.ReadInt16();
            UnkShort4_0x08 = br.ReadInt16();
            UnkShort5_0x0A = br.ReadInt16();
            StorageIndex = br.ReadInt32();
        }
    }
}
