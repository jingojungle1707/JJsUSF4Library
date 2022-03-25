
using System.IO;

namespace JJsUSF4Library.FileClasses
{
    public class RY2 : USF4File
    {
        public int PhysicsCount;
        public int PhysicsIndexPointer;
        public int PhysicsNamesCount;
        public int PhysicsNamesIndexPointer;

        public RY2()
        {

        }

        public RY2(BinaryReader br, string name, int offset = 0)
        {
            Name = name;
            ReadFromStream(br, offset);
        }

        public override void ReadFromStream(BinaryReader br, int offset = 0, int fileLength = 0)
        {
            br.BaseStream.Seek(offset + 0x10, SeekOrigin.Begin);

            PhysicsCount = br.ReadInt32();
            PhysicsIndexPointer = br.ReadInt32();
            PhysicsNamesCount = br.ReadInt32();
            PhysicsNamesIndexPointer = br.ReadInt32();

        }
    }
}