using JJsUSF4Library.FileClasses.SubfileClasses;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JJsUSF4Library.FileClasses
{
    public class USF4BAC : BAC
    {
        //0x08
        public int
            UnkShort00_0x08,
            UnkShort01_0x0A;

        public List<float> MysteryFloatBlock_0x28; //Length 0x2A0, so 0xA8 floats. No pointer to it, no float count, same length in every file...?
                                                    //(though not the same floats)
        public List<USF4Script> Scripts;
        public List<USF4Script> VFXScripts;
        public List<Hitbox> Hitboxes;

        public USF4BAC()
        {

        }
        public USF4BAC(BinaryReader br, string name, int offset = 0)
        {
            Name = name;
            ReadFromStream(br, offset);
        }
        public USF4BAC(byte[] Data, string name)
        {
            Name = name;
            ReadFile(Data);
        }

        public override void ReadFromStream(BinaryReader br, int offset = 0, int fileLength = 0)
        {
            MysteryFloatBlock_0x28 = new List<float>();
            Scripts = new List<USF4Script>();
            VFXScripts = new List<USF4Script>();
            Hitboxes = new List<Hitbox>();

            List<int> scriptPointers = new List<int>();
            List<int> scriptNamePointers = new List<int>();
            List<int> vFXPointers = new List<int>();
            List<int> vFXNamePointers = new List<int>();
            List<int> hitboxPointers = new List<int>();

            List<string> scriptNames = new List<string>();
            List<string> vfxNames = new List<string>();

            br.BaseStream.Seek(offset + 0x08, SeekOrigin.Begin);
            UnkShort00_0x08 = br.ReadInt16();
            UnkShort01_0x0A = br.ReadInt16();

            int scriptCount = br.ReadInt16();
            int vFXCount = br.ReadInt16();
            //0x10
            int hitboxCount = br.ReadInt32();
            int scriptIndexPointer = br.ReadInt32();
            //Don't need these pointers but read them to consume the stream bytes
            int vFXIndexPointer = br.ReadInt32();
            int scriptNameIndexPointer = br.ReadInt32();
            //0x20
            int vFXNameIndexPointer = br.ReadInt32();
            int hitboxIndexPointer = br.ReadInt32();

            for (int i = 0; i < 0xA8; i++) MysteryFloatBlock_0x28.Add(br.ReadSingle());

            //Bunch of back-to-back pointer indexes so we can just read them in with a single seek
            br.BaseStream.Seek(offset + scriptIndexPointer, SeekOrigin.Begin);
            for (int i = 0; i < scriptCount; i++) scriptPointers.Add(br.ReadInt32());
            for (int i = 0; i < vFXCount; i++) vFXPointers.Add(br.ReadInt32());
            for (int i = 0; i < scriptCount; i++) scriptNamePointers.Add(br.ReadInt32());
            for (int i = 0; i < vFXCount; i++) vFXNamePointers.Add(br.ReadInt32());
            for (int i = 0; i < hitboxCount; i++) hitboxPointers.Add(br.ReadInt32());

            //Seek to start of the name index then just trust the z-strings
            br.BaseStream.Seek(offset + scriptNamePointers[0], SeekOrigin.Begin);
            for (int i = 0; i < scriptCount; i++)
            {
                if (scriptNamePointers[i] != 0) scriptNames.Add(USF4Utils.ReadZString(br));
                else scriptNames.Add(string.Empty);
            }
            for (int i = 0; i < vFXCount; i++)
            {
                if (vFXNamePointers[i] != 0) vfxNames.Add(USF4Utils.ReadZString(br));
                else vfxNames.Add(string.Empty);
            }

            //Read scripts
            for (int i = 0; i < scriptCount; i++)
            {
                if (scriptPointers[i] != 0) Scripts.Add(new USF4Script(br, scriptNames[i], offset + scriptPointers[i]));
                else Scripts.Add(new USF4Script());
            }
            //Read vfx - these are actually just more scripts, HURRAY NO EXTRA WORK!!!!
            for (int i = 0; i < vFXCount; i++)
            {
                if (vFXPointers[i] != 0) VFXScripts.Add(new USF4Script(br, vfxNames[i], offset + vFXPointers[i]));
                else VFXScripts.Add(new USF4Script());
            }
            //Read hitboxes
            for (int i = 0; i < hitboxCount; i++)
            {
                if (hitboxPointers[i] != 0) Hitboxes.Add(new Hitbox(br, offset + hitboxPointers[i]));
                else Hitboxes.Add(new Hitbox());
            }
        }

        public override byte[] GenerateBytes()
        {
            List<byte> Data = new List<byte>();
            Data.AddRange(new byte[] { 0x23, 0x42, 0x41, 0x43, 0xFE, 0xFF, 0x28, 0x00 });

            USF4Utils.AddIntAsBytes(Data, UnkShort00_0x08, false);
            USF4Utils.AddIntAsBytes(Data, UnkShort01_0x0A, false);
            USF4Utils.AddIntAsBytes(Data, Scripts.Count, false);
            USF4Utils.AddIntAsBytes(Data, VFXScripts.Count, false);
            //0x10
            USF4Utils.AddIntAsBytes(Data, Hitboxes.Count, true);
            int scriptIndexPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            int vFXIndexPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            int scriptNameIndexPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            //0x20
            int vfxNameIndexPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            int hitboxIndexPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            //Write mystery floats
            for (int i = 0; i < 0xA8; i++) USF4Utils.AddFloatAsBytes(Data, MysteryFloatBlock_0x28[i]);
            //Write script pointers
            List<int> scriptPointerPositions = new List<int>();
            USF4Utils.UpdateIntAtPosition(Data, scriptIndexPointerPosition, Data.Count);
            for (int i = 0; i < Scripts.Count; i++)
            {
                scriptPointerPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, -1, true);
            }
            List<int> vfxPointerPositions = new List<int>();
            //Write VFX script pointers
            USF4Utils.UpdateIntAtPosition(Data, vFXIndexPointerPosition, Data.Count);
            for (int i = 0; i < VFXScripts.Count; i++)
            {
                vfxPointerPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, -1, true);
            }
            //Write script name pointers
            List<int> scriptNamePointerPositions = new List<int>();
            USF4Utils.UpdateIntAtPosition(Data, scriptNameIndexPointerPosition, Data.Count);
            for (int i = 0; i < Scripts.Count; i++)
            {
                scriptNamePointerPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, -1, true);
            }
            //Write vfx name pointers
            List<int> vfxNamePointerPositions = new List<int>();
            USF4Utils.UpdateIntAtPosition(Data, vfxNameIndexPointerPosition, Data.Count);
            for (int i = 0; i < VFXScripts.Count; i++)
            {
                vfxNamePointerPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, -1, true);
            }
            //Write hitbox pointers
            List<int> hitboxPointerPositions = new List<int>();
            USF4Utils.UpdateIntAtPosition(Data, hitboxIndexPointerPosition, Data.Count);
            for (int i = 0; i < Hitboxes.Count; i++)
            {
                hitboxPointerPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, -1, true);
            }
            //Write scripts
            for (int i = 0; i < Scripts.Count; i++)
            {
                //Check for empty commands
                if (Scripts[i].Commands == null) USF4Utils.UpdateIntAtPosition(Data, scriptPointerPositions[i], 0);
                else
                {
                    USF4Utils.UpdateIntAtPosition(Data, scriptPointerPositions[i], Data.Count);
                    Data.AddRange(Scripts[i].GenerateBytes());
                }
            }
            //Write VFX scripts
            for (int i = 0; i < VFXScripts.Count; i++)
            {
                //Check for empty commands
                if (VFXScripts[i].Commands == null) USF4Utils.UpdateIntAtPosition(Data, vfxPointerPositions[i], 0);
                else
                {
                    USF4Utils.UpdateIntAtPosition(Data, vfxPointerPositions[i], Data.Count);
                    Data.AddRange(VFXScripts[i].GenerateBytes());
                }
            }
            //Write Hitbox data
            for (int i = 0; i < Hitboxes.Count; i++)
            {
                if (Hitboxes[i].Datablocks == null) USF4Utils.UpdateIntAtPosition(Data, hitboxPointerPositions[i], 0);
                else
                {
                    USF4Utils.UpdateIntAtPosition(Data, hitboxPointerPositions[i], Data.Count);
                    Data.AddRange(Hitboxes[i].GenerateBytes());
                }
            }
            //Write name index
            for (int i = 0; i < Scripts.Count; i++)
            {
                if (Scripts[i].Commands == null) USF4Utils.UpdateIntAtPosition(Data, scriptNamePointerPositions[i], 0);
                else
                {
                    USF4Utils.UpdateIntAtPosition(Data, scriptNamePointerPositions[i], Data.Count);
                    Data.AddRange(Encoding.ASCII.GetBytes(Scripts[i].Name));
                    Data.Add(0x00);
                }
            }
            for (int i = 0; i < VFXScripts.Count; i++)
            {
                if (VFXScripts[i].Commands == null) USF4Utils.UpdateIntAtPosition(Data, vfxNamePointerPositions[i], 0);
                else
                {
                    USF4Utils.UpdateIntAtPosition(Data, vfxNamePointerPositions[i], Data.Count);
                    Data.AddRange(Encoding.ASCII.GetBytes(VFXScripts[i].Name));
                    Data.Add(0x00);
                }
            }

            return Data.ToArray();
        }

    }
}