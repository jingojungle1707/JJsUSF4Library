using System.Collections.Generic;
using System.IO;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class CMDTrack
    {
        public byte[] HEXBytes;
        public int BoneID;
        public byte TransformType;
        public byte BitFlag;
        public List<Step> Steps;
        public List<float> IndicesValues;

        public class Step
        {
            public int Frame;
            public int Index;

            public Step(int frame, int index)
            {
                Frame = frame;
                Index = index;
            }
        }

        public CMDTrack()
        {
        }

        public CMDTrack(BinaryReader br, int offset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            BoneID = br.ReadInt16();
            TransformType = br.ReadByte();
            BitFlag = br.ReadByte();
            int stepCount = br.ReadInt16();
            int indicesListPointer = br.ReadInt16();

            Steps = new List<Step>();
            bool short_steps = (BitFlag & 0x20) == 0x20;
            bool long_indices = (BitFlag & 0x40) == 0x40;

            List<int> stepList = new List<int>();
            List<int> indicesList = new List<int>();

            if (short_steps)
            {
                for (int i = 0; i < stepCount; i++) stepList.Add(br.ReadInt16());
            }
            else
            {
                for (int i = 0; i < stepCount; i++) stepList.Add(br.ReadByte());
            }

            br.BaseStream.Seek(offset + indicesListPointer, SeekOrigin.Begin);
            if (long_indices)
            {
                for (int i = 0; i < stepCount; i++) indicesList.Add(br.ReadInt32());
            }
            else
            {
                for (int i = 0; i < stepCount; i++) indicesList.Add(br.ReadInt16());
            }
            for (int i = 0; i < stepCount; i++) Steps.Add(new Step(stepList[i], indicesList[i]));
        }
    }
}