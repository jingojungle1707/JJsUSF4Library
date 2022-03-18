using JJsUSF4Library.FileClasses.ScriptClasses;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JJsUSF4Library.FileClasses
{
    public class SFxTBAC : BAC
    {
        public List<ScriptFile> ScriptFiles;
        public List<HitEffect> HitEffects;

        public SFxTBAC()
        {

        }

        public SFxTBAC(BinaryReader br, string name, int offset = 0)
        {
            Name = name;
            ReadFromStream(br, offset);
        }

        public override void ReadFromStream(BinaryReader br, int offset = 0, int fileLength = 0)
        {
            ScriptFiles = new List<ScriptFile>();
            HitEffects = new List<HitEffect>();

            List<int> scriptFilePointers = new List<int>();
            List<int> scriptFileNamePointers = new List<int>();
            List<int> hitEffectPointers = new List<int>();
            List<int> hitEffectNamePointers = new List<int>();
            List<string> scriptNames = new List<string>();
            List<string> hitEffectNames = new List<string>();

            br.BaseStream.Seek(offset + 0x0C, SeekOrigin.Begin);

            int scriptFileCount = br.ReadInt16();
            int hitEffectCount = br.ReadInt16();
            //0x10
            int scriptFileIndexPointer = br.ReadInt32();
            int scriptFileNameIndexPointer = br.ReadInt32();
            int hitEffectIndexPointer = br.ReadInt32();
            int hitEffectNameIndexPointer = br.ReadInt32();
            //0x20
            //Read script file pointers
            br.BaseStream.Seek(offset + scriptFileIndexPointer, SeekOrigin.Begin);
            for (int i = 0; i < scriptFileCount; i++) scriptFilePointers.Add(br.ReadInt32());
            //Read script file name pointers
            br.BaseStream.Seek(offset + scriptFileNameIndexPointer, SeekOrigin.Begin);
            for (int i = 0; i < scriptFileCount; i++) scriptFileNamePointers.Add(br.ReadInt32());
            //Read hiteffect pointers
            br.BaseStream.Seek(offset + hitEffectIndexPointer, SeekOrigin.Begin);
            for (int i = 0; i < hitEffectCount; i++) hitEffectPointers.Add(br.ReadInt32());
            //Read hiteffect name pointers
            br.BaseStream.Seek(offset + hitEffectNameIndexPointer, SeekOrigin.Begin);
            for (int i = 0; i < hitEffectCount; i++) hitEffectNamePointers.Add(br.ReadInt32());
            //Read script names
            for (int i = 0; i < scriptFileCount; i++)
            {
                if (scriptFileNamePointers[i] == 0) scriptNames.Add(string.Empty);
                else
                {
                    br.BaseStream.Seek(offset + scriptFileNamePointers[i], SeekOrigin.Begin);
                    scriptNames.Add(USF4Utils.ReadZString(br));
                }
            }
            //Read hiteffect names
            for (int i = 0; i < hitEffectCount; i++)
            {
                if (hitEffectNamePointers[i] == 0) hitEffectNames.Add(string.Empty); 
                else
                {
                    br.BaseStream.Seek(offset + hitEffectNamePointers[i], SeekOrigin.Begin);
                    hitEffectNames.Add(USF4Utils.ReadZString(br));
                }
            }
            //Read script files
            for (int i = 0; i < scriptFileCount; i++)
            {
                if (scriptFilePointers[i] == 0) ScriptFiles.Add(new ScriptFile());
                else ScriptFiles.Add(new ScriptFile(br, scriptNames[i], scriptFilePointers[i]));
            }
            //Read hiteffect data
            for (int i = 0; i < hitEffectCount; i++)
            {
                if (hitEffectPointers[i] == 0) HitEffects.Add(new HitEffect());
                else HitEffects.Add(new HitEffect(br, hitEffectNames[i], offset + hitEffectPointers[i]));
            }
        }

        public SFxTBAC(byte[] Data)
        {
            ReadFile(Data);
        }

        public override void ReadFile(byte[] Data)
        {
            List<int> scriptFilePointers = new List<int>();
            List<int> scriptFileNamePointers = new List<int>();
            List<int> hitEffectPointers = new List<int>();
            List<int> hitEffectNamePointers = new List<int>();

            ScriptFiles = new List<ScriptFile>();
            HitEffects = new List<HitEffect>();

            int scriptFileCount = USF4Utils.ReadInt(false, 0x0C, Data);
            int hitEffectCount = USF4Utils.ReadInt(false, 0x0E, Data);

            int scriptFileIndexPointer = USF4Utils.ReadInt(true, 0x10, Data);
            int scriptFileNameIndexPointer = USF4Utils.ReadInt(true, 0x14, Data);
            int hitEffectIndexPointer = USF4Utils.ReadInt(true, 0x18, Data);
            int hitEffectNameIndexPointer = USF4Utils.ReadInt(true, 0x1C, Data);

            for (int i = 0; i < scriptFileCount; i++)
            {
                scriptFilePointers.Add(USF4Utils.ReadInt(true, scriptFileIndexPointer + i * 4, Data));
                scriptFileNamePointers.Add(USF4Utils.ReadInt(true, scriptFileNameIndexPointer + i * 4, Data));
                ScriptFiles.Add(new ScriptFile(Data, scriptFilePointers[i], Encoding.ASCII.GetString(USF4Utils.ReadZeroTermStringToArray(scriptFileNamePointers[i], Data, Data.Length))));
            }
            for (int i = 0; i < hitEffectCount; i++)
            {
                hitEffectPointers.Add(USF4Utils.ReadInt(true, hitEffectIndexPointer + i * 4, Data));
                hitEffectNamePointers.Add(USF4Utils.ReadInt(true, hitEffectNameIndexPointer + i * 4, Data));
                if (hitEffectPointers[i] != 0 && hitEffectNamePointers[i] != 0)
                {
                    //TODO PASS THE CORRECT AMOUNT OF DATA INSTEAD OF THIS BULLSHIT
                    HitEffects.Add(new HitEffect(Data.Slice(hitEffectPointers[i], 0), Encoding.ASCII.GetString(USF4Utils.ReadZeroTermStringToArray(hitEffectNamePointers[i], Data, Data.Length))));
                }
                else HitEffects.Add(new HitEffect());
            }
        }

        public override byte[] GenerateBytes()
        {
            List<byte> Data = new List<byte>();

            //Whenever we write dummy data for a name pointer, we add its position to the dictionary so we can update it later
            Dictionary<string, int> ScriptFileNameDictionary = new Dictionary<string, int>();
            Dictionary<string, int> ScriptNameDictionary = new Dictionary<string, int>();
            Dictionary<string, int> HitEffectNameDictionary = new Dictionary<string, int>();
            foreach (ScriptFile sf in ScriptFiles)
            {
                ScriptFileNameDictionary.Add(sf.Name, 0);

                //Where there's multiple script files, there can be name collisions between stances
                //So we prepend the script file name and use a "#" as a delimiter in case we later need to retrieve just the script name
                foreach (SFxTScript s in sf.Scripts)
                {
                    if (s.ScriptSections == null) continue;
                    ScriptNameDictionary.Add($"{sf.Name}#{s.Name}", 0);
                }
            }
            foreach (HitEffect he in HitEffects)
            {
                if (he.HitEffectDatas == null) continue;
                HitEffectNameDictionary.Add(he.Name, 0);
            }

            Data.AddRange(new byte[] { 0x23, 0x42, 0x41, 0x43, 0xFE, 0xFF, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00 });
            USF4Utils.AddIntAsBytes(Data, ScriptFiles.Count, false);
            USF4Utils.AddIntAsBytes(Data, HitEffects.Count, false);
            //0x10
            int ScriptFileIndexPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            int ScriptFileNameIndexPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            int HitEffectIndexPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            int HitEffectNameIndexPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            //0x20
            //Write out main indexes
            USF4Utils.UpdateIntAtPosition(Data, ScriptFileIndexPointerPosition, Data.Count);
            List<int> ScriptFilePointerPositions = new List<int>();
            for (int i = 0; i < ScriptFiles.Count; i++)
            {
                ScriptFilePointerPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, -1, true);
            }
            USF4Utils.UpdateIntAtPosition(Data, ScriptFileNameIndexPointerPosition, Data.Count);
            List<int> ScriptFileNamePointerPositions = new List<int>();
            for (int i = 0; i < ScriptFiles.Count; i++)
            {
                ScriptFileNameDictionary[ScriptFiles[i].Name] = Data.Count;
                ScriptFileNamePointerPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, -1, true);
            }
            USF4Utils.UpdateIntAtPosition(Data, HitEffectIndexPointerPosition, Data.Count);
            List<int> HitEffectPointerPositions = new List<int>();
            for (int i = 0; i < HitEffects.Count; i++)
            {
                HitEffectPointerPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, -1, true);
            }
            USF4Utils.UpdateIntAtPosition(Data, HitEffectNameIndexPointerPosition, Data.Count);
            List<int> HitEffectNamePointerPositions = new List<int>();
            for (int i = 0; i < HitEffects.Count; i++)
            {
                if (HitEffects[i].HitEffectDatas != null)
                {
                    HitEffectNameDictionary[HitEffects[i].Name] = Data.Count;
                }
                HitEffectNamePointerPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, 0, true);
            }
            //Start writing ScriptFiles
            List<int> scriptFileStartOSList = new List<int>();
            for (int i = 0; i < ScriptFiles.Count; i++)
            {
                scriptFileStartOSList.Add(Data.Count);

                USF4Utils.UpdateIntAtPosition(Data, ScriptFilePointerPositions[i], Data.Count);

                ScriptFile sf = ScriptFiles[i];

                USF4Utils.AddIntAsBytes(Data, sf.UnkShort0_0x00, false);
                USF4Utils.AddIntAsBytes(Data, sf.Scripts.Count, false);
                USF4Utils.AddIntAsBytes(Data, 0x0C, true); //Script Index Pointer should always be 0x0C, so don't bother doing an "update"
                int ScriptNameIndexPointerPosition = Data.Count;
                USF4Utils.AddIntAsBytes(Data, 0, true);
                List<int> ScriptPointerPositions = new List<int>();
                for (int j = 0; j < sf.Scripts.Count; j++)
                {
                    ScriptPointerPositions.Add(Data.Count);
                    USF4Utils.AddIntAsBytes(Data, -1, true);
                }
                USF4Utils.UpdateIntAtPosition(Data, ScriptNameIndexPointerPosition, Data.Count - scriptFileStartOSList[i]);
                List<int> ScriptNamePointerPositions = new List<int>();
                for (int j = 0; j < sf.Scripts.Count; j++)
                {
                    if (sf.Scripts[j].ScriptSections != null)
                    {
                        ScriptNameDictionary[$"{sf.Name}#{sf.Scripts[j].Name}"] = Data.Count;
                    }
                    ScriptNamePointerPositions.Add(Data.Count);
                    USF4Utils.AddIntAsBytes(Data, 0, true);
                }
                for (int j = 0; j < sf.Scripts.Count; j++)
                {
                    if (sf.Scripts[j].ScriptSections == null)
                    {
                        USF4Utils.UpdateIntAtPosition(Data, ScriptPointerPositions[j], 0x00);
                        continue;
                    }
                    USF4Utils.UpdateIntAtPosition(Data, ScriptPointerPositions[j], Data.Count - scriptFileStartOSList[i]);

                    SFxTScript s = sf.Scripts[j];

                    USF4Utils.AddIntAsBytes(Data, s.HitboxStart, false);
                    USF4Utils.AddIntAsBytes(Data, s.HitboxEnd, false);
                    USF4Utils.AddIntAsBytes(Data, s.IASA, false);
                    USF4Utils.AddIntAsBytes(Data, s.End, false);
                    USF4Utils.AddIntAsBytes(Data, s.UnkLong4_0x08, true);
                    USF4Utils.AddFloatAsBytes(Data, s.XOffset);
                    USF4Utils.AddIntAsBytes(Data, s.ScriptFlags, false);
                    USF4Utils.AddIntAsBytes(Data, s.EndsOn_, false);
                    USF4Utils.AddIntAsBytes(Data, s.Loop, true);
                    USF4Utils.AddIntAsBytes(Data, s.ScriptSections.Count, true);
                    USF4Utils.AddIntAsBytes(Data, 0x20, true); //ScriptSectionPointer; should always be 0x20 so don't bother with "update"

                    Data.AddRange(s.GenerateScriptBytes().ToList());
                }
            }
            //FINISHED WRITING SCRIPTS, START HIT EFFECTS
            for (int i = 0; i < HitEffects.Count; i++)
            {
                if (HitEffects[i].HitEffectDatas == null)
                {
                    USF4Utils.UpdateIntAtPosition(Data, HitEffectPointerPositions[i], 0);
                }
                else
                {
                    USF4Utils.UpdateIntAtPosition(Data, HitEffectPointerPositions[i], Data.Count);
                    Data.AddRange(HitEffects[i].GenerateHitEffectBytes());
                }
            }
            //FINISHED WRITING HIT EFFECTS, NAME INDEX GOOOO
            for (int i = 0; i < ScriptFiles.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(Data, ScriptFileNameDictionary[ScriptFiles[i].Name], Data.Count);
                Data.AddRange(Encoding.ASCII.GetBytes(ScriptFiles[i].Name));
                Data.Add(0x00);

                for (int j = 0; j < ScriptFiles[i].Scripts.Count; j++)
                {
                    if (ScriptFiles[i].Scripts[j].ScriptSections != null)
                    {
                        USF4Utils.UpdateIntAtPosition(Data, ScriptNameDictionary[$"{ScriptFiles[i].Name}#{ScriptFiles[i].Scripts[j].Name}"], Data.Count - scriptFileStartOSList[i]);
                        Data.AddRange(Encoding.ASCII.GetBytes(ScriptFiles[i].Scripts[j].Name));
                        Data.Add(0x00);
                    }
                }
            }
            for (int i = 0; i < HitEffects.Count; i++)
            {
                if (HitEffects[i].HitEffectDatas != null)
                {
                    USF4Utils.UpdateIntAtPosition(Data, HitEffectNameDictionary[HitEffects[i].Name], Data.Count);
                    Data.AddRange(Encoding.ASCII.GetBytes(HitEffects[i].Name));
                    Data.Add(0x00);
                }
            }
            return Data.ToArray();
        }
    }
}
