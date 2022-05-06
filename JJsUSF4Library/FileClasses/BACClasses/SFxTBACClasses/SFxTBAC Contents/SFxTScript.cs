using System.Collections.Generic;
using System.IO;

namespace JJsUSF4Library.FileClasses.BACClasses.SFxTBACClasses
{
    public class SFxTScript
    {
        #region Members
        public string Name;
        //0x00
        public int
            HitboxStart, //Short
            HitboxEnd, //Short
            IASA, //Short
            End, //Short
            UnkLong4_0x08; //Long
        public float
            XOffset;
        //0x10
        public int
            ScriptFlags, //Short
            EndsOn_, //Short
            Loop; //Long?

        public List<ScriptSection> ScriptSections;

        #endregion

        #region Constructors
        public SFxTScript()
        {

        }

        public SFxTScript(BinaryReader br, string name, int offset = 0)
        {
            Name = name;

            ScriptSections = new List<ScriptSection>();

            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            HitboxStart = br.ReadInt16();
            HitboxEnd = br.ReadInt16();
            IASA = br.ReadInt16();
            End = br.ReadInt16();
            UnkLong4_0x08 = br.ReadInt32();
            XOffset = br.ReadSingle();
            //0x10
            ScriptFlags = br.ReadInt16();
            EndsOn_ = br.ReadInt16();
            Loop = br.ReadInt32();
            int scriptSectionCount = br.ReadInt32();
            br.ReadInt32(); //script section pointer - always 0x20 so just skip it
            //0x20
            //Read script sections - each header is length 0x0C, so we use i * 0x0C for each section's start offset
            for (int i = 0; i < scriptSectionCount; i++) ScriptSections.Add(new ScriptSection(br, offset + 0x20 + i * 0x0C));
        }
        #endregion

        #region Methods
        public byte[] GenerateScriptBytes()
        {
            List<byte> Data = new List<byte>();

            List<int> CommandHeaderPointerPositions = new List<int>();
            List<int> CommandDataPointerPositions = new List<int>();
            for (int i = 0; i < ScriptSections.Count; i++)
            {
                ScriptSection ss = ScriptSections[i];
                Data.Add((byte)ss.Type);
                Data.Add(ss.UnkByte1_0x01);
                USF4Utils.AddIntAsBytes(Data, ss.Commands.Count, false);
                CommandHeaderPointerPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, -1, true);
                CommandDataPointerPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, -1, true);
            }
            for (int i = 0; i < ScriptSections.Count; i++)
            {
                //Adjust by - i * 0x0C to account for pointers counting from header positions
                USF4Utils.UpdateIntAtPosition(Data, CommandHeaderPointerPositions[i], Data.Count - i * 0x0c);
                ScriptSection ss = ScriptSections[i];

                for (int j = 0; j < ss.Commands.Count; j++)
                {
                    USF4Utils.AddIntAsBytes(Data, ss.Commands[j].StartTick, false);
                    USF4Utils.AddIntAsBytes(Data, ss.Commands[j].EndTick, false);
                }

                USF4Utils.UpdateIntAtPosition(Data, CommandDataPointerPositions[i], Data.Count - i * 0x0c);
                Data.AddRange(ss.GenerateScriptSectionBytes());
            }
            return Data.ToArray();
        }
#endregion
    }
    
}