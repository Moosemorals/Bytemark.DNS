using System;
using System.Collections.Generic;
using System.Text;

namespace Bytemark.DNS.Models
{
    public class Result<T> where T : class
    {
        public T Payload { get; internal set; }

        public bool IsSuccess => StatusCode <= 299;

        public int StatusCode { get; internal set; }

        public string Error { get; internal set; }
    }
}
