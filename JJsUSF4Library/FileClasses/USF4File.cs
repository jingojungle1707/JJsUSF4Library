using JJsUSF4Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses
{
    public class USF4File : INameableUSF4Object
    {
        private byte[] _binaryData = new byte[0];

        public virtual byte[] GenerateBytes()
        {
            return _binaryData;
        }
        public string Name { get; set; }

        //Offer the option to pass fileLength for non-SF4 filetypes (like DDS, CSB) that we otherwise couldn't calculate the filesize for
        //Files that we CAN calculate the fileLength for will just ignore this variable.
        public virtual void ReadFromStream(BinaryReader br, int offset = 0, int fileLength = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            _binaryData = br.ReadBytes(fileLength);
        }
        /// <summary>
        /// Saves the object as a binary file at the specified path.
        /// <br>Rebuilds binary data from the class isntance.</br>
        /// </summary>
        /// <param name="path"></param>
        public virtual void SaveToPath(string path)
        {
            USF4Utils.WriteDataToStream(path, GenerateBytes());
        }
    }
}
