namespace In.ServiceCommon.Streaming
{
    public interface IStreamingAdapter<T>
    {
        bool[] Subscribe(T[] keys);
        bool Subscribe(T key);
        bool[] Unsubscribe(T[] keys);
        bool Unsubscribe(T keys);
    }
}