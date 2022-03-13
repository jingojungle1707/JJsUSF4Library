using System.Collections.Generic;
using System.IO;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class IKNode
    {
        public string Name;
        public List<int> BoneList; //Possibly?
        public int BoneCount;
        public int BoneListPointer;

        public IKNode()
        {

        }

        public IKNode(BinaryReader br)
        {
            BoneList = new List<int>();

            int chainLength = br.ReadInt32();
            int chainPointer = br.ReadInt32();
            //-8, SeekOrigin.Current so we're seeking relative to the start of this node datablock
            br.BaseStream.Seek(chainPointer - 0x08, SeekOrigin.Current);

            for (int j = 0; j < chainLength; j++) BoneList.Add(br.ReadInt16());
            
        }
    }
}