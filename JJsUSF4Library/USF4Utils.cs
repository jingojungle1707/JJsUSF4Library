using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.IO.Compression;
using System.Text;
using JJsUSF4Library.FileClasses;
using System.Numerics;

namespace JJsUSF4Library
{
    public static class USF4Utils
    {
        public static byte[] Compress(byte[] data)
        {
            using (var compressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(compressedStream, CompressionLevel.Optimal))
            {
                zipStream.Write(data, 0, data.Length);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }

        public static USF4File OpenFileStreamCheckCompression(string filepath, bool blind = false)
        {
            byte[] bytes = Array.Empty<byte>();
            USF4Methods.FileType type;
            USF4Methods.BACFileVersion version;

            using (FileStream fsSource = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                //First, we check the header bytes and see if it's an .emz
                using (BinaryReader br = new BinaryReader(fsSource, Encoding.ASCII, false))
                {
                    type = USF4Methods.CheckFileType(br);
                    version = USF4Methods.CheckFileVersion(br);

                    if (type == USF4Methods.FileType.EMZ)
                    {
                        bytes = DecompressStreamToBytes(br);
                        type = USF4Methods.FileType.EMB;
                    }
                }
            }

            USF4File uf = USF4Methods.FetchClass(type);

            if (type is USF4Methods.FileType.BAC) uf = USF4Methods.FetchVersion(version);
            if (type is USF4Methods.FileType.BCM) uf = USF4Methods.FetchVersion((USF4Methods.BCMFileVersion)version);

            if (blind == true)
            {
                if (uf.GetType() == typeof(EMB)) uf = new BlindEMB();
                else uf = new OtherFile();
            }

            uf.Name = Path.GetFileName(filepath);

            //We decompressed bytes, so we need to work from a MemoryStream instead of FileStream
            if (bytes.Length > 0)
            {
                using (MemoryStream msSource = new MemoryStream(bytes))
                {
                    using (BinaryReader br = new BinaryReader(msSource))
                    {
                        uf.ReadFromStream(br);
                    }
                }
            }
            else
            {
                using (FileStream fsSource = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                {
                    using (BinaryReader br = new BinaryReader(fsSource))
                    {
                        uf.ReadFromStream(br, 0, (int)fsSource.Length);
                    }
                }
            }
            return uf;
        }

        public static byte[] DecompressStreamToBytes(BinaryReader br)
        {
            br.BaseStream.Seek(0x10, SeekOrigin.Begin);
            //Convert to byte array via memorystream so we can decompress
            using (MemoryStream ms = new MemoryStream())
            {
                br.BaseStream.CopyTo(ms);
                byte[] bytes = ms.ToArray();
                bytes = ZlibDecoder.Inflate(bytes.ToList()).ToArray();

                return bytes;
            }
        }

        public static List<int[]> FaceIndicesFromDaisyChain(int[] DaisyChain, bool readmode = false)
        {
            List<int[]> FaceIndices = new List<int[]>();

            if (readmode == true && DaisyChain.Length % 3 == 0)
            {
                for (int i = 0; i < DaisyChain.Length / 3; i++)
                {
                    FaceIndices.Add(new int[] { DaisyChain[3 * i + 2], DaisyChain[3 * i + 1], DaisyChain[3 * i] });
                }
            }
            else
            {
                bool bForwards = true;
                for (int i = 0; i < DaisyChain.Length - 2; i++)
                {
                    if (bForwards) //This seems to be backwards?? But it works.
                    {
                        int[] temp = new int[] { DaisyChain[i + 2], DaisyChain[i + 1], DaisyChain[i] };

                        if (temp[0] != temp[1] && temp[1] != temp[2] && temp[2] != temp[0])
                        {
                            FaceIndices.Add(temp);
                        }
                    }
                    else
                    {
                        int[] temp = new int[] { DaisyChain[i], DaisyChain[i + 1], DaisyChain[i + 2] };

                        if (temp[0] != temp[1] && temp[1] != temp[2] && temp[2] != temp[0])
                        {
                            FaceIndices.Add(temp);
                        }
                    }

                    bForwards = !bForwards;
                }
            }

            return FaceIndices;
        }

        #region Binary Methods
        public static void WriteDataToStream(string FilePath, byte[] Data)
        {
            FileStream writeStream;
            writeStream = new FileStream(FilePath, FileMode.Create);

            using (BinaryWriter binWriter = new BinaryWriter(writeStream))
            {
                binWriter.Write(Data, 0, Data.Length);
            }
        }

        public static void AddZeroToLineEnd(List<byte> targetList)
        {
            while (targetList.Count % 0x10 != 0)
            {
                targetList.Add(0x00);
            }
        }

        public static void AddPaddingZeros(List<byte> targetList, int endoffset, int ListLength)
        {
            for (int i = ListLength; i < endoffset; i++)
            {
                targetList.Add(Convert.ToByte("00"));
            }
        }

        public static byte[] StringToNullPaddedBytes(string name, int length,  out bool truncated)
        {
            byte[] name_bytes = Encoding.ASCII.GetBytes(name);
            byte[] bytes = new byte[length];
            truncated = name.Length > length;

            //Cap at zero as that's our max
            for (int i = 0; i < Math.Min(length, name_bytes.Length); i++)
            {
                bytes[i] = name_bytes[i];
            }

            return bytes;
        }

        public static void AddFloatAsBytes(List<byte> targetList, float Data)
        {
            targetList.AddRange(BitConverter.GetBytes(Data));
        }

        public static void AddMatrix4x4AsBytes(List<byte> data, Matrix4x4 m)
        {
            AddFloatAsBytes(data, m.M11);
            AddFloatAsBytes(data, m.M12);
            AddFloatAsBytes(data, m.M13);
            AddFloatAsBytes(data, m.M14);
            AddFloatAsBytes(data, m.M21);
            AddFloatAsBytes(data, m.M22);
            AddFloatAsBytes(data, m.M23);
            AddFloatAsBytes(data, m.M24);
            AddFloatAsBytes(data, m.M31);
            AddFloatAsBytes(data, m.M32);
            AddFloatAsBytes(data, m.M33);
            AddFloatAsBytes(data, m.M34);
            AddFloatAsBytes(data, m.M41);
            AddFloatAsBytes(data, m.M42);
            AddFloatAsBytes(data, m.M43);
            AddFloatAsBytes(data, m.M44);
        }

        public static void AddIntAsBytes(List<byte> targetList, int Data, bool IsLong)
        {
            if (IsLong)
            {
                targetList.AddRange(BitConverter.GetBytes(Data));
            }
            else
            {
                if (Data > 0xFFFF)
                {
                    Console.WriteLine("AddIntAsBytes: Short out of range, using 0xFFFF instead.");
                    targetList.AddRange(BitConverter.GetBytes((ushort)0xFFFF));
                }
                else targetList.AddRange(BitConverter.GetBytes((ushort)Data));
            }
        }

        public static void AddSignedShortAsBytes(List<byte> targetList, int Data)
        {
            targetList.AddRange(BitConverter.GetBytes((short)Data));
        }

        public static List<byte> UpdateIntAtPosition(List<byte> targetList, int Position, int newValue)
        {
            byte[] bytes = BitConverter.GetBytes(newValue);
            for (int i = 0; i < 4; i++)
            {
                targetList[Position + i] = bytes[i];
            }

            return targetList;
        }

        public static List<byte> UpdateIntAtPosition(List<byte> targetList, int Position, int newValue, out int outValue)
        {
            byte[] bytes = BitConverter.GetBytes(newValue);
            for (int i = 0; i < 4; i++)
            {
                targetList[Position + i] = bytes[i];
            }
            outValue = newValue;

            return targetList;
        }

        public static List<byte> UpdateShortAtPosition(List<byte> targetList, int Position, int newValue)
        {
            byte[] bytes = BitConverter.GetBytes(newValue);
            for (int i = 0; i < 2; i++)
            {
                targetList[Position + i] = bytes[i];
            }

            return targetList;
        }
        public static List<byte> UpdateShortAtPosition(List<byte> targetList, int Position, int newValue, out int outValue)
        {
            byte[] bytes = BitConverter.GetBytes(newValue);
            for (int i = 0; i < 2; i++)
            {
                targetList[Position + i] = bytes[i];
            }
            outValue = newValue;

            return targetList;
        }

        public static float ReadShortFloat(byte[] bytes)
        {
            var intVal = BitConverter.ToInt32(new byte[] { bytes[0], bytes[1], 0, 0 }, 0);

            int mant = intVal & 0x03ff;
            int exp = intVal & 0x7c00;
            if (exp == 0x7c00) exp = 0x3fc00;
            else if (exp != 0)
            {
                exp += 0x1c000;
                if (mant == 0 && exp > 0x1c400)
                    return BitConverter.ToSingle(BitConverter.GetBytes((intVal & 0x8000) << 16 | exp << 13 | 0x3ff), 0);
            }
            else if (mant != 0)
            {
                exp = 0x1c400;
                do
                {
                    mant <<= 1;
                    exp -= 0x400;
                } while ((mant & 0x400) == 0);
                mant &= 0x3ff;
            }
            return BitConverter.ToSingle(BitConverter.GetBytes((intVal & 0x8000) << 16 | (exp | mant) << 13), 0);
        }

        private static byte[] I2B(int input)
        {
            var bytes = BitConverter.GetBytes(input);
            return new byte[] { bytes[0], bytes[1] };
        }

        public static byte[] ToInt(float twoByteFloat)
        {
            int fbits = BitConverter.ToInt32(BitConverter.GetBytes(twoByteFloat), 0);
            int sign = fbits >> 16 & 0x8000;
            int val = (fbits & 0x7fffffff) + 0x1000;
            if (val >= 0x47800000)
            {
                if ((fbits & 0x7fffffff) >= 0x47800000)
                {
                    if (val < 0x7f800000) return I2B(sign | 0x7c00);
                    return I2B(sign | 0x7c00 | (fbits & 0x007fffff) >> 13);
                }
                return I2B(sign | 0x7bff);
            }
            if (val >= 0x38800000) return I2B(sign | val - 0x38000000 >> 13);
            if (val < 0x33000000) return I2B(sign);
            val = (fbits & 0x7fffffff) >> 23;
            return I2B(sign | ((fbits & 0x7fffff | 0x800000) + (0x800000 >> val - 102) >> 126 - val));
        }

        //TODO LARGE UINT32s WILL GET BODIED BY CONVERSION TO INT32?
        public static int ReadInt(bool IsLong, int Offset, byte[] Data)
        {
            int ReturnValue;
            int HexInt;
            if (IsLong)
            {
                HexInt = BitConverter.ToInt32(Data, Offset);
            }
            else
            {
                HexInt = BitConverter.ToUInt16(Data, Offset);
            }
            ReturnValue = HexInt;
            //Console.WriteLine(ReturnValue);
            return ReturnValue;
        }

        public static void ReadToNextNonNullByte(int Offset, byte[] Data, out int EndOffset, out int ByteValue)
        {
            ByteValue = 0;
            EndOffset = 0;

            for (int i = Offset; i < Data.Length; i++)
            {
                if (Data[i] != 0x00)
                {
                    ByteValue = Data[i];
                    EndOffset = i + 0x01; //We want the next byte
                    break;
                }
            }
        }

        public static float ReadFloat(int Offset, byte[] Data)
        {
            float ReturnValue;
            float HexFloat;
            HexFloat = BitConverter.ToSingle(Data, Offset);
            ReturnValue = HexFloat;
            return ReturnValue;
        }

        public static byte[] ReadStringToArray(int offset, int length, byte[] targetArray, int targetArrayLength)
        {

            int maxLen = targetArrayLength - offset;

            if (maxLen < length) { length = maxLen; }

            byte[] byteReturn = new byte[length];

            for (int i = 0; i < length; i++)
            {
                byteReturn[i] = targetArray[i + offset];
            }
            return byteReturn;
        }

        public static string ReadZString(this BinaryReader reader)
        {
            var result = new StringBuilder();
            while (true)
            {
                byte b = reader.ReadByte();
                if (0 == b)
                    break;
                result.Append((char)b);
            }
            return result.ToString();
        }

        public static byte[] ReadZeroTermStringToArray(int offset, byte[] targetArray, int targetArrayLength)
        {

            int maxLen = targetArrayLength - offset;

            int length = 0;

            for (int i = 0; i < maxLen; i++)
            {
                if (targetArray[offset + i] == 0) { length = i; break; }
            }

            byte[] byteReturn = new byte[length];

            for (int i = 0; i < length; i++)
            {
                byteReturn[i] = targetArray[i + offset];
            }
            return byteReturn;
        }
        public static byte[] ReadZeroTermStringToArray(uint offset, byte[] targetArray, uint targetArrayLength)
        {

            uint maxLen = targetArrayLength - offset;

            uint length = 0;

            for (uint i = 0; i < maxLen; i++)
            {
                if (targetArray[offset + i] == 0) { length = i; break; }
            }

            byte[] byteReturn = new byte[length];

            for (uint i = 0; i < length; i++)
            {
                byteReturn[i] = targetArray[i + offset];
            }
            return byteReturn;
        }

        public static T[] Slice<T>(this T[] source, int start, int length)
        {
            if (length == 0) length = source.Length - start;
            // Return new array.
            T[] res = new T[length];
            for (int i = 0; i < length; i++)
            {
                res[i] = source[i + start];
            }
            return res;
        }
        #endregion Binary Methods
    }
}
