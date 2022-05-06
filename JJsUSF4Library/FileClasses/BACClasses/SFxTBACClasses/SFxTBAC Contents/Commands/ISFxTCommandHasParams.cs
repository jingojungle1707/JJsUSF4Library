using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BACClasses.SFxTBACClasses
{
    internal interface ISFxTCommandHasParams : ISFxTCommand
    {
        List<int> Params { get; set; }

        void ReadParamData(BinaryReader br, int paramsCount, int globalPointer);

        byte[] GenerateParamBytes();
    }
}
