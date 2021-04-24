using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using System.Threading.Tasks;

using Bytemark.DNS.Models;

using Microsoft.Extensions.Configuration;

namespace Bytemark.DNS.Commands
{
    internal class CreateRecord : ICommand
    {
        public string Noun => "Record";

        public string Verb => "Create";

        public string Usage => "--type <A|AAAA|CNAME|MX|NS|SOA|SRV|TXT> --ttl <seconds> --name <FQDN> --content <record contents>";

        public async Task<int> Execute(BytemarkDNSClient client, IConfiguration config)
        {
            string rawType = config["type"];
            string name = config["name"];
            string rawTTL = config["ttl"];
            string content = config["content"];

            if (string.IsNullOrEmpty(rawType) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(rawTTL) || string.IsNullOrEmpty(content)) {
                Console.WriteLine("Missing parameter");
                return ICommand.Fail;
            }

            if (!Enum.TryParse(rawType, out RecordType type)) {
                Console.Write("Invalid value for --type: {0}", rawType);
                return ICommand.Fail;
            }

            if (!name.Contains('.')) {
                Console.Write("--name must be a fully qualified domain name");
                return ICommand.Fail;
            }

            if (!int.TryParse(rawTTL, out int TTL)) {
                Console.Write("--ttl must be a whole number of seconds");
                return ICommand.Fail;
            }

            Domain? target = null;
            Result<IEnumerable<Domain>> domains = await client.ListDomainsAsync(Overview: true);
            if (domains.IsSuccess && domains.Payload != null) {
                foreach (Domain d in domains.Payload) {
                    if (name.EndsWith(d.Name)) {
                        target = d;
                        break;
                    }
                }
            } else {
                Console.WriteLine("Could not fetch list of current domains: {0}", domains.Error);
                return ICommand.Fail;
            }

            if (target == null) {
                Console.WriteLine("Could not find target domain");
                return ICommand.Fail;
            }

            Result<Record>? create = await client.CreateRecordAsync(new Record(name, content) {
                DomainID = target.ID,
                TTL = TTL,
                Type = type,
            }) ;


            if (!create.IsSuccess) {
                Console.WriteLine("Could not create record {0}: {1}", name, create.Error);
                return ICommand.Fail;
            }

            return ICommand.Success;
        }
    }
}
