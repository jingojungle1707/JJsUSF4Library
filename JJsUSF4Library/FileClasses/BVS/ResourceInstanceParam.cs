using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class ResourceInstanceParam
    {
        public float UnkFloat0x00 { get; set; } 
        public float UnkFloat0x04 { get; set; } 
        public float UnkFloat0x08 { get; set; }
        public float UnkFloat0x0C { get; set; }
        public float UnkFloat0x10 { get; set; }
        public float UnkFloat0x14 { get; set; }
        public float UnkFloat0x18 { get; set; }
        public float UnkFloat0x1C { get; set; }

        public ResourceInstanceParam(BinaryReader br, int offset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            UnkFloat0x00 = br.ReadSingle();
            UnkFloat0x04 = br.ReadSingle();
            UnkFloat0x08 = br.ReadSingle();
            UnkFloat0x0C = br.ReadSingle();
            UnkFloat0x10 = br.ReadSingle();
            UnkFloat0x14 = br.ReadSingle();
            UnkFloat0x18 = br.ReadSingle();
            UnkFloat0x1C = br.ReadSingle();
        }
    }
}
