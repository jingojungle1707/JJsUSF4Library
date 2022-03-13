using JJsUSF4Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses
{
    public class USF4File
    {

        public byte[] HEXBytes;
        public string Name;

        //Offer the option to pass fileLength for non-SF4 filetypes (like DDS, CSB) that we otherwise couldn't calculate the filesize for
        //Files that we CAN calculate the fileLength for will just ignore this variable.
        public virtual void ReadFromStream(BinaryReader br, int offset = 0, int fileLength = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            HEXBytes = br.ReadBytes(fileLength); 
        }
        public virtual void ReadFile(byte[] Data)
        {
            HEXBytes = Data;
        }
        public virtual byte[] GenerateBytes()
        {
            return HEXBytes;
        }
        /// <summary>
        /// Saves the object as a binary file at the specified path.
        /// <br>Rebuilds binary data from the class isntance.</br>
        /// </summary>
        /// <param name="path"></param>
        public virtual void SaveFile(string path)
        {
            USF4Utils.WriteDataToStream(path, GenerateBytes());
        }
        public virtual void DeleteSubfile(int index)
        {

        }
    }
}
