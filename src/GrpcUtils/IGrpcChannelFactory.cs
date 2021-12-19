using Grpc.Net.Client;

namespace CommonLibs.GrpcUtils
{
    public interface IGrpcChannelFactory
    {
        GrpcChannel GetGrpcChannel(string ip, int port, bool isHttps = false);
    }
}