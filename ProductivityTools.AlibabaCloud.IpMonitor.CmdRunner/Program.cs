using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProductivityTools.MasterConfiguration;

namespace ProductivityTools.AlibabaCloud.IpMonitor.CmdRunner
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Hello from CmdRunner");
            var service = new Service1();
            service.OnDebug();
            Console.ReadLine();
        }
    }
}
