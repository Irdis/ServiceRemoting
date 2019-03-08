namespace In.SomeService
{
    public class MyBService : IMyBService
    {
        public int Rotate(Bar bar)
        {
            return bar.Name.Length;
        }
    }
}