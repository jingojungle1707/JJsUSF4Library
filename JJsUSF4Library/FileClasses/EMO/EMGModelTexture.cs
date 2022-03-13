using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class EMGModelTexture
    {
        public int TextureLayers;
        public List<EMGModelTextureLayer> Layers;

        public EMGModelTexture()
        {

        }

        public EMGModelTexture(BinaryReader br)
        {
            Layers = new List<EMGModelTextureLayer>();

            int textureLayers = br.ReadInt32();

            for (int i = 0; i < textureLayers; i++)
            {
                br.ReadByte();
                Layers.Add(new EMGModelTextureLayer()
                {
                    TextureIndex = br.ReadInt16(),
                    UnkByte1_0x03 = br.ReadByte(),
                    ScaleUV = new Vector2(br.ReadSingle(), br.ReadSingle())
                });
            }
        }

        /// <summary>
        /// <para>Generates a texture layer with textureIndex.</para>
        /// If normalMapIndex is provided, generates a 2-layer texture.
        /// U/V scale values default to 1 if not provided.
        /// </summary>
        /// <param name="textureIndex"></param>
        /// <param name="normalMapIndex"></param>
        /// <param name="scaleX"></param>
        /// <param name="scaleY"></param>
        public EMGModelTexture(int textureIndex, int normalMapIndex = -1, float scaleX_tex = 0, float scaleY_tex = 0, float scaleX_norm = 0, float scaleY_norm = 0)
        {
            Layers = new List<EMGModelTextureLayer>();

            Layers.Add(new EMGModelTextureLayer()
            {
                TextureIndex = textureIndex,
                ScaleUV = new Vector2((scaleX_tex != 0) ? scaleX_tex : 1, (scaleY_tex != 0) ? scaleX_tex : 1)
            });
            if (normalMapIndex >= 0)
            {
                Layers.Add(new EMGModelTextureLayer()
                {
                    TextureIndex = normalMapIndex,
                    ScaleUV = new Vector2((scaleX_norm != 0) ? scaleX_norm : 1, (scaleY_norm != 0) ? scaleY_norm : 1)
                });
            }
        }

        public byte[] GenerateBytes()
        {
            List<byte> Data = new List<byte>();

            USF4Utils.AddIntAsBytes(Data, Layers.Count, true);
            for (int j = 0; j < Layers.Count; j++)
            {
                Data.Add(0x00);
                USF4Utils.AddIntAsBytes(Data, Layers[j].TextureIndex, false);
                Data.Add(0x22);
                USF4Utils.AddFloatAsBytes(Data, Layers[j].ScaleUV.X);
                USF4Utils.AddFloatAsBytes(Data, Layers[j].ScaleUV.Y);
            }

            return Data.ToArray();
        }

        public class EMGModelTextureLayer
        {
            public int TextureIndex;
            public int UnkByte1_0x03;
            public Vector2 ScaleUV;
        }
    }
}