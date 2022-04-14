using JJsUSF4Library.FileClasses.EMEClasses.Emitters;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JJsUSF4Library.FileClasses.EMEClasses
{
    public class Effect
    {
        public byte UnkByte0x00 { get; set; }
        public byte UnkByte0x01 { get; set; }
        public byte UnkByte0x02 { get; set; }
        public byte UnkByte0x03 { get; set; }
        public byte UnkByte0x04 { get; set; } // = 0x01
        public byte UnkByte0x05 { get; set; }
        public byte UnkByte0x06 { get; set; }
        public byte UnkByte0x07 { get; set; }
        public string Name { get; set; } //Max length 0x0C
        //public int ModifierPointer 0x14
        //public int NextEffectPointer 0x18
        //public int ModifierToPointer 0x1C
        public List<float> Values { get; set; } = new List<float>(); //Total length 0x50, 0x14 floats

        public List<EmitterItem> EmitterItems { get; set; } = new List<EmitterItem>();

        public Effect()
        {

        }
        public Effect(BinaryReader br, out int nextEffectPointer, int offset = 0, int basePosition = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            UnkByte0x00 = br.ReadByte();
            UnkByte0x01 = br.ReadByte();
            UnkByte0x02 = br.ReadByte();
            UnkByte0x03 = br.ReadByte();
            UnkByte0x04 = br.ReadByte();
            UnkByte0x05 = br.ReadByte();
            UnkByte0x06 = br.ReadByte();
            UnkByte0x07 = br.ReadByte();
            Name = Encoding.ASCII.GetString(br.ReadBytes(0x0C)).Split('\0')[0];
            int nextEmitterItemPointer = br.ReadInt32();
            nextEffectPointer = br.ReadInt32();

            for (int i = 0; i < 0x14; i++)
            {
                Values.Add(br.ReadSingle());
            }

            while (nextEmitterItemPointer != 0)
            {
                EmitterItems.Add(new EmitterItem(br, out nextEmitterItemPointer, basePosition + nextEmitterItemPointer, basePosition));
            }
        }
    }
}