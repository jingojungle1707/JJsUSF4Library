using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class StaticSpriteInstance : ISpriteInstance
    {
        public List<ISpriteFrame> Frames { get; set; } = new List<ISpriteFrame>();
    }
}
