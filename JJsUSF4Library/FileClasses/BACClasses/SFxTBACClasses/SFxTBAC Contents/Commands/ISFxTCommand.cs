using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BACClasses.SFxTBACClasses
{
    internal interface ISFxTCommand
    {
        int StartTick { get; set; }
        int EndTick { get; set; }

        void ReadCommandDataBlock(BinaryReader br, int startTick, int endTick);

        byte[] GenerateDataBlockBytes();
    }
}
