using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Bytemark.DNS.Models;

using Microsoft.Extensions.Configuration;

namespace Bytemark.DNS.Commands {
    internal class ListDomains : ICommand
    {
        public string Noun => "Domain";

        public string Verb => "List";

        public string Usage => "";

        public async Task<int> Execute(BytemarkDNSClient client, IConfiguration config)
        {
            Result<IEnumerable<Domain>> result = await client.ListDomainsAsync(Overview: true);

            if (result.IsSuccess) {
                foreach (Domain d in result.Payload) {

                    Console.Write("Domain {0} (Account {1})", d.Name, d.AccountID);

                    if (d.Records != null) {
                        Console.WriteLine(":");
                        foreach (Record r in d.Records) {
                            Console.WriteLine("\t{0}\t{1}\t{2}\t{3}", r.Type, r.TTL, r.Name, r.Content);
                        }
                    }
                    Console.WriteLine();
                }
                return ICommand.Success;
            } else {
                return ICommand.Fail;
            }
        }
    }
}
