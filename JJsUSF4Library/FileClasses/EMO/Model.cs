using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class Model
    {
        public int BitFlag;
        public int BitDepth;
        public List<Vertex> VertexData;
        public int ReadMode; //0 = plain triangles, 1 = stripped (normal type for USF4)   
        public List<SubModel> SubModels;
        public List<EMGModelTexture> Textures;
        public List<Vector4> CullData;

        public Model()
        { 
        }
        public Model(BinaryReader br, int offset = 0, List<string> boneNames = default(List<string>))
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            #region Initialise Lists
            SubModels = new List<SubModel>();
            Textures = new List<EMGModelTexture>();
            VertexData = new List<Vertex>();
            List<int> subModelsPointers = new List<int>();
            #endregion

            #region Read Header
            BitFlag = br.ReadInt32();
            int textureCount = br.ReadInt32();
            br.ReadInt32();
            int textureListPointer = br.ReadInt32();
            //0x10
            int vertexCount = br.ReadInt16();
            BitDepth = br.ReadInt16();
            int vertexListPointer = br.ReadInt32();
            ReadMode = br.ReadInt16();
            int subModelCount = br.ReadInt16();
            int subModelIndexPointer = br.ReadInt32();
            //0x20
            CullData = new List<Vector4>()
            {
                new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
            };
            #endregion

            #region Read Textures
            //0x50
            br.BaseStream.Seek(offset + textureListPointer, SeekOrigin.Begin);
            br.BaseStream.Seek(offset + br.ReadInt32(), SeekOrigin.Begin);
            for (int i = 0; i < textureCount; i++) Textures.Add(new EMGModelTexture(br));
            #endregion

            #region Read SubModels
            br.BaseStream.Seek(offset + subModelIndexPointer, SeekOrigin.Begin);
            for (int i = 0; i < subModelCount; i++) subModelsPointers.Add(br.ReadInt32());
            for (int i = 0; i < subModelCount; i++) SubModels.Add(new SubModel(br, offset + subModelsPointers[i]));
            #endregion

            #region Read Vertices
            br.BaseStream.Seek(offset + vertexListPointer, SeekOrigin.Begin);
            for (int i = 0; i < vertexCount; i++)
            {
                Vertex v = new Vertex(br, BitFlag);
                if (boneNames != null)
                {
                    foreach (SubModel sm in SubModels)
                    {
                        if (sm.DaisyChain.Contains(i))
                        {
                            for (int j = 0; j < v.BoneIDWeightPairs.Count; j++)
                            {
                                //Skip
                                //if (j > 0 && v.BoneIDWeightPairs[j].BoneID == 0)
                                //{
                                //    v.BoneIDWeightPairs.RemoveRange(j, v.BoneIDWeightPairs.Count - j);
                                //    break;
                                //}

                                v.BoneIDWeightPairs[j].BoneName = boneNames[sm.BoneIntegersList[v.BoneIDWeightPairs[j].BoneID]];
                            }
                            break;
                        }
                    }
                }

                VertexData.Add(v);
            }
            #endregion
        }

        public byte[] GenerateBytes(List<string> boneNames = default)
        {
            List<byte> Data = new List<byte>();

            Data.AddRange(GenerateHeaderBytes(out int vertexListPointerPosition));

            //Generate boneMaps to pass to submodels
            List<string>[] boneMaps = default;
            if (boneNames != null) boneMaps = BuildBoneMaps(boneNames);
            Data.AddRange(GenerateSubModelBytes(Data, boneMaps, boneNames));

            USF4Utils.UpdateIntAtPosition(Data, vertexListPointerPosition, Data.Count);

            Data.AddRange(GenerateVertexBytes(boneNames));

            return Data.ToArray();
        }

        public byte[] GenerateSFxTBytes(List<string> boneNames)
        {
            List<byte> data = new List<byte>();

            List<string>[] boneMaps = default;
            if (boneNames != null) boneMaps = BuildBoneMaps(boneNames);

            data.AddRange(GenerateHeaderBytes(out int _));

            data.AddRange(GenerateSubModelBytes(data, boneMaps, boneNames));

            return data.ToArray();
        }

        public List<byte> GenerateVertexBytes(List<string> boneNames = default)
        {
            List<byte> data = new List<byte>();

            if (boneNames == default)
            {
                for (int i = 0; i < VertexData.Count; i++) data.AddRange(VertexData[i].GenerateBytes(BitFlag));
            }
            else
            {
                List<string>[] boneMaps = BuildBoneMaps(boneNames);

                for (int i = 0; i < VertexData.Count; i++)
                {
                    if (boneMaps != null)
                    {
                        for (int j = 0; j < SubModels.Count; j++)
                        {
                            if (SubModels[j].DaisyChain.Contains(i))
                            {
                                data.AddRange(VertexData[i].GenerateBytes(BitFlag, boneMaps[j]));
                                break;
                            }
                        }
                    }
                    else data.AddRange(VertexData[i].GenerateBytes(BitFlag));
                }
            }

            return data;
        }

        private List<byte> GenerateHeaderBytes(out int vertexListPointerPosition)
        {
            List<byte> data = new List<byte>();

            USF4Utils.AddIntAsBytes(data, BitFlag, true);
            USF4Utils.AddIntAsBytes(data, Textures.Count, true);
            USF4Utils.AddIntAsBytes(data, 0x00, true);  //Padding
            int textureListPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, -1, true);
            USF4Utils.AddIntAsBytes(data, VertexData.Count, false);
            USF4Utils.AddIntAsBytes(data, BitDepth, false);
            vertexListPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, -1, true);
            USF4Utils.AddIntAsBytes(data, ReadMode, false);
            USF4Utils.AddIntAsBytes(data, SubModels.Count, false);
            int subModelListPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, -1, true);
            if (CullData == null) //If we don't have cull data, make some up
            {
                data.AddRange(new List<byte> {  0x00, 0x0A, 0x1B, 0x3C, 0xC3, 0xA4, 0x9E, 0x40,
                                                0x80, 0x89, 0x27, 0x3E, 0xF6, 0x79, 0x3E, 0x41,
                                                0x50, 0x94, 0xA1, 0xC0, 0xFD, 0x14, 0x9D, 0xBF,
                                                0xEE, 0x94, 0x0A, 0xC1, 0x43, 0xB9, 0x0D, 0x43,
                                                0x5A, 0x2F, 0xA2, 0x40, 0x63, 0x47, 0x32, 0x41,
                                                0x3A, 0xD1, 0x0F, 0x41, 0xF6, 0x79, 0xBE, 0x41 });
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    USF4Utils.AddFloatAsBytes(data, CullData[i].X);
                    USF4Utils.AddFloatAsBytes(data, CullData[i].Y);
                    USF4Utils.AddFloatAsBytes(data, CullData[i].Z);
                    USF4Utils.AddFloatAsBytes(data, CullData[i].W);
                }
            }

            USF4Utils.UpdateIntAtPosition(data, textureListPointerPosition, data.Count);
            List<int> texturePointerPositions = new List<int>();
            for (int i = 0; i < Textures.Count; i++)
            {
                texturePointerPositions.Add(data.Count);
                USF4Utils.AddIntAsBytes(data, -1, true);
            }
            for (int i = 0; i < Textures.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(data, texturePointerPositions[i], data.Count);
                data.AddRange(Textures[i].GenerateBytes());
            }

            USF4Utils.UpdateIntAtPosition(data, subModelListPointerPosition, data.Count);

            return data;
        }

        private List<byte> GenerateSubModelBytes(List<byte> data, List<string>[] boneMaps = default, List<string> boneNames = default)
        {
            List<int> submodelPointerPositions = new List<int>();
            for (int i = 0; i < SubModels.Count; i++)
            {
                submodelPointerPositions.Add(data.Count);
                USF4Utils.AddIntAsBytes(data, -1, true);
            }

            for (int i = 0; i < SubModels.Count; i++)
            {
                //Convert string bonemaps to shorts
                if (boneMaps != null)
                {
                    SubModels[i].BoneIntegersList.Clear();
                    foreach (string boneName in boneMaps[i]) SubModels[i].BoneIntegersList.Add(boneNames.IndexOf(boneName));
                }
                USF4Utils.AddZeroToLineEnd(data);
                USF4Utils.UpdateIntAtPosition(data, submodelPointerPositions[i], data.Count);

                data.AddRange(SubModels[i].GenerateBytes());
            }

            return data;
        }

        private List<string>[] BuildBoneMaps(List<string> boneNames)
        {
            List<string>[] boneMaps = new List<string>[SubModels.Count];
            for (int j = 0; j < SubModels.Count; j++) boneMaps[j] = new List<string>();
            
            for (int i = 0; i < VertexData.Count; i++)
            {
                Vertex v = VertexData[i];
                
                for (int j = 0; j < SubModels.Count; j++)
                {
                    if (SubModels[j].DaisyChain.Contains(i))
                    {
                        foreach (string name in v.BoneNames)
                        {
                            if (!boneMaps[j].Contains(name)) boneMaps[j].Add(name);
                        }
                    }
                }
            }

            return boneMaps;
        }
    }
}