using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.ScriptClasses
{
    class USF4BCMInputMotion
    {
        public string Name;

        public List<InputDetail> InputDetails;

        public USF4BCMInputMotion(BinaryReader br, string name, int offset = 0)
        {
            Name = name;

            InputDetails = new List<InputDetail>();

            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            int inputCount = br.ReadInt32();
            
            for (int i = 0; i < inputCount; i++)
            {
                InputDetails.Add(new InputDetail()
                {
                    Type = br.ReadInt16(),
                    Buffer = br.ReadInt16(),
                    Input = br.ReadInt16(),
                    MoveFlags = br.ReadInt16(),
                    Flags = br.ReadInt16(),
                    Requirement = br.ReadInt16(),
                });
            }
        }

        public List<byte> GenerateBytes()
        {
            List<byte> data = new List<byte>();

            USF4Utils.AddIntAsBytes(data, InputDetails.Count, true);

            foreach (InputDetail id in InputDetails)
            {
                USF4Utils.AddIntAsBytes(data, id.Type, false);
                USF4Utils.AddIntAsBytes(data, id.Buffer, false);
                USF4Utils.AddIntAsBytes(data, id.Input, false);
                USF4Utils.AddIntAsBytes(data, id.MoveFlags, false);
                USF4Utils.AddIntAsBytes(data, id.Flags, false);
                USF4Utils.AddIntAsBytes(data, id.Requirement, false);
            }
            //Pad out to full length
            while (data.Count < 0xC4) data.Add(0x00);

            return data;
        }

        public class InputDetail
        {
            public int
                Input,
                Type,
                Buffer,
                MoveFlags,
                Flags,
                Requirement;
        }
    }
}
