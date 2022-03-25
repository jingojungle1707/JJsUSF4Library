using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class Node
    {
        public string Name { get; set; }
        public string Parent { get; set; }
        public string Child1 { get; set; }
        public string Sibling { get; set; }
        public string Child3 { get; set; }
        public string Child4 { get; set; }
        public int BitFlag { get; set; }
        public float BoneLength { get; set; }
        public Matrix4x4 LocalMatrix { get; set; }
        public Matrix4x4 TransformMatrix { get; set; }
        public Matrix4x4 InverseSkinBindPoseMatrix { get; set; }

        public Node()
        {

        }

        public Node(BinaryReader br, Skeleton.SkeletonType type, string name, List<string> nodeNames, int offset = 0, int iSBPMatrixOffset = 0)
        {
            Name = name;

            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            int parentIndex = br.ReadInt16();
            int child1Index = br.ReadInt16();
            int siblingIndex = br.ReadInt16();
            int child3Index = br.ReadInt16();
            int child4Index = br.ReadInt16();

            Parent = parentIndex != -1 ? nodeNames[parentIndex] : string.Empty;
            Child1 = child1Index != -1 ? nodeNames[child1Index] : string.Empty;
            Sibling = siblingIndex != -1 ? nodeNames[siblingIndex] : string.Empty;
            Child3 = child3Index != -1 ? nodeNames[child3Index] : string.Empty;
            Child4 = child4Index != -1 ? nodeNames[child4Index] : string.Empty;
            BitFlag = br.ReadInt16();
            BoneLength = br.ReadSingle();
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
                LocalMatrix = m;

                //Fetch secondary matrix
                br.BaseStream.Seek(iSBPMatrixOffset, SeekOrigin.Begin);

                InverseSkinBindPoseMatrix = new Matrix4x4(
                    br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                    br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                    br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                    br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle()
                );
            }
        }

        /// <summary>
        /// Generates the binary data for the primary node entry.
        /// <para>Relationships, bitflag, bonelength, local matrix or default transform matrix</para>
        /// </summary>
        /// <param name="nodeNames"></param>
        /// <returns></returns>
        public List<byte> GenerateMainBytes(Skeleton.SkeletonType type, List<string> nodeNames)
        {
            List<byte> data = new List<byte>();

            USF4Utils.AddSignedShortAsBytes(data, nodeNames.IndexOf(Parent));
            USF4Utils.AddSignedShortAsBytes(data, nodeNames.IndexOf(Child1));
            USF4Utils.AddSignedShortAsBytes(data, nodeNames.IndexOf(Sibling));
            USF4Utils.AddSignedShortAsBytes(data, nodeNames.IndexOf(Child3));
            USF4Utils.AddSignedShortAsBytes(data, nodeNames.IndexOf(Child4));
            USF4Utils.AddSignedShortAsBytes(data, BitFlag);
            USF4Utils.AddFloatAsBytes(data, BoneLength);

            //Check if we need LocalMatrix or TransformMatrix
            Matrix4x4 m = (type == Skeleton.SkeletonType.EMO) ? LocalMatrix : TransformMatrix;
            USF4Utils.AddMatrix4x4AsBytes(data, m);

            return data;
        }
    }
}