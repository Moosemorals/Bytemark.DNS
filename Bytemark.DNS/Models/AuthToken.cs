using System;

namespace Bytemark.DNS.Models {
    internal class AuthToken
    {
        public AuthToken(string token, DateTimeOffset lastUpdated) {
            Token=token;
            LastUpdated=lastUpdated;
        }

        public string Token { get; init; }

        public DateTimeOffset LastUpdated { get;set; }

        public bool IsFresh => LastUpdated.AddMinutes(5) > DateTimeOffset.UtcNow;
    }
}
