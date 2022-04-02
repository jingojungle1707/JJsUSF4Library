using System.Collections.Generic;
using System.IO;

namespace JJsUSF4Library.FileClasses.ScriptClasses
{
    public class SFxTBCMCancel
    {
        public string Name;

        public List<CancellableMove> CancellableMoves { get; set; } = new List<CancellableMove>();

        public SFxTBCMCancel()
        {

        }
        public SFxTBCMCancel(BinaryReader br, string name, List<string> moveNames, int offset = 0)
        {
            Name = name;

            br.BaseStream.Seek(offset + 0x04, SeekOrigin.Begin);

            int cancellableMoveCount = br.ReadInt32();
            int cancellableMoveIndexPointer = br.ReadInt32();
            int cancelFlagsPointer = br.ReadInt32();

            List<int> moveIDs = new List<int>();
            List<List<byte>> cancelFlags = new List<List<byte>>();

            br.BaseStream.Seek(offset + cancellableMoveIndexPointer, SeekOrigin.Begin);
            for (int i = 0; i < cancellableMoveCount; i++) moveIDs.Add(br.ReadInt16());
            br.BaseStream.Seek(offset + cancelFlagsPointer, SeekOrigin.Begin);
            for (int i = 0; i < cancellableMoveCount; i++)
            {
                cancelFlags.Add(new List<byte> (br.ReadBytes(8)));
            }

            for (int i = 0; i < cancellableMoveCount; i++)
            {
                CancellableMoves.Add(new CancellableMove()
                {
                    CancellableMoveName = moveNames[moveIDs[i]],
                    UnkByte0_0x00 = cancelFlags[i][0],
                    UnkByte1_0x01 = cancelFlags[i][1],
                    UnkByte2_0x02 = cancelFlags[i][2],
                    UnkByte3_0x03 = cancelFlags[i][3],
                    UnkByte4_0x04 = cancelFlags[i][4],
                    UnkByte5_0x05 = cancelFlags[i][5],
                    UnkByte6_0x06 = cancelFlags[i][6],
                    UnkByte7_0x07 = cancelFlags[i][7],
                });
            }
        }
        public SFxTBCMCancel(byte[] Data, string name, List<string> moveNames)
        {
            Name = name;

            CancellableMoves = new List<CancellableMove>();

            int cancellableMoveCount = USF4Utils.ReadInt(true, 0x04, Data);
            int cancellableMoveIndexPointer = USF4Utils.ReadInt(true, 0x08, Data);
            int cancelFlagsPointer = USF4Utils.ReadInt(true, 0x0C, Data);

            List<int> moveIDs = new List<int>();
            List<List<byte>> cancelFlags = new List<List<byte>>();
            for (int i = 0; i < cancellableMoveCount; i++)
            {
                moveIDs.Add(USF4Utils.ReadInt(false, cancellableMoveIndexPointer + i * 2, Data));
                cancelFlags.Add(new List<byte> (Data.Slice(cancelFlagsPointer + i * 8, 8)));
            }
            for (int i = 0; i < cancellableMoveCount; i++)
            {
                CancellableMoves.Add(new CancellableMove()
                {
                    CancellableMoveName = moveNames[moveIDs[i]],
                    UnkByte0_0x00 = cancelFlags[i][0],
                    UnkByte1_0x01 = cancelFlags[i][1],
                    UnkByte2_0x02 = cancelFlags[i][2],
                    UnkByte3_0x03 = cancelFlags[i][3],
                    UnkByte4_0x04 = cancelFlags[i][4],
                    UnkByte5_0x05 = cancelFlags[i][5],
                    UnkByte6_0x06 = cancelFlags[i][6],
                    UnkByte7_0x07 = cancelFlags[i][7],
                });
            }
        }

        public class CancellableMove
        {
            public string CancellableMoveName { get; set; }
            public byte
                UnkByte0_0x00,
                UnkByte1_0x01,
                UnkByte2_0x02,
                UnkByte3_0x03,
                UnkByte4_0x04,
                UnkByte5_0x05,
                UnkByte6_0x06,
                UnkByte7_0x07;
        }
    }
}
