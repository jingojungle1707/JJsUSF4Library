using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.EMEClasses.Emitters
{
    public interface IEmitterHasSprites
    {
        int UnkLong0x00 { get; set; }
        int UnkLong0x04 { get; set; }
        int UnkLong0x08 { get; set; }
        int TextureIndex { get; set; } //short
        byte UnkByte0x0E { get; set; } //These bytes might be mirroring/tiling data. Usually [2,2,2,2] but on smoke RING where the sprite is a quarter-circle, it's [2,2,4,4]
        byte UnkByte0x0F { get; set; }
        byte UnkByte0x10 { get; set; }
        byte UnkByte0x11 { get; set; }
        int UnkShort0x12 { get; set; }
        float UnkFloat0x14 { get; set; }
        float UnkFloat0x18 { get; set; }
        float UnkFloat0x1C { get; set; }
        float UnkFloat0x20 { get; set; }
        List<float> Values2 { get; set; } //0x0A
        float ParticleSpawnCount { get; set; }
        //ParticleVariantsCount 0x60
        List<ParticleVariant> ParticleVariants { get; set; }

    }
}
