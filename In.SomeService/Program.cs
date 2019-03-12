using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using In.ServiceCommon.Client;
using In.ServiceCommon.Interface;
using log4net.Config;

namespace In.SomeService
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");
            var services = new List<Type>
            {
                typeof(IMyAService),
                typeof(IMyBService),
            };
        }
    }
}