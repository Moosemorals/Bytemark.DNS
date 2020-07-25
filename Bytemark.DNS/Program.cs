using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;

using Bytemark.DNS.Models;

using Microsoft.Extensions.Configuration;

namespace Bytemark.DNS
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("secrets.json", optional: false)
                .Build();

            HttpClient client = new HttpClient();

            BytemarkDNSClient _dns = new BytemarkDNSClient(client, config["Auth:Username"], config["Auth:Password"]);

            Result<IEnumerable<Domain>> result = await _dns.ListDomainsAsync(AccountID: 3840, Overview: true);

            if (result.IsSuccess) {
                foreach (Domain d in result.Payload) {
                    Console.WriteLine("Got Domain " + d.Name + ", " + d.AccountID);

                    foreach (Record r in d.Records) {



                        //     var recordResult = await _dns.ListRecordsAsync(d.ID, d.AccountID);

                        //      if (recordResult.IsSuccess) {
                        //          foreach (Record r in recordResult.Payload) {
                        Console.WriteLine("  Got Record " + r.Type + ", " + r.Name + ", " + r.Content);
                        //          }
                        //     } else {
                        //          Console.WriteLine("Couldn't fetch results: " + recordResult.Error);
                        //      }
                    }
                }
            } else {
                Console.WriteLine("There was a problem: " + result.Error);
            }

        }
    }
}
