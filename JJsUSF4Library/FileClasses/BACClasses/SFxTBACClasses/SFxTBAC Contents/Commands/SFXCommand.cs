using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BACClasses.SFxTBACClasses
{
    public class SFXCommand : CommandHasParamsBase
    {
        public int
            ID, //short
            File; //byte

        public SFXCommand() { }
        public override void ReadCommandDataBlock(BinaryReader br, int startTick, int endTick)
        {
            int startOffset = (int)br.BaseStream.Position;
            StartTick = startTick;
            EndTick = endTick;
            ID = br.ReadInt16();
            File = br.ReadByte();
            int paramsCount = br.ReadByte();
            int paramsPointer = br.ReadInt32();

            if (paramsCount > 0) ReadParamData(br, paramsCount, paramsPointer + startOffset);
        }
        public override byte[] GenerateDataBlockBytes()
        {
            List<byte> Data = new List<byte>();

            USF4Utils.AddIntAsBytes(Data, ID, false);
            Data.Add((byte)File);
            Data.Add((byte)(Params == null ? 0 : Params.Count));
            USF4Utils.AddIntAsBytes(Data, 0, true); //params pointer

            return Data.ToArray();
        }
    }
}
