using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.EMEClasses.Emitters
{
    public enum EmitterItemType
    {
        ADD = 0x00,
        SPRITE = 0x01,
        LINE = 0x02,
        UNK3 = 0x03,
        DRAG = 0x04,
        
    }
    public class EmitterItem
    {
        public EmitterItemHeader Header { get; }
        public EmitterItemBody Body { get; }
        public List<EmitterItem> Modifiers { get; } = new List<EmitterItem>();
        public EmitterItem()
        {

        }
        public EmitterItem(BinaryReader br, out int nextEmitterItemPointer, int offset = 0, int basePosition = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            Header = new EmitterItemHeader(br, out int nextEmitterItemModifierPointer, out nextEmitterItemPointer);
            Body = (new EmitterItemBodyFactory().ReadEmitterItemBody(br, Header.Type));
            
            while (nextEmitterItemModifierPointer != 0)
            {
                Modifiers.Add(new EmitterItem(br, out nextEmitterItemModifierPointer, basePosition + nextEmitterItemModifierPointer, basePosition));
            }
        }
    }
}
