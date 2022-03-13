using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class SubModel
    {
        public byte[] HEXBytes;
        public int EMGTextureIndex;
        public int[] DaisyChain;
        public string Name;
        public List<int> BoneIntegersList;
        public Vector4 MysteryFloats;

        public SubModel()
        {

        }
        public SubModel(BinaryReader br, int offset)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            BoneIntegersList = new List<int>();

            MysteryFloats = new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            //0x10
            br.ReadInt16();
            int daisyChainLength = br.ReadInt16();
            int boneIntegersCount = br.ReadInt16();
            //Convert to string and chop off the null padding with split[0]
            Name = Encoding.ASCII.GetString(br.ReadBytes(0x20)).Split('\0')[0];

            DaisyChain = new int[daisyChainLength];
            for (int i = 0; i < daisyChainLength; i++) DaisyChain[i] = br.ReadInt16();
            for (int i = 0; i < boneIntegersCount; i++) BoneIntegersList.Add(br.ReadInt16());
        }

        public byte[] GenerateBytes()
        {
            List<byte> Data = new List<byte>();

            USF4Utils.AddFloatAsBytes(Data, MysteryFloats.X);
            USF4Utils.AddFloatAsBytes(Data, MysteryFloats.Y);
            USF4Utils.AddFloatAsBytes(Data, MysteryFloats.Z);
            USF4Utils.AddFloatAsBytes(Data, MysteryFloats.W);
            USF4Utils.AddIntAsBytes(Data, EMGTextureIndex, false);
            USF4Utils.AddIntAsBytes(Data, DaisyChain.Length, false);
            USF4Utils.AddIntAsBytes(Data, BoneIntegersList.Count, false);
            Data.AddRange(USF4Utils.StringToNullPaddedBytes(Name, 0x20, out bool _));
            for (int k = 0; k < DaisyChain.Length; k++)
            {
                USF4Utils.AddIntAsBytes(Data, DaisyChain[k], false);
            }
            if (BoneIntegersList != null)
            {
                for (int k = 0; k < BoneIntegersList.Count; k++)
                {
                    USF4Utils.AddIntAsBytes(Data, BoneIntegersList[k], false);
                }
            }

            return Data.ToArray();
        }
    }
}