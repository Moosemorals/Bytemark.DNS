using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace Bytemark.DNS.Models
{
   internal class ErrorResponse
    {
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
