using JJsUSF4Library.FileClasses.ScriptClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JJsUSF4Library.FileClasses
{
    public class SFxTBCM : BCM
    {
        public List<SFxTBCMInputMotion> InputMotions { get; set; } = new List<SFxTBCMInputMotion>();
        public List<SFxTBCMCharge> Charges { get; set; } = new List<SFxTBCMCharge>();
        public List<SFxTBCMMove> Moves { get; set; } = new List<SFxTBCMMove>();
        public List<SFxTBCMCancel> Cancels { get; set; } = new List<SFxTBCMCancel>();

        public List<string> NameIndex;

        [Flags]
        public enum Inputs
        {
            UNK0 = 0x00,
            UP = 0x01,
            DOWN = 0x02,
            BACK = 0x04,
            FORWARD = 0x08,
            LP = 0x10,
            MP = 0x20,
            HP = 0x40,
            LK = 0x80,
            MK = 0x100,
            HK = 0x200,
            UNK1 = 0x400,
            SELECT = 0x800,
            NEUTRAL = 0x1000,
            UNK4 = 0x2000,
            UNK5 = 0x4000,
            UNK6 = 0x8000,
            NONE = 0xFFFF
        }

        //Header data
        //23 42 43 4D FE FF 4C 00 00 00 00 00

        public byte
            UnkByte_0x34, //These 4 bytes have real values
            UnkByte_0x35,
            UnkByte_0x36,
            UnkByte_0x37;
        public int
            UnkShort_0x38, //Empty?
            UnkShort_0x3A, //Real
            UnkShort_0x3C, //Empty?
            UnkShort_0x3E; //Empty?
        //0x40
        public int
            UnkInt_0x40, //Real, probably a pointer of some kind?
            UnkInt_0x44, //Real, seems to be the a pointer to the end of the file from this position?
            UnkInt_0x48; //Real, always same value as previous uint?
        //END HEADER

        public SFxTBCM()
        {

        }

        public SFxTBCM(BinaryReader br, string name, int offset = 0)
        {
            Name = name;
            ReadFromStream(br, offset);
        }

        public override void ReadFromStream(BinaryReader br, int offset = 0, int fileLength = 0)
        {
            //Initialise lists
            Charges = new List<SFxTBCMCharge>();
            InputMotions = new List<SFxTBCMInputMotion>();
            Moves = new List<SFxTBCMMove>();
            Cancels = new List<SFxTBCMCancel>();
            List<string> chargeNames = new List<string>();
            List<string> inputNames = new List<string>();
            List<string> moveNames = new List<string>();
            List<string> cancelNames = new List<string>();

            List<int> chargePointers = new List<int>();
            List<int> inputMotionPointers = new List<int>();
            List<int> movePointers = new List<int>();
            List<int> cancelPointers = new List<int>();
            List<int> inputMotionNamePointers = new List<int>();
            List<int> chargeNamePointers = new List<int>();
            List<int> moveNamePointers = new List<int>();
            List<int> cancelNamePointers = new List<int>();

            br.BaseStream.Seek(offset + 0x0C, SeekOrigin.Begin);
            //Read header
            int chargeCount = br.ReadInt16();
            int inputMotionCount = br.ReadInt16();
            //0x10
            int moveCount = br.ReadInt16();
            int cancelCount = br.ReadInt16();
            int chargeIndexPointer = br.ReadInt32();
            int chargeNameIndexPointer = br.ReadInt32();
            int inputMotionIndexPointer = br.ReadInt32();
            //0x20
            int inputMotionNameIndexPointer = br.ReadInt32();
            int moveIndexPointer = br.ReadInt32();
            int moveNameIndexPointer = br.ReadInt32();
            int cancelIndexPointer = br.ReadInt32();
            //0x30
            int cancelNameIndexPointer = br.ReadInt32();
            UnkByte_0x34 = br.ReadByte();
            UnkByte_0x35 = br.ReadByte();
            UnkByte_0x36 = br.ReadByte();
            UnkByte_0x37 = br.ReadByte();
            UnkShort_0x38 = br.ReadInt16();
            UnkShort_0x3A = br.ReadInt16();
            UnkShort_0x3C = br.ReadInt16();
            UnkShort_0x3E = br.ReadInt16();
            //0x40
            UnkInt_0x40 = br.ReadInt32();
            UnkInt_0x44 = br.ReadInt32();
            UnkInt_0x48 = br.ReadInt32();
            //END HEADER

            #region Read Pointer Indexes
            //Read pointer and name pointer indexes
            br.BaseStream.Seek(offset + chargeIndexPointer, SeekOrigin.Begin);
            for (int i = 0; i < chargeCount; i++) chargePointers.Add(br.ReadInt32());
            br.BaseStream.Seek(offset + chargeNameIndexPointer, SeekOrigin.Begin);
            for (int i = 0; i < chargeCount; i++) chargeNamePointers.Add(br.ReadInt32());
            br.BaseStream.Seek(offset + inputMotionIndexPointer, SeekOrigin.Begin);
            for (int i = 0; i < inputMotionCount; i++) inputMotionPointers.Add(br.ReadInt32());
            br.BaseStream.Seek(offset + inputMotionNameIndexPointer, SeekOrigin.Begin);
            for (int i = 0; i < inputMotionCount; i++) inputMotionNamePointers.Add(br.ReadInt32());
            br.BaseStream.Seek(offset + moveIndexPointer, SeekOrigin.Begin);
            for (int i = 0; i < moveCount; i++) movePointers.Add(br.ReadInt32());
            br.BaseStream.Seek(offset + moveNameIndexPointer, SeekOrigin.Begin);
            for (int i = 0; i < moveCount; i++) moveNamePointers.Add(br.ReadInt32());
            br.BaseStream.Seek(offset + cancelIndexPointer, SeekOrigin.Begin);
            for (int i = 0; i < cancelCount; i++) cancelPointers.Add(br.ReadInt32());
            br.BaseStream.Seek(offset + cancelNameIndexPointer, SeekOrigin.Begin);
            for (int i = 0; i < cancelCount; i++) cancelNamePointers.Add(br.ReadInt32());
            #endregion

            #region Read Names
            //Read names
            for (int i = 0; i < chargeCount; i++)
            {
                if (chargeNamePointers[i] == 0) chargeNames.Add(string.Empty);
                else
                {
                    br.BaseStream.Seek(offset + chargeNamePointers[i], SeekOrigin.Begin);
                    chargeNames.Add(USF4Utils.ReadZString(br));
                }
            }
            for (int i = 0; i < inputMotionCount; i++)
            {
                if (inputMotionNamePointers[i] == 0) inputNames.Add(string.Empty);
                else
                {
                    br.BaseStream.Seek(offset + inputMotionNamePointers[i], SeekOrigin.Begin);
                    inputNames.Add(USF4Utils.ReadZString(br));
                }
            }
            for (int i = 0; i < moveCount; i++)
            {
                if (moveNamePointers[i] == 0) moveNames.Add(string.Empty);
                else
                {
                    br.BaseStream.Seek(offset + moveNamePointers[i], SeekOrigin.Begin);
                    moveNames.Add(USF4Utils.ReadZString(br));
                }
            }
            for (int i = 0; i < cancelCount; i++)
            {
                if (cancelNamePointers[i] == 0) cancelNames.Add(string.Empty);
                else
                {
                    br.BaseStream.Seek(offset + cancelNamePointers[i], SeekOrigin.Begin);
                    cancelNames.Add(USF4Utils.ReadZString(br));
                }
            }
            #endregion

            #region Read Data
            for (int i = 0; i < chargeCount; i++)
            {
                if (chargePointers[i] == 0) Charges.Add(new SFxTBCMCharge());
                else
                {
                    Charges.Add(new SFxTBCMCharge(br, chargeNames[i], offset + chargePointers[i]));
                }
            }
            for (int i = 0; i < inputMotionCount; i++)
            {
                if (inputMotionPointers[i] == 0) InputMotions.Add(new SFxTBCMInputMotion());
                else
                {
                    InputMotions.Add(new SFxTBCMInputMotion(br, inputNames[i], offset + inputMotionPointers[i]));
                }
            }
            for (int i = 0; i < moveCount; i++)
            {
                if (movePointers[i] == 0) Moves.Add(new SFxTBCMMove());
                else
                {
                    Moves.Add(new SFxTBCMMove(br, moveNames[i], inputNames, offset + movePointers[i]));
                }
            }
            for (int i = 0; i < cancelCount; i++)
            {
                if (cancelPointers[i] == 0) Cancels.Add(new SFxTBCMCancel());
                else
                {
                    Cancels.Add(new SFxTBCMCancel(br, cancelNames[i], offset + cancelPointers[i]));
                }
            }

            #endregion
        }

        public override byte[] GenerateBytes()
        {
            List<byte> Data = new List<byte>();

            //Add header
            Data.AddRange(new byte[] { 0x23, 0x42, 0x43, 0x4D, 0xFE, 0xFF, 0x4C, 0x00, 0x00, 0x00, 0x00, 0x00 });
            USF4Utils.AddIntAsBytes(Data, Charges.Count, false);
            USF4Utils.AddIntAsBytes(Data, InputMotions.Count, false);
            //0x10
            USF4Utils.AddIntAsBytes(Data, Moves.Count, false);
            USF4Utils.AddIntAsBytes(Data, Cancels.Count, false);
            int chargeIndexPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, 0, true);
            int chargeNameIndexPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, 0, true);
            int inputMotionIndexPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, 0, true);
            //0x20
            int inputMotionNameIndexPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, 0, true);
            int moveIndexPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, 0, true);
            int moveNameIndexPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, 0, true);
            int cancelIndexPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, 0, true);
            //0x30
            int cancelNameIndexPointerPosition = Data.Count;
            USF4Utils.AddIntAsBytes(Data, 0, true);
            Data.Add(UnkByte_0x34);
            Data.Add(UnkByte_0x35);
            Data.Add(UnkByte_0x36);
            Data.Add(UnkByte_0x37);
            USF4Utils.AddIntAsBytes(Data, UnkShort_0x38, false);
            USF4Utils.AddIntAsBytes(Data, UnkShort_0x3A, false);
            USF4Utils.AddIntAsBytes(Data, UnkShort_0x3C, false);
            USF4Utils.AddIntAsBytes(Data, UnkShort_0x3E, false);
            USF4Utils.AddIntAsBytes(Data, UnkInt_0x40, true);
            USF4Utils.AddIntAsBytes(Data, UnkInt_0x44, true);
            USF4Utils.AddIntAsBytes(Data, UnkInt_0x48, true);

            List<int> chargeIndexPositions = new List<int>();
            List<int> chargeNameIndexPositions = new List<int>();
            if (Charges.Count > 0)
            {
                USF4Utils.UpdateIntAtPosition(Data, chargeIndexPointerPosition, Data.Count);
                for (int i = 0; i < Charges.Count; i++)
                {
                    chargeIndexPositions.Add(Data.Count);
                    USF4Utils.AddIntAsBytes(Data, i, true);
                }
                USF4Utils.UpdateIntAtPosition(Data, chargeNameIndexPointerPosition, Data.Count);
                for (int i = 0; i < Charges.Count; i++)
                {
                    chargeNameIndexPositions.Add(Data.Count);
                    USF4Utils.AddIntAsBytes(Data, i, true);
                }
            }
            List<int> inputMotionIndexPositions = new List<int>();
            List<int> inputMotionNameIndexPostions = new List<int>();
            if (InputMotions.Count > 0)
            {
                USF4Utils.UpdateIntAtPosition(Data, inputMotionIndexPointerPosition, Data.Count);
                for (int i = 0; i < InputMotions.Count; i++)
                {
                    inputMotionIndexPositions.Add(Data.Count);
                    USF4Utils.AddIntAsBytes(Data, i, true);
                }
                USF4Utils.UpdateIntAtPosition(Data, inputMotionNameIndexPointerPosition, Data.Count);
                for (int i = 0; i < InputMotions.Count; i++)
                {
                    inputMotionNameIndexPostions.Add(Data.Count);
                    USF4Utils.AddIntAsBytes(Data, i, true);
                } 
            }
            List<int> moveIndexPositions = new List<int>();
            List<int> moveNameIndexPostions = new List<int>();
            if (Moves.Count > 0)
            {
                USF4Utils.UpdateIntAtPosition(Data, moveIndexPointerPosition, Data.Count);
                for (int i = 0; i < Moves.Count; i++)
                {
                    moveIndexPositions.Add(Data.Count);
                    USF4Utils.AddIntAsBytes(Data, i, true);
                }
                USF4Utils.UpdateIntAtPosition(Data, moveNameIndexPointerPosition, Data.Count);
                for (int i = 0; i < Moves.Count; i++)
                {
                    moveNameIndexPostions.Add(Data.Count);
                    USF4Utils.AddIntAsBytes(Data, i, true);
                } 
            }
            List<int> cancelIndexPositions = new List<int>();
            List<int> cancelNameIndexPositions = new List<int>();
            if (Cancels.Count > 0)
            {
                USF4Utils.UpdateIntAtPosition(Data, cancelIndexPointerPosition, Data.Count);
                for (int i = 0; i < Cancels.Count; i++)
                {
                    cancelIndexPositions.Add(Data.Count);
                    USF4Utils.AddIntAsBytes(Data, 0, true);
                }
                USF4Utils.UpdateIntAtPosition(Data, cancelNameIndexPointerPosition, Data.Count);
                for (int i = 0; i < Cancels.Count; i++)
                {
                    cancelNameIndexPositions.Add(Data.Count);
                    USF4Utils.AddIntAsBytes(Data, 0, true);
                } 
            }

            //Indexes are set up, write Charges
            for (int i = 0; i < Charges.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(Data, chargeIndexPositions[i], Data.Count);
                USF4Utils.AddIntAsBytes(Data, (int)Charges[i].Input, false);
                USF4Utils.AddIntAsBytes(Data, Charges[i].Flags, false);
                USF4Utils.AddIntAsBytes(Data, Charges[i].UnkShort2_0x04, false);
                USF4Utils.AddIntAsBytes(Data, Charges[i].UnkShort3_0x06, false);
                USF4Utils.AddIntAsBytes(Data, Charges[i].ChargeTime, false);
                USF4Utils.AddIntAsBytes(Data, Charges[i].LossTime, false);
                USF4Utils.AddIntAsBytes(Data, Charges[i].StorageIndex, false);
                USF4Utils.AddIntAsBytes(Data, Charges[i].UnkShort7_0x0E, false);
            }
            //Write InputMotions
            for (int i = 0; i < InputMotions.Count; i++)
            {
                //List<int> inputMotionDetailsPointerPositions = new List<int>();
                USF4Utils.UpdateIntAtPosition(Data, inputMotionIndexPositions[i], Data.Count);
                for (int j = 0; j < InputMotions[i].InputDetails.Count; j++)
                {
                    //inputMotionDetailsPointerPositions.Add(Data.Count);
                    USF4Utils.AddIntAsBytes(Data, InputMotions[i].InputDetails.Count * 4 + j * 0x104, true);
                }
                for (int j = 0; j < InputMotions[i].InputDetails.Count; j++)
                {
                    int start_offset = Data.Count;
                    USF4Utils.AddIntAsBytes(Data, InputMotions[i].InputDetails[j].InputCount, false);
                    for (int k = 0; k < InputMotions[i].InputDetails[j].Type.Count; k++)
                    {
                        USF4Utils.AddIntAsBytes(Data, InputMotions[i].InputDetails[j].Type[k], true);
                        USF4Utils.AddIntAsBytes(Data, InputMotions[i].InputDetails[j].Buffer[k], false);
                        USF4Utils.AddIntAsBytes(Data, (int)InputMotions[i].InputDetails[j].Input[k], false);
                        USF4Utils.AddIntAsBytes(Data, InputMotions[i].InputDetails[j].MoveFlags[k], false);
                        USF4Utils.AddIntAsBytes(Data, InputMotions[i].InputDetails[j].Flags[k], true);
                        USF4Utils.AddIntAsBytes(Data, InputMotions[i].InputDetails[j].Requirement[k], false);
                    }
                    USF4Utils.AddPaddingZeros(Data, start_offset + 0x104, Data.Count);
                }
            }
            //Write Moves
            for (int i = 0; i < Moves.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(Data, moveIndexPositions[i], Data.Count);
                USF4Utils.AddIntAsBytes(Data, (int)Moves[i].Input, false);
                USF4Utils.AddIntAsBytes(Data, (int)Moves[i].InputFlags, false);
                USF4Utils.AddIntAsBytes(Data, (int)Moves[i].PositionRestriction, false);
                USF4Utils.AddIntAsBytes(Data, Moves[i].UnkShort3_0x06, false);
                USF4Utils.AddIntAsBytes(Data, Moves[i].UnkShort4_0x08, false);
                USF4Utils.AddIntAsBytes(Data, Moves[i].UnkShort5_0x0A, false);
                USF4Utils.AddFloatAsBytes(Data, Moves[i].UnknownFloat_0x0C);
                //0x10
                USF4Utils.AddIntAsBytes(Data, Moves[i].PositionRestrictionDistance, false);
                USF4Utils.AddIntAsBytes(Data, Moves[i].Restriction, false);
                USF4Utils.AddIntAsBytes(Data, Moves[i].UnkShort9_0x14, false);
                USF4Utils.AddIntAsBytes(Data, Moves[i].UnkShort10_0x16, false);
                USF4Utils.AddIntAsBytes(Data, Moves[i].MeterReq, false);
                USF4Utils.AddIntAsBytes(Data, Moves[i].MeterLoss, false);
                int inputMotion = InputMotions.Select(o => o.Name).ToList().IndexOf(Moves[i].InputMotion);
                USF4Utils.AddIntAsBytes(Data, inputMotion, false);
                USF4Utils.AddIntAsBytes(Data, Moves[i].Script, false);
                //0x20
                Data.Add(Moves[i].UnkByte15_0x20);
                Data.Add(Moves[i].UnkByte16_0x21);
                USF4Utils.AddIntAsBytes(Data, Moves[i].UnkShort17_0x22, false);
                USF4Utils.AddIntAsBytes(Data, Moves[i].UnkShort18_0x24, false);
                USF4Utils.AddIntAsBytes(Data, Moves[i].AIMinimumDistance, false);
                USF4Utils.AddFloatAsBytes(Data, Moves[i].AIMaxDistance);
                USF4Utils.AddFloatAsBytes(Data, Moves[i].UnknownFloat_0x2C);
                //0x30
                USF4Utils.AddFloatAsBytes(Data, Moves[i].UnknownFloat_0x30);
                USF4Utils.AddIntAsBytes(Data, Moves[i].UnkShort23_0x34, false);
                USF4Utils.AddIntAsBytes(Data, Moves[i].UnkShort24_0x36, false);
                USF4Utils.AddIntAsBytes(Data, Moves[i].UnkShort25_0x38, false);
                Data.Add(Moves[i].UnkByte26_0x3A);
                Data.Add(Moves[i].UnkByte27_0x3B);
                Data.Add(Moves[i].UnkByte28_0x3C);
                Data.Add(Moves[i].UnkByte29_0x3D);
                Data.Add(Moves[i].AIFar);
                Data.Add(Moves[i].AIVeryFar);
            }
            //Write cancels
            List<int> CancellableMovePointerPositions = new List<int>();
            List<int> CancelFlagsPointerPositions = new List<int>();
            int CancelIndexPosition = Data.Count;
            for (int i = 0; i < Cancels.Count; i++)
            {
                //If we don't have any data, null the pointer
                if (Cancels[i].CancellableMoves == null || Cancels[i].CancelFlags == null)
                {
                    USF4Utils.UpdateIntAtPosition(Data, cancelIndexPositions[i], 0);
                    //Add junk here to make sure the indices stay aligned
                    CancellableMovePointerPositions.Add(0);
                    CancelFlagsPointerPositions.Add(0);
                }
                else
                {
                    USF4Utils.UpdateIntAtPosition(Data, cancelIndexPositions[i], Data.Count);
                    USF4Utils.AddIntAsBytes(Data, 0, true);
                    USF4Utils.AddIntAsBytes(Data, Cancels[i].CancellableMoveCount, true);
                    CancellableMovePointerPositions.Add(Data.Count);
                    USF4Utils.AddIntAsBytes(Data, Cancels[i].CancellableMoveIndexPointer, true);
                    CancelFlagsPointerPositions.Add(Data.Count);
                    USF4Utils.AddIntAsBytes(Data, Cancels[i].CancelFlagsPointer, true);
                }
            }
            for (int i = 0; i < Cancels.Count; i++)
            {
                //Skip if no data
                if (Cancels[i].CancellableMoves == null || Cancels[i].CancelFlags == null)
                    continue;

                USF4Utils.UpdateIntAtPosition(Data, CancellableMovePointerPositions[i], Data.Count - CancelIndexPosition);
                for (int j = 0; j < Cancels[i].CancellableMoves.Count; j++)
                    USF4Utils.AddIntAsBytes(Data, Cancels[i].CancellableMoves[j], false);

                USF4Utils.UpdateIntAtPosition(Data, CancelFlagsPointerPositions[i], Data.Count - CancelIndexPosition);
                for (int j = 0; j < Cancels[i].CancelFlags.Count; j++)
                    Data.AddRange(Cancels[i].CancelFlags[j]);

                CancelIndexPosition += 0x10;
            }
            //Original file seems to have some random padding zeroes between some of the cancellablemove lists and cancelflag lists... probably ok?

            //Write aaaaaall the names, update pointers all over the shop, this is such a stupid file layout.
            for (int i = 0; i < Charges.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(Data, chargeNameIndexPositions[i], Data.Count);
                Data.AddRange(Encoding.ASCII.GetBytes(Charges[i].Name));
                Data.Add(0);
            }
            for (int i = 0; i < InputMotions.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(Data, inputMotionNameIndexPostions[i], Data.Count);
                Data.AddRange(Encoding.ASCII.GetBytes(InputMotions[i].Name));
                Data.Add(0);
            }
            for (int i = 0; i < Moves.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(Data, moveNameIndexPostions[i], Data.Count);
                Data.AddRange(Encoding.ASCII.GetBytes(Moves[i].Name));
                Data.Add(0);
            }
            for (int i = 0; i < Cancels.Count; i++)
            {
                if (Cancels[i].CancellableMoves == null || Cancels[i].CancelFlags == null)
                    continue;
                USF4Utils.UpdateIntAtPosition(Data, cancelNameIndexPositions[i], Data.Count);
                Data.AddRange(Encoding.ASCII.GetBytes(Cancels[i].Name));
                Data.Add(0);
            }
            return Data.ToArray();
        }
    }
}
