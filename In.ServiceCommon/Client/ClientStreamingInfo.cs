namespace In.ServiceCommon.Client
{
    public class ClientStreamingInfo
    {
        public string Type { get; set; }
        public ClientProxyBase Adapter { get; set; }
        public DelegateCallback Callback { get; set; }
    }
}