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

            IConfigurationRoot configuration = new ConfigurationBuilder()
              .AddMasterConfiguration()
              .Build();

            var r = configuration["Region"];

            Application application = new Application();
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
