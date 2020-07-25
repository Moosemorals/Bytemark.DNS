using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Bytemark.DNS.Models;

using Microsoft.Extensions.Configuration;

namespace Bytemark.DNS
{
    class Program
    {
        private const string ACME_Challenge = "_acme-challenge.";

        static async Task<int> Main(string[] args)
        {

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("secrets.json", optional: false)
                .AddEnvironmentVariables()
                .Build();


            if (args.Length != 1) {
                // TODO: Proper logging
                Console.WriteLine("Must specifiy either 'AuthHook' or 'Cleanup' as only arguments");
                return 1;
            }

            if (string.Equals(args[0], "authhook", StringComparison.OrdinalIgnoreCase)) {
                return await AuthHookAsync(config);
            } else if (string.Equals(args[0], "cleanup", StringComparison.OrdinalIgnoreCase)) {
                return await Cleanup(config);
            } else {
                // TODO: Proper logging
                Console.WriteLine("Unrecognised command {0}", args[0]);
                return 1;
            }
            
        }
        private static async Task<int> AuthHookAsync(IConfiguration config) { 
            string targetDomainName = config["CERTBOT_DOMAIN"];
            string validationString = config["CERTBOT_VALIDATION"];

            HttpClient client = new HttpClient();

            BytemarkDNSClient _dns = new BytemarkDNSClient(client, config["Auth:Username"], config["Auth:Password"]);

            Result<IEnumerable<Domain>> domains = await _dns.ListDomainsAsync();

            if (!domains.IsSuccess) {
                // TODO: Proper logging
                Console.WriteLine("Couldn't get list of domains: {0}", domains.Error);
                return 1;
            }

            Domain target = domains.Payload.FirstOrDefault(d => d.Name == targetDomainName);
            if (target == null) {
                // TODO: Proper logging
                Console.WriteLine("Can't find target domain {0}", targetDomainName);
            }

            Result<Record> result =  await _dns.CreateRecordAsync(new Record() {
                DomainID = target.ID,
                Name = ACME_Challenge + target.Name,
                TTL = 30,
                Content =  validationString,
                Type = RecordType.TXT,
            });

            if (!result.IsSuccess) {
                //TODO: Proper logging
                Console.WriteLine("Couldn't create challenge record: {0}", result.Error);
                return 1;
            }

            // Success!
            return 0;
        }

        private static async Task<int> Cleanup(IConfiguration config)
        {
            string targetDomainName = config["CERTBOT_DOMAIN"];

            HttpClient client = new HttpClient();

            BytemarkDNSClient _dns = new BytemarkDNSClient(client, config["Auth:Username"], config["Auth:Password"]);

            Result<IEnumerable<Domain>> domains = await _dns.ListDomainsAsync(Overview: true);

            if (!domains.IsSuccess) {
                // TODO: Proper logging
                Console.WriteLine("Couldn't get list of domains: {0}", domains.Error);
                return 1;
            }

            Domain target = domains.Payload.FirstOrDefault(d => d.Name == targetDomainName);
            if (target == null) {
                // TODO: Proper logging
                Console.WriteLine("Can't find target domain {0}", targetDomainName);
            }

            foreach (Record r in target.Records) {
                if (r.Name.StartsWith(ACME_Challenge)) {
                    Result<string> deleteResult = await _dns.DeleteRecordAsync(r.ID);
                    if (!deleteResult.IsSuccess) {
                        // TODO: Proper logging
                        Console.WriteLine("Couldn't delete challenge record: {0}", deleteResult.Error);
                        return 1;
                    }
                }
            }

            // Success
            return 0;
        }


    }
}
