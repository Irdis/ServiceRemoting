namespace In.ServiceCommon.Network
{
    public interface INetworkMessageProcessor
    {
        void OnMessage(object sender, byte[] message);
    }
}