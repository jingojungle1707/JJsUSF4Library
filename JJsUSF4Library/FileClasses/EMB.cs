using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JJsUSF4Library.FileClasses
{
    public class EMB : USF4File
    {
        public List<USF4File> Files;
        public List<string> FileNames;

        public EMB()
        {
        }
        public EMB(byte[] Data, string name)
        {
            Name = name;
            ReadFile(Data);
        }

        public EMB(BinaryReader br, string name, int offset = 0)
        {
            Name = name;
            ReadFromStream(br, offset);
        }

        public override void ReadFromStream(BinaryReader br, int offset = 0, int fileLength = 0)
        {
            br.BaseStream.Seek(offset + 0x0C, SeekOrigin.Begin);

            #region Initialize Lists
            Files = new List<USF4File>();
            FileNames = new List<string>();

            List<int> fileLengthsList = new List<int>();
            List<int> filePointersList = new List<int>();
            List<int> fileNamePointersList = new List<int>();
            #endregion

            #region Read Header
            int numberOfFiles = br.ReadInt32();
            //0x10
            br.ReadInt64();
            int fileListPointer = br.ReadInt32();
            int fileNamesPointer = br.ReadInt32();
            //0x20
            br.BaseStream.Seek(fileListPointer + offset, SeekOrigin.Begin);
            for (int i = 0; i < numberOfFiles; i++)
            {
                filePointersList.Add(br.ReadInt32());
                fileLengthsList.Add(br.ReadInt32());
            }
            if (fileNamesPointer != 0)
            {
                br.BaseStream.Seek(fileNamesPointer + offset, SeekOrigin.Begin);
                for (int i = 0; i < numberOfFiles; i++) fileNamePointersList.Add(br.ReadInt32());
            }
            else
            {
                for (int i = 0; i < numberOfFiles; i++) FileNames.Add($"{Name}_DDS{i:D2}");
            }
            #endregion

            #region Read Names
            //Loop over fileNamePointersList - if the file doesn't contain an index,
            //list length will be zero and we don't need to read any names
            //Names are z-strings
            for (int i = 0; i < fileNamePointersList.Count; i++)
            {
                br.BaseStream.Seek(fileNamePointersList[i] + offset, SeekOrigin.Begin);
                FileNames.Add(USF4Utils.ReadZString(br));
            }

            #endregion

            #region Read Files
            for (int i = 0; i < numberOfFiles; i++)
            {
                //File starts at pointer positions + pointer
                br.BaseStream.Seek(fileListPointer + i * 8 + filePointersList[i] + offset, SeekOrigin.Begin);
                USF4File file = USF4Methods.FetchClass((USF4Methods.FileType)br.ReadInt32());
                //USF4File file = USF4Methods.CheckFile(br.ReadBytes(4));
                file.ReadFromStream(br, fileListPointer + i * 8 + filePointersList[i] + offset, fileLengthsList[i]);
                file.Name = FileNames[i];
                Files.Add(file);
            }
            #endregion
        }

        //public override void ReadFile(byte[] Data)
        //{
        //    HEXBytes = Data;
        //    int numberOfFiles = USF4Utils.ReadInt(true, 0x0C, Data);
        //    int fileListPointer = USF4Utils.ReadInt(true, 0x18, Data);
        //    int fileNamesPointer = USF4Utils.ReadInt(true, 0x1C, Data);
        //    List<int> fileLengthsList = new List<int>();
        //    List<int> filePointersList = new List<int>();
        //    Files = new List<USF4File>();
        //    FileNames = new List<string>();
        //    List<int> fileNamePointersList = new List<int>();

        //    for (int i = 0; i < numberOfFiles; i++)
        //    {
        //        filePointersList.Add(USF4Utils.ReadInt(true, fileListPointer + i * 8, Data));
        //        fileLengthsList.Add(USF4Utils.ReadInt(true, fileListPointer + i * 8 + 4, Data));

        //        if (fileNamesPointer == 0x00) //if there wasn't a file index, add a dummy one
        //        {
        //            FileNames.Add("Unnamed_DDS");
        //        }
        //        else
        //        {
        //            fileNamePointersList.Add(USF4Utils.ReadInt(true, fileNamesPointer + i * 4, Data));
        //            FileNames.Add(Encoding.ASCII.GetString(USF4Utils.ReadZeroTermStringToArray(fileNamePointersList[i], Data, Data.Length)));
        //        }

        //        int FileType = USF4Utils.ReadInt(true, filePointersList[i] + fileListPointer + i * 8, Data);

        //        USF4File file = USF4Methods.CheckFile(Data.Slice(filePointersList[i] + fileListPointer + i * 8, 0x08));

        //        file.ReadFile(Data.Slice(filePointersList[i] + fileListPointer + i * 8, fileLengthsList[i]));
        //        file.Name = FileNames[i];
        //        Files.Add(file);
        //    }

        //}

        public override byte[] GenerateBytes()
        {
            List<byte> Data = new List<byte>();
            List<int> filePointerPositions = new List<int>();
            List<int> fileLengthPositions = new List<int>();
            List<int> fileNamePointerPositions = new List<int>();
            Data.AddRange(new byte[] { 0x23, 0x45, 0x4D, 0x42, 0xFE, 0xFF, 0x20, 0x00, 0x01, 0x00, 0x01, 0x00 });
            USF4Utils.AddIntAsBytes(Data, Files.Count, true);
            USF4Utils.AddPaddingZeros(Data, 0x18, Data.Count);
            int fileListPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);
            int fileNameListPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, -1, true);

            USF4Utils.UpdateIntAtPosition(Data, fileListPointerPosition, Data.Count);
            for (int i = 0; i < Files.Count; i++)
            {
                filePointerPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, -1, true);
                fileLengthPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, -1, true);
            }

            USF4Utils.UpdateIntAtPosition(Data, fileNameListPointerPosition, Data.Count);

            for (int i = 0; i < Files.Count; i++)
            {
                fileNamePointerPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, -1, true);
            }

            USF4Utils.AddZeroToLineEnd(Data);

            for (int i = 0; i < Files.Count; i++)
            {
                byte[] bytes = Files[i].GenerateBytes();
                USF4File file = Files[i];
                USF4Utils.UpdateIntAtPosition(Data, filePointerPositions[i], Data.Count - (0x20 + i * 8));
                USF4Utils.UpdateIntAtPosition(Data, fileLengthPositions[i], bytes.Length);
                Data.AddRange(bytes);

                USF4Utils.AddZeroToLineEnd(Data);
            }

            for (int i = 0; i < FileNames.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(Data, fileNamePointerPositions[i], Data.Count);
                Data.AddRange(Encoding.ASCII.GetBytes(Files[i].Name));
                Data.Add(0x00);
            }

            return Data.ToArray();
        }
        public override void DeleteSubfile(int index)
        {
            Files.RemoveAt(index);
            FileNames.RemoveAt(index);
            GenerateBytes();
        }

        public void AddSubfile(USF4File uf)
        {
            Files.Add(uf);
            FileNames.Add(uf.Name);
            GenerateBytes();
        }
    }
}
