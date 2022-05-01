using JJsUSF4Library.FileClasses.ScriptClasses;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JJsUSF4Library.FileClasses
{
    public class USF4BCM : BCM
    {
        List<USF4BCMCharge> Charges;
        List<USF4BCMInputMotion> InputMotions;
        List<USF4BCMMove> Moves;
        List<USF4BCMCancel> Cancels;

        public USF4BCM()
        {

        }

        public USF4BCM(BinaryReader br, string name, int offset = 0)
        {
            Name = name;
            ReadFromStream(br, offset);          
        }

        public override void ReadFromStream(BinaryReader br, int offset = 0, int fileLength = 0)
        {
            Charges = new List<USF4BCMCharge>();
            InputMotions = new List<USF4BCMInputMotion>();
            Moves = new List<USF4BCMMove>();
            Cancels = new List<USF4BCMCancel>();
            

            List<int> chargeNamePointers = new List<int>();
            List<int> inputMotionNamePointers = new List<int>();
            List<int> movesNamePointers = new List<int>();
            List<int> cancelsNamePointers = new List<int>();

            List<string> chargeNames = new List<string>();
            List<string> inputMotionNames = new List<string>();
            List<string> moveNames = new List<string>();
            List<string> cancelNames = new List<string>();

            br.BaseStream.Seek(offset + 0x10, SeekOrigin.Begin);
            #region Read Header
            //0x10
            int chargeCount = br.ReadInt16();
            int inputMotionCount = br.ReadInt16();
            int movesCount = br.ReadInt16();
            int cancelsCount = br.ReadInt16();

            int chargePointer = br.ReadInt32();
            int chargeNameIndexPointer = br.ReadInt32();
            //0x20
            int inputMotionPointer = br.ReadInt32();
            int inputMotionNameIndexPointer = br.ReadInt32();
            int movesPointer = br.ReadInt32();
            int movesNameIndexPointer = br.ReadInt32();
            //0x30
            int cancelsPointer = br.ReadInt32();
            int cancelsNameIndexPointer = br.ReadInt32();
            #endregion

            #region Read Name Index
            //Read names
            //Charges
            br.BaseStream.Seek(offset + chargeNameIndexPointer, SeekOrigin.Begin);
            for (int i = 0; i < chargeCount; i++) chargeNamePointers.Add(br.ReadInt32());
            for (int i = 0; i < chargeCount; i++)
            {
                br.BaseStream.Seek(offset + chargeNamePointers[i], SeekOrigin.Begin);
                chargeNames.Add(USF4Utils.ReadZString(br));
            }
            //Input Motions
            br.BaseStream.Seek(offset + inputMotionNameIndexPointer, SeekOrigin.Begin);
            for (int i = 0; i < inputMotionCount; i++) inputMotionNamePointers.Add(br.ReadInt32());
            for (int i = 0; i < inputMotionCount; i++)
            {
                br.BaseStream.Seek(offset + inputMotionNamePointers[i], SeekOrigin.Begin);
                inputMotionNames.Add(USF4Utils.ReadZString(br));
            }
            //Moves
            br.BaseStream.Seek(offset + movesNameIndexPointer, SeekOrigin.Begin);
            for (int i = 0; i < movesCount; i++) movesNamePointers.Add(br.ReadInt32());
            for (int i = 0; i < movesCount; i++)
            {
                br.BaseStream.Seek(offset + movesNamePointers[i], SeekOrigin.Begin);
                moveNames.Add(USF4Utils.ReadZString(br));
            }
            //Cancels
            br.BaseStream.Seek(offset + cancelsNameIndexPointer, SeekOrigin.Begin);
            for (int i = 0; i < cancelsCount; i++) cancelsNamePointers.Add(br.ReadInt32());
            for (int i = 0; i < cancelsCount; i++)
            {
                br.BaseStream.Seek(offset + cancelsNamePointers[i], SeekOrigin.Begin);
                cancelNames.Add(USF4Utils.ReadZString(br));
            }
            #endregion

            #region Read Data
            br.BaseStream.Seek(offset + chargePointer, SeekOrigin.Begin);
            for (int i = 0; i < chargeCount; i++) Charges.Add(new USF4BCMCharge(br, chargeNames[i]));

            for (int i = 0; i < inputMotionCount; i++) InputMotions.Add(new USF4BCMInputMotion(br, inputMotionNames[i], offset + inputMotionPointer + i * 0xC4));

            for (int i = 0; i < movesCount; i++) Moves.Add(new USF4BCMMove(br, moveNames[i], offset + movesPointer + i * 0x54));

            for (int i = 0; i < cancelsCount; i++) Cancels.Add(new USF4BCMCancel(br, cancelNames[i], moveNames, offset + cancelsPointer + i * 0x08));

            #endregion
        }

        public override byte[] GenerateBytes()
        {
            List<byte> data = new List<byte>()
            {
                0x23, 0x42, 0x43, 0x4D, 0xFE, 0xFF, 0x28, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            #region Write Header
            //0x10
            USF4Utils.AddIntAsBytes(data, Charges.Count, false);
            USF4Utils.AddIntAsBytes(data, InputMotions.Count, false);
            USF4Utils.AddIntAsBytes(data, Moves.Count, false);
            USF4Utils.AddIntAsBytes(data, Cancels.Count, false);

            int chargePointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, 0, true);
            int chargeNameIndexPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, 0, true);
            //0x20
            int inputMotionPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, 0, true);
            int inputMotionNameIndexPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, 0, true);
            int movesPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, 0, true);
            int movesNameIndexPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, 0, true);
            //0x30
            int cancelsPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, 0, true);
            int cancelsNameIndexPointerPosition = data.Count;
            USF4Utils.AddIntAsBytes(data, 0, true);
            #endregion

            #region Write Charges
            List<int> chargeNamePointerPositions = new List<int>();
            if (Charges.Count > 0)
            {
                USF4Utils.UpdateIntAtPosition(data, chargePointerPosition, data.Count);

                foreach (USF4BCMCharge charge in Charges) data.AddRange(charge.GenerateBytes());

                USF4Utils.UpdateIntAtPosition(data, chargeNameIndexPointerPosition, data.Count);
                for (int i = 0; i < Charges.Count; i++)
                {
                    chargeNamePointerPositions.Add(data.Count);
                    USF4Utils.AddIntAsBytes(data, 0, true);
                }
            }
            #endregion

            #region Write InputMotions
            List<int> inputMotionNamePointerPositions = new List<int>();
            if (InputMotions.Count > 0)
            {
                USF4Utils.UpdateIntAtPosition(data, inputMotionPointerPosition, data.Count);

                foreach (USF4BCMInputMotion input in InputMotions) data.AddRange(input.GenerateBytes());

                USF4Utils.UpdateIntAtPosition(data, inputMotionNameIndexPointerPosition, data.Count);
                for (int i = 0; i < InputMotions.Count; i++)
                {
                    inputMotionNamePointerPositions.Add(data.Count);
                    USF4Utils.AddIntAsBytes(data, 0, true);
                }
            }
            #endregion

            #region Write Moves
            List<int> moveNamePointerPositions = new List<int>();
            if (Moves.Count > 0)
            {
                USF4Utils.UpdateIntAtPosition(data, movesPointerPosition, data.Count);

                foreach (USF4BCMMove move in Moves) data.AddRange(move.GenerateBytes());

                USF4Utils.UpdateIntAtPosition(data, movesNameIndexPointerPosition, data.Count);
                for (int i = 0; i < Moves.Count; i++)
                {
                    moveNamePointerPositions.Add(data.Count);
                    USF4Utils.AddIntAsBytes(data, 0, true);
                }
            }
            #endregion

            #region Write Cancels
            List<int> cancelNamePointerPositions = new List<int>();
            List<int> cancelPointerPositions = new List<int>();

            if (Cancels.Count > 0)
            {
                int cancelStartOffset = data.Count;
                USF4Utils.UpdateIntAtPosition(data, cancelsPointerPosition, data.Count);

                foreach (USF4BCMCancel cancel in Cancels)
                {
                    USF4Utils.AddIntAsBytes(data, cancel.CancelsInto.Count, true);
                    cancelPointerPositions.Add(data.Count);
                    USF4Utils.AddIntAsBytes(data, 0, true);
                }
                USF4Utils.UpdateIntAtPosition(data, cancelsNameIndexPointerPosition, data.Count);
                for (int i = 0; i < Cancels.Count; i++)
                {
                    cancelNamePointerPositions.Add(data.Count);
                    USF4Utils.AddIntAsBytes(data, 0, true);
                }
                //Fetch list of move names so we can calculate indices from strings
                List<string> moveNames = Moves.Select(o => o.Name).ToList();
                for (int i = 0; i < Cancels.Count; i++)
                {
                    USF4Utils.UpdateIntAtPosition(data, cancelPointerPositions[i], data.Count - (cancelStartOffset + i * 0x08));
                    for (int j = 0; j < Cancels[i].CancelsInto.Count; j++)
                    {
                        if (Cancels[i].CancelsInto[j] == "!NONE")
                        {
                            USF4Utils.AddIntAsBytes(data, -1, false);
                        }
                        else USF4Utils.AddIntAsBytes(data, moveNames.IndexOf(Cancels[i].CancelsInto[j]), false);
                    }
                }
            }
            #endregion

            #region Write Name Index

            for (int i = 0; i < Charges.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(data, chargeNamePointerPositions[i], data.Count);
                data.AddRange(Encoding.ASCII.GetBytes(Charges[i].Name));
                data.Add(0x00);
            }
            for (int i = 0; i < InputMotions.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(data, inputMotionNamePointerPositions[i], data.Count);
                data.AddRange(Encoding.ASCII.GetBytes(InputMotions[i].Name));
                data.Add(0x00);
            }
            for (int i = 0; i < Moves.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(data, moveNamePointerPositions[i], data.Count);
                data.AddRange(Encoding.ASCII.GetBytes(Moves[i].Name));
                data.Add(0x00);
            }
            for (int i = 0; i < Cancels.Count; i++)
            {
                USF4Utils.UpdateIntAtPosition(data, cancelNamePointerPositions[i], data.Count);
                data.AddRange(Encoding.ASCII.GetBytes(Cancels[i].Name));
                data.Add(0x00);
            }
            #endregion
            return data.ToArray();
        }
    }
}