using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.EMEClasses.Emitters
{
    public class EmitterItemBodyFactory
    {
        public EmitterItemBody ReadEmitterItemBody(BinaryReader br, EmitterItemType type)
        {
            return type switch
            {
                EmitterItemType.ADD => new Add(br),
                EmitterItemType.SPRITE => new Sprite(br),
                EmitterItemType.LINE => new Line(br),
                EmitterItemType.UNK3 => throw new NotImplementedException(),
                EmitterItemType.DRAG => new Drag(br),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
