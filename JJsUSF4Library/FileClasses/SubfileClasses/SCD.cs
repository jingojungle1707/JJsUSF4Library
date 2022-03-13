using System.Collections.Generic;
using System.Linq;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class SCD
    {
        public byte[] HEXBytes;
        public int NodeNameIndexPointer;
        public float Gravity;
        public float AirResistance;

        public int PreNodeCount;
        public int PreNodePointer;
        public int PTCCount;
        public int PTCPointer;
        public int JNTCount;
        public int JNTPointer;
        public int COLCount;
        public int COLPointer;
        public int NodeNameCount;
        public int NodeNameIndexPointer2;

        public List<byte> FFFF;

        public SCD(byte[] Data)
        {
            NodeNameIndexPointer = USF4Utils.ReadInt(true, 0x04, Data);
            Gravity = USF4Utils.ReadFloat(0x08, Data);
            AirResistance = USF4Utils.ReadFloat(0x0C, Data);

            PreNodeCount = USF4Utils.ReadInt(true, 0x10, Data);
            PreNodePointer = USF4Utils.ReadInt(true, 0x14, Data);
            PTCCount = USF4Utils.ReadInt(true, 0x18, Data);
            PTCPointer = USF4Utils.ReadInt(true, 0x1C, Data);
            JNTCount = USF4Utils.ReadInt(true, 0x20, Data);
            JNTPointer = USF4Utils.ReadInt(true, 0x24, Data);
            COLCount = USF4Utils.ReadInt(true, 0x28, Data);
            COLPointer = USF4Utils.ReadInt(true, 0x2C, Data);
            NodeNameCount = USF4Utils.ReadInt(true, 0x30, Data);
            NodeNameIndexPointer2 = USF4Utils.ReadInt(true, 0x34, Data);

            FFFF = Data.Slice(0x38, 0x08).ToList();

            for (int i = 0; i < PreNodeCount; i++)
            {

            }
        }
    }
}