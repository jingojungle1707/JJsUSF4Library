using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class HitboxDatablock
    {
        public int
            Damage,
            Stun,
            Effect,
            UnkShort3_0x06,
            SelfHitstop,
            SelfShaking,
            TargetHitStop,
            TargetShaking,
            //0x10
            HitGFX1,
            UnkShort9_0x12,
            UnkLong10_0x14,
            HitGFX2,
            UnkShort12_0x1A,
            UnkLong13_0x1C,
            //0x20
            HitSFX1,
            HitSFX2,
            TargetSFX,
            ArcadeScore,
            SelfMeter,
            TargetMeter,
            JuggleStart,
            TargetAnimTime,
            //0x30
            MiscFlag;
        public float
            VelX, VelY, VelZ,
            //0x40
            PushbackDistance, AccX, AccY, AccZ;

        public HitboxDatablock(BinaryReader br, int offset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            Damage = br.ReadInt16();
            Stun = br.ReadInt16();
            Effect = br.ReadInt16();
            UnkShort3_0x06 = br.ReadInt16();
            SelfHitstop = br.ReadInt16();
            SelfShaking = br.ReadInt16();
            TargetHitStop = br.ReadInt16();
            TargetShaking = br.ReadInt16();
            //0x10
            HitGFX1 = br.ReadInt16();
            UnkShort9_0x12 = br.ReadInt16();
            UnkLong10_0x14 = br.ReadInt32();
            HitGFX2 = br.ReadInt16();
            UnkShort12_0x1A = br.ReadInt16();
            UnkLong13_0x1C = br.ReadInt32();
            //0x20
            HitSFX1 = br.ReadInt16();
            HitSFX2 = br.ReadInt16();
            TargetSFX = br.ReadInt16();
            ArcadeScore = br.ReadInt16();
            SelfMeter = br.ReadInt16();
            TargetMeter = br.ReadInt16();
            JuggleStart = br.ReadInt16();
            TargetAnimTime = br.ReadInt16();
            //0x30
            MiscFlag = br.ReadInt32();
            VelX = br.ReadSingle();
            VelY = br.ReadSingle();
            VelZ = br.ReadSingle();
            //0x40
            PushbackDistance = br.ReadSingle();
            AccX = br.ReadSingle();
            AccY = br.ReadSingle();
            AccZ = br.ReadSingle();
        }

        public byte[] GenerateBytes()
        {
            List<byte> Data = new List<byte>();

            USF4Utils.AddIntAsBytes(Data, Damage, false);
            USF4Utils.AddIntAsBytes(Data, Stun, false);
            USF4Utils.AddIntAsBytes(Data, Effect, false);
            USF4Utils.AddIntAsBytes(Data, UnkShort3_0x06, false);
            USF4Utils.AddIntAsBytes(Data, SelfHitstop, false);
            USF4Utils.AddIntAsBytes(Data, SelfShaking, false);
            USF4Utils.AddIntAsBytes(Data, TargetHitStop, false);
            USF4Utils.AddIntAsBytes(Data, TargetShaking, false);
            //0x10
            USF4Utils.AddIntAsBytes(Data, HitGFX1, false);
            USF4Utils.AddIntAsBytes(Data, UnkShort9_0x12, false);
            USF4Utils.AddIntAsBytes(Data, UnkLong10_0x14, true);
            USF4Utils.AddIntAsBytes(Data, HitGFX2, false);
            USF4Utils.AddIntAsBytes(Data, UnkShort12_0x1A, false);
            USF4Utils.AddIntAsBytes(Data, UnkLong13_0x1C, true);
            //0x20
            USF4Utils.AddIntAsBytes(Data, HitSFX1, false);
            USF4Utils.AddIntAsBytes(Data, HitSFX2, false);
            USF4Utils.AddIntAsBytes(Data, TargetSFX, false);
            USF4Utils.AddIntAsBytes(Data, ArcadeScore, false);
            USF4Utils.AddIntAsBytes(Data, SelfMeter, false);
            USF4Utils.AddIntAsBytes(Data, TargetMeter, false);
            USF4Utils.AddIntAsBytes(Data, JuggleStart, false);
            USF4Utils.AddIntAsBytes(Data, TargetAnimTime, false);
            //0x30
            USF4Utils.AddIntAsBytes(Data, MiscFlag, true);
            USF4Utils.AddFloatAsBytes(Data, VelX);
            USF4Utils.AddFloatAsBytes(Data, VelY);
            USF4Utils.AddFloatAsBytes(Data, VelZ);
            //0x40
            USF4Utils.AddFloatAsBytes(Data, PushbackDistance);
            USF4Utils.AddFloatAsBytes(Data, AccX);
            USF4Utils.AddFloatAsBytes(Data, AccY);
            USF4Utils.AddFloatAsBytes(Data, AccZ);

            return Data.ToArray();
        }
    }
}
