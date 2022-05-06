using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace JJsUSF4Library.FileClasses.BACClasses.SFxTBACClasses
{
    public class ScriptFile
    {
        public string Name;
        public int UnkShort0_0x00;
        [XmlIgnore]
        public List<SFxTScript> Scripts;
        [XmlIgnore]
        public List<string> ScriptNames
        {
            get { return Scripts.Select(o => o.Name).ToList(); }
        }

        public ScriptFile()
        {
            Scripts = new List<SFxTScript>();
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
    }
}