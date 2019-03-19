namespace In.ServiceCommon.Client
{
    public interface IClientServiceProxy
    {
        object Call(string type, string method, object[] args);
        void CallVoidSync(string type, string method, object[] args);
        void CallVoidAsync(string type, string method, object[] args);
    }
}