using System;
using System.Collections.Generic;
using System.Text;

namespace Bytemark.DNS.Models
{
    class BytemarkDNSException : Exception
    {
        public BytemarkDNSException(string message) : base(message)
        {
        }

        public BytemarkDNSException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
