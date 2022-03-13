using System.Collections.Generic;
using System.IO;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class IKDataBlock
    {
        public int Method;
        public int Length;
        public int Flag0x00;
        public int Flag0x01;
        public List<int> BoneIDs;
        public List<float> IKFloats;

        public IKDataBlock()
        {

        }

        public IKDataBlock(BinaryReader br)
        {
            BoneIDs = new List<int>();
            IKFloats = new List<float>();

            Method = br.ReadInt16();
            Length = br.ReadInt16();
            Flag0x00 = br.ReadByte();
            Flag0x01 = br.ReadByte();

            if (Method == 0x00)
            {
                for (int i = 0; i < 5; i++) BoneIDs.Add(br.ReadInt16());
            }

            else if (Method == 0x01)
            {
                for (int i = 0; i < 3; i++) BoneIDs.Add(br.ReadInt16());
                for (int i = 0; i < 3; i++) IKFloats.Add(br.ReadSingle());
            }
            
        }
    }
}