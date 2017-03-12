using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeAwesomeWebApi
{

    [Serializable]
    public class InvalidTypeExportException : Exception
    {
        public InvalidTypeExportException() { }
        public InvalidTypeExportException(string message) : base(message) { }
        public InvalidTypeExportException(string message, Exception inner) : base(message, inner) { }
        protected InvalidTypeExportException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}
