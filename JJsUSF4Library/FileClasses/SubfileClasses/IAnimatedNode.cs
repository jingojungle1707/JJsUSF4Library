using System.Numerics;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public interface IAnimatedNode
    {
        bool AnimatedAbsoluteRotationFlag { get; set; }
        bool AnimatedAbsoluteScaleFlag { get; set; }
        bool AnimatedAbsoluteTranslationFlag { get; set; }
        Matrix4x4 AnimatedLocalMatrix { get; set; }
        Matrix4x4 AnimatedMatrix { get; set; }
        Vector3 AnimatedRotation { get; set; }
        Quaternion AnimatedRotationQuaternion { get; set; }
        Vector3 AnimatedScale { get; set; }
        Vector3 AnimatedTranslation { get; set; }
        bool AnimationProcessingDone { get; set; }
        int ID { get; set; }
        bool IKanimatedNode { get; set; }
        Vector3 Rotation { get; set; }
        Quaternion RotationQuaternion { get; set; }
        Vector3 Scale { get; set; }
        Vector3 Translation { get; set; }
    }
}