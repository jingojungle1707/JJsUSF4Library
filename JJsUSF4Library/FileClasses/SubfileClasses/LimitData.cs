using System.Collections.Generic;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class LimitData
    {
        public int bitflag;
        public int ID1;
        public int ID2;
        public List<float> LimitValues; //x1, x2, y1, y2, z1, z2
    }
}