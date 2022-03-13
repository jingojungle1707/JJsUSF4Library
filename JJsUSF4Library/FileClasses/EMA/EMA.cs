using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JJsUSF4Library.FileClasses.SubfileClasses;

namespace JJsUSF4Library.FileClasses
{
    public class EMA : USF4File
    {
        public int MysteryIntOS12;
        public List<Animation> Animations;

        public Skeleton Skeleton;

        public EMA()
        {

        }
        public EMA(BinaryReader br, string name, int offset = 0)
        {
            Name = name;
            ReadFromStream(br, offset);
        }
        public EMA(byte[] Data, string name)
        {
            Name = name;
            ReadFile(Data);
        }

        public override void ReadFromStream(BinaryReader br, int offset = 0, int fileLength = 0)
        {
            //Populate EMA header data
            List<int> animationPointersList = new List<int>();
            Animations = new List<Animation>();

            br.BaseStream.Seek(offset + 0x0C, SeekOrigin.Begin);

            int skeletonPointer = br.ReadInt32();
            //0x10
            int animationCount = br.ReadInt16();
            MysteryIntOS12 = br.ReadInt32();

            //Padding nulls out to 0x20, then animation pointer list
            br.BaseStream.Seek(offset + 0x20, SeekOrigin.Begin);
            //Read animation pointers
            for (int i = 0; i < animationCount; i++) animationPointersList.Add(br.ReadInt32());
            //Read animations
            for (int i = 0; i < animationCount; i++) Animations.Add(new Animation(br, offset + animationPointersList[i]));

            //Read skeleton
            if (skeletonPointer != 0) Skeleton = new Skeleton(br, Skeleton.SkeletonType.EMA, skeletonPointer + offset);
            else Skeleton = new Skeleton();
        }

        public override byte[] GenerateBytes()
        {
            List<byte> Data = new List<byte>();

            //EMA + some stuff
            USF4Utils.AddCopiedBytes(Data, 0x00, 0x0C, new byte[] { 0x23, 0x45, 0x4D, 0x41, 0xFE, 0xFF, 0x20, 0x00, 0x01, 0x00, 0x00, 0x00 });

            int SkeletonPointerPosition = Data.Count;   //Store skeleton pointer pos for later updating
            USF4Utils.AddIntAsBytes(Data, -1, true);
            USF4Utils.AddIntAsBytes(Data, Animations.Count, false);
            USF4Utils.AddIntAsBytes(Data, MysteryIntOS12, false); //Always 0x03?
            USF4Utils.AddZeroToLineEnd(Data); //Pad out to O/S 0x20

            List<int> AnimationPointerPositions = new List<int>(); //To store animation pointer pos for later updating
            for (int i = 0; i < Animations.Count; i++)
            {
                AnimationPointerPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, -1, true);
            }

            List<int> animationStartOSs = new List<int>();
            List<int> AnimationNamePointerPositions = new List<int>();

            for (int i = 0; i < Animations.Count; i++)
            {
                List<int> CMDTrackPointerPositions = new List<int>();
                animationStartOSs.Add(Data.Count);

                USF4Utils.UpdateIntAtPosition(Data, AnimationPointerPositions[i], Data.Count);
                USF4Utils.AddIntAsBytes(Data, Animations[i].Duration, false);
                USF4Utils.AddIntAsBytes(Data, Animations[i].CMDTracks.Count, false);
                USF4Utils.AddIntAsBytes(Data, Animations[i].ValuesList.Count, true);
                USF4Utils.AddIntAsBytes(Data, 0x00, true); //Padding zeroes

                AnimationNamePointerPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, -1, true);
                int valuePointerPosition = Data.Count;
                USF4Utils.AddIntAsBytes(Data, -1, true);

                for (int j = 0; j < Animations[i].CMDTracks.Count; j++)
                {   //Write out dummy CMD track pointers and store the pos for later update
                    CMDTrackPointerPositions.Add(Data.Count);
                    USF4Utils.AddIntAsBytes(Data, -1, true);
                }
                for (int j = 0; j < Animations[i].CMDTracks.Count; j++)
                {   //Start writing out CMD tracks, update pos
                    int CMDStartOS = Data.Count;
                    USF4Utils.UpdateIntAtPosition(Data, CMDTrackPointerPositions[j], Data.Count - animationStartOSs[i]);
                    USF4Utils.AddIntAsBytes(Data, Animations[i].CMDTracks[j].BoneID, false); //BoneID
                    Data.Add(Convert.ToByte(Animations[i].CMDTracks[j].TransformType)); //Transform type
                    Data.Add(Convert.ToByte(Animations[i].CMDTracks[j].BitFlag));       //Bitflag
                    USF4Utils.AddIntAsBytes(Data, Animations[i].CMDTracks[j].Steps.Count, false);
                    int IndicesPointerPosition = Data.Count; //Store position of indices pointer
                    USF4Utils.AddIntAsBytes(Data, -1, false);

                    for (int k = 0; k < Animations[i].CMDTracks[j].Steps.Count; k++)
                    {
                        if ((Animations[i].CMDTracks[j].BitFlag & 0x20) != 0x20) Data.Add((byte)Animations[i].CMDTracks[j].Steps[k].Frame);

                        else USF4Utils.AddIntAsBytes(Data, Animations[i].CMDTracks[j].Steps[k].Frame, false);
                    }

                    if ((Data.Count - CMDStartOS) % 2 != 0)
                    {
                        Data.Add(0x00);
                    }
                    USF4Utils.UpdateShortAtPosition(Data, IndicesPointerPosition, Data.Count - CMDStartOS);

                    for (int k = 0; k < Animations[i].CMDTracks[j].Steps.Count; k++)
                    {
                        USF4Utils.AddIntAsBytes(Data, Animations[i].CMDTracks[j].Steps[k].Index, (Animations[i].CMDTracks[j].BitFlag & 0x40) == 0x40);
                    }
                }
                //Value list...
                USF4Utils.UpdateIntAtPosition(Data, valuePointerPosition, Data.Count - animationStartOSs[i]);
                for (int j = 0; j < Animations[i].ValuesList.Count; j++)
                {
                    USF4Utils.AddFloatAsBytes(Data, Animations[i].ValuesList[j]);
                }
            }

            USF4Utils.AddZeroToLineEnd(Data);

            //Start skeleton header
            if (Skeleton.Nodes != null && Skeleton.Nodes.Count > 0) //NEED TO CHECK FOR SKELETON PRESENCE?
            {
                USF4Utils.UpdateIntAtPosition(Data, SkeletonPointerPosition, Data.Count);
                Data.AddRange(Skeleton.GenerateBytes());
            }

            //The padding and pointers for the name list are weird, so name 0 is handled on its own, then the rest are handled in a loop.
            //I can't work out how it "really" works but this seems to do the trick.
            USF4Utils.UpdateIntAtPosition(Data, AnimationNamePointerPositions[0], Data.Count - animationStartOSs[0]);

            USF4Utils.AddIntAsBytes(Data, 0x00, true);
            USF4Utils.AddIntAsBytes(Data, 0x00, true);
            USF4Utils.AddIntAsBytes(Data, 0x00, false);

            Data.Add(Convert.ToByte(Animations[0].Name.Length));
            Data.AddRange(Encoding.ASCII.GetBytes(Animations[0].Name));

            //From 1 because we already wrote name 0
            for (int i = 1; i < Animations.Count; i++)
            {
                USF4Utils.AddIntAsBytes(Data, 0x00, true);
                USF4Utils.UpdateIntAtPosition(Data, AnimationNamePointerPositions[i], Data.Count - animationStartOSs[i]);
                USF4Utils.AddIntAsBytes(Data, 0x00, true);
                USF4Utils.AddIntAsBytes(Data, 0x00, true);
                USF4Utils.AddIntAsBytes(Data, 0x00, false);

                Data.Add(Convert.ToByte(Animations[i].Name.Length));
                Data.AddRange(Encoding.ASCII.GetBytes(Animations[i].Name));
            }

            Data.Add(0x00);

            return Data.ToArray();
        }

        public override void DeleteSubfile(int index)
        {
            Animations.RemoveAt(index);
        }
    }
}