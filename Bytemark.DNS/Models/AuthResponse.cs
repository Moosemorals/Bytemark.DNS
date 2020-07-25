using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace Bytemark.DNS.Models
{
    internal class AuthResponse
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("factors")]
        public string[] Factors { get; set; }

        [JsonProperty("group_memberships")]
        public string[] Groups { get; set; }
    }
}
