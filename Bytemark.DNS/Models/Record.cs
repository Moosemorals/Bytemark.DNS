using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

using Newtonsoft.Json;

namespace Bytemark.DNS.Models
{
    public class Record
    {
        public Record(string name, string content) {
            Name=name;
            Content=content;
        }

        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("domain_id"), JsonRequired]
        public int? DomainID { get; set; }

        /// <summary>
        /// Fully qualified hostname for the record. Must be a sub-domain of
        /// the domain in domain_id. RFC 1035 stipulates that there is a 253
        /// byte maximum for this value, as two bytes are required to transmit
        /// the record length. It must not end with a dot (.).
        /// </summary>
        [JsonProperty("name"), JsonRequired, MaxLength(253)]
        public string Name { get; set; }

        [JsonProperty("type"), JsonRequired]
        public RecordType Type { get; set; }

        /// <summary>
        /// Data for the DNS record.
        /// NB If an SOA record has its serial set to 0, then it will be
        /// automatically set from the date of the last record in the database
        /// </summary>
        [JsonProperty("content"), JsonRequired]
        public string Content { get; set; }

        /// <summary>
        /// Time to live for the record
        /// </summary>
        [JsonProperty("ttl"), JsonRequired]
        public int TTL { get; set; }

        /// <summary>
        ///  Mark a record as authoritative.
        ///  This field should be set to true for data for which the zone itself
        ///  is authoritative, which includes the SOA record and its own NS
        ///  records.
        ///  This field should be false however for NS records which are used for
        ///  delegation, and also for any glue (A, AAAA) records present for this
        ///  purpose.
        /// </summary>
        [JsonProperty("authoriatitive")]
        public bool Authoriative { get; set; }

        /// <summary>

        ///        Mark a record as disabled(not in service).

        /// This means it can still be viewed and edited in the API, but will not be
        /// returned in DNS requests.

        /// </summary>
        [JsonProperty("disabled")]
        public bool Disabled { get; set; }


    }
}
