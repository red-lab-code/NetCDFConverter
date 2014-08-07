using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCDFConverter
{
    [Serializable]
    public class InvalidTimeIndexException : System.Exception
    {
        public InvalidTimeIndexException(string message)
            : base(message) { }
    }
}
