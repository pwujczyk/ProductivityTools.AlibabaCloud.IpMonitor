using Microsoft.Extensions.Configuration;
using ProductivityTools.AlibabaCloud.IpMonitor.App;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using ProductivityTools.MasterConfiguration;
using System.Threading;

namespace ProductivityTools.AlibabaCloud.IpMonitor
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ThreadStart start = new ThreadStart(FaxWorker); // FaxWorker is where the work gets done
            Thread faxWorkerThread = new Thread(start);

            // set flag to indicate worker thread is active
           // serviceStarted = true;

            // start threads
            faxWorkerThread.Start();


           
        }

        private void FaxWorker()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
             .AddMasterConfiguration(force: true)
             .Build();

            var r = configuration["Region"];

            Application application = new Application(configuration);
            application.Run();
        }

        public void OnDebug()
        {
            Console.WriteLine("OnDebug Hello");
            this.OnStart(null);

        }

        protected override void OnStop()
        {
        }
    }
}
