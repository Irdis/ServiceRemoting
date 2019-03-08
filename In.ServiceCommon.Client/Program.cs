using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace In.ServiceCommon.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var tcpClient = new TcpClient();
            tcpClient.Connect("localhost", 8000);
            var stream = tcpClient.GetStream();
            var sb = new StringBuilder();
            for (int i = 0; i < 1000; i++)
            {
                sb.Append("Kappa" + i);
            }
            var bytes = ASCIIEncoding.ASCII.GetBytes(sb.ToString());
            var length = BitConverter.GetBytes(bytes.Length);
            stream.Write(length, 0, length.Length);
            stream.Write(bytes, 0, bytes.Length);
            stream.Write(length, 0, length.Length);
            stream.Write(bytes, 0, bytes.Length);
            Console.ReadKey();
        }
    }
}
