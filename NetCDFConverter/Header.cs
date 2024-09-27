using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCDFConverter
{
    public class Header
    {
        public string magic;
        public int version;

        public LinkedList<Dimension> dim_list = new LinkedList<Dimension>();
        public LinkedList<Attribute> gatt_list = new LinkedList<Attribute>();
        public LinkedList<Variable> var_list = new LinkedList<Variable>();

        public Header(Stream fs)
        {
            this.ReadHeader(fs);

            this.ReadData(fs);
        }

        private static LinkedList<Attribute> att_list(Stream fs)
        {
            LinkedList<Attribute> list = new LinkedList<Attribute>();

            int attribute_tag = NetCDFTools.int4(fs); // should be 0xC
            uint num_attrs = NetCDFTools.non_neg(fs);
            if (attribute_tag == 0xC && num_attrs > 0)
            {
                for (int i = 0; i < num_attrs; i++)
                {
                    string name = NetCDFTools.name(fs);

                    NC_TYPE type = (NC_TYPE)NetCDFTools.non_neg(fs);

                    uint nelems = NetCDFTools.non_neg(fs);

                    byte[][] valueArray = NetCDFTools.values(type, nelems, fs);

                    list.AddLast(new Attribute(name, type, valueArray));
                }
            }

            return list;
        }

        private void ReadHeader(Stream fs)
        {
            // magic
            magic = ""; // should be "CDF"
            magic += Convert.ToChar(fs.ReadByte());
            magic += Convert.ToChar(fs.ReadByte());
            magic += Convert.ToChar(fs.ReadByte());

            version = fs.ReadByte(); // should be 1

            // numrecs
            uint num_recs = NetCDFTools.non_neg(fs);
            if (num_recs == 0xFFFFFFFF)
            {
                // streaming
            }

            // dim_list
            int dimension_tag = NetCDFTools.int4(fs); // should be 0xA
            uint num_dims = NetCDFTools.non_neg(fs);
            if (dimension_tag == 0xA && num_dims > 0)
            {
                for (int i = 0; i < num_dims; i++)
                {
                    string name = NetCDFTools.name(fs);

                    int dim_length = NetCDFTools.int4(fs);

                    bool is_record = false;
                    if (dim_length == 0)
                    {
                        // if dim_length is 0, then this is the record dimension
                        is_record = true;
                    }

                    dim_list.AddLast(new Dimension(name, dim_length, is_record));
                }
            }

            // gatt_list
            gatt_list = att_list(fs);

            // var_list
            int variable_tag = NetCDFTools.int4(fs); // should be 0xB
            uint num_vars = NetCDFTools.non_neg(fs);
            if (variable_tag == 0xB && num_vars > 0)
            {
                for (int i = 0; i < num_vars; i++)
                {
                    string name = NetCDFTools.name(fs);

                    uint nelems = NetCDFTools.non_neg(fs);

                    // dimid list
                    int[] dimid = new int[nelems];
                    for (int j = 0; j < nelems; j++)
                    {
                        dimid[j] = NetCDFTools.int4(fs);
                    }

                    // vatt_list
                    LinkedList<Attribute> vatt_list = att_list(fs);

                    NC_TYPE type = (NC_TYPE)NetCDFTools.non_neg(fs);

                    uint vsize = NetCDFTools.non_neg(fs);

                    uint begin = NetCDFTools.non_neg(fs);

                    var_list.AddLast(new Variable(name, dimid, vatt_list, type, vsize, begin));
                }
            }
        }

        private void ReadData(Stream fs)
        {
            foreach (Variable v in this.var_list) {
                if (fs.Position != v.begin)
                {
                    throw new FormatException("The variable " + v.name + " begins at " + v.begin + " but the stream is at " + fs.Position + " probably because the file is malformed.");
                }
                byte[][] data = NetCDFTools.values(v.type, v.length, fs, true);

                uint typeLength = NetCDFTools.getTypeLength(v.type);
                
                // too hard to organise the data into its actual shape until it's time to use it

                v.data = data;
            }
        }
    }
}
