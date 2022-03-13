using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JJsUSF4Library.FileClasses.SubfileClasses;

namespace JJsUSF4Library.FileClasses
{
    public class EMG : USF4File
    {
        public int RootBoneID;
        public string RootBoneName;
        public List<Model> Models;

        public EMG()
        {

        }
        public EMG(BinaryReader br, string name, int offset = 0, List<string> nodeNames = default(List<string>))
        {
            Name = name;

            #region Initialise Lists
            Models = new List<Model>();
            List<int> modelPointersList = new List<int>();
            #endregion

            #region Read Header
            br.BaseStream.Seek(offset + 0x04, SeekOrigin.Begin);
            RootBoneID = br.ReadInt16();
            if (nodeNames != null) RootBoneName = nodeNames[RootBoneID];
            int modelCount = Math.Max((int)br.ReadInt16(), 1); //If model count < 1, set to 1.

            for (int i = 0; i < modelCount; i++) modelPointersList.Add(br.ReadInt32());
            #endregion

            #region Read Models
            for (int i = 0; i < modelCount; i++) Models.Add(new Model(br, offset + modelPointersList[i], nodeNames));
            #endregion
        }

        /// <summary>
        /// Generates binary file data for this object. 
        /// <para>This method is used for "orphan" EMGs,
        /// ie, where it is not contained in an EMO and no skeleton data is available.
        /// <br>It does not check for bone mapping, and uses vertex-local bone IDs generated when the EMG was read from file.</br></para>
        /// </summary>
        /// <returns>byte[] containing binary data</returns>
        public override byte[] GenerateBytes()
        {
            return GenerateBytes(default);
        }
        /// <summary>
        /// Generates binary file data for this object. 
        /// <para>This version is for EMGs which are part of an EMO and where skeleton nodeName data is available.
        /// <br>BoneMaps are fully recalculated using the names stored in each Vertex' BoneName list</br></para>
        /// </summary>
        /// <returns>byte[] containing binary data</returns>
        public byte[] GenerateBytes(List<string> nodeNames)
        {
            List<byte> data = new List<byte>();

            data.AddRange(GenerateHeaderBytes(out List<int> modelPointerPositions));

            for (int i = 0; i < Models.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(data, modelPointerPositions[i], data.Count);
                data.AddRange(Models[i].GenerateBytes(nodeNames));
            }

            return data.ToArray();
        }

        public byte[] GenerateSFxTBytes(List<string> nodeNames, int startOffset, List<List<int>> vertexPointerPositions)
        {
            List<byte> data = new List<byte>();

            data.AddRange(GenerateHeaderBytes(out List<int> modelPointerPositions));

            //Create a new list in our List<List<int>> for storing vertex pointers for this EMG
            //Each Model has an entry in the EMG's List<int>
            vertexPointerPositions.Add(new List<int>());

            for (int i = 0; i < Models.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(data, modelPointerPositions[i], data.Count);
                vertexPointerPositions.Last().Add(data.Count + startOffset + 0x14);
                data.AddRange(Models[i].GenerateSFxTBytes(nodeNames));
            }

            return data.ToArray();
        }

        private List<byte> GenerateHeaderBytes(out List<int> modelPointerPositions)
        {
            List<byte> data = new List<byte>();

            modelPointerPositions = new List<int>();

            data.AddRange(new List<byte> { 0x23, 0x45, 0x4D, 0x47 });
            USF4Utils.AddIntAsBytes(data, RootBoneID, false);
            USF4Utils.AddIntAsBytes(data, Models.Count, false);

            for (int i = 0; i < Models.Count; i++)
            {
                modelPointerPositions.Add(data.Count);
                USF4Utils.AddIntAsBytes(data, -1, true);
            }

            USF4Utils.AddZeroToLineEnd(data);

            return data;
        }

        public override void DeleteSubfile(int index)
        {
            Models.RemoveAt(index);
            GenerateBytes();
        }
    }
}