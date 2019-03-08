using log4net.Config;

namespace In.SomeService
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();
        }
    }
}