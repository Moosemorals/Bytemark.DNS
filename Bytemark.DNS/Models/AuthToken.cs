using System;
using System.Collections.Generic;
using System.Text;

namespace Bytemark.DNS.Models
{
    internal class AuthToken
    {
        public string Token { get; set; }

        public DateTimeOffset LastUpdated { get; set; }

        public bool IsFresh {
            get {
                return LastUpdated.AddMinutes(5) > DateTimeOffset.UtcNow;
            }
        }
    }
}
