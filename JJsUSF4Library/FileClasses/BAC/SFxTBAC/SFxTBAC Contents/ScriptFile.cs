using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JJsUSF4Library.FileClasses.ScriptClasses
{
    public partial class ScriptFile
    {
        public string Name;
        public int UnkShort0_0x00;
        public List<SFxTScript> Scripts;
        public List<string> ScriptNames
        {
            get { return Scripts.Select(o => o.Name).ToList(); }
        }

        public ScriptFile()
        {

        }

        public ScriptFile(BinaryReader br, string name, int offset = 0)
        {
            Name = name;
            
            Scripts = new List<SFxTScript>();
            List<int> scriptPointers = new List<int>();
            List<int> scriptNamePointers = new List<int>();
            List<string> scriptNames = new List<string>();

            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            UnkShort0_0x00 = br.ReadInt16();
            int scriptCount = br.ReadInt16();
            int scriptIndexPointer = br.ReadInt32();
            int scriptNameIndexPointer = br.ReadInt32();
            //Read script pointers
            br.BaseStream.Seek(offset + scriptIndexPointer, SeekOrigin.Begin);
            for (int i = 0; i < scriptCount; i++) scriptPointers.Add(br.ReadInt32());
            //Read name pointers
            br.BaseStream.Seek(offset + scriptNameIndexPointer, SeekOrigin.Begin);
            for (int i = 0; i < scriptCount; i++) scriptNamePointers.Add(br.ReadInt32());
            //Read script names
            for (int i = 0; i < scriptCount; i++)
            {
                if (scriptNamePointers[i] == 00) scriptNames.Add(string.Empty);
                else
                {
                    br.BaseStream.Seek(offset + scriptNamePointers[i], SeekOrigin.Begin);
                    scriptNames.Add(USF4Utils.ReadZString(br));
                }
            }
            //Read scripts
            for (int i = 0; i < scriptCount; i++)
            {
                if (scriptPointers[i] == 0) Scripts.Add(new SFxTScript());
                else Scripts.Add(new SFxTScript(br, scriptNames[i], offset + scriptPointers[i]));
            }
        }

        public ScriptFile(byte[] Data, int Offset, string name)
        {
            int os = Offset;
            Name = name;
            Scripts = new List<SFxTScript>();
            List<int> scriptPointers = new List<int>();
            List<int> scriptNamePointers = new List<int>();

            UnkShort0_0x00 = USF4Utils.ReadInt(false, os + 0x00, Data);
            int scriptCount = USF4Utils.ReadInt(false, os + 0x02, Data);
            int scriptIndexPointer = USF4Utils.ReadInt(true, os + 0x04, Data);
            int scriptNameIndexPointer = USF4Utils.ReadInt(true, os + 0x08, Data);

            for (int i = 0; i < scriptCount; i++)
            {
                scriptPointers.Add(USF4Utils.ReadInt(true, os + scriptIndexPointer + i * 4, Data));
                scriptNamePointers.Add(USF4Utils.ReadInt(true, os + scriptNameIndexPointer + i * 4, Data));
                if (scriptPointers[i] != 0 && scriptNamePointers[i] != 0)
                {
                    Scripts.Add(new SFxTScript(Data, os + scriptPointers[i], Encoding.ASCII.GetString(USF4Utils.ReadZeroTermStringToArray(os + scriptNamePointers[i], Data, Data.Length))));
                }
                else Scripts.Add(new SFxTScript());
            }
        }
    }
}