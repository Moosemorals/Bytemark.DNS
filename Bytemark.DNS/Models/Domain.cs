using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

using Newtonsoft.Json;

namespace Bytemark.DNS.Models
{
    public class Domain
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        /// <summary>
        /// This must not end with a dot (.).
        /// </summary>
        [JsonProperty("name"), JsonRequired, MaxLength(253)]
        public string Name { get; set; }

        [JsonProperty("account_id"), JsonRequired]
        public int AccountID { get; set; }

        [JsonProperty("records")]
        public IEnumerable<Record> Records { get; set; }
    }
}
