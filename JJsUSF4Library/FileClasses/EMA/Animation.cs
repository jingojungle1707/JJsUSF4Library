using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class Animation : INameableUSF4Object
    {
        public int Duration { get; set; }
        public int MysteryFlag0x08 { get; set; }
        public List<CMDTrack> CMDTracks { get; set; }
        public List<float> ValuesList { get; set; }
        public string Name { get; set; }

        public byte[] GenerateBytes()
        {
            List<byte> data = new List<byte>();
            List<int> cMDTrackPointerPositions = new List<int>();

            //Simulate building a value list to determine if we need short or long indices
            List<float> valuesList = new List<float>();
            foreach (CMDTrack c in CMDTracks)
            {
                foreach (CMDTrack.Step s in c.Steps)
                {
                    //If we have a tangent, we add both value and tangent to list
                    if (s.HasTangent)
                    {
                        valuesList.Add(s.Value);
                        valuesList.Add(s.Tangent);
                    }
                    //If we've got no tangent to worry about, add value to list if it's not already there
                    else if (!valuesList.Contains(s.Value)) valuesList.Add(s.Value);
                }
            }
            bool longIndices = (valuesList.Count > 0x3FFF);

            USF4Utils.AddIntAsBytes(data, Duration, false);
            USF4Utils.AddIntAsBytes(data, CMDTracks.Count, false);
            USF4Utils.AddIntAsBytes(data, valuesList.Count, true);
            USF4Utils.AddIntAsBytes(data, MysteryFlag0x08, true);

            USF4Utils.AddIntAsBytes(data, 0, true);
            int valuePointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, 0, true);

            for (int i = 0; i < CMDTracks.Count; i++)
            {   //Write out dummy CMD track pointers and store the pos for later update
                cMDTrackPointerPositions.Add(data.Count);
                USF4Utils.AddIntAsBytes(data, 0, true);
            }

            //Brute force value list
            valuesList = new List<float>();
            for (int i = 0; i < CMDTracks.Count; i++)
            {   
                //Based on value list simulation, set or clear bitflag
                int flag = longIndices ? CMDTracks[i].BitFlag |= 0x40 : CMDTracks[i].BitFlag &= 0xBF;
                //Start writing out CMD tracks, update pos
                int cMDStartOS = data.Count;
                USF4Utils.UpdateIntAtPosition(data, cMDTrackPointerPositions[i], data.Count);
                USF4Utils.AddIntAsBytes(data, CMDTracks[i].BoneID, false); //BoneID
                data.Add(Convert.ToByte(CMDTracks[i].TransformType)); //Transform type
                data.Add(Convert.ToByte(flag));       //Bitflag, with adjustment from simulation result
                USF4Utils.AddIntAsBytes(data, CMDTracks[i].Steps.Count, false);
                int indicesPointerPosition = data.Count; //Store position of indices pointer
                USF4Utils.AddIntAsBytes(data, 0, false);

                for (int j = 0; j < CMDTracks[i].Steps.Count; j++)
                {
                    if ((CMDTracks[i].BitFlag & 0x20) != 0x20) data.Add((byte)CMDTracks[i].Steps[j].Frame);

                    else USF4Utils.AddIntAsBytes(data, CMDTracks[i].Steps[j].Frame, false);
                }

                if ((data.Count - cMDStartOS) % 2 != 0)
                {
                    data.Add(0x00);
                }
                USF4Utils.UpdateShortAtPosition(data, indicesPointerPosition, data.Count - cMDStartOS);

                for (int j = 0; j < CMDTracks[i].Steps.Count; j++)
                {
                    CMDTrack.Step s = CMDTracks[i].Steps[j];
                    //If we have a tangent, just play safe and chuck both into the list
                    if (s.HasTangent)
                    {
                        if (longIndices)
                        {
                            int index = 0x40000000 | valuesList.Count;
                            USF4Utils.AddIntAsBytes(data, index, true);
                        }
                        else
                        {
                            int index = 0x4000 | valuesList.Count;
                            USF4Utils.AddIntAsBytes(data, index, false);
                        }
                        valuesList.Add(s.Value);
                        valuesList.Add(s.Tangent);
                    }
                    //If we've got no tangent to worry about, add value to list if it's not already there
                    else
                    {
                        //Check if value is already in list
                        int index = valuesList.IndexOf(s.Value);
                        if (index == -1)
                        {
                            index = valuesList.Count;
                            valuesList.Add(s.Value);
                        }
                        //Add the index to the data as a short or a long
                        USF4Utils.AddIntAsBytes(data, index, longIndices);
                    }
                }
            }
            //Value list...
            USF4Utils.UpdateIntAtPosition(data, valuePointerPosition, data.Count);
            for (int i = 0; i < valuesList.Count; i++)
            {
                USF4Utils.AddFloatAsBytes(data, valuesList[i]);
            }

            return data.ToArray();
            
        }

        public Animation()
        {

        }

        public Animation (BinaryReader br, int offset = 0)
        {
            ReadFromStream(br, offset);
        }

        public void ReadFromStream(BinaryReader br, int offset = 0, int fileLength = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            Duration = br.ReadInt16();
            int cmdTrackCount = br.ReadInt16();
            int valueCount = br.ReadInt32();
            MysteryFlag0x08 = br.ReadInt32();
            int namePointer = br.ReadInt32();
            int valuesListPointer = br.ReadInt32();
            int valueSizeFlag = MysteryFlag0x08;

            //Read values first so they're available when we read CMD tracks
            ValuesList = new List<float>();
            br.BaseStream.Seek(offset + valuesListPointer, SeekOrigin.Begin);
            for (int i = 0; i < valueCount; i++)
            {
                //The game doesn't seem to care what this is set to, but it DOES appear to be a correct marker for half-floats in existing .ema files...?
                if (valueSizeFlag != 0)
                {
                    ValuesList.Add(USF4Utils.ReadShortFloat(br.ReadBytes(2)));
                }
                else ValuesList.Add(br.ReadSingle());
            }

            //Reading CMDtracks with from stream seems to be extremely slow. (Lots of jumping around?)
            //Chuck the whole lot into a byte[] and parse it the "old fashioned way"
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            byte[] cmdTrackData = br.ReadBytes(valuesListPointer);
            CMDTracks = ReadCMDTracks(cmdTrackData, cmdTrackCount);

            //Read name
            br.BaseStream.Seek(offset + namePointer, SeekOrigin.Begin);
            while (br.PeekChar() == 0x00) br.ReadByte();
            Name = br.ReadString();
        }

        private List<CMDTrack> ReadCMDTracks(byte[] data, int cmdTrackCount)
        {
            //Populate CMD tracks
            CMDTracks = new List<CMDTrack>();
            for (int i = 0; i < cmdTrackCount; i++)
            {
                //Read CMD track header
                int curCmdOS = USF4Utils.ReadInt(true, 0x14 + 4 * i, data);

                CMDTrack WorkingCMD = new CMDTrack()
                {
                    BoneID = USF4Utils.ReadInt(false, curCmdOS, data),
                    TransformType = data[curCmdOS + 0x02],
                    BitFlag = data[curCmdOS + 0x03],
                };

                int stepCount = USF4Utils.ReadInt(false, curCmdOS + 0x04, data);
                int indicesListPointer = USF4Utils.ReadInt(false, curCmdOS + 0x06, data);

                //Populate keyframe list and indices list
                WorkingCMD.Steps = new List<CMDTrack.Step>();

                for (int j = 0; j < stepCount; j++)
                {
                    //Check flags and read step as short/byte
                    int step = (WorkingCMD.BitFlag & 0x20) == 0x20 ?
                        USF4Utils.ReadInt(false, curCmdOS + 0x08 + 2 * j, data) : data[curCmdOS + 0x08 + j];

                    //Check flags and read value indice as long/short
                    int index = (WorkingCMD.BitFlag & 0x40) == 0x40 ?
                        USF4Utils.ReadInt(true, curCmdOS + indicesListPointer + 4 * j, data) : USF4Utils.ReadInt(false, curCmdOS + indicesListPointer + 2 * j, data);

                    WorkingCMD.Steps.Add(new CMDTrack.Step(step, ValuesList, index, (WorkingCMD.BitFlag & 0x40) == 0x40));
                }

                //cmdHeader finished, push to list and start the next one...
                CMDTracks.Add(WorkingCMD);
            }

            return CMDTracks;
        }

        public virtual void SaveToPath(string path)
        {
            USF4Utils.WriteDataToStream(path, GenerateBytes());
        }
    }
}