using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public interface ISpriteInstance
    {
        List<ISpriteFrame> Frames { get; set; }
    }
}
