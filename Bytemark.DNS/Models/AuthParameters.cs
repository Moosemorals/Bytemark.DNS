using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace Bytemark.DNS.Models
{
    internal class AuthParameters
    {
        [JsonProperty("username"), JsonRequired]
        public string Username { get; set; }

        [JsonProperty("password"), JsonRequired]
        public string Password { get; set; }
    }
}
