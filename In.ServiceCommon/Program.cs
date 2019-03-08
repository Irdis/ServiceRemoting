using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace In.ServiceCommon
{
    class Program
    {
        static void Main(string[] args)
        {
            var listener = new NetworkListener();
            listener.Listen();
            Console.ReadKey();
        }
    }
}
