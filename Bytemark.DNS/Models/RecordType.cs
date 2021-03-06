﻿using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bytemark.DNS.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RecordType
    {
        A, AAAA, CNAME, MX, NS, SOA, SRV, TXT
    }
}
