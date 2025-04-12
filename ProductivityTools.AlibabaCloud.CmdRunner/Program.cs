using Microsoft.Extensions.Configuration;
using ProductivityTools.AlibabaCloud.NetCoreService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProductivityTools.AlibabaCloud.CmdRunner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello from CmdRunner");
            var service = new WindowsBackgroundService();
            var cancellationToken = new CancellationToken();
            await service.OnDebug(cancellationToken);
           // var service = new Service1();
           // service.OnDebug();
            Console.ReadLine();
        }
    }
}
