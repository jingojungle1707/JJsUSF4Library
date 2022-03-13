using System.Collections.Generic;
using System.IO;
using System.Text;
using JJsUSF4Library.FileClasses.SubfileClasses;

namespace JJsUSF4Library.FileClasses
{
    public class EMO : USF4File //Header
    {
        public int NumberEMMMaterials;
        public List<EMG> EMGs;
        public int temp_bitdepth;

        public Skeleton Skeleton;

        public EMO()
        {

        }
        public EMO(BinaryReader br, string name, int offset = 0)
        {
            Name = name;
            ReadFromStream(br, offset);
        }
        public override void ReadFromStream(BinaryReader br, int offset = 0, int fileLength = 0)
        {
            #region Initialise Lists
            EMGs = new List<EMG>();
            List<string> namesList = new List<string>();
            List<int> eMGPointersList = new List<int>();
            #endregion

            #region Read Header
            br.BaseStream.Seek(offset + 0x10, SeekOrigin.Begin);
            int skeletonPointer = br.ReadInt32();
            br.BaseStream.Seek(offset + 0x20, SeekOrigin.Begin);
            int eMGCount = br.ReadInt16();
            NumberEMMMaterials = br.ReadInt16();
            int namingListPointer = br.ReadInt32();

            for (int i = 0; i < eMGCount; i++) eMGPointersList.Add(br.ReadInt32());
            #endregion

            #region Read NameList
            //Read names list "early" so we can pass names to EMGs as we construct them
            //We're going to read the first name pointer, then rely on z-strings to find the rest
            br.BaseStream.Seek(namingListPointer + 0x20 + offset, SeekOrigin.Begin);
            br.BaseStream.Seek(br.ReadInt32() + 0x20 + offset, SeekOrigin.Begin);
            for (int i = 0; i < eMGCount; i++) namesList.Add(USF4Utils.ReadZString(br));
            #endregion
            
            #region Read Skeleton
            //Read the skeleton "early" so we have nodenames available to pass to the EMG constructor
            if (skeletonPointer != 0) Skeleton = new Skeleton(br, Skeleton.SkeletonType.EMO, skeletonPointer + offset);
            else Skeleton = new Skeleton();
            #endregion

            #region Read EMGs
            for (int i = 0; i < eMGCount; i++)
            {
                EMGs.Add(new EMG(br, namesList[i], eMGPointersList[i] + 0x30 + offset, Skeleton.NodeNames));
            }
            #endregion
        }

        public override byte[] GenerateBytes()
        {
            List<byte> Data = new List<byte>();
            List<int> eMGPointerPositions = new List<int>();
            List<int> namePointerPositions = new List<int>();
            //Header line
            Data.AddRange(new List<byte> { 0x23, 0x45, 0x4D, 0x4F, 0xFE, 0xFF, 0x20, 0x00, 0x02, 0x00, 0x01, 0x00, 0x20, 0x00, 0x00, 0x00 });

            int skeletonPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            USF4Utils.AddPaddingZeros(Data, 0x20, Data.Count);
            USF4Utils.AddIntAsBytes(Data, EMGs.Count, false);
            USF4Utils.AddIntAsBytes(Data, NumberEMMMaterials, false);
            int namingListPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);

            for (int i = 0; i < EMGs.Count; i++)
            {
                eMGPointerPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, -1, true);
            }
            USF4Utils.AddZeroToLineEnd(Data);

            //Write out EMGs and Update EMG pointers
            for (int i = 0; i < EMGs.Count; i++)
            { 
                USF4Utils.AddZeroToLineEnd(Data);
                USF4Utils.AddCopiedBytes(Data, 0x00, 0x10, new byte[0x10] { 0x01, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

                USF4Utils.UpdateIntAtPosition(Data, eMGPointerPositions[i], Data.Count - 0x30);
                Data.AddRange(EMGs[i].GenerateBytes(Skeleton.NodeNames));
            }

            USF4Utils.AddZeroToLineEnd(Data);

            USF4Utils.UpdateIntAtPosition(Data, namingListPointerPosition, Data.Count - 0x20);

            for (int i = 0; i < EMGs.Count; i++)
            {
                namePointerPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, -1, true);
            }

            for (int i = 0; i < EMGs.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(Data, namePointerPositions[i], Data.Count - 0x20);
                Data.AddRange(Encoding.ASCII.GetBytes(EMGs[i].Name));
                Data.Add(0x00);
            }

            USF4Utils.AddZeroToLineEnd(Data);

            USF4Utils.UpdateIntAtPosition(Data, skeletonPointerPosition, Data.Count);
            Data.AddRange(Skeleton.GenerateBytes());
            USF4Utils.AddZeroToLineEnd(Data);

            return Data.ToArray();
        }

        public byte[] GenerateSFxTBytes()
        {
            //SFxT style 

            List<byte> data = new List<byte>();
            List<int> eMGPointerPositions = new List<int>();
            List<int> namePointerPositions = new List<int>();
            //Header line
            data.AddRange(new List<byte> { 0x23, 0x45, 0x4D, 0x4F, 0xFE, 0xFF, 0x20, 0x00, 0x02, 0x00, 0x01, 0x00, 0x20, 0x00, 0x00, 0x00 });

            int skeletonPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, -1, true);
            USF4Utils.AddPaddingZeros(data, 0x20, data.Count);
            USF4Utils.AddIntAsBytes(data, EMGs.Count, false);
            USF4Utils.AddIntAsBytes(data, NumberEMMMaterials, false);
            int namingListPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, -1, true);

            for (int i = 0; i < EMGs.Count; i++)
            {
                eMGPointerPositions.Add(data.Count);
                USF4Utils.AddIntAsBytes(data, -1, true);
            }
            USF4Utils.AddZeroToLineEnd(data);

            List<List<int>> vertexPointerPositions = new List<List<int>>();

            //Write out EMGs and Update EMG pointers
            for (int i = 0; i < EMGs.Count; i++)
            {
                USF4Utils.AddZeroToLineEnd(data);
                USF4Utils.AddCopiedBytes(data, 0x00, 0x10, new byte[0x10] { 0x01, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

                USF4Utils.UpdateIntAtPosition(data, eMGPointerPositions[i], data.Count - 0x30);
                data.AddRange(EMGs[i].GenerateSFxTBytes(Skeleton.NodeNames, data.Count, vertexPointerPositions));
            }

            USF4Utils.AddZeroToLineEnd(data);

            USF4Utils.UpdateIntAtPosition(data, namingListPointerPosition, data.Count - 0x20);

            for (int i = 0; i < EMGs.Count; i++)
            {
                namePointerPositions.Add(data.Count);
                USF4Utils.AddIntAsBytes(data, -1, true);
            }

            for (int i = 0; i < EMGs.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(data, namePointerPositions[i], data.Count - 0x20);
                data.AddRange(Encoding.ASCII.GetBytes(EMGs[i].Name));
                data.Add(0x00);
            }

            USF4Utils.AddZeroToLineEnd(data);

            USF4Utils.UpdateIntAtPosition(data, skeletonPointerPosition, data.Count);
            data.AddRange(Skeleton.GenerateBytes());
            USF4Utils.AddZeroToLineEnd(data);

            for (int i = 0; i < EMGs.Count; i++)
            {
                for (int j = 0; j < EMGs[i].Models.Count; j++)
                {
                    //-0x14 because we want to measure from the start of the model header
                    USF4Utils.UpdateIntAtPosition(data, vertexPointerPositions[i][j], data.Count - vertexPointerPositions[i][j] + 0x14);

                    data.AddRange(EMGs[i].Models[j].GenerateVertexBytes(Skeleton.NodeNames));

                    USF4Utils.AddZeroToLineEnd(data);
                }
            }

            return data.ToArray();
        }

        public virtual void SaveAsSFxTEMO(string path)
        {
            USF4Utils.WriteDataToStream(path, GenerateSFxTBytes());
        }

        public override void DeleteSubfile(int index)
        {
            if (index < EMGs.Count)
            {
                EMGs.RemoveAt(index);
            }
        }
    }
}