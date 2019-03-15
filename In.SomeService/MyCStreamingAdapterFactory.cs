using In.ServiceCommon.Streaming;

namespace In.SomeService
{
    public class MyCStreamingAdapterFactory : IAdapterFactory<int, MyCStreamingData>
    {
        private MyCStreamingAdapter _adapter;

        public MyCStreamingAdapterFactory()
        {
            _adapter = new MyCStreamingAdapter();
        }

        public IStreamingAdapter<int> GetAdapter()
        {
            var adapter = new MyCStreamingAdapter();
            return adapter;
        }
    }
}