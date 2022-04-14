using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.EMEClasses.Emitters
{
    public class ParticleVariant
    {
        public float SpawnCount { get; set; }
        public float XOffset { get; set; }
        public float YOffset { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public ParticleVariant()
        {

        }
        public ParticleVariant(BinaryReader br)
        {
            SpawnCount = br.ReadSingle();
            XOffset = br.ReadSingle();
            YOffset = br.ReadSingle();
            Width = br.ReadSingle();
            Height = br.ReadSingle();
        }
    }
}
