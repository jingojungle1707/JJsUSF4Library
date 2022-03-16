using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.ScriptClasses
{
    class USF4BCMMove
    {
        public string Name;

        public int
            Input,
            MoveFlags,
            PositionRestriction,
            Restriction,
            StateRestriction,
            MiscRestriction,
            UltraRestriction;
        public float
            PositionRestrictionDistance;
        public int
            EXRequirement,
            EXCost,
            UltraRequirement,
            UltraCost,
            Script,
            InputMotion,
            Features;
        public float 
            CPUMinRange,
            //0x30
            CPUMaxRange;
        public int
            UnkLong_0x34,
            UnkShort_0x38,
            CPUPassiveMove,
            CPUCounterMove,
            CPUVsStand,
            //0x40
            CPUVsCrouch,
            CPUVsAir,
            CPUVsDown,
            CPUVsStunned,
            CPUProbeMove,
            CPUVsVeryClose,
            CPUVsClose,
            CPUVsMidRange,
            //050
            CPUVsFar,
            CPUVsVeryFar;

        public USF4BCMMove(BinaryReader br, string name, int offset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            Name = name;
            //Length 0x54
            Input = br.ReadInt16();
            MoveFlags = br.ReadInt16();
            PositionRestriction = br.ReadInt16();
            Restriction = br.ReadInt32();
            StateRestriction = br.ReadInt16();
            MiscRestriction = br.ReadInt32();
            //0x10
            UltraRestriction = br.ReadInt32();
            PositionRestrictionDistance = br.ReadSingle();
            EXRequirement = br.ReadInt16();
            EXCost = br.ReadInt16();
            UltraRequirement = br.ReadInt16();
            UltraCost = br.ReadInt16();
            //0x20
            InputMotion = br.ReadInt32();
            Script = br.ReadInt32();//Maybe short?
            Features = br.ReadInt32();
            CPUMinRange = br.ReadSingle();
            //0x30
            CPUMaxRange = br.ReadSingle();
            UnkLong_0x34 = br.ReadInt32();
            UnkShort_0x38 = br.ReadInt16();
            CPUPassiveMove = br.ReadInt16();
            CPUCounterMove = br.ReadInt16();
            CPUVsStand = br.ReadInt16();
            //0x40
            CPUVsCrouch = br.ReadInt16();
            CPUVsAir = br.ReadInt16();
            CPUVsDown = br.ReadInt16();
            CPUVsStunned = br.ReadInt16();
            CPUProbeMove = br.ReadInt16();
            CPUVsVeryClose = br.ReadInt16();
            CPUVsClose = br.ReadInt16();
            CPUVsMidRange = br.ReadInt16();
            //0x50
            CPUVsFar = br.ReadInt16();
            CPUVsVeryFar = br.ReadInt16();
        }

        public List<byte> GenerateBytes()
        {
            List<byte> data = new List<byte>();

            USF4Utils.AddIntAsBytes(data, Input, false);
            USF4Utils.AddIntAsBytes(data, MoveFlags, false);
            USF4Utils.AddIntAsBytes(data, PositionRestriction, false);
            USF4Utils.AddIntAsBytes(data, Restriction, true);
            USF4Utils.AddIntAsBytes(data, StateRestriction, false);
            USF4Utils.AddIntAsBytes(data, MiscRestriction, true);
            //0x10
            USF4Utils.AddIntAsBytes(data, UltraRestriction, true);
            USF4Utils.AddFloatAsBytes(data, PositionRestrictionDistance);
            USF4Utils.AddIntAsBytes(data, EXRequirement, false);
            USF4Utils.AddIntAsBytes(data, EXCost, false);
            USF4Utils.AddIntAsBytes(data, UltraRequirement, false);
            USF4Utils.AddIntAsBytes(data, UltraCost, false);
            //0x20
            USF4Utils.AddIntAsBytes(data, InputMotion, true);
            USF4Utils.AddIntAsBytes(data, Script, true);
            USF4Utils.AddIntAsBytes(data, Features, true);
            USF4Utils.AddFloatAsBytes(data, CPUMinRange);
            //0x30
            USF4Utils.AddFloatAsBytes(data, CPUMaxRange);
            USF4Utils.AddIntAsBytes(data, UnkLong_0x34, true);
            USF4Utils.AddIntAsBytes(data, UnkShort_0x38, false);
            USF4Utils.AddIntAsBytes(data, CPUPassiveMove, false);
            USF4Utils.AddIntAsBytes(data, CPUCounterMove, false);
            USF4Utils.AddIntAsBytes(data, CPUVsStand, false);
            //0x40
            USF4Utils.AddIntAsBytes(data, CPUVsCrouch, false);
            USF4Utils.AddIntAsBytes(data, CPUVsAir, false);
            USF4Utils.AddIntAsBytes(data, CPUVsDown, false);
            USF4Utils.AddIntAsBytes(data, CPUVsStunned, false);
            USF4Utils.AddIntAsBytes(data, CPUProbeMove, false);
            USF4Utils.AddIntAsBytes(data, CPUVsVeryClose, false);
            USF4Utils.AddIntAsBytes(data, CPUVsClose, false);
            USF4Utils.AddIntAsBytes(data, CPUVsMidRange, false);
            //0x50
            USF4Utils.AddIntAsBytes(data, CPUVsFar, false);
            USF4Utils.AddIntAsBytes(data, CPUVsVeryFar, false);

            return data;
        }
    }
}
