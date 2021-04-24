using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

namespace Bytemark.DNS {
    public interface ICommand
    {
        public const int Fail = 1;
        public const int Success = 0;
        public string Noun { get; }

        public string Verb { get; }

        public string Usage { get; }

        public Task<int> Execute(
            BytemarkDNSClient client, 
            IConfiguration config
            );

    }
}
