using System;

namespace Bytemark.DNS.Models {
    internal class BytemarkDNSException : Exception
    {
        public BytemarkDNSException(string message) : base(message)
        {
        }

        public BytemarkDNSException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
