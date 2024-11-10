using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.SubfileClasses
{
    public class AnimatedSkeleton : Skeleton
    {
        public List<AnimatedNode> AnimatedNodes { get; set; } = new List<AnimatedNode>();
        new public List<string> NodeNames
        {
            get
            {
                return AnimatedNodes.Select(x => x.Name).ToList();
            }
        }
        public AnimatedSkeleton(Skeleton skeleton)
        {
            AnimatedNodes = new List<AnimatedNode>();
            IKNodes = new List<IKNode>(skeleton.IKNodes);
            IKDataBlocks = new List<IKDataBlock>(skeleton.IKDataBlocks);

            foreach (Node n in skeleton.Nodes)
            {
                AnimatedNodes.Add(new AnimatedNode(n, AnimatedNodes.Count));
            }
        }
    }
}
