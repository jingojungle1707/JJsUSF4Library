using System;
using System.IO;
using JJsUSF4Library.FileClasses;

namespace JJsUSF4Library
{
    public static class USF4Methods
    {
        /// <summary>
        /// Bitflag declaring what vertex data is contained in this model's vertex block
        /// </summary>
        [Flags]
        public enum ModelFlag
        {
            /// <summary>
            /// Length 0x0C
            /// </summary>
            POSITION = 0x01,
            /// <summary>
            /// Length 0x0C
            /// </summary>
            NORMAL = 0x02,
            /// <summary>
            /// Lengh 0x08
            /// </summary>
            UV = 0x04,
            /// <summary>
            /// Length 0x04
            /// </summary>
            COLOR = 0x40,
            /// <summary>
            /// Length 0x0C
            /// </summary>
            TANGENT = 0x80,
            /// <summary>
            /// Length 0x10
            /// </summary>
            BONEWEIGHT = 0x200
        }

        public enum FileType
        {
            UNK = 0x00000000,
            EMZ = 0x5A4D4523,
            CSB = 0x46545540,
            EMA = 0x414D4523,
            EMB = 0x424D4523,
            EME = 0x454D4523,
            EMG = 0x474D4523,
            EMM = 0x4D4D4523,
            EMO = 0x4F4D4523,
            LUA = 0x61754C1B,
            DDS = 0x20534444,
            BSR = 0x52534223,
            RY2 = 0x59523223,
            BAC = 0x43414223,
            BCM = 0x4D434223,
            BVS = 0x53564223,
            BTR = 0x52544223,
        }

        public enum BACFileVersion
        {
            UNK,
            SXT = 0x0020,    //Street Fighter x Tekken
            SF4 = 0x0028,    //Street Fighter 4
        }
        public enum BCMFileVersion
        {
            UNK,
            SF4 = 0x0028,    //Street Fighter 4
            SXT = 0x004C,    //Street Fighter x Tekken
        }

        public static FileType CheckFileType(BinaryReader br, int offset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            FileType ft = (FileType)br.ReadInt32();

            return ft;
        }
        public static BACFileVersion CheckFileVersion(BinaryReader br, int offset = 0)
        {
            br.BaseStream.Seek(offset + 0x06, SeekOrigin.Begin);
            BACFileVersion fv = (BACFileVersion)br.ReadInt16();

            return fv;
        }

        public static USF4File FetchClass(FileType ft)
        {
            return ft switch
            {
                FileType.CSB => new CSB(),
                FileType.EMZ => new EMZ(),
                FileType.EMA => new EMA(),
                FileType.EMB => new EMB(),
                FileType.EME => new FileClasses.EMEClasses.BasicEME(),
                FileType.EMG => new EMG(),
                FileType.EMM => new EMM(),
                FileType.EMO => new EMO(),
                FileType.LUA => new LUA(),
                FileType.DDS => new DDS(),
                FileType.BSR => new BSR(),
                FileType.RY2 => new RY2(),
                FileType.BAC => new BAC(),
                FileType.BCM => new BCM(),
                FileType.BVS => new FileClasses.BVSClasses.BVS(),
                FileType.BTR => new FileClasses.BTRClasses.BTR(),
                _ => new OtherFile(),
            };
        }
        public static USF4File FetchVersion(BACFileVersion fileversion)
        {
            return fileversion switch
            {
                BACFileVersion.SF4 => new USF4BAC(),
                BACFileVersion.SXT => new SFxTBAC(),
                BACFileVersion.UNK => new OtherFile(),
                _ => new OtherFile(),
            };
        }

        public static USF4File FetchVersion(BCMFileVersion fileversion)
        {
            return fileversion switch
            {
                BCMFileVersion.SF4 => new USF4BCM(),
                BCMFileVersion.SXT => new SFxTBCM(),
                BCMFileVersion.UNK => new OtherFile(),
                _ => new OtherFile(),
            };
        }
    }
}
