using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetCDFConverter
{
    public class GeoSpatialData
    {
        public static bool ConsoleOutput = false;
        public static string ConsoleTabPrefix = "";

        private static string DateTimeConversionString = "yyyyMMdd HH:mm:ss";

        private static int binaryHeaderSize = 5 * sizeof(float);

        public string units;
        public string data_name;
        public string data_type;
        public float data_missing_value;
        public float data_max;
        public float data_min;

        public float scale_max;
        public float scale_min;
        public float scale_step;

        public float[, ,] data;
        public decimal[] lat; // use decimal for precision so that the lat/lng is the same as the value out of the DB
        public decimal[] lng;
        public int[] time;
        public DateTime[] dateTime;

        public GeoSpatialData()
        {

        }

        /// <summary>
        /// Convenience constructor for reading from an NC file
        /// </summary>
        /// <param name="file">the filename to read from</param>>
        public GeoSpatialData(string file)
            : this(new FileStream(file, FileMode.Open, FileAccess.Read)) { }

        /// <summary>
        /// Convenience constructor for reading from any stream (file, memory, etc)
        /// </summary>
        /// <param name="fs">the stream to read from</param>>
        public GeoSpatialData(Stream fs)
            : this(new Header(fs)) { }

        /// <summary>
        /// Constructor for interpreting from an NC file
        /// </summary>
        /// <param name="head">the raw NC data</param>
        public GeoSpatialData(Header head)
        {
            Variable latVar = null;
            Variable lngVar = null;
            Variable timeVar = null;
            Variable dataVar = null;

            Dimension[] dataDimensions = null;

            int latDimID = 0;
            Dimension latDim = null;
            int lngDimID = 0;
            Dimension lngDim = null;
            int timeDimID = 0;
            Dimension timeDim = null;

            dataDimensions = new Dimension[3];

            foreach (Variable v in head.var_list)
            {
                if (v.name.Equals("latitude") || v.name.Equals("lat"))
                {
                    latVar = v;
                    latDimID = v.dimid[0];
                    latDim = head.dim_list.ElementAt(latDimID);
                    dataDimensions[latDimID] = latDim;
                }
                else if (v.name.Equals("longitude") || v.name.Equals("lng") || v.name.Equals("long"))
                {
                    lngVar = v;
                    lngDimID = v.dimid[0];
                    lngDim = head.dim_list.ElementAt(lngDimID);
                    dataDimensions[lngDimID] = lngDim;
                }
                else if (v.name.Equals("time"))
                {
                    timeVar = v;
                    timeDimID = v.dimid[0];
                    timeDim = head.dim_list.ElementAt(timeDimID);
                    dataDimensions[timeDimID] = timeDim;
                }
                else
                {
                    dataVar = v;
                }
            }

            // pull out the relevant attributes from the dataVar
            foreach (Attribute a in dataVar.vatt_list) {
                if (a.name == "long_name")
                {
                    this.data_name = NetCDFTools.byteToString(a.values);
                }
                else if (a.name == "units")
                {
                    this.units = NetCDFTools.byteToString(a.values);
                }
            }
            this.data_type = dataVar.name;
            this.data_missing_value = dataVar.missing_value;
            this.data_max = dataVar.valid_max;
            this.data_min = dataVar.valid_min;

            // convert each set of data into its real format
            this.data = new float[timeVar.length, latVar.length, lngVar.length];
            for (int i = 0; i < dataDimensions[0].length; i++)
            {
                for (int j = 0; j < dataDimensions[1].length; j++)
                {
                    for (int k = 0; k < dataDimensions[2].length; k++)
                    {
                        int index = i * dataDimensions[1].length * dataDimensions[2].length +
                                    j * dataDimensions[2].length +
                                    k;

                        data[i,j,k] = NetCDFTools.byteToFloat(dataVar.data[index], dataVar.type);
                    }
                }
            }

            this.lat = new decimal[latVar.length];
            for (int i = 0; i < this.lat.Length; i++)
            {
                this.lat[i] = (decimal)NetCDFTools.byteToFloat(latVar.data[i], latVar.type);
            }

            this.lng = new decimal[lngVar.length];
            for (int i = 0; i < this.lng.Length; i++)
            {
                this.lng[i] = (decimal)NetCDFTools.byteToFloat(lngVar.data[i], lngVar.type);
            }

            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            this.time = new int[timeVar.length];
            this.dateTime = new DateTime[timeVar.length];
            for (int i = 0; i < this.time.Length; i++)
            {
                this.time[i] = NetCDFTools.byteToInt(timeVar.data[i]);
                this.dateTime[i] = epoch.AddSeconds( this.time[i] );
            }
        }

        // some convenience and cleanliness functions because we have to convert from object to decimal before we can convert to our other type
        private static int fieldToInt(object o)
        {
            if (o is decimal)
            {
                return (int)(decimal)o;
            }
            else if (o is short)
            {
                return (int)(short)o;
            }
            else
            {
                return (int)o;
            }
        }
        private static float fieldToFloat(object o)
        {
            if (o is decimal)
            {
                return (float)(decimal)o;
            }
            else if (o is double)
            {
                return (float)(double)o;
            }
            else
            {
                return (float)o;
            }
        }

        // construtor cloning
        private GeoSpatialData(GeoSpatialData g)
        {
            this.units = (string)g.units.Clone();
            this.data_name = (string)g.data_name.Clone();
            this.data_type = (string)g.data_type.Clone();
            this.data_max = g.data_max;
            this.data_min = g.data_min;
            this.data_missing_value = g.data_missing_value;

            this.data = (float[, ,])g.data.Clone();
            this.lat = (decimal[])g.lat.Clone();
            this.lng = (decimal[])g.lng.Clone();
            this.time = (int[])g.time.Clone();
            this.dateTime = (DateTime[])g.dateTime.Clone();
        }

        /// <summary>
        /// Returns a new instance with clones of all data members
        /// </summary>
        /// <returns></returns>
        public GeoSpatialData Clone()
        {
            return new GeoSpatialData(this);
        }

        /// <summary>
        /// Convenience method to Write the data to a file in binary representation
        /// </summary>
        /// <param name="filename">the file to write to</param>
        /// <param name="timeIndex">the time slice to write</param>
        public void WriteToFile(string filename, int timeIndex)
        {
            // print out the data

            // print it out in javascript float32array arraybuffer format
            // this is just the bytes in a continuous stream
            this.WriteToBinaryStream(new FileStream(filename, FileMode.Open, FileAccess.Read), timeIndex);
        }

        /// <summary>
        /// Converts this object to its binary representation
        /// </summary>
        /// <param name="timeIndex">the time slice to write</param>
        /// <returns></returns>
        public byte[] ToBinary(int timeIndex)
        {
            MemoryStream stream = new MemoryStream(this.lat.Length * this.lng.Length * 3 * sizeof(float) + binaryHeaderSize);
            
            this.WriteToBinaryStream(stream, timeIndex);

            return stream.GetBuffer();
        }

        /// <summary>
        /// Writes the data to a stream in binary representation
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="timeIndex"></param>
        public void WriteToBinaryStream(Stream stream, int timeIndex)
        {
            if (timeIndex > this.time.Length)
            {
                throw new InvalidTimeIndexException(this.data_type + ":" + timeIndex);
            }
            // print out the data

            // print it out in javascript float32array arraybuffer format
            // this is just the bytes in a continuous stream
            BinaryWriter writer = new BinaryWriter(stream);

            int[] len = new int[3] { data.GetLength(0), data.GetLength(1), data.GetLength(2) };

            // write out some 'header' information for this record
            writer.Write(this.data_missing_value); // the missing-val
            writer.Write(this.data_max); // the real maximum
            writer.Write(this.data_min); // the real minimum
            writer.Write(len[1]); // the number of lats
            writer.Write(len[2]); // the number of lngs

            // print out the points and values
            for (int j = 0; j < len[1]; j++)
            {
                for (int k = 0; k < len[2]; k++)
                {
                    writer.Write(lat[j]);
                    writer.Write(lng[k]);
                    writer.Write(data[timeIndex, j, k]);
                }
            }

            writer.Close();
        }

        /// <summary>
        /// Checks if the given float is valid according to the defined bounds of the GSD
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public bool IsValidValue(float f)
        {
            return !Double.Equals(f, this.data_missing_value) &&
                   f >= this.data_min &&
                   f <= this.data_max;
        }

        // writes to log if debugging is turned on
        private static void Log(string str)
        {
            if (ConsoleOutput)
            {
                Console.WriteLine(ConsoleTabPrefix + str);
            }
        }
    }
}
