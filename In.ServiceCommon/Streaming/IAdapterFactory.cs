namespace In.ServiceCommon.Streaming
{
    public interface IAdapterFactory<TKey, TData>
    {
        IStreamingAdapter<TKey> GetAdapter();
    }
}