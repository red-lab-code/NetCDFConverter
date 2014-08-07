using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCDFConverter
{
    [Serializable]
    public class InvalidDataTypeException : System.Exception
    {
        public InvalidDataTypeException(string message)
            : base(message) { }
    }
}
