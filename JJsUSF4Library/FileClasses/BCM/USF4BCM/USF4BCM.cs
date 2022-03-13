using JJsUSF4Library.FileClasses.ScriptClasses;
using System.Collections.Generic;
using System.IO;

namespace JJsUSF4Library.FileClasses
{
    public class USF4BCM : BCM
    {
        List<USF4BCMCharge> Charges;
        List<USF4BCMInputMotion> InputMotions;
        List<USF4BCMMove> Moves;

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

            //Input motions are just back-to-back, no pointers to individual blocks
            for (int i = 0; i < inputMotionCount; i++) InputMotions.Add(new USF4BCMInputMotion(br, inputMotionNames[i], offset + inputMotionPointer + i * 0xC4));
            
            br.BaseStream.Seek(offset + movesPointer, SeekOrigin.Begin);
            for (int i = 0; i < movesCount; i++) Moves.Add(new USF4BCMMove(br, moveNames[i], offset + movesPointer + i * 0x54));

            #endregion
        }
    }
}