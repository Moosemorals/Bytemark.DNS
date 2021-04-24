using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

namespace Bytemark.DNS {
    internal class Program
    {
        private static async Task<int> Main(string[] args)
        {
            IList<ICommand> commands = GetCommands();
            if (args.Length < 2) {
                ShowUsage(commands);
                return 1;
            }

            string verb = args[0];
            string noun = args[1];

            ICommand? cmd = commands.FirstOrDefault(c => 
                   string.Equals(c.Verb, verb, StringComparison.OrdinalIgnoreCase) 
                && string.Equals(c.Noun, noun, StringComparison.OrdinalIgnoreCase)
            );

            if (cmd == null) {
                Console.WriteLine("Can't find command to {0} {1}", verb, noun);
                ShowUsage(commands);
                return 1;
            }

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("secrets.json", optional: false)
                .AddEnvironmentVariables()
                .AddCommandLine(args.Skip(2).ToArray())
                .Build();

            HttpClient client = new HttpClient();
            BytemarkDNSClient _dns = new BytemarkDNSClient(client, config["Auth:Username"], config["Auth:Password"]);

            if (!(await cmd.Execute(_dns, config) == ICommand.Success)) {
                ShowUsage(commands);
                return ICommand.Fail;
            }

            return ICommand.Success;
        }

        private static void ShowUsage(IList<ICommand> commands)
        {
            Console.WriteLine("Usage: BytemarkDNS <verb> <noun> [option...]");
            foreach (ICommand c in commands) {
                Console.WriteLine("\t{0} {1} {2}", c.Verb, c.Noun, c.Usage);
            }
        }

        private static IList<ICommand> GetCommands()
        {
            Type type = typeof(ICommand);

            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(t => type.IsAssignableFrom(t) && t.IsClass)
                .Select(t => t.GetConstructor(Type.EmptyTypes))
                .Select(c => c?.Invoke(null))
                .OfType<ICommand>()
                .ToList();
        }

        /*
        private static async Task<int> AuthHookAsync(IConfiguration config) { 
            string targetDomainName = config["CERTBOT_DOMAIN"];
            string validationString = config["CERTBOT_VALIDATION"];

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
                Console.WriteLine("Couldn't create challenge record for {1}: {0}", result.Error, target.Name);
                return 1;
            }

	    Console.WriteLine("Added challenge record for {0}", target.Name);

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
		    Console.WriteLine("Deleting challenge record {0}", r.Name);
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

*/
    }
}
