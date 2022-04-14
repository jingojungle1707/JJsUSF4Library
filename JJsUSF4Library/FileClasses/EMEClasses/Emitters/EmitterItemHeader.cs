using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.EMEClasses.Emitters
{
    public class EmitterItemHeader
    {
        public byte UnkByte0x00 { get; set; }
        public byte UnkByte0x01 { get; set; }
        public EmitterItemType Type { get; } //0x00 = Add, 0x01 = Sprite, 0x02 = Line, 0x03 = Unknown, 0x04 = Drag
        public byte UnkByte0x03 { get; set; }
        public byte UnkByte0x04 { get; set; }
        public byte UnkByte0x05 { get; set; }
        public byte UnkByte0x06 { get; set; }
        public byte UnkByte0x07 { get; set; }
        public string Name { get; set; }

        public EmitterItemHeader(BinaryReader br, out int nextEmitterItemModifierPointer, out int nextEmitterItemPointer)
        {
            UnkByte0x00 = br.ReadByte();
            UnkByte0x01 = br.ReadByte();
            Type = (EmitterItemType)br.ReadByte();
            UnkByte0x03 = br.ReadByte();
            UnkByte0x04 = br.ReadByte();
            UnkByte0x05 = br.ReadByte();
            UnkByte0x06 = br.ReadByte();
            UnkByte0x07 = br.ReadByte();
            Name = Encoding.ASCII.GetString(br.ReadBytes(0x0C)).Split('\0')[0];
            nextEmitterItemModifierPointer = br.ReadInt32();
            nextEmitterItemPointer = br.ReadInt32();
            int modifiedItemPointer = br.ReadInt32();
        }
    }
}
