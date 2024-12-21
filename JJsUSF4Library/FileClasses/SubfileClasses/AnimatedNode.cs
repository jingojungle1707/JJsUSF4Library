using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class AnimatedNode : Node, IAnimatedNode
    {
        public int ID { get; set; }
        public Vector3 Translation { get; set; }
        public Vector3 Rotation { get; set; } //RADIANS
        public Vector3 Scale { get; set; }
        public Quaternion RotationQuaternion { get; set; }

        public bool AnimationProcessingDone { get; set; }
        public bool IKanimatedNode { get; set; }

        public bool AnimatedAbsoluteRotationFlag { get; set; }
        public bool AnimatedAbsoluteScaleFlag { get; set; }
        public bool AnimatedAbsoluteTranslationFlag { get; set; }


        public Matrix4x4 AnimatedMatrix { get; set; }
        public Matrix4x4 AnimatedLocalMatrix { get; set; }
        public Vector3 AnimatedTranslation { get; set; }
        public Vector3 AnimatedRotation { get; set; } //RADIANS
        public Vector3 AnimatedScale { get; set; }
        public Quaternion AnimatedRotationQuaternion { get; set; }
        public AnimatedNode()
        {

        }
        public AnimatedNode(Node n, int id)
        {
            Name = n.Name;
            ID = id;
            Parent = n.Parent;
            Child1 = n.Child1;
            Sibling = n.Sibling;
            BitFlag = n.BitFlag;
            BoneLength = n.BoneLength;
            TransformMatrix = n.TransformMatrix;
            AnimatedMatrix = n.TransformMatrix;

            AnimationProcessingDone = false;
            IKanimatedNode = false;

            AnimatedAbsoluteRotationFlag = false;
            AnimatedAbsoluteScaleFlag = false;
            AnimatedAbsoluteTranslationFlag = false;

            Matrix4x4.Decompose(TransformMatrix, out Vector3 scale, out Quaternion rotationQuaternion, out Vector3 translation);
            Scale = scale;
            RotationQuaternion = rotationQuaternion;
            Translation = translation;
            Vector3 rotation;
            EMAProcessorUtils.DecomposeMatrix_AE(TransformMatrix, out _, out _, out _, out rotation.X, out rotation.Y, out rotation.Z, out _, out _, out _);
            Rotation = rotation;

            AnimatedTranslation = Translation;
            AnimatedRotation = Rotation;
            AnimatedScale = Scale;
            AnimatedRotationQuaternion = RotationQuaternion;
        }

    }
}
