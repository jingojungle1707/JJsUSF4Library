
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

        public RY2(byte[] Data, string name)
        {
            Name = name;
            ReadFile(Data);
        }

        public override void ReadFile(byte[] Data)
        {
            PhysicsCount = USF4Utils.ReadInt(true, 0x10, Data);
            PhysicsIndexPointer = USF4Utils.ReadInt(true, 0x14, Data);
            PhysicsNamesCount = USF4Utils.ReadInt(true, 0x18, Data);
            PhysicsNamesIndexPointer = USF4Utils.ReadInt(true, 0x1C, Data);

            for (int i = 0; i < PhysicsCount; i++)
            {

            }
        }
    }
}