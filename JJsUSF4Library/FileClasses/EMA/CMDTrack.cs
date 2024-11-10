using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class CMDTrack
    {
        public int BoneID;
        public byte TransformType;
        public byte BitFlag;
        private List<Step> _steps = new List<Step>();
        public Skeleton Skeleton { get; private set; } = new Skeleton();

        [Flags]
        public enum CMDBitFlag
        {
            X = 0x00,
            Y = 0x01,
            Z = 0x02,
            UNK0x04 = 0x04,
            UNK0x08 = 0x08,
            ABSOLUTE = 0x10,
            SHORTSTEPS = 0x20,
            LONGINDICES = 0x40,
            UNK0x80 = 0x80
        }

        //Ensure step list is always ordered
        public List<Step> Steps { get; set; }

        public class Step
        {
            //Nullable tangent so we can differentiate between 0f and "no tangent"
            private float? _tangent = null;
            public int Frame { get; set; }
            public float Value { get; set; }
            public float Tangent 
            {
                get 
                {
                    return _tangent ?? default;
                }
                set
                {
                    _tangent = value;
                    HasTangent = true;
                }
            }
            public bool HasTangent { get; private set; }

            public Step(int frame, float value, float? tangent = 0)
            {
                Frame = frame;
                Value = value;
                if (tangent != null) Tangent = tangent ?? default;
            }

            public Step(int frame, List<float> values, int index, bool long_indices)
            {
                int valueIndex;
                int tangentIndex;

                if (long_indices)
                {
                    valueIndex = index & 0x3FFFFFFF;
                    tangentIndex = (index >> 30) & 3;
                }
                else
                {
                    valueIndex = index & 0x3FFF;
                    tangentIndex = (index >> 14) & 3;
                }

                Frame = frame;
                Value = values[valueIndex];
                if (tangentIndex > 0)
                {
                    Tangent = values[valueIndex + tangentIndex];
                }
            }
        }

        public CMDTrack()
        {
        }

        public CMDTrack(BinaryReader br, List<float> values, int offset = 0)
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

            for (int i = 0; i < stepCount; i++) Steps.Add(new Step(stepList[i], values, indicesList[i], long_indices));
        }
    }
}