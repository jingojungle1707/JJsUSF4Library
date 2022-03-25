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

        public override byte[] GenerateBytes()
        {
            List<byte> data = new List<byte>();
            List<int> materialPointerPositions = new List<int>();
            //#EMM Header
            data.AddRange(new List<byte> { 0x23, 0x45, 0x4D, 0x4D, 0xFE, 0xFF, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00 });
            USF4Utils.AddIntAsBytes(data, Materials.Count, true);
            for (int i = 0; i < Materials.Count; i++)
            {
                materialPointerPositions.Add(data.Count);
                USF4Utils.AddIntAsBytes(data, -1, true);
            }
            for (int i = 0; i < Materials.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(data, materialPointerPositions[i], data.Count - 0x10);
                data.AddRange(Materials[i].GenerateBytes());
            }

            return data.ToArray();
        }
    }
}