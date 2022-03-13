using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class Animation
    {
        public string Name;
        public int Duration;
        public List<CMDTrack> CMDTracks;
        public List<float> ValuesList;

        public Animation()
        {

        }

        public Animation(byte[] Data, string name)
        {
            Name = name;
            ReadFile(Data);
        }

        public Animation (BinaryReader br, int offset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            Duration = br.ReadInt16();
            int cmdTrackCount = br.ReadInt16();
            int valueCount = br.ReadInt32();
            int valueSizeFlag = br.ReadInt32();
            int namePointer = br.ReadInt32();
            int valuesListPointer = br.ReadInt32();

            //Read cmdtracks
            //CMDTracks = new List<CMDTrack>();
            //List<int> cMDTrackPointers = new List<int>();

            //Reading CMDtracks with from stream seems to be extremely slow. (Lots of jumping around)
            //Chuck the whole lot into a byte[] and parse it the "old fashioned way"
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            byte[] cmdTrackData = br.ReadBytes(valuesListPointer);
            CMDTracks = ReadCMDTracks(cmdTrackData, cmdTrackCount);

            //for (int i = 0; i < cmdTrackCount; i++) cMDTrackPointers.Add(br.ReadInt32());
            //for (int i = 0; i < cmdTrackCount; i++) CMDTracks.Add(new CMDTrack(br, offset + cMDTrackPointers[i]));

            //Read values
            ValuesList = new List<float>();
            br.BaseStream.Seek(offset + valuesListPointer, SeekOrigin.Begin);
            for (int i = 0; i < valueCount; i++)
            {

                if (valueSizeFlag != 0)
                {
                    ValuesList.Add(USF4Utils.ReadShortFloat(br.ReadBytes(2)));
                }
                else ValuesList.Add(br.ReadSingle());
            }

            //Read name
            br.BaseStream.Seek(offset + namePointer, SeekOrigin.Begin);
            while (br.PeekChar() == 0x00) br.ReadByte();
            Name = br.ReadString();
            
        }

        private List<CMDTrack> ReadCMDTracks(byte[] Data, int cmdTrackCount)
        {
            //Populate CMD tracks
            CMDTracks = new List<CMDTrack>();
            for (int i = 0; i < cmdTrackCount; i++)
            {
                //Read CMD track header
                int curCmdOS = USF4Utils.ReadInt(true, 0x14 + 4 * i, Data);

                CMDTrack WorkingCMD = new CMDTrack()
                {
                    BoneID = USF4Utils.ReadInt(false, curCmdOS, Data),
                    TransformType = Data[curCmdOS + 0x02],
                    BitFlag = Data[curCmdOS + 0x03],
                };

                int stepCount = USF4Utils.ReadInt(false, curCmdOS + 0x04, Data);
                int indicesListPointer = USF4Utils.ReadInt(false, curCmdOS + 0x06, Data);

                //Populate keyframe list and indices list
                WorkingCMD.Steps = new List<CMDTrack.Step>();

                for (int j = 0; j < stepCount; j++)
                {
                    //Check flags and read step as short/byte
                    int step = (WorkingCMD.BitFlag & 0x20) == 0x20 ?
                        USF4Utils.ReadInt(false, curCmdOS + 0x08 + 2 * j, Data) : Data[curCmdOS + 0x08 + j];

                    //Check flags and read value indice as long/short
                    int index = (WorkingCMD.BitFlag & 0x40) == 0x40 ?
                        USF4Utils.ReadInt(true, curCmdOS + indicesListPointer + 4 * j, Data) : USF4Utils.ReadInt(false, curCmdOS + indicesListPointer + 2 * j, Data);

                    WorkingCMD.Steps.Add(new CMDTrack.Step(step, index));
                }

                //cmdHeader finished, push to list and start the next one...
                CMDTracks.Add(WorkingCMD);
            }

            return CMDTracks;
        }

        public void ReadFile(byte[] Data)
        {
            //Read animation header data
            Duration = USF4Utils.ReadInt(false, 0x00, Data);
            int cmdTrackCount = USF4Utils.ReadInt(false, 0x02, Data);
            int valueCount = USF4Utils.ReadInt(false, 0x04, Data);
            int valuesListPointer = USF4Utils.ReadInt(true, 0x10, Data);

            //Populate value list
            ValuesList = new List<float>();

            for (int j = 0; j < valueCount; j++)
            {
                ValuesList.Add(USF4Utils.ReadFloat(valuesListPointer + 4 * j, Data));
            }

            //Populate CMD tracks
            CMDTracks = new List<CMDTrack>();
            for (int j = 0; j < cmdTrackCount; j++)
            {
                //Read CMD track header
                int curCmdOS = USF4Utils.ReadInt(true, 0x14 + 4 * j, Data);

                CMDTrack WorkingCMD = new CMDTrack();
                WorkingCMD.BoneID = USF4Utils.ReadInt(false, curCmdOS, Data);
                WorkingCMD.TransformType = Data[curCmdOS + 0x02];
                WorkingCMD.BitFlag = Data[curCmdOS + 0x03];
                int stepCount = USF4Utils.ReadInt(false, curCmdOS + 0x04, Data);
                int indicesListPointer = USF4Utils.ReadInt(false, curCmdOS + 0x06, Data);

                //Populate keyframe list and indices list
                WorkingCMD.Steps = new List<CMDTrack.Step>();

                for (int k = 0; k < stepCount; k++)
                {
                    //Check flags and read step as short/byte
                    int step = (WorkingCMD.BitFlag & 0x20) == 0x20 ? 
                        USF4Utils.ReadInt(false, curCmdOS + 0x08 + 2 * k, Data) : Data[curCmdOS + 0x08 + k];

                    //Check flags and read value indice as long/short
                    int index = (WorkingCMD.BitFlag & 0x40) == 0x40 ?
                        USF4Utils.ReadInt(true, curCmdOS + indicesListPointer + 4 * k, Data) : USF4Utils.ReadInt(false, curCmdOS + indicesListPointer + 2 * k, Data);

                    WorkingCMD.Steps.Add(new CMDTrack.Step(step, index));
                }

                //cmdHeader finished, push to list and start the next one...
                CMDTracks.Add(WorkingCMD);
            }
        }
    }
}