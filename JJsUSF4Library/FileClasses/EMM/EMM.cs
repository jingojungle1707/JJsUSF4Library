using JJsUSF4Library.FileClasses.SubfileClasses;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JJsUSF4Library.FileClasses
{
    public class EMM : USF4File
    {
        public List<Material> Materials;

        public EMM()
        {

        }
        public EMM(BinaryReader br, string name, int offset = 0)
        {
            Name = name;
            ReadFromStream(br, offset);
        }
        public override void ReadFromStream(BinaryReader br, int offset = 0, int fileLength = 0)
        {
            #region Initialise Lists
            Materials = new List<Material>();
            List<int> materialPointers = new List<int>();
            #endregion

            br.BaseStream.Seek(offset + 0x10, SeekOrigin.Begin);
            #region Read Header
            int materialCount = br.ReadInt32();
            #endregion
            #region Read Materials
            for (int i = 0; i < materialCount; i++) materialPointers.Add(br.ReadInt32());
            for (int i = 0; i < materialCount; i++) Materials.Add(new Material(br, offset + materialPointers[i] + 0x10));
            #endregion
        }
        public EMM(byte[] Data, string name)
        {
            Name = name;
            ReadFile(Data);
        }

        public override void ReadFile(byte[] Data)
        {
            int materialCount = USF4Utils.ReadInt(true, 0x10, Data);
            List<int> materialPointersList = new List<int>();
            Materials = new List<Material>();

            for (int i = 0; i < materialCount; i++)
            {
                materialPointersList.Add(USF4Utils.ReadInt(true, 0x14 + i * 4, Data));
                Materials.Add(new Material(Data.Slice(materialPointersList[i] + 0x10, Data.Length - (materialPointersList[i] + 0x10))));
            }
        }
        public override byte[] GenerateBytes()
        {
            List<byte> Data = new List<byte>();
            List<int> materialPointerPositions = new List<int>();
            //#EMM Header
            Data.AddRange(new List<byte> { 0x23, 0x45, 0x4D, 0x4D, 0xFE, 0xFF, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00 });
            USF4Utils.AddIntAsBytes(Data, Materials.Count, true);
            for (int i = 0; i < Materials.Count; i++)
            {
                materialPointerPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, -1, true);
            }
            for (int i = 0; i < Materials.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(Data, materialPointerPositions[i], Data.Count - 0x10);
                Data.AddRange(Materials[i].GenerateBytes());
            }

            return Data.ToArray();
        }

        public override void DeleteSubfile(int index)
        {
            Materials.RemoveAt(index);
            GenerateBytes();
        }
    }
}