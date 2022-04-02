using System.Collections.Generic;
using System.IO;

namespace JJsUSF4Library.FileClasses.ScriptClasses
{
    public class SFxTBCMCancel
    {
        public string Name;

        public List<int> CancellableMoves;
        public List<byte[]> CancelFlags;

        public SFxTBCMCancel()
        {

        }
        public SFxTBCMCancel(BinaryReader br, string name, int offset = 0)
        {
            Name = name;

            CancellableMoves = new List<int>();
            CancelFlags = new List<byte[]>();

            br.BaseStream.Seek(offset + 0x04, SeekOrigin.Begin);

            int cancellableMoveCount = br.ReadInt32();
            int cancellableMoveIndexPointer = br.ReadInt32();
            int cancelFlagsPointer = br.ReadInt32();

            br.BaseStream.Seek(offset + cancellableMoveIndexPointer, SeekOrigin.Begin);
            for (int i = 0; i < cancellableMoveCount; i++) CancellableMoves.Add(br.ReadInt16());
            br.BaseStream.Seek(offset + cancelFlagsPointer, SeekOrigin.Begin);
            for (int i = 0; i < cancellableMoveCount; i++) CancelFlags.Add(br.ReadBytes(8));
        }
        public SFxTBCMCancel(byte[] Data, string name)
        {
            Name = name;

            CancellableMoves = new List<int>();
            CancelFlags = new List<byte[]>();

            int cancellableMoveCount = USF4Utils.ReadInt(true, 0x04, Data);
            int cancellableMoveIndexPointer = USF4Utils.ReadInt(true, 0x08, Data);
            int cancelFlagsPointer = USF4Utils.ReadInt(true, 0x0C, Data);

            for (int i = 0; i < cancellableMoveCount; i++)
            {
                CancellableMoves.Add(USF4Utils.ReadInt(false, cancellableMoveIndexPointer + i * 2, Data));
                CancelFlags.Add(Data.Slice(cancelFlagsPointer + i * 8, 8));
            }
        }
    }
}
