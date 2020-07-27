using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

namespace Bytemark.DNS
{
    public interface ICommand
    {

        public string Noun { get; }

        public string Verb { get; }

        public string Usage { get; }

        public Task<int> Execute(
            BytemarkDNSClient client, 
            IConfiguration config
            );

    }
}
