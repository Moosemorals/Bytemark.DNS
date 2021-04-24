using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Bytemark.DNS.Models;

using Microsoft.Extensions.Configuration;

namespace Bytemark.DNS.Commands
{
    internal class DeleteRecord : ICommand
    {
        public string Noun => "Record";

        public string Verb => "Delete";

        public string Usage => "--name <FQDN>";

        public async Task<int> Execute(BytemarkDNSClient client, IConfiguration config)
        {
            string name = config["name"];

            if (string.IsNullOrEmpty(name)) {
                Console.WriteLine("--name not given");
                return ICommand.Fail;
            }

            Domain? targetDomain = null;
            Result<IEnumerable<Domain>> domains = await client.ListDomainsAsync(Overview: true);
            if (domains.IsSuccess && domains.Payload != null) {
                foreach (Domain d in domains.Payload) {
                    if (name.EndsWith(d.Name)) {
                        targetDomain = d;
                        break;
                    }
                }
            } else {
                Console.WriteLine("Could not fetch list of current domains: {0}", domains.Error);
                return ICommand.Fail;
            }

            if (targetDomain == null) {
                Console.WriteLine("Can't find target domain for {0}", name);
                return ICommand.Fail;
            }

            Record? targetRecord = null;
            foreach (Record r in targetDomain.Records) {
                if (r.Name == name) {
                    targetRecord = r;
                    break;
                }
            }

            if (targetRecord == null) {
                Console.WriteLine("Can't find target record {0}", name);
                return ICommand.Fail;
            }

            Result<string> delete = await client.DeleteRecordAsync(targetRecord.ID);

            if (!delete.IsSuccess) {
                Console.WriteLine("Could not delete record {0}: {1}", name, delete.Error);
                return ICommand.Fail;
            }

            return ICommand.Success;
        }
    }
}
