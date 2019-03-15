namespace In.ServiceCommon.Streaming
{
    public interface IStreamingContract<TKey, TData> 
    {
        bool Subscribe(TKey key, IStreamingCallback<TData> callback);
        
        bool Unsubscribe(TKey key, IStreamingCallback<TData> callback);
        
        bool Unsubscribe(IStreamingCallback<TData> callback);
    }
}