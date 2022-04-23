using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BTRClasses
{
    public interface ISpriteFrame
    {
        float XOffset { get; set; }
        float YOffset { get; set; }
        float Width { get; set; }
        float Height { get; set; }
    }
}
