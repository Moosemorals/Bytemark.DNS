using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace Bytemark.DNS.Models
{
    internal class AuthParameters
    {
        public AuthParameters(string username, string password) {
            Username=username;
            Password=password;
        }

        [JsonProperty("username"), JsonRequired]
        public string Username { get; init; }

        [JsonProperty("password"), JsonRequired]
        public string Password { get; init; }
    }
}
