using System.IO;

namespace JJsUSF4Library.FileClasses
{
    public interface IUSF4Object
    {
        byte[] GenerateBytes();
        void ReadFromStream(BinaryReader br, int offset = 0, int fileLength = 0);
        void SaveToPath(string path);
    }
}