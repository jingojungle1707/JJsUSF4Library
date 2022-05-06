using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BACClasses.SFxTBACClasses
{
    public class Unk6Command : CommandHasParamsBase
    {
        public byte UnkByte0_0x00;
        public int UnkShort1_0x01;

        public Unk6Command() { }
        public override void ReadCommandDataBlock(BinaryReader br, int startTick, int endTick)
        {
            int startOffset = (int)br.BaseStream.Position;
            StartTick = startTick;
            EndTick = endTick;
            UnkByte0_0x00 = br.ReadByte();
            UnkShort1_0x01 = br.ReadInt16();
            int paramsCount = br.ReadByte();
            int paramsPointer = br.ReadInt32();

            if (paramsCount > 0) ReadParamData(br, paramsCount, paramsPointer + startOffset);
        }
        public override byte[] GenerateDataBlockBytes()
        {
            List<byte> Data = new List<byte>();
            Data.Add(UnkByte0_0x00);
            USF4Utils.AddIntAsBytes(Data, UnkShort1_0x01, false);
            Data.Add((byte)(Params == null ? 0 : Params.Count));
            USF4Utils.AddIntAsBytes(Data, 0, true); //Params pointer, need to overwrite it later

            return Data.ToArray();
        }
    }
}
