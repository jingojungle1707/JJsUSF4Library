using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class Node
    {
        public string Name;
        public int Parent;
        public int Child1;
        public int Sibling; //sibling??
        public int Child3;
        public int Child4;
        public int BitFlag;
        public float PreMatrixFloat;
        public Matrix4x4 NodeMatrix;
        public Matrix4x4 TransformMatrix;
        public Matrix4x4 SkinBindPoseMatrix;
        public List<string> child_strings; //Used to rebuild tree relationships from Collada imports

        public Node()
        {

        }

        public Node(BinaryReader br, Skeleton.SkeletonType type, int offset = 0, int iSBPMatrixOffset = 0)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            Parent = br.ReadInt16();
            Child1 = br.ReadInt16();
            Sibling = br.ReadInt16();
            Child3 = br.ReadInt16();
            Child4 = br.ReadInt16();
            BitFlag = br.ReadInt16();
            PreMatrixFloat = br.ReadSingle();
            //0x10
            Matrix4x4 m = new Matrix4x4(
                br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle()
            );

            if (type == Skeleton.SkeletonType.EMA) TransformMatrix = m;
            else if (type == Skeleton.SkeletonType.EMO)
            {
                NodeMatrix = m;

                //Fetch secondary matrix
                br.BaseStream.Seek(iSBPMatrixOffset, SeekOrigin.Begin);

                SkinBindPoseMatrix = new Matrix4x4(
                    br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                    br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                    br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                    br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle()
                );
            }
            
        }
    }
}