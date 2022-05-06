using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BACClasses.SFxTBACClasses
{
    public class AnimationCommand : CommandBase
    {
        public int
            ID; //Short
        public AnimationFile File;
        public byte
            Flags;
        public int
            Start, //Short
            End; //Short

        public AnimationCommand() { }
        public override void ReadCommandDataBlock(BinaryReader br, int startTick, int endTick)
        {
            StartTick = startTick;
            EndTick = endTick;
            ID = br.ReadInt16();
            File = (AnimationFile)br.ReadByte();
            Flags = br.ReadByte();
            Start = br.ReadInt16();
            End = br.ReadInt16();
        }
        public override byte[] GenerateDataBlockBytes()
        {
            List<byte> Data = new List<byte>();

            USF4Utils.AddIntAsBytes(Data, ID, false);
            Data.Add((byte)File);
            Data.Add(Flags);
            USF4Utils.AddIntAsBytes(Data, Start, false);
            USF4Utils.AddIntAsBytes(Data, End, false);

            return Data.ToArray();
        }
    }
}
