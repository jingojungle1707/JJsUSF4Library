using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class AnimatedSpriteFrame : ISpriteFrame
    {
        public float Frame { get; set; }
        public float XOffset { get; set; }
        public float YOffset { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

    }
}
