
using Newtonsoft.Json;

namespace Bytemark.DNS.Models {
    internal class ErrorResponse
    {
        public ErrorResponse(string error) {
            Error=error;
        }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
