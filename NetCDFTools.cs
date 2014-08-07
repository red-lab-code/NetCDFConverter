using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCDFConverter
{
    public class NetCDFTools
    {
        /// <summary>
        /// True if the data read in is in little endian, false otherwise (default false)
        /// </summary>
        public static bool DataIsLittleEndian = false;

        /// <summary>
        /// True if the computer is little endian, false otherwise (default is autocalculated to be true/false)
        /// </summary>
        public static bool ComputerIsLittleEndian = BitConverter.IsLittleEndian;

        /// <summary>
        /// Reds in a non-negative integer
        /// </summary>
        /// <param name="fs">the stream to read from</param>
        /// <returns>the integer</returns>
        public static uint non_neg(Stream fs)
        {
            byte[] bytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                bytes[i] = (byte)fs.ReadByte();
            }
            return byteToUint(bytes);
        }

        /// <summary>
        /// Reads in a 4 byte integer
        /// </summary>
        /// <param name="fs">the filestream to read from</param>
        /// <returns></returns>
        public static int int4(Stream fs)
        {
            byte[] bytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                bytes[i] = (byte)fs.ReadByte();
            }
            return byteToInt(bytes);
        }

        /// <summary>
        /// Reads in a "name" field
        /// </summary>
        /// <param name="fs">the stream to read from</param>
        /// <returns>the name string</returns>
        public static string name(Stream fs)
        {
            int length = NetCDFTools.int4(fs);

            string str = "";

            for (int i = 0; i < length; i++)
            {
                str += Convert.ToChar(fs.ReadByte());
            }

            padding(str.Length, fs);

            return str;
        }

        /// <summary>
        /// Reads in the padding bytes to the next 4-byte boundary
        /// </summary>
        /// <param name="length">the length of the data that was just read, or the number of padding bytes to read</param>
        /// <param name="fs">the filestream to read from</param>
        public static void padding(uint length, Stream fs)
        {
            var remainder = 4 - length % 4;
            if (remainder == 4)
            {
                return;
            }

            for (int i = 0; i < remainder; i++)
            {
                fs.ReadByte();
            }
        }
        public static void padding(int length, Stream fs)
        {
            padding(Convert.ToUInt32(length), fs);
        }


        /// <summary>
        /// Gets the C# Type for the given type enum
        /// </summary>
        /// <param name="type">the type enum to conver</param>
        /// <returns>the C# type</returns>
        public static Type getType(NC_TYPE type)
        {
            switch (type)
            {
                case NC_TYPE.NC_BYTE:
                    return typeof(byte);

                case NC_TYPE.NC_CHAR:
                    return typeof(char);

                case NC_TYPE.NC_DOUBLE:
                    return typeof(double);

                case NC_TYPE.NC_FLOAT:
                    return typeof(float);

                case NC_TYPE.NC_INT:
                    return typeof(int);

                case NC_TYPE.NC_SHORT:
                    return typeof(short);
            }

            return null;
        }

        /// <summary>
        /// Gets the number of bytes that a given type requires
        /// </summary>
        /// <param name="type">the type to get</param>
        /// <returns>the number of bytes</returns>
        public static uint getTypeLength(NC_TYPE type)
        {
            switch (type)
            {
                case NC_TYPE.NC_BYTE:
                    return 1;

                case NC_TYPE.NC_CHAR:
                    return 1;

                case NC_TYPE.NC_DOUBLE:
                    return 8;

                case NC_TYPE.NC_FLOAT:
                    return 4;

                case NC_TYPE.NC_INT:
                    return 4;

                case NC_TYPE.NC_SHORT:
                    return 2;
            }

            return 0;
        }

        /// <summary>
        /// Reads in an array of values
        /// </summary>
        /// <param name="type">the type of values</param>
        /// <param name="length">the length of values</param>
        /// <param name="fs">the stream to read from</param>
        /// <param name="ignorePadding">optional. true to not ensure the stream sticks to 4-byte boundaries</param>
        /// <returns>a generically typed array</returns>
        public static byte[][] values(NC_TYPE type, uint length, Stream fs, bool ignorePadding)
        {
            uint typeLength = getTypeLength(type);
            // define the array as a jagged array so it is easier to pass rows of it around for conversion
            byte[][] valueArray = new byte[length][];
            for (int i = 0; i < length; i++)
            {
                valueArray[i] = new byte[typeLength];
            }

            // read in the values
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < typeLength; j++)
                {
                    valueArray[i][j] = (byte)fs.ReadByte();
                }
            }

            // read in the padding bytes to the 4-byte boundary
            if (!ignorePadding)
            {
                padding(length * typeLength, fs);
            }

            return valueArray;
        }
        public static byte[][] values(NC_TYPE type, uint length, Stream fs)
        {
            return values(type, length, fs, false);
        }

        // byte conversion functions
        public static char byteToChar(byte[] bytes)
        {
            return BitConverter.ToChar(bytes, 0);
        }
        public static string byteToString(byte[][] bytes)
        {
            byte[] byteClone = new byte[bytes.GetLength(0)];
            for (int i = 0; i < bytes.GetLength(0); i++)
            {
                byteClone[i] = bytes[i][0];
            }

            return byteToString(byteClone);
        }
        public static string byteToString(byte[] bytes)
        {
            string str = "";
            foreach (byte b in bytes)
            {
                str += Convert.ToChar(b);
            }
            return str;
        }
        public static double byteToDouble(byte[] bytes)
        {
            return BitConverter.ToDouble(NetCDFTools.endianConversion(bytes), 0);
        }
        public static float byteToFloat(byte[] bytes)
        {
            if (bytes.Length > 1)
            {
                return BitConverter.ToSingle(NetCDFTools.endianConversion(bytes), 0);
            }
            else
            {
                return Convert.ToSingle(bytes[0]);
            }
        }
        public static int byteToInt(byte[] bytes)
        {
            return BitConverter.ToInt32(NetCDFTools.endianConversion(bytes), 0);
        }
        public static uint byteToUint(byte[] bytes)
        {
            return BitConverter.ToUInt32(NetCDFTools.endianConversion(bytes), 0);
        }
        public static short byteToShort(byte[] bytes)
        {
            return BitConverter.ToInt16(NetCDFTools.endianConversion(bytes), 0);
        }

        // convert any numeric byte type to a float
        public static float byteToFloat(byte[] bytes, NC_TYPE type)
        {
            switch (type)
            {
                case NC_TYPE.NC_SHORT:
                    return Convert.ToSingle(NetCDFTools.byteToShort(bytes));

                case NC_TYPE.NC_INT:
                    return Convert.ToSingle(NetCDFTools.byteToInt(bytes));

                case NC_TYPE.NC_DOUBLE:
                    return Convert.ToSingle(NetCDFTools.byteToDouble(bytes));
            }

            return NetCDFTools.byteToFloat(bytes);
        }
        public static float byteToFloat(byte[] bytes, bool isLittleEndian)
        {
            if (bytes.Length > 1)
            {
                return BitConverter.ToSingle(NetCDFTools.endianConversion(bytes, isLittleEndian != ComputerIsLittleEndian), 0);
            }
            else
            {
                return Convert.ToSingle(bytes[0]);
            }
        }

        /// <summary>
        /// Convers a byte string to a float array
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static float[] byteStringToFloatArray(byte[] bytes)
        {
            return byteStringToFloatArray(bytes, DataIsLittleEndian);
        }
        /// <summary>
        /// Convers a byte string to a float array
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="isLittleEndian">true if the bytes are little endian</param>
        /// <returns></returns>
        public static float[] byteStringToFloatArray(byte[] bytes, bool isLittleEndian)
        {
            float[] ret = new float[bytes.Length / 4];

            int counter = 0;
            for (int i = 0; i < bytes.Length; i += 4)
            {
                byte[] f = { bytes[i], bytes[i + 1], bytes[i + 2], bytes[i + 3] };
                ret[counter++] = NetCDFTools.byteToFloat(f, isLittleEndian);
            }

            return ret;
        }

        /// <summary>
        /// Does an automatic check to see if a conversion is needed
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>The correctly ordered byte array</returns>
        public static byte[] endianConversion(byte[] bytes)
        {
            if (ComputerIsLittleEndian == DataIsLittleEndian) // if the endian-ness is the same, do nothing
            {
                return bytes;
            }
            else // else no matter which uses which endian, the data must be reversed
            {
                return endianConversion(bytes, true);
            }
        }
        /// <summary>
        /// Forces a conversion
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="doConversion"></param>
        /// <returns></returns>
        public static byte[] endianConversion(byte[] bytes, bool doConversion)
        {
            if (doConversion)
            {
                byte[] bytesNew = new byte[bytes.Length];
                Array.Copy(bytes, bytesNew, bytes.Length);
                Array.Reverse(bytesNew);
                return bytesNew;
            }
            else
            {
                return bytes;
            }
        }

        /// <summary>
        /// Floating point equality check, checks if they are near enough to be considered equal
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>true if considered equal, false otherwise</returns>
        public static bool floatNearlyEqual(float a, float b)
        {
            // floating point comparison function, adapted from http://stackoverflow.com/questions/3874627/floating-point-comparison-functions-for-c-sharp

            // short circuit check for infinites and clones
            if (a == b)
            {
                return true;
            }

            float absA = Math.Abs(a);
            float absB = Math.Abs(b);
            float diff = Math.Abs(a - b);

            float eps = Single.Epsilon;
            float min = Single.MinValue; // for some reason it complains about doing an explicit conversion

            if (a == 0 || b == 0 || diff < min)
            {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < eps * min;
            }
            else
            {
                // use relative error
                return diff / (absA + absB) < eps;
            }
        }

        /// <summary>
        /// Floating point equality check, checks if they are near enough to be considered equal.
        /// Decimal convenience overload
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>true if considered equal, false otherwise</returns>
        public static bool floatNearlyEqual(decimal a, decimal b)
        {
            return floatNearlyEqual((float)a, (float)b);
        }
    }
}
