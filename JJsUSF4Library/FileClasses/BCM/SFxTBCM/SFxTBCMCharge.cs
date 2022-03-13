using System.IO;

namespace JJsUSF4Library.FileClasses.ScriptClasses
{
    public class SFxTBCMCharge
    {
        public string Name;

        public int Input,
            Flags, //Maybe
            UnkShort2_0x04,
            UnkShort3_0x06,
            ChargeTime,
            UnkShort5_0x0A,
            StorageIndex,
            UnkShort7_0x0E;

        public SFxTBCMCharge()
        {

        }

        public SFxTBCMCharge(BinaryReader br, string name, int offset = 0)
        {
            Name = name;

            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            Input = br.ReadInt16();
            Flags = br.ReadInt16();
            UnkShort2_0x04 = br.ReadInt16();
            UnkShort3_0x06 = br.ReadInt16();
            ChargeTime = br.ReadInt16();
            UnkShort5_0x0A = br.ReadInt16();
            StorageIndex = br.ReadInt16();
            UnkShort7_0x0E = br.ReadInt16();
        }

        public SFxTBCMCharge(byte[] Data, string name)
        {
            Name = name;

            Input = USF4Utils.ReadInt(false, 0x00, Data);
            Flags = USF4Utils.ReadInt(false, 0x02, Data); //Maybe
            UnkShort2_0x04 = USF4Utils.ReadInt(false, 0x04, Data);
            UnkShort3_0x06 = USF4Utils.ReadInt(false, 0x06, Data);
            ChargeTime = USF4Utils.ReadInt(false, 0x08, Data);
            UnkShort5_0x0A = USF4Utils.ReadInt(false, 0x0A, Data);
            StorageIndex = USF4Utils.ReadInt(false, 0x0C, Data);
            UnkShort7_0x0E = USF4Utils.ReadInt(false, 0x0E, Data);
        }
    }
}
