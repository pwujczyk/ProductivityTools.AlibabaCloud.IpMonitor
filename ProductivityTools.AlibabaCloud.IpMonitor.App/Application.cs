using Microsoft.Extensions.Configuration;
using ProductivityTools.AlibabaCloud.IpMonitor.Alibaba;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProductivityTools.AlibabaCloud.IpMonitor.App
{
    public class Application
    {
        private Dictionary<string, string> LastPublicAddress = new Dictionary<string, string>();
        private string Domain = "productivitytools.tech";
        private DateTime LastMonitorEmailSent = DateTime.MinValue;
        private int ExceptionsCount = 0;
        private readonly IConfigurationRoot Configuration;

        public Application(IConfigurationRoot configuration)
        {
            this.Configuration = configuration;
        }

        public void Run()
        {
            Check();

            while (true)
            {
                try
                {
                    Check();
                    ExceptionsCount = 0;
                }
                catch (Exception ex)
                {
                    ExceptionsCount++;
                    Console.WriteLine(ex.ToString());
                    SendEmail(string.Format($"Some exception was throw{ex.ToString()}"));
                    Thread.Sleep(TimeSpan.FromMinutes(1));
                    if (ExceptionsCount > 10)
                    {
                        Thread.Sleep(TimeSpan.FromHours(1));
                    }
                }
            }
        }

        AlibabaGate alibabaGate;
        AlibabaGate AlibabaGate
        {
            get
            {
                if (alibabaGate == null)
                {
                    string region = Configuration["Region"];
                    string accessKeyId = Configuration["AccessKeyId"];
                    string accessKeySecret = Configuration["AccessKeySecret"];

                    alibabaGate = new AlibabaGate(region, accessKeyId, accessKeySecret);
                }
                return alibabaGate;
            }
        }

        private void Check()
        {
            var valuesSection = this.Configuration.GetSection("Hosts");
            var itemArray = valuesSection.AsEnumerable();
            foreach (var item in itemArray)
            {
                if (!string.IsNullOrEmpty(item.Value))
                {
                    Check(item.Value);
                }
            }
            Thread.Sleep(TimeSpan.FromMinutes(1));
        }

        //private void InitialCheck(string host)
        //{
        //    var currentExternalIp = Ifconfig.GetPublicIpAddress();
        //    Console.WriteLine(currentExternalIp);


        //    string currentAlibabaConfiguration = AlibabaGate.GetcurrentIpConfiguration(Domain, host);
        //    if (currentExternalIp == currentAlibabaConfiguration)
        //    {
        //        SendEmail($"Current IP address ({currentExternalIp}) for host '{host}' is the same as set up in Alibaba ({currentAlibabaConfiguration}), no action.");
        //        this.LastPublicAddress = currentExternalIp;
        //    }
        //    else
        //    {
        //        alibabaGate.UpdateDnsValue(Domain, host, currentExternalIp);
        //        var updatedValue = alibabaGate.GetcurrentIpConfiguration(Domain, host);
        //        SendEmail($"Current IP address ({currentExternalIp}) for host '{host}' was different than in Alibaba({currentAlibabaConfiguration}).Address updated to {updatedValue}.");
        //    }
        //}

        private void Check(string host)
        {
            Console.WriteLine($"Perform check. Last remember Ip:{(LastPublicAddress.ContainsKey(host)? LastPublicAddress[host] : string.Empty )}");
            var currentExternalIp = Ifconfig.GetPublicIpAddress();
            if (LastPublicAddress.ContainsKey(host) == false || LastPublicAddress[host] != currentExternalIp)
            {
                string currentAlibabaConfiguration = AlibabaGate.GetcurrentIpConfiguration(Domain, host);
                if (currentAlibabaConfiguration != currentExternalIp)
                {
                    AlibabaGate.UpdateDnsValue(Domain, host, currentExternalIp);
                    //var updatedAlibabaConfiguration = alibabaGate.GetcurrentIpConfiguration(Domain, host);

                    SendEmail(string.Format($"[Changed!] Last public addres:{LastPublicAddress}, new public address{currentExternalIp}. Value changed to: {currentAlibabaConfiguration}"));
                    this.LastPublicAddress[host] = currentExternalIp;
                }
                this.LastPublicAddress[host] = currentExternalIp;
            }
            else
            {
                if ((DateTime.Now.Hour % 6 == 0 && LastMonitorEmailSent.AddHours(6) < DateTime.Now)
                    || this.LastMonitorEmailSent == DateTime.MinValue)
                {
                    LastMonitorEmailSent = DateTime.Now;

                    SendEmail($"No changes current ip:{currentExternalIp}");
                }
            }

            Console.WriteLine($"Waiting 1 minute:{ DateTime.Now}");
        }

        //pw: to be changed sent na send
        private static void SendEmail(string body)
        {
            Console.WriteLine(body);
            SentEmailGmail.Gmail.Send("pwujczyk@gmail.com", "Hexagones1", "pwujczyk@hotmail.com", "DNSMonitor", body);
        }
    }
}
