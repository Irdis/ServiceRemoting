namespace In.ServiceCommon.Client
{
    public interface IClientAdapter<T>
    {
        void SetCallback(T callback);
    }
}