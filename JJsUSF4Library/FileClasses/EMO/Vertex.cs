using System.Collections.Generic;
using System.Numerics;
using System.Drawing;
using System.IO;
using System.Linq;
using System;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class Vertex   //Your classic everyday point in 3D space.
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector2 UV;
        public Color Color;

        public int BoneCount;
        public List<BoneIDWeightPair> BoneIDWeightPairs;

        public List<string> BoneNames
        {
            get { return BoneIDWeightPairs.Select(o => o.BoneName).ToList(); }
        }
        public List<float> BoneWeights
        {
            get { return BoneIDWeightPairs.Select(o => o.Weight).ToList(); }
        }

        public Vertex()
        {

        }
        public Vertex(BinaryReader br, int bitFlag, List<string> boneMap = default(List<string>))
        {
            BoneIDWeightPairs = new List<BoneIDWeightPair>();

            //Vertex Data - really hoping this is always true
            if ((bitFlag & 0x01) == 0x01) Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

            //Normals
            if ((bitFlag & 0x02) == 0x02) Normal = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

            //UV Co-ordinates
            if ((bitFlag & 0x04) == 0x04) UV = new Vector2(br.ReadSingle(), br.ReadSingle());

            //Vertex Tangents
            if ((bitFlag & 0x80) == 0x80) Tangent = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

            //UV Colour.
            if ((bitFlag & 0x40) == 0x40) Color = Color.FromArgb(br.ReadInt32());

            //Bone weighting
            if ((bitFlag & 0x0200) == 0x0200)
            {
                byte[] ids = br.ReadBytes(4);
                List<float> weights = new List<float>() { br.ReadSingle(), br.ReadSingle(), br.ReadSingle() };
                //Fill in weight 4
                if (weights.Sum() < 1f) weights.Add((float)Math.Round(1f - weights.Sum(),6));
                else weights.Add(0);

                for (int i = 0; i < 4; i++) BoneIDWeightPairs.Add(new BoneIDWeightPair(ids[i], weights[i]));
            }
        }

        public byte[] GenerateBytes(int bitFlag, List<string> boneMap = default)
        {
            List<byte> Data = new List<byte>();

            if ((bitFlag & 0x01) == 0x01) 
            {
                USF4Utils.AddFloatAsBytes(Data, Position.X);
                USF4Utils.AddFloatAsBytes(Data, Position.Y);
                USF4Utils.AddFloatAsBytes(Data, Position.Z);
            }
            //Normals
            if ((bitFlag & 0x02) == 0x02)
            {
                USF4Utils.AddFloatAsBytes(Data, Normal.X);
                USF4Utils.AddFloatAsBytes(Data, Normal.Y);
                USF4Utils.AddFloatAsBytes(Data, Normal.Z);
            }

            //UV Co-ordinates
            if ((bitFlag & 0x04) == 0x04)
            {
                USF4Utils.AddFloatAsBytes(Data, UV.X);
                USF4Utils.AddFloatAsBytes(Data, UV.Y);
            }

            //Vertex Tangents
            if ((bitFlag & 0x80) == 0x80)
            {
                USF4Utils.AddFloatAsBytes(Data, Tangent.X);
                USF4Utils.AddFloatAsBytes(Data, Tangent.Y);
                USF4Utils.AddFloatAsBytes(Data, Tangent.Z);
            }

            //UV Colour.
            if ((bitFlag & 0x40) == 0x40) USF4Utils.AddIntAsBytes(Data, Color.ToArgb(), true);

            //Bone weighting
            if ((bitFlag & 0x0200) == 0x0200)
            {
                //If we've not been passed any bone data, use the stored bone IDs
                if (boneMap == null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (BoneNames != null && BoneNames.Count > i)
                        {
                            Data.Add(Convert.ToByte(BoneIDWeightPairs[i].BoneID));
                        }
                        else Data.Add(0x00);
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        if (BoneWeights != null && BoneWeights.Count > i)
                        {
                            USF4Utils.AddFloatAsBytes(Data, BoneWeights[i]);
                        }
                        else USF4Utils.AddFloatAsBytes(Data, 0f);
                    }
                }
                else
                {
                    List<object[]> idWeightPairs = new List<object[]>();
                    List<byte> ids = new List<byte>();
                    List<float> weights = new List<float>();
                    foreach (BoneIDWeightPair biwp in BoneIDWeightPairs)
                    {
                        idWeightPairs.Add(new object[] { (byte)boneMap.IndexOf(biwp.BoneName), biwp.Weight });
                    }

                    //List<int> ids2 = idWeightPairs

                    //idWeightPairs = idWeightPairs.OrderBy(o => o[0]).ToList();

                    ids = idWeightPairs.Select(o => (byte)o[0]).ToList();
                    weights = idWeightPairs.Select(o => (float)o[1]).ToList();

                    while (ids.Count < 4) ids.Add(0x00);
                    while (weights.Count < 3) weights.Add(0f);

                    Data.AddRange(ids);
                    USF4Utils.AddFloatAsBytes(Data, weights[0]);
                    USF4Utils.AddFloatAsBytes(Data, weights[1]);
                    USF4Utils.AddFloatAsBytes(Data, weights[2]);
                }
            }

            return Data.ToArray();
        }

        /// <summary>
        /// Stores vertex bone weight data, or partially complete weight data until mapping can be completed.
        /// bool Mapped indicated whether completed weight data is available, becomes true once a string is assigned to BoneName
        /// </summary>
        public class BoneIDWeightPair
        {

            private int _boneID;
            private string _boneName = string.Empty;
            public float Weight;
            private bool _mapped;

            /// <summary>
            /// Read-only variable containing the bone index from a partially-initialised WeightPair.
            /// Used to re-construct Vertex data for "orphaned" EMGs with no skeleton data available.
            /// </summary>
            public int BoneID
            {
                get { return _boneID; }
            }
            /// <summary>
            /// True if a string has been assigned to BoneName
            /// </summary>
            public bool Mapped
            {
                get { return _mapped; }
            }
            /// <summary>
            /// Assigning a string to BoneName completes the mapping the sets Mapped to true.
            /// </summary>
            public string BoneName
            {
                get { return _boneName; }
                set
                {
                    _boneName = value;
                    _mapped = true;
                }
            }
            /// <summary>
            /// Constructs a completed mapping. Sets Mapped to true.
            /// </summary>
            public BoneIDWeightPair(string boneName, float weight)
            {
                BoneName = boneName;
                Weight = weight;
                _mapped = true;
            }
            /// <summary>
            /// Constructs a partially completed map. Sets Mapped to false until a value is assigned to BoneName.
            /// </summary>
            public BoneIDWeightPair(int boneId, float weight)
            {
                _boneID = boneId;
                Weight = weight;
            }
        }
    }
}