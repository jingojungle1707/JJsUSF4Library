using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using JJsUSF4Library.FileClasses;

namespace JJsUSF4Library
{
    #region ZlibDecoder
    /// <summary>
    /// public domain zlib decode    
    /// original: v0.2  Sean Barrett 2006-11-18
    /// ported to C# by Tammo Hinrichs, 2012-08-02
    /// simple implementation
    /// - all input must be provided in an upfront buffer
    /// - all output is written to a single output buffer
    /// - Warning: This is SLOW. It's no miracle .NET as well as Mono implement DeflateStream natively.
    /// </summary>
    public class ZlibDecoder
    {
        /// <summary>
        /// Decode deflated data
        /// </summary>
        /// <param name="compressed">deflated input data</param>
        /// <returns>uncompressed output</returns>
        public static List<byte> Inflate(IList<byte> compressed)
        {
            return new ZlibDecoder { In = compressed }.Inflate();
        }

        #region internal

        // fast-way is faster to check than jpeg huffman, but slow way is slower
        private const int FastBits = 9; // accelerate all cases in default tables
        private const int FastMask = ((1 << FastBits) - 1);

        private static readonly int[] DistExtra = new[]
        {
            0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9,
            10, 10, 11, 11, 12, 12, 13, 13
        };

        private static readonly int[] LengthBase = new[]
        {
            3, 4, 5, 6, 7, 8, 9, 10, 11, 13,
            15, 17, 19, 23, 27, 31, 35, 43, 51, 59,
            67, 83, 99, 115, 131, 163, 195, 227, 258, 0, 0
        };

        private static readonly int[] LengthExtra = new[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4,
            4, 4, 4, 5, 5, 5, 5, 0, 0, 0
        };

        private static readonly int[] DistBase = new[]
        {
            1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193,
            257, 385, 513, 769, 1025, 1537, 2049, 3073, 4097, 6145, 8193,
            12289, 16385, 24577, 0, 0
        };

        private static readonly int[] LengthDezigzag = new[]
        {
            16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2,
            14,
            1, 15
        };

        // @TODO: should statically initialize these for optimal thread safety
        private static readonly byte[] DefaultLength = new byte[288];
        private static readonly byte[] DefaultDistance = new byte[32];

        private List<byte> Out;
        private UInt32 CodeBuffer;
        private int NumBits;

        private Huffman Distance;
        private Huffman Length;

        private int InPos;
        private IList<byte> In;

        private static void InitDefaults()
        {
            int i; // use <= to match clearly with spec
            for (i = 0; i <= 143; ++i) DefaultLength[i] = 8;
            for (; i <= 255; ++i) DefaultLength[i] = 9;
            for (; i <= 279; ++i) DefaultLength[i] = 7;
            for (; i <= 287; ++i) DefaultLength[i] = 8;

            for (i = 0; i <= 31; ++i) DefaultDistance[i] = 5;
        }

        private static int BitReverse16(int n)
        {
            n = ((n & 0xAAAA) >> 1) | ((n & 0x5555) << 1);
            n = ((n & 0xCCCC) >> 2) | ((n & 0x3333) << 2);
            n = ((n & 0xF0F0) >> 4) | ((n & 0x0F0F) << 4);
            n = ((n & 0xFF00) >> 8) | ((n & 0x00FF) << 8);
            return n;
        }

        private static int BitReverse(int v, int bits)
        {
            Debug.Assert(bits <= 16);
            // to bit reverse n bits, reverse 16 and shift
            // e.g. 11 bits, bit reverse and shift away 5
            return BitReverse16(v) >> (16 - bits);
        }

        private int Get8()
        {
            return InPos >= In.Count ? 0 : In[InPos++];
        }

        private void FillBits()
        {
            do
            {
                Debug.Assert(CodeBuffer < (1U << NumBits));
                CodeBuffer |= (uint)(Get8() << NumBits);
                NumBits += 8;
            } while (NumBits <= 24);
        }

        private uint Receive(int n)
        {
            if (NumBits < n) FillBits();
            var k = (uint)(CodeBuffer & ((1 << n) - 1));
            CodeBuffer >>= n;
            NumBits -= n;
            return k;
        }

        private int HuffmanDecode(Huffman z)
        {
            int s;
            if (NumBits < 16) FillBits();
            int b = z.Fast[CodeBuffer & FastMask];
            if (b < 0xffff)
            {
                s = z.Size[b];
                CodeBuffer >>= s;
                NumBits -= s;
                return z.Value[b];
            }

            // not resolved by fast table, so compute it the slow way
            // use jpeg approach, which requires MSbits at top
            int k = BitReverse((int)CodeBuffer, 16);
            for (s = FastBits + 1; ; ++s)
                if (k < z.MaxCode[s])
                    break;
            if (s == 16) return -1; // invalid code!
                                    // code size is s, so:
            b = (k >> (16 - s)) - z.FirstCode[s] + z.FirstSymbol[s];
            Debug.Assert(z.Size[b] == s);
            CodeBuffer >>= s;
            NumBits -= s;
            return z.Value[b];
        }

        private void ParseHuffmanBlock()
        {
            for (; ; )
            {
                int z = HuffmanDecode(Length);
                if (z < 256)
                {
                    if (z < 0) throw new Exception("bad huffman code"); // error in huffman codes
                    Out.Add((byte)z);
                }
                else
                {
                    if (z == 256) return;
                    z -= 257;
                    int len = LengthBase[z];
                    if (LengthExtra[z] != 0) len += (int)Receive(LengthExtra[z]);
                    z = HuffmanDecode(Distance);
                    if (z < 0) throw new Exception("bad huffman code");
                    int dist = DistBase[z];
                    if (DistExtra[z] != 0) dist += (int)Receive(DistExtra[z]);
                    dist = Out.Count - dist;
                    if (dist < 0) throw new Exception("bad dist");
                    for (int i = 0; i < len; i++, dist++)
                        Out.Add(Out[dist]);
                }
            }
        }

        private void ComputeHuffmanCodes()
        {
            var lenCodes = new byte[286 + 32 + 137]; //padding for maximum single op
            var codeLengthSizes = new byte[19];

            uint hlit = Receive(5) + 257;
            uint hdist = Receive(5) + 1;
            uint hclen = Receive(4) + 4;

            for (int i = 0; i < hclen; ++i)
                codeLengthSizes[LengthDezigzag[i]] = (byte)Receive(3);

            var codeLength = new Huffman(new ArraySegment<byte>(codeLengthSizes));

            int n = 0;
            while (n < hlit + hdist)
            {
                int c = HuffmanDecode(codeLength);
                Debug.Assert(c >= 0 && c < 19);
                if (c < 16)
                    lenCodes[n++] = (byte)c;
                else if (c == 16)
                {
                    c = (int)Receive(2) + 3;
                    for (int i = 0; i < c; i++) lenCodes[n + i] = lenCodes[n - 1];
                    n += c;
                }
                else if (c == 17)
                {
                    c = (int)Receive(3) + 3;
                    for (int i = 0; i < c; i++) lenCodes[n + i] = 0;
                    n += c;
                }
                else
                {
                    Debug.Assert(c == 18);
                    c = (int)Receive(7) + 11;
                    for (int i = 0; i < c; i++) lenCodes[n + i] = 0;
                    n += c;
                }
            }
            if (n != hlit + hdist) throw new Exception("bad codelengths");
            Length = new Huffman(new ArraySegment<byte>(lenCodes, 0, (int)hlit));
            Distance = new Huffman(new ArraySegment<byte>(lenCodes, (int)hlit, (int)hdist));
        }

        private void ParseUncompressedBlock()
        {
            var header = new byte[4];
            if ((NumBits & 7) != 0)
                Receive(NumBits & 7); // discard
                                      // drain the bit-packed data into header
            int k = 0;
            while (NumBits > 0)
            {
                header[k++] = (byte)(CodeBuffer & 255); // wtf this warns?
                CodeBuffer >>= 8;
                NumBits -= 8;
            }
            Debug.Assert(NumBits == 0);
            // now fill header the normal way
            while (k < 4)
                header[k++] = (byte)Get8();
            int len = header[1] * 256 + header[0];
            int nlen = header[3] * 256 + header[2];
            if (nlen != (len ^ 0xffff)) throw new Exception("zlib corrupt");
            if (InPos + len > In.Count) throw new Exception("read past buffer");

            // TODO: this sucks. DON'T USE LINQ.
            Out.AddRange(In.Skip(InPos).Take(len));
            InPos += len;
        }

        private List<byte> Inflate()
        {
            Out = new List<byte>();
            NumBits = 0;
            CodeBuffer = 0;

            bool final;
            do
            {
                final = Receive(1) != 0;
                var type = (int)Receive(2);
                if (type == 0)
                {
                    ParseUncompressedBlock();
                }
                else if (type == 3)
                {
                    throw new Exception("invalid block type");
                }
                else
                {
                    if (type == 1)
                    {
                        // use fixed code lengths
                        if (DefaultDistance[31] == 0) InitDefaults();
                        Length = new Huffman(new ArraySegment<byte>(DefaultLength));
                        Distance = new Huffman(new ArraySegment<byte>(DefaultDistance));
                    }
                    else
                    {
                        ComputeHuffmanCodes();
                    }
                    ParseHuffmanBlock();
                }
            } while (!final);

            return Out;
        }


        private class Huffman
        {
            public readonly UInt16[] Fast = new UInt16[1 << FastBits];
            public readonly UInt16[] FirstCode = new UInt16[16];
            public readonly UInt16[] FirstSymbol = new UInt16[16];
            public readonly int[] MaxCode = new int[17];
            public readonly Byte[] Size = new Byte[288];
            public readonly UInt16[] Value = new UInt16[288];

            public Huffman(ArraySegment<byte> sizeList)
            {
                int i;
                int k = 0;
                var nextCode = new int[16];
                var sizes = new int[17];

                // DEFLATE spec for generating codes
                for (i = 0; i < Fast.Length; i++) Fast[i] = 0xffff;
                for (i = 0; i < sizeList.Count; ++i)
                    ++sizes[sizeList.Array[i + sizeList.Offset]];
                sizes[0] = 0;
                for (i = 1; i < 16; ++i)
                    Debug.Assert(sizes[i] <= (1 << i));
                int code = 0;
                for (i = 1; i < 16; ++i)
                {
                    nextCode[i] = code;
                    FirstCode[i] = (UInt16)code;
                    FirstSymbol[i] = (UInt16)k;
                    code = (code + sizes[i]);
                    if (sizes[i] != 0)
                        if (code - 1 >= (1 << i)) throw new Exception("bad codelengths");
                    MaxCode[i] = code << (16 - i); // preshift for inner loop
                    code <<= 1;
                    k += sizes[i];
                }
                MaxCode[16] = 0x10000; // sentinel
                for (i = 0; i < sizeList.Count; ++i)
                {
                    int s = sizeList.Array[i + sizeList.Offset];
                    if (s != 0)
                    {
                        int c = nextCode[s] - FirstCode[s] + FirstSymbol[s];
                        Size[c] = (byte)s;
                        Value[c] = (UInt16)i;
                        if (s <= FastBits)
                        {
                            int j = BitReverse(nextCode[s], s);
                            while (j < (1 << FastBits))
                            {
                                Fast[j] = (UInt16)c;
                                j += (1 << s);
                            }
                        }
                        ++nextCode[s];
                    }
                }
            }
        }

        #endregion
    }
    #endregion

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

        public static USF4File OpenFileStreamCheckCompression(string filepath)
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
