using System;
using System.Collections.Generic;
using System.IO;

namespace JJsUSF4Library.FileClasses.BACClasses.SFxTBACClasses
{
    public class SFxTScript
    {
        #region Properties
        public string Name { get; set; }
        //0x00
        public short HitboxStart { get; set; } //Short
        public short HitboxEnd { get; set; } //Short
        public short IASA { get; set; } //Short
        public short End { get; set; } //Short
        public PhysicsFlag PhysicsFlags { get; set; } //Short
        public short UnkShort5_0x0A { get; set; } //Short
        public float XOffset { get; set; }
        //0x10
        public short ScriptFlags { get; set; } //Short
        public short EndsOn_ { get; set; } //Short
        public int LoopToFrame { get; set; } //Long?
        public List<ScriptSection> ScriptSections { get; set; } = new List<ScriptSection>();
        #endregion

        [Flags]
        public enum PhysicsFlag : ushort
        {
            UNK0x01 = 0x01,
            RETAIN_X_VELOCITY = 0x02,
            RETAIN_Y_VELOCITY = 0x04,
            RETAIN_Z_VELOCITY = 0x08,
            RETAIN_X_ACCELERATION = 0x10,
            RETAIN_Y_ACCELERATION = 0x20,
            RETAIN_Z_ACCELERATION = 0x40,
            RETAIN_X_VELOCITY_2 = 0x80,
            RETAIN_Y_VELOCITY_2 = 0x0100,
            RETAIN_Z_VELOCITY_2 = 0x0200,
            UNK0x0400 = 0x0400,
            UNK0x0800 = 0x0800,
            UNK0x1000 = 0x1000,
            UNK0x2000 = 0x2000,
            UNK0x4000 = 0x4000,
            UNK0x8000 = 0x8000,
        }

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
            PhysicsFlags = (PhysicsFlag)br.ReadUInt16();
            UnkShort5_0x0A = br.ReadInt16();
            XOffset = br.ReadSingle();
            //0x10
            ScriptFlags = br.ReadInt16();
            EndsOn_ = br.ReadInt16();
            LoopToFrame = br.ReadInt32();
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

            List<int> commandHeaderPointerPositions = new List<int>();
            List<int> commandDataPointerPositions = new List<int>();
            for (int i = 0; i < ScriptSections.Count; i++)
            {
                ScriptSection ss = ScriptSections[i];
                Data.Add((byte)ss.Type);
                Data.Add(ss.UnkByte1_0x01);
                USF4Utils.AddIntAsBytes(Data, ss.Commands.Count, false);
                commandHeaderPointerPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, -1, true);
                commandDataPointerPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, -1, true);
            }
            for (int i = 0; i < ScriptSections.Count; i++)
            {
                //Adjust by - i * 0x0C to account for pointers counting from header positions
                USF4Utils.UpdateIntAtPosition(Data, commandHeaderPointerPositions[i], Data.Count - i * 0x0c);
                ScriptSection ss = ScriptSections[i];

                for (int j = 0; j < ss.Commands.Count; j++)
                {
                    USF4Utils.AddIntAsBytes(Data, ss.Commands[j].StartTick, false);
                    USF4Utils.AddIntAsBytes(Data, ss.Commands[j].EndTick, false);
                }

                USF4Utils.UpdateIntAtPosition(Data, commandDataPointerPositions[i], Data.Count - i * 0x0c);
                Data.AddRange(ss.GenerateScriptSectionBytes());
            }
            return Data.ToArray();
        }
#endregion
    }
    
}