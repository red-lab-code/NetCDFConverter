using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCDFConverter
{
    public class Attribute
    {
        // https://www.unidata.ucar.edu/software/netcdf/docs/netcdf/Attributes.html#Attributes
        public string name; // the name of the attribute
        public NC_TYPE type; // the type of the attribute values
        public byte[][] values; // the values of the attribute

        private Attribute(Attribute a)
        {
            this.name = String.Copy(a.name);
            this.type = a.type;
            this.values = (byte[][])a.values.Clone();
        }

        public Attribute(string name, NC_TYPE type, byte[][] values)
        {
            this.name = name;
            this.type = type;
            this.values = values;
        }

        public override string ToString()
        {
            string ret = "Attribute:[\"" + name + "\"|" + type.ToString() + "|";

            // if it's
            if (type == NC_TYPE.NC_CHAR)
            {
                ret += "\"" + NetCDFTools.byteToString(values) + "\"";
            }
            else
            {
                ret += values.Length;
            }

            ret += "]";
            return ret;
        }

        public Attribute Clone()
        {
            return new Attribute(this);
        }
    }
}
