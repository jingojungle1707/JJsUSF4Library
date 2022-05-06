using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BACClasses.SFxTBACClasses
{
    public abstract class CommandHasParamsBase : CommandBase, ISFxTCommandHasParams
    {
        public List<int> Params { get; set; } = new List<int>();
        public virtual void ReadParamData(BinaryReader br, int paramsCount, int globalPointer)
        {
            int startOffset = (int)br.BaseStream.Position;
            Params = new List<int>();
            br.BaseStream.Seek(globalPointer, SeekOrigin.Begin);
            for (int i = 0; i < paramsCount; i++)
            {
                Params.Add(br.ReadInt32());
            }
            br.BaseStream.Seek(startOffset, SeekOrigin.Begin);
        }
        public virtual byte[] GenerateParamBytes()
        {
            List<byte> Data = new List<byte>();

            if (Params != null && Params.Count > 0)
            {
                for (int i = 0; i < Params.Count; i++) USF4Utils.AddIntAsBytes(Data, Params[i], true);
            }

            return Data.ToArray();
        }
    }
}
