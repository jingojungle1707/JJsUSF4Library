using JJsUSF4Library.FileClasses.ScriptClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace JJsUSF4Library.FileClasses.BACClasses.SFxTBACClasses
{
    public class SFxTBAC : BAC
    {
        public List<ScriptFile> ScriptFiles { get; set; } = new List<ScriptFile>();
        public List<HitEffect> HitEffects { get; set; } = new List<HitEffect>();

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
                else
                {
#if DEBUG
                    //Debug.WriteLine($"HitEffect: {hitEffectNames[i]}");
#endif
                    HitEffects.Add(new HitEffect(br, hitEffectNames[i], offset + hitEffectPointers[i]));
                }
            }
        }

        public override byte[] GenerateBytes()
        {
            List<byte> data = new List<byte>();

            List<List<int>> scriptNamePointerPositions = new List<List<int>>();
            List<int> hitEffectNamePointerPositions = new List<int>();
            List<int> scriptFileNamePointerPositions = new List<int>();

            //Whenever we write dummy data for a name pointer, we add its position to the dictionary so we can update it later
            //Dictionary<string, int> scriptFileNameDictionary = new Dictionary<string, int>();
            //Dictionary<string, int> scriptNameDictionary = new Dictionary<string, int>();
            //Dictionary<string, int> hitEffectNameDictionary = new Dictionary<string, int>();

            //foreach (ScriptFile sf in ScriptFiles)
            //{
            //    scriptFileNameDictionary.Add(sf.Name, 0);

            //    //Where there's multiple script files, there can be name collisions between stances
            //    //So we prepend the script file name and use a "#" as a delimiter in case we later need to retrieve just the script name
            //    foreach (SFxTScript s in sf.Scripts)
            //    {
            //        if (s.ScriptSections == null) continue;
            //        scriptNameDictionary.Add($"{sf.Name}#{s.Name}", 0);
            //        //{ScriptFiles.IndexOf(sf)}#
            //    }
            //}
            //foreach (HitEffect he in HitEffects)
            //{
            //    if (he.HitEffectDatas == null) continue;
            //    hitEffectNameDictionary.Add(he.Name, 0);
            //}

            data.AddRange(new byte[] { 0x23, 0x42, 0x41, 0x43, 0xFE, 0xFF, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00 });
            USF4Utils.AddIntAsBytes(data, ScriptFiles.Count, false);
            USF4Utils.AddIntAsBytes(data, HitEffects.Count, false);
            //0x10
            int scriptFileIndexPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, 0, true);
            int scriptFileNameIndexPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, 0, true);
            int hitEffectIndexPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, 0, true);
            int hitEffectNameIndexPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, 0, true);
            //0x20
            //Write out main indexes
            USF4Utils.UpdateIntAtPosition(data, scriptFileIndexPointerPosition, data.Count);
            List<int> scriptFilePointerPositions = new List<int>();
            for (int i = 0; i < ScriptFiles.Count; i++)
            {
                scriptFilePointerPositions.Add(data.Count);
                USF4Utils.AddIntAsBytes(data, -1, true);
            }
            USF4Utils.UpdateIntAtPosition(data, scriptFileNameIndexPointerPosition, data.Count);
            for (int i = 0; i < ScriptFiles.Count; i++)
            {
                //scriptFileNameDictionary[ScriptFiles[i].Name] = data.Count;
                scriptFileNamePointerPositions.Add(data.Count);
                USF4Utils.AddIntAsBytes(data, -1, true);
            }
            USF4Utils.UpdateIntAtPosition(data, hitEffectIndexPointerPosition, data.Count);
            List<int> hitEffectPointerPositions = new List<int>();
            for (int i = 0; i < HitEffects.Count; i++)
            {
                hitEffectPointerPositions.Add(data.Count);
                USF4Utils.AddIntAsBytes(data, -1, true);
            }
            USF4Utils.UpdateIntAtPosition(data, hitEffectNameIndexPointerPosition, data.Count);
            //List<int> hitEffectNamePointerPositions = new List<int>();
            for (int i = 0; i < HitEffects.Count; i++)
            {
                if (HitEffects[i].HitEffectDatas != null)
                {
                    //hitEffectNameDictionary[HitEffects[i].Name] = data.Count;
                    hitEffectNamePointerPositions.Add(data.Count);
                }
                else
                {
                    hitEffectNamePointerPositions.Add(-1);
                }
                USF4Utils.AddIntAsBytes(data, 0, true);
            }
            //Start writing ScriptFiles
            List<int> scriptFileStartOSList = new List<int>();
            for (int i = 0; i < ScriptFiles.Count; i++)
            {
                scriptNamePointerPositions.Add(new List<int>());
                scriptFileStartOSList.Add(data.Count);

                USF4Utils.UpdateIntAtPosition(data, scriptFilePointerPositions[i], data.Count);

                ScriptFile sf = ScriptFiles[i];

                USF4Utils.AddIntAsBytes(data, sf.UnkShort0_0x00, false);
                USF4Utils.AddIntAsBytes(data, sf.Scripts.Count, false);
                USF4Utils.AddIntAsBytes(data, 0x0C, true); //Script Index Pointer should always be 0x0C, so don't bother doing an "update"
                int scriptNameIndexPointerPosition = data.Count;
                USF4Utils.AddIntAsBytes(data, 0, true);
                List<int> scriptPointerPositions = new List<int>();
                for (int j = 0; j < sf.Scripts.Count; j++)
                {
                    scriptPointerPositions.Add(data.Count);
                    USF4Utils.AddIntAsBytes(data, -1, true);
                }
                USF4Utils.UpdateIntAtPosition(data, scriptNameIndexPointerPosition, data.Count - scriptFileStartOSList[i]);
                for (int j = 0; j < sf.Scripts.Count; j++)
                {
                    if (sf.Scripts[j].ScriptSections != null)
                    {
                        //scriptNameDictionary[$"{sf.Name}#{sf.Scripts[j].Name}"] = data.Count;
                        scriptNamePointerPositions[i].Add(data.Count);
                    }
                    else scriptNamePointerPositions[i].Add(-1);
                    USF4Utils.AddIntAsBytes(data, 0, true);
                }
                for (int j = 0; j < sf.Scripts.Count; j++)
                {
                    if (sf.Scripts[j].ScriptSections.Count == 0)
                    {
                        USF4Utils.UpdateIntAtPosition(data, scriptPointerPositions[j], 0x00);
                        continue;
                    }
                    USF4Utils.UpdateIntAtPosition(data, scriptPointerPositions[j], data.Count - scriptFileStartOSList[i]);

                    SFxTScript s = sf.Scripts[j];

                    USF4Utils.AddIntAsBytes(data, s.HitboxStart, false);
                    USF4Utils.AddIntAsBytes(data, s.HitboxEnd, false);
                    USF4Utils.AddIntAsBytes(data, s.IASA, false);
                    USF4Utils.AddIntAsBytes(data, s.End, false);
                    USF4Utils.AddIntAsBytes(data, (short)s.PhysicsFlags, false);
                    USF4Utils.AddIntAsBytes(data, s.UnkShort5_0x0A, false);
                    USF4Utils.AddFloatAsBytes(data, s.XOffset);
                    USF4Utils.AddIntAsBytes(data, s.ScriptFlags, false);
                    USF4Utils.AddIntAsBytes(data, s.EndsOn_, false);
                    USF4Utils.AddIntAsBytes(data, s.LoopToFrame, true);
                    USF4Utils.AddIntAsBytes(data, s.ScriptSections.Count, true);
                    USF4Utils.AddIntAsBytes(data, 0x20, true); //ScriptSectionPointer; should always be 0x20 so don't bother with "update"

                    data.AddRange(s.GenerateScriptBytes().ToList());
                }
            }
            //FINISHED WRITING SCRIPTS, START HIT EFFECTS
            for (int i = 0; i < HitEffects.Count; i++)
            {
                if (HitEffects[i].HitEffectDatas == null || HitEffects[i].Name == null)
                {
                    USF4Utils.UpdateIntAtPosition(data, hitEffectPointerPositions[i], 0);
                }
                else
                {
                    USF4Utils.UpdateIntAtPosition(data, hitEffectPointerPositions[i], data.Count);
                    data.AddRange(HitEffects[i].GenerateHitEffectBytes());
                }
            }
            //FINISHED WRITING HIT EFFECTS, NAME INDEX GOOOO
            for (int i = 0; i < ScriptFiles.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(data, scriptFileNamePointerPositions[i], data.Count);
                data.AddRange(Encoding.ASCII.GetBytes(ScriptFiles[i].Name));
                data.Add(0x00);

                for (int j = 0; j < ScriptFiles[i].Scripts.Count; j++)
                {
                    if (ScriptFiles[i].Scripts[j].ScriptSections != null && ScriptFiles[i].Scripts[j].Name != null && scriptNamePointerPositions[i][j] > 0)
                    {
                        USF4Utils.UpdateIntAtPosition(data, scriptNamePointerPositions[i][j], data.Count - scriptFileStartOSList[i]);
                        data.AddRange(Encoding.ASCII.GetBytes(ScriptFiles[i].Scripts[j].Name));
                        data.Add(0x00);
                    }
                }
            }
            for (int i = 0; i < HitEffects.Count; i++)
            {
                if (HitEffects[i].HitEffectDatas != null && HitEffects[i].Name != null)
                {
                    USF4Utils.UpdateIntAtPosition(data, hitEffectNamePointerPositions[i], data.Count);
                    data.AddRange(Encoding.ASCII.GetBytes(HitEffects[i].Name));
                    data.Add(0x00);
                }
            }
            return data.ToArray();
        }
    }
}
