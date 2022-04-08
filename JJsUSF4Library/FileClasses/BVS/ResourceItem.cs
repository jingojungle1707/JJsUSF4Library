using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class ResourceItem
    {
        public string ResourceName { get; set; }
        public int UnkShort0x04 { get; set; } //Probably unused?
        public int BitFlag { get; set; }
        public float UnkFloat0x08 { get; set; } //Always 1.0?
        public float UnkFloat0x0C { get; set; } //Always 0/unused?

        public ResourceItem(BinaryReader br, int offset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            int resourcePointer = br.ReadInt32();
            UnkShort0x04 = br.ReadInt16();
            BitFlag = br.ReadInt16();
            UnkFloat0x08 = br.ReadSingle();
            UnkFloat0x0C = br.ReadSingle();

            //We just grab the resource name, so we can pull it from the BVS resource list when it's needed
            br.BaseStream.Seek(offset + resourcePointer, SeekOrigin.Begin);
            ResourceName = br.ReadZString();
        }
    }
}
