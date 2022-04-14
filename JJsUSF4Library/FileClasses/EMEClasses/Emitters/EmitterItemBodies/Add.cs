using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.EMEClasses.Emitters
{
    public class Add : EmitterItemBody, IEmitterHasSprites
    {
        private const int _valuesCount = 0x15;
        private const int _values2Count = 0x0A;
        
        public int UnkLong0x00 { get; set; }
        public int UnkLong0x04 { get; set; }
        public int UnkLong0x08 { get; set; }
        public int TextureIndex { get; set; } //short
        public byte UnkByte0x0E { get; set; } //These bytes might be mirroring/tiling data. Usually [2,2,2,2] but on smoke RING where the sprite is a quarter-circle, it's [2,2,4,4]
        public byte UnkByte0x0F { get; set; }
        public byte UnkByte0x10 { get; set; }
        public byte UnkByte0x11 { get; set; }
        public int UnkShort0x12 { get; set; }
        public float UnkFloat0x14 { get; set; } //x-offset? (always zero so far)
        public float UnkFloat0x18 { get; set; } //y-offset? (always zero so far)
        public float UnkFloat0x1C { get; set; } //Width?
        public float UnkFloat0x20 { get; set; } //Height?
        public List<float> Values2 { get; set; } = new List<float>();//Float x 0x0A, total length
        public float ParticleSpawnCount { get; set; }
        //ParticleVariantsCount, 0x60, long
        //For ParticleVariants, "frame" == number of particles to spawn of that type?
        public List<ParticleVariant> ParticleVariants { get; set; } = new List<ParticleVariant>();
        public EmitterItem Modifier { get; set; }

        public byte[] Register1 { get; set; } = new byte[8];
        public Add()
        {

        }
        public Add(BinaryReader br)
        {
            for (int i = 0; i < _valuesCount; i++)
            {
                Values.Add(br.ReadSingle());
            }
            Register1 = br.ReadBytes(8);
            UnkLong0x00 = br.ReadInt32();
            UnkLong0x04 = br.ReadInt32();
            UnkLong0x08 = br.ReadInt32();
            TextureIndex = br.ReadInt16();
            UnkByte0x0E = br.ReadByte();
            UnkByte0x0F = br.ReadByte();
            UnkByte0x10 = br.ReadByte();
            UnkByte0x11 = br.ReadByte();
            UnkShort0x12 = br.ReadInt16();
            UnkFloat0x14 = br.ReadSingle();
            UnkFloat0x18 = br.ReadSingle();
            UnkFloat0x1C = br.ReadSingle();
            UnkFloat0x20 = br.ReadSingle();
            for (int i = 0; i < _values2Count; i++)
            {
                Values2.Add(br.ReadSingle());
            }
            ParticleSpawnCount = br.ReadSingle();
            int particalVariantCount = br.ReadInt32();

            for (int i = 0; i < particalVariantCount; i++)
            {
                ParticleVariants.Add(new ParticleVariant(br));
            }
        }
    }
}
