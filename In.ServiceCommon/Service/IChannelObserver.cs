using In.ServiceCommon.Network;

namespace In.ServiceCommon.Service
{
    public interface IChannelObserver
    {
        void OnChannelConnected(INetworkChannel networkChannel);
        void OnChannelDisconnected(INetworkChannel networkChannel);
    }
}