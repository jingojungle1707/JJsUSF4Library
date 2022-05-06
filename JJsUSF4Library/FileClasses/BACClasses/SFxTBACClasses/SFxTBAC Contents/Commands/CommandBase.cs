using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.BACClasses.SFxTBACClasses
{
    public abstract class CommandBase : ISFxTCommand
    {
        public int StartTick { get; set; }
        public int EndTick { get; set; }
        public virtual void ReadCommandDataBlock(BinaryReader br, int startTick, int endTick)
        {

        }
        public virtual byte[] GenerateDataBlockBytes()
        {
            List<byte> Data = new List<byte>();
            return Data.ToArray();
        }

        public enum COMMANDTYPE
        {
            UNKNOWN = 0xFF,
            Flow = 0x00,
            Speed = 0x01,
            Status = 0x02,
            Physics = 0x03,
            Cancel = 0x04,
            ETC = 0x05,
            Unk6 = 0x06,
            Hitbox = 0x07,
            Hurtbox = 0x08,
            Pushbox = 0x09,
            Animation = 0x0A,
            AnimationMod = 0x0B,
            SFX = 0x0C,
            GFX = 0x0D,
        }
        public enum AnimationFile
        {
            COMMON = 0x00,
            UNK1 = 0x01, //Could be CMN.cam.ema from resource\battle\chara\CMN? Appears in unused cody ex knifethrow?
            OBJ = 0x02,
            CAM = 0x03,
            FCE = 0x04,
            INTRO = 0x05,
            APPEAL = 0x06,
            UNK7 = 0x07,
            UNK8 = 0x08,
            UNK9 = 0x09,
            UNKA = 0x0A
        }
        public enum SoundFile
        {
            COMMON = 0x00,
            UNK1 = 0x01,
            CV = 0x02,
            UNK3 = 0x03,
            UNK4 = 0x04,
            UNK5 = 0x05,
            UNK6 = 0x06,
            UNK7 = 0x07,
            UNK8 = 0x08,
            UNK9 = 0x09,
            UNKA = 0x0A
        }

        public static Dictionary<COMMANDTYPE, int> CommandLengths = new Dictionary<COMMANDTYPE, int>()
        {
            { COMMANDTYPE.Flow, 0x0C},
            { COMMANDTYPE.Speed, 0x04},
            { COMMANDTYPE.Status, 0x08},
            { COMMANDTYPE.Physics, 0x0C},
            { COMMANDTYPE.Cancel, 0x04},
            { COMMANDTYPE.ETC, 0x08},
            { COMMANDTYPE.Unk6, 0x08},
            { COMMANDTYPE.Hitbox, 0x30},
            { COMMANDTYPE.Hurtbox, 0x2C},
            { COMMANDTYPE.Pushbox, 0x18},
            { COMMANDTYPE.Animation, 0x08},
            { COMMANDTYPE.AnimationMod, 0x08},
            { COMMANDTYPE.SFX, 0x08},
            { COMMANDTYPE.GFX, 0x08}
        };
    }
}
