﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.ScriptClasses
{
    class USF4BCMCharge
    {
        public string Name;
        public int
            Input,
            UnkShort1_0x02,
            MoveFlags,
            ChargeTime,
            UnkShort4_0x08,
            LossTime,
            StorageIndex; //long

        public USF4BCMCharge(BinaryReader br, string name)
        {
            Name = name;

            Input = br.ReadInt16();
            UnkShort1_0x02 = br.ReadInt16();
            MoveFlags = br.ReadInt16();
            ChargeTime = br.ReadInt16();
            UnkShort4_0x08 = br.ReadInt16();
            LossTime = br.ReadInt16();
            StorageIndex = br.ReadInt32();
        }

        public List<byte> GenerateBytes()
        {
            List<byte> data = new List<byte>();

            USF4Utils.AddIntAsBytes(data, Input, false);
            USF4Utils.AddIntAsBytes(data, UnkShort1_0x02, false);
            USF4Utils.AddIntAsBytes(data, MoveFlags, false);
            USF4Utils.AddIntAsBytes(data, ChargeTime, false);
            USF4Utils.AddIntAsBytes(data, UnkShort4_0x08, false);
            USF4Utils.AddIntAsBytes(data, LossTime, false);
            USF4Utils.AddIntAsBytes(data, StorageIndex, true);

            return data;
        }
    }
}
