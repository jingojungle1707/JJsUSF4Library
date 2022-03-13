using System.Collections.Generic;
using System.IO;
using JJsUSF4Library.FileClasses;

namespace JJsUSF4Library
{
    public static class USF4Methods
    {
        public static Dictionary<int, int> GetModelFlag = new Dictionary<int, int>
        {
            { 0x14, 0x0005 },
            { 0x18, 0x0045 },
            { 0x20, 0x0007 },
            { 0x24, 0x0047 },
            { 0x28, 0x0203 },
            { 0x34, 0x0247 },
            { 0x40, 0x02C7 },
        };
        public static Dictionary<int, int> GetModelBitDepth = new Dictionary<int, int>
        {
            { 0x0005, 0x14 },
            { 0x0045, 0x18 },
            { 0x0007, 0x20 },
            { 0x0047, 0x24 },
            { 0x0203, 0x28 },
            { 0x0247, 0x34 },
            { 0x02C7, 0x40 }
        };

        public enum FileType
        {
            UNK = 0x00000000,
            EMZ = 0x5A4D4523,
            CSB = 0x46545540,
            EMA = 0x414D4523,
            EMB = 0x424D4523,
            EMG = 0x474D4523,
            EMM = 0x4D4D4523,
            EMO = 0x4F4D4523,
            LUA = 0x61754C1B,
            DDS = 0x20534444,
            BSR = 0x52534223,
            RY2 = 0x59523223,
            BAC = 0x43414223,
            BCM = 0x4D434223,
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

        public static USF4File CheckFile(byte[] Data)
        {
            USF4File uf = CheckFile(USF4Utils.ReadInt(true, 0x00, Data));

            if (uf.GetType() == typeof(BAC))
            {
                uf = CheckBACFileVersion(USF4Utils.ReadInt(false, 0x06, Data));
            }
            if (uf.GetType() == typeof(BCM))
            {
                uf = CheckBCMFileVersion(USF4Utils.ReadInt(false, 0x06, Data));
            }

            return uf;
        }
        public static USF4File FetchClass(FileType ft)
        {
            switch (ft)
            {
                case FileType.CSB:
                    return new CSB();
                case FileType.EMZ:
                    return new EMZ();
                case FileType.EMA:
                    return new EMA();
                case FileType.EMB:
                    return new EMB();
                case FileType.EMG:
                    return new EMG();
                case FileType.EMM:
                    return new EMM();
                case FileType.EMO:
                    return new EMO();
                case FileType.LUA:
                    return new LUA();
                case FileType.DDS:
                    return new DDS();
                case FileType.BSR:
                    return new BSR();
                case FileType.RY2:
                    return new RY2();
                case FileType.BAC:
                    return new BAC();
                case FileType.BCM:
                    return new BCM();
                default:
                    return new OtherFile();
            }
        }
        public static USF4File FetchVersion(BACFileVersion fileversion)
        {
            switch (fileversion)
            {
                case BACFileVersion.SF4:
                    return new USF4BAC();
                case BACFileVersion.SXT:
                    return new SFxTBAC();
                default:
                    return new OtherFile();
            }
        }

        public static USF4File FetchVersion(BCMFileVersion fileversion)
        {
            switch (fileversion)
            {
                case BCMFileVersion.SF4:
                    return new USF4BCM();
                case BCMFileVersion.SXT:
                    return new SFxTBCM();
                default:
                    return new OtherFile();
            }
        }

        private static USF4File CheckFile(int FileNumber)
        {
            switch ((FileType)FileNumber)
            {
                case FileType.CSB:
                    return new CSB();
                case FileType.EMZ:
                    return new EMZ();
                case FileType.EMA:
                    return new EMA();
                case FileType.EMB:
                    return new EMB();
                case FileType.EMG:
                    return new EMG();
                case FileType.EMM:
                    return new EMM();
                case FileType.EMO:
                    return new EMO();
                case FileType.LUA:
                    return new LUA();
                case FileType.DDS:
                    return new DDS();
                case FileType.BSR:
                    return new BSR();
                case FileType.RY2:
                    return new RY2();
                case FileType.BAC:
                    return new BAC();
                case FileType.BCM:
                    return new BCM();
                default:
                    return new OtherFile();
            }
        }

        private static USF4File CheckBACFileVersion(int VersionNumber)
        {
            if (VersionNumber == 0x20) return new SFxTBAC();
            else if (VersionNumber == 0x28) return new USF4BAC();
            else return new BAC();
        }

        private static USF4File CheckBCMFileVersion(int VersionNumber)
        {
            if (VersionNumber == 0x4C) return new SFxTBCM();
            else if (VersionNumber == 0x28) return new USF4BCM();
            else return new BCM();
        }
    }
}
