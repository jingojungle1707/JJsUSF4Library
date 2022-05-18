using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJsUSF4Library.FileClasses.EMEClasses
{
    public class BasicEME : USF4File
    {
        public SortedDictionary<int, byte[]> OffsetByteArrayPairs { get; set; } = new SortedDictionary<int, byte[]>();
        public byte[] HeaderData { get; set; } = new byte[0x60];

        public BasicEME()
        {

        }
        public BasicEME(BinaryReader br, string name, int offset = 0)
        {
            Name = name;
            ReadFromStream(br, offset);
        }

        public override void ReadFromStream(BinaryReader br, int offset = 0, int fileLength = 0)
        {
            //So the plan here is to chunk the file into individual item byte arrays so we can scan for texture indices and modify them
            //Then GenerateBytes is just going to take the chunked byte arrays and write them back out in order

            //Clear the dictionary ready to populate
            OffsetByteArrayPairs.Clear();

            //Store the header data
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            HeaderData = br.ReadBytes(0x60);

            //br.BaseStream.Seek(offset + 0x08, SeekOrigin.Begin);
            //int itemCount = br.ReadInt32();

            //Grab the first pointer
            List<int> pointers = new List<int>() { USF4Utils.ReadInt(true, 0x0C, HeaderData) };

            while (pointers.Count > 0)
            {
                //Check if we've already read item, delete and skip
                if (pointers[0] == 0 || OffsetByteArrayPairs.TryGetValue(pointers[0], out _))
                {
                    pointers.RemoveAt(0);
                    continue;
                }

                int currentPointer = pointers[0];
                pointers.RemoveAt(0);

                //Skip to the pointers
                br.BaseStream.Seek(offset + currentPointer + 0x14, SeekOrigin.Begin);
                //Read the first 2 pointers, one will be to next main item, one will be next sub-item
                int pointer1 = br.ReadInt32();
                int pointer2 = br.ReadInt32();

                if (pointer1 > 0) pointers.Add(pointer1);
                if (pointer2 > 0) pointers.Add(pointer2);

                pointers.Sort();

                //Next item is the "nearer" pointer
                int currentItemLength = pointers.Count > 0 ? pointers[0] - currentPointer : fileLength > 0 ? fileLength - currentPointer : throw new ArgumentException();

                //Move back to the start of the current item and read the calculated number of bytes, add to dictionary
                br.BaseStream.Seek(offset + currentPointer, SeekOrigin.Begin);
                OffsetByteArrayPairs.Add(currentPointer, br.ReadBytes(currentItemLength));
            }
        }

        public override byte[] GenerateBytes()
        {
            List<byte> data = new List<byte>(HeaderData);

            foreach (byte[] item in OffsetByteArrayPairs.Values)
            {
                data.AddRange(item);
            }

            return data.ToArray();
        }

        public int GetItemTextureIndex(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex > OffsetByteArrayPairs.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            int key = OffsetByteArrayPairs.Keys.ToList()[itemIndex];
            byte[] item = OffsetByteArrayPairs[key];

            if (item[0x03] == 2)
            {
                //Sprite (1)/core(0) is < 2, Line is == 2, Plane is == 3
                if (item[0x02] < 2) return item[0x88];
                else if (item[0x02] == 2) return item[0x84];
                else if (item[0x02] == 3) return item[0x8C]; //AGL SHmr_GShock.ep.eme
                else throw new ArgumentException("Unrecognised value at 0x02!");
            }
            else
            {
                return -1;
            }
        }

        public void SetItemTextureIndex(int itemIndex, sbyte newIndex)
        {
            int key = OffsetByteArrayPairs.Keys.ToList()[itemIndex];
            byte[] item = OffsetByteArrayPairs[key];

            if (item[0x03] == 2)
            {
                int textureIndexPosition;
                //Sprite (1)/core(0) is < 2, Line is == 2, Plane is == 3
                if (item[0x02] < 2) textureIndexPosition = 0x88;
                else if (item[0x02] == 2) textureIndexPosition = 0x84;
                else if (item[0x02] == 3) textureIndexPosition = 0x8C; //AGL SHmr_GShock.ep.eme
                else throw new ArgumentException("Unrecognised value at 0x02!");

                OffsetByteArrayPairs[key][textureIndexPosition] = (byte)newIndex;
            }
        }

        public void UpdateTextureIndices(int valueToAdd)
        {
            foreach (byte[] item in OffsetByteArrayPairs.Values)
            {
                //Check for particle blocks
                if (item[0x03] == 2)
                {
                    int textureIndexPosition;
                    //Sprite (1)/core(0) is < 2, Line is == 2, Plane is == 3
                    if (item[0x02] < 2) textureIndexPosition = 0x88;
                    else if (item[0x02] == 2) textureIndexPosition = 0x84;
                    else if (item[0x02] == 3) textureIndexPosition = 0x8C; //AGL SHmr_GShock.ep.eme
                    else throw new ArgumentException("Unrecognised value at 0x02!");

                    int newValue = USF4Utils.ReadInt(false, textureIndexPosition, item) + valueToAdd;
                    byte[] newBytes = BitConverter.GetBytes((ushort)newValue);
                    item[textureIndexPosition] = newBytes[0];
                    item[textureIndexPosition + 1] = newBytes[1];
                }
            }
        }
    }
}
