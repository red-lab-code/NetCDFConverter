using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCDFConverter
{
    public class Variable
    {
        // https://www.unidata.ucar.edu/software/netcdf/docs/netcdf/Variables.html#Variables
        public string name; // name of the var
        public int[] dimid; // dimid.length = dimensionality of variable. index into dim_list for variable shape
        public LinkedList<Attribute> vatt_list; // variable-specific attributes
        public NC_TYPE type; // type of variable in data
        public uint vsize; // the amount of space in bytes allocated in the data
        public uint length; // the number of data points
        public uint begin; // the byte offset of the variable's data

        public byte[][] data;

        public string units;
        public float missing_value;
        public float valid_min;
        public float valid_max;
        public float scale_factor;
        public float add_offset;

        private Variable(Variable v)
        {
            this.name = String.Copy(v.name);
            this.dimid = (int[])v.dimid.Clone();
            this.vatt_list = new LinkedList<Attribute>();
            foreach (Attribute a in v.vatt_list)
            {
                this.vatt_list.AddLast(a.Clone());
            }
            
            this.type = v.type;
            this.vsize = v.vsize;
            this.length = v.length;
            this.begin = v.begin;

            this.data = (byte[][])v.data.Clone();

            this.units = String.Copy(v.units);
            this.missing_value = v.missing_value;
            this.valid_max = v.valid_max;
            this.scale_factor = v.scale_factor;
            this.add_offset = v.add_offset;
        }

        public Variable(string name, int[] dimid, LinkedList<Attribute> vatt_list, NC_TYPE type, uint vsize, uint begin)
        {
            this.name = name;
            this.dimid = dimid;
            this.vatt_list = vatt_list;
            this.type = type;
            this.vsize = vsize;
            this.begin = begin;

            this.length = this.vsize / NetCDFTools.getTypeLength(this.type);

            foreach (Attribute att in vatt_list)
            {
                if (att.name.Equals("units"))
                {
                    this.units = NetCDFTools.byteToString(att.values);
                }
                else if (att.name.Equals("missing_value"))
                {
                    this.missing_value = NetCDFTools.byteToFloat(att.values[0], att.type);
                }
                else if (att.name.Equals("valid_min"))
                {
                    this.valid_min = NetCDFTools.byteToFloat(att.values[0], att.type);
                }
                else if (att.name.Equals("valid_max"))
                {
                    this.valid_max = NetCDFTools.byteToFloat(att.values[0], att.type);
                }
                else if (att.name.Equals("valid_max"))
                {
                    this.valid_max = NetCDFTools.byteToFloat(att.values[0], att.type);
                }
                else if (att.name.Equals("scale_factor"))
                {
                    this.scale_factor = NetCDFTools.byteToFloat(att.values[0], att.type);
                }
                else if (att.name.Equals("add_offset"))
                {
                    this.add_offset = NetCDFTools.byteToFloat(att.values[0], att.type);
                }
            }
        }

        public override string ToString()
        {
            return "Variable:[\"" + name + "\"|" + type.ToString() + "|" + vsize + "bytes|" + begin + "]";
        }

        public Variable Clone()
        {
            return new Variable(this);
        }
    }
}
