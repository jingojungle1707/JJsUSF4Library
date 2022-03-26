﻿using System;
using System.Collections.Generic;
using System.IO;

namespace JJsUSF4Library.FileClasses.ScriptClasses
{
    public class SFxTBCMMove
    {
        public string Name;

        //0x00
        public SFxTBCM.Inputs Input;
        public InputFlag InputFlags;
        public PositionRestrictions PositionRestriction;
        public int
            UnkShort3_0x06,
            UnkShort4_0x08,
            UnkShort5_0x0A;
        public float
            UnknownFloat_0x0C;
        //0x10
        public int
            PositionRestrictionDistance,
            Restriction,
            UnkShort9_0x14,
            UnkShort10_0x16,
            MeterReq,
            MeterLoss;
        public string InputMotion;
        public int Script;
        //0x20
        public byte
            UnkByte15_0x20,
            UnkByte16_0x21;
        public int
            UnkShort17_0x22,
            UnkShort18_0x24,
            AIMinimumDistance;
        public float
            AIMaxDistance,
            UnknownFloat_0x2C,
            //0x30
            UnknownFloat_0x30;
        public int
            UnkShort23_0x34,
            UnkShort24_0x36,
            UnkShort25_0x38;
        public byte
            UnkByte26_0x3A,
            UnkByte27_0x3B,
            UnkByte28_0x3C,
            UnkByte29_0x3D,
            AIFar,
            AIVeryFar;

        public enum PositionRestrictions
        {
            NONE = 0x00,
            CLOSE = 0x01,
            FAR = 0x02,
            HIGH = 0x03
        }
        [Flags]
        public enum InputFlag
        {
            NONE = 0x00,
            LENIENT_DIRECTION = 0x01, //Exclusive 0x02?
            STRICT_DIRECTION = 0x02, //Exclusive 0x01?
            UNK0x0004 = 0x04,
            UNK0x0008 = 0x08,
            BUTTONS = 0x10, //Exclusive 0x20?
            ALL_BUTTONS = 0x20, //Exclusive 010?
            UNK0x0040 = 0x40,
            UNK0x0080 = 0x80,
            UNK0x0100 = 0x100, //USED ON SOME OF PAUL's MOVES
            DIRECTION = 0x200,
            UNK0x0400 = 0x400,
            UNK0x0800 = 0x800,
            ON_PRESS = 0x1000,
            UNK0x02000 = 0x2000,
            ON_RELEASE = 0x4000,
            UNK0x08000 = 0x8000
        }

        public SFxTBCMMove()
        {

        }
        public SFxTBCMMove(BinaryReader br, string name, List<string> inputMotionNames, int offset = 0)
        {
            Name = name;

            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            Input = (SFxTBCM.Inputs)br.ReadInt16();
            InputFlags = (InputFlag)br.ReadInt16();
            PositionRestriction = (PositionRestrictions)br.ReadInt16();
            UnkShort3_0x06 = br.ReadInt16();
            UnkShort4_0x08 = br.ReadInt16();
            UnkShort5_0x0A = br.ReadInt16();
            UnknownFloat_0x0C = br.ReadSingle();
            //0x10
            PositionRestrictionDistance = br.ReadInt16();
            Restriction = br.ReadInt16();
            UnkShort9_0x14 = br.ReadInt16();
            UnkShort10_0x16 = br.ReadInt16();
            MeterReq = br.ReadInt16();
            MeterLoss = br.ReadInt16();
            int inputMotionIndex = br.ReadInt16();
            InputMotion = inputMotionIndex >= 0 ? inputMotionNames[inputMotionIndex] : "NONE";
            Script = br.ReadInt16();
            //0x20
            UnkByte15_0x20 = br.ReadByte();
            UnkByte16_0x21 = br.ReadByte();
            UnkShort17_0x22 = br.ReadInt16();
            UnkShort18_0x24 = br.ReadInt16();
            AIMinimumDistance = br.ReadInt16();
            AIMaxDistance = br.ReadSingle();
            UnknownFloat_0x2C = br.ReadSingle();
            //0x30
            UnknownFloat_0x30 = br.ReadSingle();
            UnkShort23_0x34 = br.ReadInt16();
            UnkShort24_0x36 = br.ReadInt16();
            UnkShort25_0x38 = br.ReadInt16();
            UnkByte26_0x3A = br.ReadByte();
            UnkByte27_0x3B = br.ReadByte();
            UnkByte28_0x3C = br.ReadByte();
            UnkByte29_0x3D = br.ReadByte();
            AIFar = br.ReadByte();
            AIVeryFar = br.ReadByte();
        }

        public SFxTBCMMove(byte[] Data, string name, List<string> inputMotionNames)
        {
            Name = name;

            Input = (SFxTBCM.Inputs)USF4Utils.ReadInt(false, 0x00, Data);
            InputFlags = (InputFlag)USF4Utils.ReadInt(false, 0x02, Data);
            PositionRestriction = (PositionRestrictions)USF4Utils.ReadInt(false, 0x04, Data);
            UnkShort3_0x06 = USF4Utils.ReadInt(false, 0x06, Data);
            UnkShort4_0x08 = USF4Utils.ReadInt(false, 0x08, Data);
            UnkShort5_0x0A = USF4Utils.ReadInt(false, 0x0A, Data);
            UnknownFloat_0x0C = USF4Utils.ReadFloat(0x0C, Data);
            //0x10
            PositionRestrictionDistance = USF4Utils.ReadInt(false, 0x10, Data);
            Restriction = USF4Utils.ReadInt(false, 0x12, Data);
            UnkShort9_0x14 = USF4Utils.ReadInt(false, 0x14, Data);
            UnkShort10_0x16 = USF4Utils.ReadInt(false, 0x16, Data);
            MeterReq = USF4Utils.ReadInt(false, 0x18, Data);
            MeterLoss = USF4Utils.ReadInt(false, 0x1A, Data);
            InputMotion = inputMotionNames[USF4Utils.ReadInt(false, 0x1C, Data)];
            Script = USF4Utils.ReadInt(false, 0x1E, Data);
            //0x20
            UnkByte15_0x20 = Data[0x20];
            UnkByte16_0x21 = Data[0x21];
            UnkShort17_0x22 = USF4Utils.ReadInt(false, 0x22, Data);
            UnkShort18_0x24 = USF4Utils.ReadInt(false, 0x24, Data);
            AIMinimumDistance = USF4Utils.ReadInt(false, 0x26, Data);
            AIMaxDistance = USF4Utils.ReadFloat(0x28, Data);
            UnknownFloat_0x2C = USF4Utils.ReadFloat(0x2C, Data);
            //0x30
            UnknownFloat_0x30 = USF4Utils.ReadFloat(0x30, Data);
            UnkShort23_0x34 = USF4Utils.ReadInt(false, 0x34, Data);
            UnkShort24_0x36 = USF4Utils.ReadInt(false, 0x36, Data);
            UnkShort25_0x38 = USF4Utils.ReadInt(false, 0x38, Data);
            UnkByte26_0x3A = Data[0x3A];
            UnkByte27_0x3B = Data[0x3B];
            UnkByte28_0x3C = Data[0x3C];
            UnkByte29_0x3D = Data[0x3D];
            AIFar = Data[0x3E];
            AIVeryFar = Data[0x3F];
        }
    }
}
