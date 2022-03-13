using System.Collections.Generic;
using System.IO;

namespace JJsUSF4Library.FileClasses.ScriptClasses
{
    public class SFxTBCMCancel
    {
        public string Name;

        public List<int> CancellableMoves;
        public List<byte[]> CancelFlags;

        //Header
        public int
            CancellableMoveCount,
            CancellableMoveIndexPointer, //Counts from START OF THE INDIVIDUAL CANCEL ENTRY
            CancelFlagsPointer;

        public SFxTBCMCancel()
        {

        }
        public SFxTBCMCancel(BinaryReader br, string name, int offset = 0)
        {
            Name = name;

            CancellableMoves = new List<int>();
            CancelFlags = new List<byte[]>();

            br.BaseStream.Seek(offset + 0x04, SeekOrigin.Begin);

            CancellableMoveCount = br.ReadInt32();
            CancellableMoveIndexPointer = br.ReadInt32();
            CancelFlagsPointer = br.ReadInt32();

            br.BaseStream.Seek(offset + CancellableMoveIndexPointer, SeekOrigin.Begin);
            for (int i = 0; i < CancellableMoveCount; i++) CancellableMoves.Add(br.ReadInt16());
            br.BaseStream.Seek(offset + CancelFlagsPointer, SeekOrigin.Begin);
            for (int i = 0; i < CancellableMoveCount; i++) CancelFlags.Add(br.ReadBytes(8));
        }
        public SFxTBCMCancel(byte[] Data, string name)
        {
            Name = name;

            CancellableMoves = new List<int>();
            CancelFlags = new List<byte[]>();

            CancellableMoveCount = USF4Utils.ReadInt(true, 0x04, Data);
            CancellableMoveIndexPointer = USF4Utils.ReadInt(true, 0x08, Data);
            CancelFlagsPointer = USF4Utils.ReadInt(true, 0x0C, Data);

            for (int i = 0; i < CancellableMoveCount; i++)
            {
                CancellableMoves.Add(USF4Utils.ReadInt(false, CancellableMoveIndexPointer + i * 2, Data));
                CancelFlags.Add(Data.Slice(CancelFlagsPointer + i * 8, 8));
            }
        }
    }
}
