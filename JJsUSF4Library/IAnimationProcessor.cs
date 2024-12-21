using JJsUSF4Library.FileClasses.SubfileClasses;
using System.Collections.Generic;
using System.Numerics;

namespace JJsUSF4Library
{
    public interface IAnimationProcessor
    {
        AnimatedNode[] AnimatedNodes { get; }
        AnimatedSkeleton AnimatedSkeleton { get; }
        int CurrentAnimationIndex { get; set; }
        string CurrentAnimationName { get; }
        float CurrentFrame { get; set; }

        public Matrix4x4 GetAnimatedLocalMatrixByNodeName(string nodeName);

        AnimatedNode[] ReturnAnimatedNodes();
    }
}