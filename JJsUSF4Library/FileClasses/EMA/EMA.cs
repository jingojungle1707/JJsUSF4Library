using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JJsUSF4Library.FileClasses.SubfileClasses;

namespace JJsUSF4Library.FileClasses
{
    public class EMA : USF4File
    {
        public int MysteryIntOS12 { get; set; }
        public Skeleton Skeleton { get; set; }
        public List<Animation> Animations { get; set; }

        public EMA()
        {

        }
        public EMA(BinaryReader br, string name, int offset = 0)
        {
            Name = name;
            ReadFromStream(br, offset);
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
            //EMA + some stuff
            List<byte> data = new List<byte>() { 0x23, 0x45, 0x4D, 0x41, 0xFE, 0xFF, 0x20, 0x00, 0x01, 0x00, 0x00, 0x00 };

            int skeletonPointerPosition = data.Count;   //Store skeleton pointer pos for later updating
            USF4Utils.AddIntAsBytes(data, 0, true);
            USF4Utils.AddIntAsBytes(data, Animations.Count, false);
            USF4Utils.AddIntAsBytes(data, MysteryIntOS12, false); //Always 0x03?
            USF4Utils.AddZeroToLineEnd(data); //Pad out to O/S 0x20

            List<int> animationPointerPositions = new List<int>(); //To store animation pointer pos for later updating
            for (int i = 0; i < Animations.Count; i++)
            {
                animationPointerPositions.Add(data.Count);
                USF4Utils.AddIntAsBytes(data, 0, true);
            }

            List<int> animationStartOSs = new List<int>();
            List<int> animationNamePointerPositions = new List<int>();

            for (int i = 0; i < Animations.Count; i++)
            {
                animationStartOSs.Add(data.Count);
                animationNamePointerPositions.Add(data.Count + 0x0C);
                USF4Utils.UpdateIntAtPosition(data, animationPointerPositions[i], data.Count);

                data.AddRange(Animations[i].GenerateBytes());
            }

            USF4Utils.AddZeroToLineEnd(data);

            //Start skeleton header
            if (Skeleton.Nodes != null && Skeleton.Nodes.Count > 0) //NEED TO CHECK FOR SKELETON PRESENCE?
            {
                USF4Utils.UpdateIntAtPosition(data, skeletonPointerPosition, data.Count);
                data.AddRange(Skeleton.GenerateBytes());
            }

            //The padding and pointers for the name list are weird, so name 0 is handled on its own, then the rest are handled in a loop.
            //I can't work out how it "really" works but this seems to do the trick.
            USF4Utils.UpdateIntAtPosition(data, animationNamePointerPositions[0], data.Count - animationStartOSs[0]);

            USF4Utils.AddIntAsBytes(data, 0x00, true);
            USF4Utils.AddIntAsBytes(data, 0x00, true);
            USF4Utils.AddIntAsBytes(data, 0x00, false);

            data.Add(Convert.ToByte(Animations[0].Name.Length));
            data.AddRange(Encoding.ASCII.GetBytes(Animations[0].Name));

            //From 1 because we already wrote name 0
            for (int i = 1; i < Animations.Count; i++)
            {
                USF4Utils.AddIntAsBytes(data, 0x00, true);
                USF4Utils.UpdateIntAtPosition(data, animationNamePointerPositions[i], data.Count - animationStartOSs[i]);
                USF4Utils.AddIntAsBytes(data, 0x00, true);
                USF4Utils.AddIntAsBytes(data, 0x00, true);
                USF4Utils.AddIntAsBytes(data, 0x00, false);

                data.Add(Convert.ToByte(Animations[i].Name.Length));
                data.AddRange(Encoding.ASCII.GetBytes(Animations[i].Name));
            }

            data.Add(0x00);

            return data.ToArray();
            
        }
    }
}