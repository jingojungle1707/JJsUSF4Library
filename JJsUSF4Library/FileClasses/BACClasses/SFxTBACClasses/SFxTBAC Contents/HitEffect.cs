﻿using System.Collections.Generic;
using System.IO;

namespace JJsUSF4Library.FileClasses.ScriptClasses
{
    public partial class HitEffect
    {
        public string Name;
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
            for (int i = 0; i < 20; i++) HitEffectDatas.Add(new HitEffectData(br, (HitEffectData.HitEffectType)i, offset + hitEffectDataPointers[i]));

        }

        public byte[] GenerateHitEffectBytes()
        {
            List<byte> data = new List<byte>();

            List<int> hitEffectDatasPointerPositions = new List<int>();
            for (int i = 0; i < HitEffectDatas.Count; i++)
            {
                hitEffectDatasPointerPositions.Add(data.Count);
                USF4Utils.AddIntAsBytes(data, -1, true);
            }
            for (int i = 0; i < HitEffectDatas.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(data, hitEffectDatasPointerPositions[i], data.Count);
                int hitEffectDataStartOS = data.Count;
                HitEffectData hed = HitEffectDatas[i];
                USF4Utils.AddIntAsBytes(data, hed.Damage, true);
                USF4Utils.AddIntAsBytes(data, (int)hed.Effect, false);
                USF4Utils.AddIntAsBytes(data, hed.Script, false);
                USF4Utils.AddIntAsBytes(data, hed.AHitstop, false);
                USF4Utils.AddIntAsBytes(data, hed.AShaking, false);
                USF4Utils.AddIntAsBytes(data, hed.VHitstop, false);
                USF4Utils.AddIntAsBytes(data, hed.VShaking, false);
                //0x10
                USF4Utils.AddIntAsBytes(data, (int)hed.Flags, true);
                USF4Utils.AddIntAsBytes(data, hed.HitStun, false);
                USF4Utils.AddIntAsBytes(data, hed.HitStun2, false);
                USF4Utils.AddIntAsBytes(data, hed.UnkShort10_0x18, false);
                data.Add((byte)hed.OffsetCommands.Count);
                data.Add((byte)hed.HitEffectParams.Count);
                USF4Utils.AddIntAsBytes(data, hed.AMeter, false);
                USF4Utils.AddIntAsBytes(data, hed.VMeter, false);
                //0x20
                USF4Utils.AddFloatAsBytes(data, hed.ForceX);
                USF4Utils.AddFloatAsBytes(data, hed.ForceY);
                int offsetCommandPointerPosition = data.Count;
                USF4Utils.AddIntAsBytes(data, 0, true); //0x28 - OffsetCommandPointer
                int hitEffectPointerPosition = data.Count;
                USF4Utils.AddIntAsBytes(data, 0, true); //0x2C - HitEffectParamPointer

                if (hed.OffsetCommands.Count > 0)
                {
                    USF4Utils.UpdateIntAtPosition(data, offsetCommandPointerPosition, data.Count - hitEffectDataStartOS);

                    List<int> paramPointerPositions = new List<int>();
                    for (int j = 0; j < hed.OffsetCommands.Count; j++)
                    {
                        HitEffectData.OffsetCommand ofc = hed.OffsetCommands[j];
                        USF4Utils.AddIntAsBytes(data, ofc.UnkShort0_0x00, false);
                        data.Add(ofc.UnkByte1_0x02);
                        data.Add((byte)ofc.Params.Count);
                        paramPointerPositions.Add(data.Count);
                        USF4Utils.AddIntAsBytes(data, 0, true); //params pointer, to be updated later
                    }
                    //Write OffsetCommand Params
                    for (int j = 0; j < hed.OffsetCommands.Count; j++)
                    {
                        HitEffectData.OffsetCommand offsetCommand = hed.OffsetCommands[j];
                        if (offsetCommand.Params.Count > 0)
                        {
                            USF4Utils.UpdateIntAtPosition(data, paramPointerPositions[j], data.Count - (paramPointerPositions[j] - 4));
                            for (int k = 0; k < offsetCommand.Params.Count; k++)
                            {
                                USF4Utils.AddIntAsBytes(data, offsetCommand.Params[k], true);
                            }
                        }
                    }
                }
                //Write HitEffectParams
                if (hed.HitEffectParams.Count > 0)
                {
                    USF4Utils.UpdateIntAtPosition(data, hitEffectPointerPosition, data.Count - hitEffectDataStartOS);

                    List<int> paramPointerPositions = new List<int>();
                    for (int j = 0; j < hed.HitEffectParams.Count; j++)
                    {
                        HitEffectData.HitEffectParam hitEffectParam = hed.HitEffectParams[j];
                        USF4Utils.AddIntAsBytes(data, hitEffectParam.UnkShort0_0x00, false);
                        data.Add((byte)hitEffectParam.UnkByte1_0x02);
                        data.Add((byte)hitEffectParam.Params.Count);
                        paramPointerPositions.Add(data.Count);
                        USF4Utils.AddIntAsBytes(data, 0, true); //params pointer, to be updated later
                    }
                    for (int j = 0; j < hed.HitEffectParams.Count; j++)
                    {
                        HitEffectData.HitEffectParam hitEffectParam = hed.HitEffectParams[j];
                        if (hitEffectParam.Params.Count > 0)
                        {
                            USF4Utils.UpdateIntAtPosition(data, paramPointerPositions[j], data.Count - (paramPointerPositions[j] - 4));
                            for (int k = 0; k < hitEffectParam.Params.Count; k++)
                            {
                                USF4Utils.AddIntAsBytes(data, hitEffectParam.Params[k], true);
                            }
                        }
                    }
                }
            }

            return data.ToArray();
        }
    }
}
