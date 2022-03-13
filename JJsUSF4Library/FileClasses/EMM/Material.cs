using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    /// <summary>EMM Material</summary>
    public class Material
    {
        public byte[] HEXBytes;
        /// <summary>Must be Length: 0x20</summary>
        public string Name;
        ///<summary>Must be Length: 0x20</summary>
        public string Shader;
        ///<summary>Must be Length 0x20 bytes. </summary>
        public List<string> PropertyNamesList;
        ///<summary>Must be Length 0x08 bytes. </summary>
        public List<byte[]> PropertyValuesList;

        public List<MaterialProperty> MaterialProperties;

        public Material()
        {

        }
        public Material(BinaryReader br, int offset = 0)
        {
            MaterialProperties = new List<MaterialProperty>();

            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            Name = Encoding.ASCII.GetString(br.ReadBytes(0x20)).Split('\0')[0];
            Shader = Encoding.ASCII.GetString(br.ReadBytes(0x20)).Split('\0')[0];
            int propertyCount = br.ReadInt32();
            for (int i = 0; i < propertyCount; i++) MaterialProperties.Add(new MaterialProperty(br));
        }

        public class MaterialProperty
        {
            public string Name;
            public Vector2 FloatValues;
            public List<byte> FlagValues;

            public MaterialProperty(BinaryReader br)
            {
                Name = Encoding.ASCII.GetString(br.ReadBytes(0x20)).Split('\0')[0];
                FloatValues = new Vector2(br.ReadSingle(), br.ReadSingle());
            }
        }

        public Material(byte[] Data)
        {
            Name = Encoding.ASCII.GetString(USF4Utils.ReadStringToArray(0, 0x20, Data, Data.Length)).Split('\0')[0];
            Shader = Encoding.ASCII.GetString(USF4Utils.ReadStringToArray(0x20, 0x20, Data, Data.Length)).Split('\0')[0];
            int propertyCount = USF4Utils.ReadInt(true, 0x40, Data);
            PropertyValuesList = new List<byte[]>();
            PropertyNamesList = new List<string>();
            for (int i = 0; i < propertyCount; i++)
            {
                PropertyNamesList.Add(Encoding.ASCII.GetString(USF4Utils.ReadStringToArray(0x44 + i * 0x28, 0x20, Data, Data.Length)).Split('\0')[0]);
                PropertyValuesList.Add(USF4Utils.ReadStringToArray(0x64 + i * 0x28, 0x08, Data, Data.Length));
            }
        }

        public Material(byte[] Data, string name)
        {
            Name = name;
            Shader = Encoding.ASCII.GetString(USF4Utils.ReadStringToArray(0x20, 0x20, Data, Data.Length)).Split('\0')[0];
            int propertyCount = USF4Utils.ReadInt(true, 0x40, Data);
            PropertyValuesList = new List<byte[]>();
            PropertyNamesList = new List<string>();
            for (int i = 0; i < propertyCount; i++)
            {
                PropertyNamesList.Add(Encoding.ASCII.GetString(USF4Utils.ReadStringToArray(0x44 + i * 0x28, 0x20, Data, Data.Length)).Split('\0')[0]);
                PropertyValuesList.Add(USF4Utils.ReadStringToArray(0x64 + i * 0x28, 0x08, Data, Data.Length));
            }
        }
        public byte[] GenerateBytes()
        {
            List<byte> Data = new List<byte>();
            Data.AddRange(USF4Utils.StringToNullPaddedBytes(Name, 0x20, out _).ToList());
            Data.AddRange(USF4Utils.StringToNullPaddedBytes(Shader, 0x20, out _).ToList());
            USF4Utils.AddIntAsBytes(Data, MaterialProperties.Count, true);

            for (int i = 0; i < MaterialProperties.Count; i++)
            {
                Data.AddRange(USF4Utils.StringToNullPaddedBytes(MaterialProperties[i].Name, 0x20, out _).ToList());
                //If we have flag bytes, use them, otherwise write floats
                if (MaterialProperties[i].FlagValues != null) Data.AddRange(MaterialProperties[i].FlagValues);
                else
                {
                    USF4Utils.AddFloatAsBytes(Data, MaterialProperties[i].FloatValues.X);
                    USF4Utils.AddFloatAsBytes(Data, MaterialProperties[i].FloatValues.Y);
                }
            }

            return Data.ToArray();
        }
    }
}