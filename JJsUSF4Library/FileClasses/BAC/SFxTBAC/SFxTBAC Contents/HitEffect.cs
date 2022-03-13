using System.Collections.Generic;
using System.IO;

namespace JJsUSF4Library.FileClasses.ScriptClasses
{
    public partial class HitEffect
    {
        public string Name;
        public List<int> HitEffectDataPointers;
        public List<HitEffectData> HitEffectDatas;

        public HitEffect()
        {

        }
        public HitEffect(BinaryReader br, string name, int offset = 0)
        {
            Name = name;

            HitEffectDatas = new List<HitEffectData>();
            List<int> hitEffectDataPointers = new List<int>();
            
            br.BaseStream.Seek(offset, SeekOrigin.Begin);

            for (int i = 0; i < 20; i++) hitEffectDataPointers.Add(br.ReadInt32());
            for (int i = 0; i < 20; i++) HitEffectDatas.Add(new HitEffectData(br, offset + hitEffectDataPointers[i]));

        }
        public HitEffect(byte[] Data, string name)
        {
            Name = name;

            HitEffectDataPointers = new List<int>();
            HitEffectDatas = new List<HitEffectData>();

            for (int i = 0; i < 20; i++)
            {
                HitEffectDataPointers.Add(USF4Utils.ReadInt(true, i * 4, Data));
                //TODO PASS THE CORRECT AMOUNT OF DATA INSTEAD OF THIS BULLSHIT
                HitEffectDatas.Add(new HitEffectData(Data.Slice(HitEffectDataPointers[i], 0)));
            }
        }

        public byte[] GenerateHitEffectBytes()
        {
            List<byte> Data = new List<byte>();

            List<int> HitEffectDatasPointerPositions = new List<int>();
            for (int i = 0; i < HitEffectDatas.Count; i++)
            {
                HitEffectDatasPointerPositions.Add(Data.Count);
                USF4Utils.AddIntAsBytes(Data, -1, true);
            }
            for (int i = 0; i < HitEffectDatas.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(Data, HitEffectDatasPointerPositions[i], Data.Count);
                int hitEffectDataStartOS = Data.Count;
                HitEffectData hed = HitEffectDatas[i];
                USF4Utils.AddIntAsBytes(Data, hed.Damage, true);
                USF4Utils.AddIntAsBytes(Data, hed.Effect, false);
                USF4Utils.AddIntAsBytes(Data, hed.Script, false);
                USF4Utils.AddIntAsBytes(Data, hed.AHitstop, false);
                USF4Utils.AddIntAsBytes(Data, hed.AShaking, false);
                USF4Utils.AddIntAsBytes(Data, hed.VHitstop, false);
                USF4Utils.AddIntAsBytes(Data, hed.VShaking, false);
                //0x10
                USF4Utils.AddIntAsBytes(Data, hed.UnkLong7_0x10, true);
                USF4Utils.AddIntAsBytes(Data, hed.HitStun, false);
                USF4Utils.AddIntAsBytes(Data, hed.HitStun2, false);
                USF4Utils.AddIntAsBytes(Data, hed.UnkShort10_0x18, false);
                Data.Add((byte)hed.OffsetCommands.Count);
                Data.Add((byte)hed.HitEffectParams.Count);
                USF4Utils.AddIntAsBytes(Data, hed.AMeter, false);
                USF4Utils.AddIntAsBytes(Data, hed.VMeter, false);
                //0x20
                USF4Utils.AddFloatAsBytes(Data, hed.ForceX);
                USF4Utils.AddFloatAsBytes(Data, hed.ForceY);
                int offsetCommandPointerPosition = Data.Count;
                USF4Utils.AddIntAsBytes(Data, 0, true); //0x28 - OffsetCommandPointer
                int hitEffectPointerPosition = Data.Count;
                USF4Utils.AddIntAsBytes(Data, 0, true); //0x2C - HitEffectParamPointer

                if (hed.OffsetCommands.Count > 0)
                {
                    USF4Utils.UpdateIntAtPosition(Data, offsetCommandPointerPosition, Data.Count - hitEffectDataStartOS);
                    for (int j = 0; j < hed.OffsetCommands.Count; j++)
                    {
                        HitEffectData.OffsetCommand ofc = hed.OffsetCommands[j];
                        USF4Utils.AddIntAsBytes(Data, ofc.UnkShort0_0x00, false);
                        Data.Add(ofc.UnkByte1_0x02);
                        Data.Add((byte)ofc.Params.Count);
                        USF4Utils.AddIntAsBytes(Data, 0x08, true); //Params pointer, should always be 8?

                        for (int k = 0; k < ofc.Params.Count; k++) USF4Utils.AddIntAsBytes(Data, ofc.Params[k], true);
                    }
                }
                //Write HitEffectParams
                if (hed.HitEffectParams.Count > 0)
                {
                    USF4Utils.UpdateIntAtPosition(Data, hitEffectPointerPosition, Data.Count - hitEffectDataStartOS);
                    for (int j = 0; j < hed.HitEffectParams.Count; j++)
                    {
                        HitEffectData.HitEffectParam hep = hed.HitEffectParams[j];
                        USF4Utils.AddIntAsBytes(Data, hep.UnkShort0_0x00, false);
                        USF4Utils.AddIntAsBytes(Data, hep.UnkShort1_0x02, false);
                        USF4Utils.AddIntAsBytes(Data, hep.UnkShort2_0x04, false);
                        USF4Utils.AddIntAsBytes(Data, hep.UnkShort3_0x06, false);
                    }
                }
            }

            return Data.ToArray();
        }
    }
}
