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
    }
}
