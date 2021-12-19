using System.Collections.Concurrent;
using Grpc.Net.Client;
namespace CommonLibs.GrpcUtils
{
    public class GrpcChannelFactory : IGrpcChannelFactory
    {
        private readonly ConcurrentDictionary<string, GrpcChannel> _channelDictionary = new ConcurrentDictionary<string, GrpcChannel>();
        public GrpcChannel GetGrpcChannel(string ip, int port, bool isHttps = false)
        {
            var protocol = isHttps ? "https" : "http";
            var address = $"{protocol}://{ip}:{port}";

            if (_channelDictionary.ContainsKey(address))
            {
                return _channelDictionary[address];
            }

            var channel = GrpcChannel.ForAddress(address);
            _channelDictionary.TryAdd(address, channel);
            return channel;
        }
    }
}