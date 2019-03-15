namespace In.ServiceCommon.Streaming
{
    public interface IStreamingCallback<T>
    {
        void Send(T data);
    }
}