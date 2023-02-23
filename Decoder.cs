using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModLib;

namespace CBXDecoder
{
    public static class Decoder
    {
        private static int additive = -6;
        private static int additive2 = -5;

        private static bool isSetup = false;

        private static float[] exp = new float[64];

        private static float[] flTable;

        private static byte[] smallTable;

        private static int[] wordTable;

        public static void Setup()
        {
            if (isSetup) return;

            for (int i = 0; i < 64; i++)
            {
                exp[i] = (float)(59.9246 * Math.Pow(1.0680, i + 1));
            }

            string dir = AppDomain.CurrentDomain.BaseDirectory;

            using (ModFile flFile = ModFile.Open(Path.Combine(dir, "Resources", "floats.dat")))
            {
                flTable = new float[flFile.Length / 4];
                for (int i = 0; i < flTable.Length; i++)
                {
                    flTable[i] = flFile.ReadFloat();
                }
            }

            using (ModFile smallFile = ModFile.Open(Path.Combine(dir, "Resources", "table1.dat")))
            {
                smallTable = smallFile.ReadArray((int)smallFile.Length);
            }

            using (ModFile wordFile = ModFile.Open(Path.Combine(dir, "Resources", "table2.dat")))
            {
                wordTable = new int[wordFile.Length / 4];
                for (int i = 0; i < wordTable.Length; i++)
                {
                    wordTable[i] = wordFile.ReadInt();
                }
            }

            isSetup = true;
        }



        private static byte[] buffer;

        private static byte ReadByte()
        {
            if (offset + 1 > buffer.Length) return 0;

            return buffer[offset++];
        }

        private static void Slide()
        {
            if (comparison < 8)
            {
                int readByte = ReadByte();
                readByte <<= comparison;
                activeByte |= readByte;
                comparison += 8;
            }
        }

        private static int offset = 0;
        private static int activeByte = 0;
        private static int comparison = 8;
        
        private static float unknown0c = 0;
        private static float unknown10 = 0;
        private static float unknown14 = 0;
        private static float unknown18 = 0;
        private static float unknown1c = 0;
        private static float unknown20 = 0;
        private static float unknown24 = 0;
        private static float unknown28 = 0;
        private static float unknown2c = 0;
        private static float unknown30 = 0;
        private static float unknown34 = 0;
        private static float unknown38 = 0;

        private static float[] firstLookup = new float[12];

        private static float[] lookup = new float[12];

        private static float unknown4c = 0;
        private static float unknown50 = 0;
        private static float unknown54 = 0;
        private static float unknown58 = 0;
        private static float unknown5c = 0;
        private static float unknown60 = 0;
        private static float unknown64 = 0;
        private static float unknown68 = 0;
        private static float unknown6c = 0;
        private static float unknown70 = 0;
        private static float unknown74 = 0;
        private static float unknown78 = 0;

        private static int outputSize = 0;
        private static int sampleRate = 0;

        public static void Decode(string fileLocation)
        {
            Setup();

            using (ModFile file = ModFile.Open(fileLocation))
            {
                if (!file.CheckString("!B0X", "File does not start with '!B0X' magic bytes!")) return;
                outputSize = file.ReadInt();
                sampleRate = file.ReadInt();

                buffer = file.ReadArray((int)(file.Length - file.Position));
            }

            activeByte = ReadByte();

            int resultOffset = 0;

            ushort[] finalResult = new ushort[outputSize];
            
            while (offset < buffer.Length)
            {
                SeekThrough();


                for (int i = 0; i < 432; i++)
                {
                    uint num1 = (BitConverter.SingleToUInt32Bits(block[i])) & 0x1ffff;
                    if ((num1 - 0x8000) < 0x10000)
                    {
                        num1 = (uint)(0x8000 - (num1 < 0x10000 ? 1 : 0));
                    }
                    finalResult[resultOffset + i] = (ushort)num1;

                    block[i] = 0;
                }

                resultOffset += 432;

                //if (resultOffset > 1200)
                //{
                //    break;
                //}
            }

            string outputLocation = Path.ChangeExtension(fileLocation, "wav");

            using (ModFile output = ModFile.Create(outputLocation))
            {
                int dataLength = 2 * resultOffset;

                output.WriteString("RIFF");
                output.WriteInt(dataLength + 32); // file size, fill this in later
                output.WriteString("WAVE");
                output.WriteString("fmt ");
                output.WriteInt(16);
                output.WriteShort(1); // PCM (2-byte)
                output.WriteShort(1); // 1 channel
                output.WriteInt(sampleRate);
                output.WriteInt(sampleRate * 2);
                output.WriteShort(2); // (16 * 1) / 8
                output.WriteShort(16);
                output.WriteString("data");
                output.WriteInt(dataLength);

                for (int i = 0; i < resultOffset; i++)
                {
                    output.WriteUshort(finalResult[i]);
                }
            }

            Logger.Log(new LogSeg("Success!", ConsoleColor.DarkYellow));

            Logger.Log("Saved .WAV file as: " + outputLocation);
        }

        private static void SeekThrough()
        {
            comparison += additive;

            int something = activeByte & 0x3f;
            activeByte >>= 6;
            Slide();
            bool testFunc4 = something < 0x18; // stored at esp + 44
            lookup[0] = (flTable[something] - data[0]) * 0.25f;

            comparison += additive;

            something = activeByte & 0x3f;
            activeByte >>= 6;
            Slide();
            lookup[1] = (flTable[something] - data[1]) * 0.25f;

            comparison += additive;

            something = activeByte & 0x3f;
            activeByte >>= 6;
            Slide();
            lookup[2] = (flTable[something] - data[2]) * 0.25f;

            comparison += additive;

            something = activeByte & 0x3f;
            activeByte >>= 6;
            Slide();
            lookup[3] = (flTable[something] - data[3]) * 0.25f;

            comparison += additive2;

            something = activeByte & 0x1f;
            activeByte >>= 5;
            Slide();
            lookup[4] = (flTable[something + 16] - data[4]) * 0.25f;

            comparison += additive2;

            something = activeByte & 0x1f;
            activeByte >>= 5;
            Slide();
            lookup[5] = (flTable[something + 16] - data[5]) * 0.25f;

            comparison += additive2;

            something = activeByte & 0x1f;
            activeByte >>= 5;
            Slide();
            lookup[6] = (flTable[something + 16] - data[6]) * 0.25f;

            comparison += additive2;

            something = activeByte & 0x1f;
            activeByte >>= 5;
            Slide();
            lookup[7] = (flTable[something + 16] - data[7]) * 0.25f;

            comparison += additive2;

            something = activeByte & 0x1f;
            activeByte >>= 5;
            Slide();
            lookup[8] = (flTable[something + 16] - data[8]) * 0.25f;

            comparison += additive2;

            something = activeByte & 0x1f;
            activeByte >>= 5;
            Slide();
            lookup[9] = (flTable[something + 16] - data[9]) * 0.25f;

            comparison += additive2;

            something = activeByte & 0x1f;
            activeByte >>= 5;
            Slide();
            lookup[10] = (flTable[something + 16] - data[10]) * 0.25f;

            comparison += additive2;

            something = activeByte & 0x1f;
            activeByte >>= 5;
            Slide();
            lookup[11] = (flTable[something + 16] - data[11]) * 0.25f;


            int blockPosition = 0xd8;
            int resultPosition = 0;

            while (blockPosition < 0x288)
            {
                comparison -= 8;

                something = activeByte & 0xff;
                activeByte >>= 8;
                Slide();

                int aVariable = 0;
                int something2 = blockPosition - something;

                comparison -= 4;

                something = activeByte & 0xf;
                float esp14 = (float)(something * 0.066666667);
                activeByte >>= 4;
                Slide();

                comparison -= 6;

                something = activeByte & 0x3f;
                activeByte >>= 6;
                Slide();
                float esp10 = exp[something];

                comparison -= 1;

                int flag1 = activeByte & 1;
                activeByte >>= 1;
                Slide();

                comparison -= 1;

                int flag2 = activeByte & 1;
                activeByte >>= 1;
                Slide();

                // ebx = flag1 * 4;
                func4(testFunc4 ? 1 : 0, flag1, 2);
                if (flag2 == 0)
                {
                    // sets a bunch of values in esp to 0
                    func3(flag1);
                    esp10 *= 0.5f;
                }
                else
                {
                    for (int i = 0; i < 0x36; i++)
                    {
                        collection[collectionOffset - flag1 + (i * 2)] = 0;
                    }
                }

                int ivar8 = aVariable;
                int ivar10 = something2 + 1;

                float exponentialFloat = esp10;
                float otherFloat = esp14;

                int position = collectionOffset;


                int something3 = something2 + 0x1c;

                for (int i = 0; i < 12; i++)
                {

                    int referencePos = ivar10 - 1 + 0x1c;

                    float result = 0;
                    if (ivar10 - 1 < 0x144)
                    {
                        result = data[referencePos - 4];
                    }
                    else
                    {
                        result = block[referencePos - 352];
                    }
                    esp14 = (collection[position - 1] * exponentialFloat) + (result * otherFloat);
                    esp10 = result;
                    block[resultPosition] = esp14;

                    if (ivar10 < 0x144)
                    {
                        result = data[referencePos - 3];
                    }
                    else
                    {
                        result = block[referencePos - 351];
                    }
                    esp14 = (collection[position] * exponentialFloat) + (result * otherFloat);
                    esp10 = result;
                    block[resultPosition + 1] = esp14;

                    if (ivar10 + 1 < 0x144)
                    {
                        result = data[referencePos - 2];
                    }
                    else
                    {
                        result = block[referencePos - 350];
                    }
                    esp14 = (collection[position + 1] * exponentialFloat) + (result * otherFloat);
                    esp10 = result;
                    block[resultPosition + 2] = esp14;

                    if (ivar10 + 2 < 0x144)
                    {
                        result = data[referencePos - 1];
                    }
                    else
                    {
                        result = block[referencePos - 349];
                    }
                    esp14 = (collection[position + 2] * exponentialFloat) + (result * otherFloat);
                    esp10 = result;
                    block[resultPosition + 3] = esp14;

                    if (ivar10 + 3 < 0x144)
                    {
                        result = data[referencePos - 0];
                    }
                    else
                    {
                        result = block[referencePos - 348];
                    }
                    esp14 = (collection[position + 3] * exponentialFloat) + (result * otherFloat);
                    esp10 = result;
                    block[resultPosition + 4] = esp14;

                    if (ivar10 + 4 < 0x144)
                    {
                        result = data[referencePos + 1];
                    }
                    else
                    {
                        result = block[referencePos - 347];
                    }
                    esp14 = (collection[position + 4] * exponentialFloat) + (result * otherFloat);
                    esp10 = result;
                    block[resultPosition + 5] = esp14;

                    if (ivar10 + 5 < 0x144)
                    {
                        result = data[referencePos + 2];
                    }
                    else
                    {
                        result = block[referencePos - 346];
                    }
                    esp14 = (collection[position + 5] * exponentialFloat) + (result * otherFloat);
                    esp10 = result;
                    block[resultPosition + 6] = esp14;

                    if (ivar10 + 6 < 0x144)
                    {
                        result = data[referencePos + 3];
                    }
                    else
                    {
                        result = block[referencePos - 345];
                    }
                    esp14 = (collection[position + 6] * exponentialFloat) + (result * otherFloat);
                    esp10 = result;
                    block[resultPosition + 7] = esp14;

                    if (ivar10 + 7 < 0x144)
                    {
                        result = data[referencePos + 4];
                    }
                    else
                    {
                        result = block[referencePos - 344];
                    }
                    esp14 = (collection[position + 7] * exponentialFloat) + (result * otherFloat);
                    esp10 = result;
                    block[resultPosition + 8] = esp14;

                    position += 9;
                    ivar10 += 9;
                    resultPosition += 9;
                }

                blockPosition += 0x6c;
            }

            int startPoint = 0x6c; // not sure why but it just is

            float[] header = new float[24];

            float[] copy = new float[1000];

            for (int i = 0; i < 0x24 * 9; i++)
            {
                data[24 + i] = block[startPoint + i];
            }

            func1offset = 0; // not sure when i was supposed to have set this to zero but it's every major round so here will do

            int countX = 0;
            while (countX != 12)
            {
                data[countX + 0] += lookup[countX + 0];
                data[countX + 1] += lookup[countX + 1];
                data[countX + 2] += lookup[countX + 2];
                data[countX + 3] += lookup[countX + 3];
                data[countX + 4] += lookup[countX + 4];
                data[countX + 5] += lookup[countX + 5];
                countX += 6;
            }

            func1(1);

            countX = 0;
            while (countX != 12)
            {
                data[countX + 0] += lookup[countX + 0];
                data[countX + 1] += lookup[countX + 1];
                data[countX + 2] += lookup[countX + 2];
                data[countX + 3] += lookup[countX + 3];
                data[countX + 4] += lookup[countX + 4];
                data[countX + 5] += lookup[countX + 5];
                countX += 6;
            }

            func1(1);

            countX = 0;
            while (countX != 12)
            {
                data[countX + 0] += lookup[countX + 0];
                data[countX + 1] += lookup[countX + 1];
                data[countX + 2] += lookup[countX + 2];
                data[countX + 3] += lookup[countX + 3];
                data[countX + 4] += lookup[countX + 4];
                data[countX + 5] += lookup[countX + 5];
                countX += 6;
            }

            func1(1);

            countX = 0;
            while (countX != 12)
            {
                data[countX + 0] += lookup[countX + 0];
                data[countX + 1] += lookup[countX + 1];
                data[countX + 2] += lookup[countX + 2];
                data[countX + 3] += lookup[countX + 3];
                data[countX + 4] += lookup[countX + 4];
                data[countX + 5] += lookup[countX + 5];
                countX += 6;
            }

            func1(0x21);
        }

        private static float[] block = new float[1000];
        
        
        private static float[] collection = new float[8 + 0x6b + 5]; // final 4 bytes are for padding otherwise we go out of range in func3

        private static float[] data = new float[24 + (0x24 * 9)];

        private static float[] espResult = new float[12];

        private static float[] otherResult = new float[12];

        private static int func1offset = 0;

        private static void func1(int something)
        {
            func2(1);

            if (something > 0)
            {
                do
                {
                    data[23] = ((data[12] * otherResult[0]) + block[func1offset + 0]) +
                        (data[13] * otherResult[1]) +
                        (data[14] * otherResult[2]) +
                        (data[15] * otherResult[3]) +
                        (data[16] * otherResult[4]) +
                        (data[17] * otherResult[5]) +
                        (data[18] * otherResult[6]) +
                        (data[19] * otherResult[7]) +
                        (data[20] * otherResult[8]) +
                        (data[21] * otherResult[9]) +
                        (data[22] * otherResult[10]) +
                        (data[23] * otherResult[11]);
                    block[func1offset + 0] = data[23] + 12582912f;

                    data[22] = ((data[23] * otherResult[0]) + block[func1offset + 1]) +
                        (data[12] * otherResult[1]) +
                        (data[13] * otherResult[2]) +
                        (data[14] * otherResult[3]) +
                        (data[15] * otherResult[4]) +
                        (data[16] * otherResult[5]) +
                        (data[17] * otherResult[6]) +
                        (data[18] * otherResult[7]) +
                        (data[19] * otherResult[8]) +
                        (data[20] * otherResult[9]) +
                        (data[21] * otherResult[10]) +
                        (data[22] * otherResult[11]);
                    block[func1offset + 1] = data[22] + 12582912f;

                    data[21] = ((data[22] * otherResult[0]) + block[func1offset + 2]) +
                        (data[23] * otherResult[1]) +
                        (data[12] * otherResult[2]) +
                        (data[13] * otherResult[3]) +
                        (data[14] * otherResult[4]) +
                        (data[15] * otherResult[5]) +
                        (data[16] * otherResult[6]) +
                        (data[17] * otherResult[7]) +
                        (data[18] * otherResult[8]) +
                        (data[19] * otherResult[9]) +
                        (data[20] * otherResult[10]) +
                        (data[21] * otherResult[11]);
                    block[func1offset + 2] = data[21] + 12582912f;

                    data[20] = ((data[21] * otherResult[0]) + block[func1offset + 3]) +
                        (data[22] * otherResult[1]) +
                        (data[23] * otherResult[2]) +
                        (data[12] * otherResult[3]) +
                        (data[13] * otherResult[4]) +
                        (data[14] * otherResult[5]) +
                        (data[15] * otherResult[6]) +
                        (data[16] * otherResult[7]) +
                        (data[17] * otherResult[8]) +
                        (data[18] * otherResult[9]) +
                        (data[19] * otherResult[10]) +
                        (data[20] * otherResult[11]);
                    block[func1offset + 3] = data[20] + 12582912f;

                    data[19] = ((data[20] * otherResult[0]) + block[func1offset + 4]) +
                        (data[21] * otherResult[1]) +
                        (data[22] * otherResult[2]) +
                        (data[23] * otherResult[3]) +
                        (data[12] * otherResult[4]) +
                        (data[13] * otherResult[5]) +
                        (data[14] * otherResult[6]) +
                        (data[15] * otherResult[7]) +
                        (data[16] * otherResult[8]) +
                        (data[17] * otherResult[9]) +
                        (data[18] * otherResult[10]) +
                        (data[19] * otherResult[11]);
                    block[func1offset + 4] = data[19] + 12582912f;

                    data[18] = ((data[19] * otherResult[0]) + block[func1offset + 5]) +
                        (data[20] * otherResult[1]) +
                        (data[21] * otherResult[2]) +
                        (data[22] * otherResult[3]) +
                        (data[23] * otherResult[4]) +
                        (data[12] * otherResult[5]) +
                        (data[13] * otherResult[6]) +
                        (data[14] * otherResult[7]) +
                        (data[15] * otherResult[8]) +
                        (data[16] * otherResult[9]) +
                        (data[17] * otherResult[10]) +
                        (data[18] * otherResult[11]);
                    block[func1offset + 5] = data[18] + 12582912f;

                    data[17] = ((data[18] * otherResult[0]) + block[func1offset + 6]) +
                        (data[19] * otherResult[1]) +
                        (data[20] * otherResult[2]) +
                        (data[21] * otherResult[3]) +
                        (data[22] * otherResult[4]) +
                        (data[23] * otherResult[5]) +
                        (data[12] * otherResult[6]) +
                        (data[13] * otherResult[7]) +
                        (data[14] * otherResult[8]) +
                        (data[15] * otherResult[9]) +
                        (data[16] * otherResult[10]) +
                        (data[17] * otherResult[11]);
                    block[func1offset + 6] = data[17] + 12582912f;

                    data[16] = ((data[17] * otherResult[0]) + block[func1offset + 7]) +
                        (data[18] * otherResult[1]) +
                        (data[19] * otherResult[2]) +
                        (data[20] * otherResult[3]) +
                        (data[21] * otherResult[4]) +
                        (data[22] * otherResult[5]) +
                        (data[23] * otherResult[6]) +
                        (data[12] * otherResult[7]) +
                        (data[13] * otherResult[8]) +
                        (data[14] * otherResult[9]) +
                        (data[15] * otherResult[10]) +
                        (data[16] * otherResult[11]);
                    block[func1offset + 7] = data[16] + 12582912f;

                    data[15] = ((data[16] * otherResult[0]) + block[func1offset + 8]) +
                        (data[17] * otherResult[1]) +
                        (data[18] * otherResult[2]) +
                        (data[19] * otherResult[3]) +
                        (data[20] * otherResult[4]) +
                        (data[21] * otherResult[5]) +
                        (data[22] * otherResult[6]) +
                        (data[23] * otherResult[7]) +
                        (data[12] * otherResult[8]) +
                        (data[13] * otherResult[9]) +
                        (data[14] * otherResult[10]) +
                        (data[15] * otherResult[11]);
                    block[func1offset + 8] = data[15] + 12582912f;

                    data[14] = ((data[15] * otherResult[0]) + block[func1offset + 9]) +
                        (data[16] * otherResult[1]) +
                        (data[17] * otherResult[2]) +
                        (data[18] * otherResult[3]) +
                        (data[19] * otherResult[4]) +
                        (data[20] * otherResult[5]) +
                        (data[21] * otherResult[6]) +
                        (data[22] * otherResult[7]) +
                        (data[23] * otherResult[8]) +
                        (data[12] * otherResult[9]) +
                        (data[13] * otherResult[10]) +
                        (data[14] * otherResult[11]);
                    block[func1offset + 9] = data[14] + 12582912f;

                    data[13] = ((data[14] * otherResult[0]) + block[func1offset + 10]) +
                        (data[15] * otherResult[1]) +
                        (data[16] * otherResult[2]) +
                        (data[17] * otherResult[3]) +
                        (data[18] * otherResult[4]) +
                        (data[19] * otherResult[5]) +
                        (data[20] * otherResult[6]) +
                        (data[21] * otherResult[7]) +
                        (data[22] * otherResult[8]) +
                        (data[23] * otherResult[9]) +
                        (data[12] * otherResult[10]) +
                        (data[13] * otherResult[11]);
                    block[func1offset + 10] = data[13] + 12582912f;

                    data[12] = ((data[13] * otherResult[0]) + block[func1offset + 11]) +
                        (data[14] * otherResult[1]) +
                        (data[15] * otherResult[2]) +
                        (data[16] * otherResult[3]) +
                        (data[17] * otherResult[4]) +
                        (data[18] * otherResult[5]) +
                        (data[19] * otherResult[6]) +
                        (data[20] * otherResult[7]) +
                        (data[21] * otherResult[8]) +
                        (data[22] * otherResult[9]) +
                        (data[23] * otherResult[10]) +
                        (data[12] * otherResult[11]);
                    block[func1offset + 11] = data[12] + 12582912f;

                    func1offset += 12;

                    something--;
                } while (something != 0);
            }
        }

        private static void func2(int something2)
        {
            for (int i = 11; i > 0; i--)
            {
                firstLookup[i] = data[i - 1];
            }

            firstLookup[0] = 1;

            int counter = 0;

            while (counter < 0xc)
            {
                float esp10 = (-data[11] * firstLookup[11]);

                esp10 = esp10 - (data[10] * firstLookup[10]);
                firstLookup[11] = firstLookup[10] + (data[10] * esp10);

                esp10 = esp10 - (data[9] * firstLookup[9]);
                firstLookup[10] = firstLookup[9] + (data[9] * esp10);

                esp10 = esp10 - (data[8] * firstLookup[8]);
                firstLookup[9] = firstLookup[8] + (data[8] * esp10);

                esp10 = esp10 - (data[7] * firstLookup[7]);
                firstLookup[8] = firstLookup[7] + (data[7] * esp10);

                esp10 = esp10 - (data[6] * firstLookup[6]);
                firstLookup[7] = firstLookup[6] + (data[6] * esp10);

                esp10 = esp10 - (data[5] * firstLookup[5]);
                firstLookup[6] = firstLookup[5] + (data[5] * esp10);

                esp10 = esp10 - (data[4] * firstLookup[4]);
                firstLookup[5] = firstLookup[4] + (data[4] * esp10);

                esp10 = esp10 - (data[3] * firstLookup[3]);
                firstLookup[4] = firstLookup[3] + (data[3] * esp10);

                esp10 = esp10 - (data[2] * firstLookup[2]);
                firstLookup[3] = firstLookup[2] + (data[2] * esp10);

                esp10 = esp10 - (data[1] * firstLookup[1]);
                firstLookup[2] = firstLookup[1] + (data[1] * esp10);

                esp10 = esp10 - (data[0] * firstLookup[0]);
                firstLookup[1] = firstLookup[0] + (data[0] * esp10);

                firstLookup[0] = esp10;

                espResult[counter] = esp10;

                int aVariable = 0;
                if (counter > 3)
                {
                    int rounds = ((counter - 4) >> 2) + 1;
                    aVariable = rounds * 4;

                    int roll = 0;

                    do
                    {
                        rounds--;

                        esp10 = esp10 - (espResult[(counter - 1) - (roll * 4)] * otherResult[(roll * 4) + 0]);
                        esp10 = esp10 - (espResult[(counter - 2) - (roll * 4)] * otherResult[(roll * 4) + 1]);
                        esp10 = esp10 - (espResult[(counter - 3) - (roll * 4)] * otherResult[(roll * 4) + 2]);
                        esp10 = esp10 - (espResult[(counter - 4) - (roll * 4)] * otherResult[(roll * 4) + 3]);
                        roll++;
                    } while (rounds != 0);
                }
                if (counter > aVariable)
                {
                    do
                    {
                        int offset = (counter - aVariable) - 1;
                        esp10 = esp10 - (espResult[offset] * otherResult[aVariable]);
                        aVariable++;
                    } while (counter > aVariable);
                }

                otherResult[counter] = esp10;

                counter++;
            }
            
        }

        private static void func3(int offseter)
        {
            for (int i = collectionOffset + 1 - offseter; i < (0x6b + collectionOffset + 1 - offseter); i += 2)
            {
                float previousValue = (float)((collection[i - 2] + collection[i]) * 0.597385942935944);

                float previousValue2 = (float)((collection[i - 4] + collection[i + 2]) * 0.114591561257839);

                previousValue -= previousValue2;

                float previousValue3 = (float)((collection[i - 6] + collection[i + 4]) * 0.0180326793342829);

                collection[i - 1] = previousValue + previousValue3;
            }
        }

        static int collectionOffset = 8;

        private static void func4(int testFunc4, int someOffset, int someValue)
        {

            if (testFunc4 == 0)
            {
                int counter = 0;
                do
                {
                    switch (activeByte & 3)
                    {
                        case 0:
                        case 2:
                            collection[collectionOffset - 1 + someOffset + counter] = 0;
                            comparison -= 1;
                            activeByte >>= 1;

                            Slide();
                            break;
                        case 1:
                            collection[collectionOffset - 1 + someOffset + counter] = -2;
                            comparison -= 2;
                            activeByte >>= 2;

                            Slide();
                            break;
                        case 3:
                            collection[collectionOffset - 1 + someOffset + counter] = 2;
                            comparison -= 2;
                            activeByte >>= 2;

                            Slide();
                            break;
                    }
                    counter += someValue;
                } while (counter < 0x6c);

                return;
            }

            int newValue = 0;

            int esp0c;

            testFunc4 = 0; // originally testFunc4 = 0;
            // something else = 0;

            while (true)
            {
                int something = activeByte & 0xff;
                newValue <<= 8;
                byte smallValue = smallTable[something + newValue];
                int result = smallValue * 3;

                newValue = wordTable[result];
                int temp = wordTable[result + 1];
                
                activeByte >>= temp;
                comparison -= temp;
                Slide();

                if (smallValue < 4)
                {
                    if (smallValue < 2)
                    {
                        int number = 7;
                        while (true)
                        {
                            int val = activeByte & 1;
                            comparison -= 1;
                            activeByte >>= 1;

                            Slide();

                            if (val != 1) break;
                            number++;
                        }

                        int anotherTest = activeByte & 1;
                        comparison -= 1;
                        activeByte >>= 1;

                        Slide();

                        float value;
                        if (anotherTest == 1)
                        {
                            value = (float)number;
                        }
                        else
                        {
                            value = (float)-number;
                        }

                        collection[collectionOffset - 1 + someOffset + testFunc4] = value;
                        testFunc4 += someValue;
                    }
                    else
                    {
                        int loops = (activeByte & 0x3f) + 7;
                        comparison -= 6;
                        activeByte >>= 6;

                        Slide();

                        if ((loops * 2) + testFunc4 > 0x6c)
                        {
                            loops = (0x6c - testFunc4) / someValue;
                        }

                        if (loops > 0)
                        {
                            do
                            {
                                collection[collectionOffset - 1 + someOffset + testFunc4] = 0;
                                testFunc4 += someValue;
                                loops--;
                            } while (loops != 0);
                        }
                    }
                }
                else
                {
                    float value = BitConverter.Int32BitsToSingle(wordTable[result + 2]);
                    collection[collectionOffset - 1 + someOffset + testFunc4] = value;
                    testFunc4 += someValue;
                }
                

                if (testFunc4 > 0x6b)
                {
                    return;
                }
            }
        }
    }
}
