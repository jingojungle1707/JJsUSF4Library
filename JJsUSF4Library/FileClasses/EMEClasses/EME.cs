using JJsUSF4Library.FileClasses.EMEClasses.Emitters;
using System;
using System.Collections.Generic;
using System.IO;

namespace JJsUSF4Library.FileClasses.EMEClasses
{
    public class EME : USF4File
    {
        public List<Effect> Effects { get; set; } = new List<Effect>();

        public EME()
        {
            
        }
        public EME(BinaryReader br, string name, int offset = 0)
        {
            Name = name;
            ReadFromStream(br, offset);
        }

        public override void ReadFromStream(BinaryReader br, int offset = 0, int fileLength = 0)
        {
            br.BaseStream.Seek(offset + 0x08, SeekOrigin.Begin);

            int effectCount = br.ReadInt32();
            int nextEffectPointer = br.ReadInt32();

            while (nextEffectPointer != 0)
            {
                Effects.Add(new Effect(br, out nextEffectPointer, offset + nextEffectPointer, offset));
            }
        }
    }
}