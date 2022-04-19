using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BTRClasses
{
    public class StaticSpriteInstance : ISpriteInstance
    {
        public List<ISpriteFrame> Frames { get; set; } = new List<ISpriteFrame>();
        public List<byte> GenerateHeaderBytes()
        {
            List<byte> data = new List<byte>();

            USF4Utils.AddFloatAsBytes(data, Frames[0].XOffset);
            USF4Utils.AddFloatAsBytes(data, Frames[0].YOffset);
            USF4Utils.AddFloatAsBytes(data, Frames[0].Width);
            USF4Utils.AddFloatAsBytes(data, Frames[0].Height);

            return data;
        }
}
}
