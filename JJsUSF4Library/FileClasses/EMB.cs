using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JJsUSF4Library.FileClasses
{
    /// <summary>
    /// Binary archive file. Acts as a wrapper for a list of other files, which can include nested EMB
    /// </summary>
    public class EMB : USF4File
    {
        public List<USF4File> Files { get; set; }
        public List<string> FileNames { get { return Files.Select(o => o.Name).ToList(); } }

        public override byte[] GenerateBytes()
        {
            List<byte> data = new List<byte>();
            List<int> filePointerPositions = new List<int>();
            List<int> fileLengthPositions = new List<int>();
            List<int> fileNamePointerPositions = new List<int>();
            data.AddRange(new byte[] { 0x23, 0x45, 0x4D, 0x42, 0xFE, 0xFF, 0x20, 0x00, 0x01, 0x00, 0x01, 0x00 });
            USF4Utils.AddIntAsBytes(data, Files.Count, true);
            USF4Utils.AddPaddingZeros(data, 0x18, data.Count);
            int fileListPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, -1, true);
            int fileNameListPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, -1, true);

            USF4Utils.UpdateIntAtPosition(data, fileListPointerPosition, data.Count);
            for (int i = 0; i < Files.Count; i++)
            {
                filePointerPositions.Add(data.Count);
                USF4Utils.AddIntAsBytes(data, -1, true);
                fileLengthPositions.Add(data.Count);
                USF4Utils.AddIntAsBytes(data, -1, true);
            }

            USF4Utils.UpdateIntAtPosition(data, fileNameListPointerPosition, data.Count);

            for (int i = 0; i < Files.Count; i++)
            {
                fileNamePointerPositions.Add(data.Count);
                USF4Utils.AddIntAsBytes(data, -1, true);
            }

            USF4Utils.AddZeroToLineEnd(data);

            for (int i = 0; i < Files.Count; i++)
            {
                byte[] bytes = Files[i].GenerateBytes();
                USF4Utils.UpdateIntAtPosition(data, filePointerPositions[i], data.Count - (0x20 + i * 8));
                USF4Utils.UpdateIntAtPosition(data, fileLengthPositions[i], bytes.Length);
                data.AddRange(bytes);

                USF4Utils.AddZeroToLineEnd(data);
            }

            for (int i = 0; i < FileNames.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(data, fileNamePointerPositions[i], data.Count);
                data.AddRange(Encoding.ASCII.GetBytes(Files[i].Name));
                data.Add(0x00);
            }

            return data.ToArray();
        }

        public EMB()
        {
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

    }
}
