using System;
using System.Collections.Generic;
using System.Text;

namespace Bytemark.DNS.Models
{
    internal class AuthToken
    {
        public AuthToken(string token, DateTimeOffset lastUpdated) {
            Token=token;
            LastUpdated=lastUpdated;
        }

        public string Token { get; init; }

        public DateTimeOffset LastUpdated { get;set; }

        public bool IsFresh {
            get {
                return LastUpdated.AddMinutes(5) > DateTimeOffset.UtcNow;
            }
        }
    }
}
