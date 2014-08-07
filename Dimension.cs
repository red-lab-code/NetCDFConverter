using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCDFConverter
{
    public class Dimension
    {
        // https://www.unidata.ucar.edu/software/netcdf/docs/netcdf/Dimensions.html
        public string name; // the name of the dimension
        public int length; // the length of the dimension
        public bool record_dimension; // true if this a 'record dimension'

        private Dimension(Dimension d)
        {
            this.name = String.Copy(d.name);
            this.length = d.length;
            this.record_dimension = d.record_dimension;
        }

        public Dimension(string name, int length, bool record_dimension)
        {
            this.name = name;
            this.length = length;
            this.record_dimension = record_dimension;
        }

        public override string ToString()
        {
            return "Dimension:[\"" + name + "\"|" + length + "|" + record_dimension + "]";
        }

        public Dimension Clone()
        {
            return new Dimension(this);
        }
    }
}
