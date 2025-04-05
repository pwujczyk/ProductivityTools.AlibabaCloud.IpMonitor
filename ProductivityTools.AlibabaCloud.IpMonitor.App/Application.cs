using Microsoft.Extensions.Configuration;
using ProductivityTools.AlibabaCloud.IpMonitor.Alibaba;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProductivityTools.AlibabaCloud.IpMonitor.App
{
    public class Application
    {
        private string LastPublicAddress = "";
       // private Dictionary<string, string> LastPublicAddressDictionary = new Dictionary<string, string>();
        private string Domain = "productivitytools.top";
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
            var externalIp = Ifconfig.GetPublicIpAddress();
            if (ExternalIpChanged(externalIp))
            {
                UpdateIpConfigurationForHosts(externalIp);
            }
            Log($"Waiting 1 minute:{DateTime.Now}");
            Thread.Sleep(TimeSpan.FromMinutes(1));
        }

        private void Log(string log)
        {
            Log(log, EventLogEntryType.Information);
        }

        private void Log(string log, EventLogEntryType eventLogEntryType)
        {
            string name = "PT.AlibabaCloud";

            if (!EventLog.SourceExists(name))
            {
                EventSourceCreationData eventSourceData = new EventSourceCreationData(name, name);
                EventLog.CreateEventSource(eventSourceData);
            }

            using (EventLog myLogger = new EventLog(name, ".", name))
            {
                myLogger.WriteEntry(log, eventLogEntryType);
            }

            Console.WriteLine(log);
        }

        private bool ExternalIpChanged(string externalIp)
        {
            Log($"Starting check of the IP address. Last remembered IP {LastPublicAddress}");
            if (LastPublicAddress != externalIp)
            {
                Log($"It seems that external IP changed, let us update all hosts from config", EventLogEntryType.Warning);
                return true;
               
            }
            else
            {
                return false;
            }

        }

        private void UpdateIpConfigurationForHosts(string externalIp)
        {
            var valuesSection = this.Configuration.GetSection("Hosts");
            var itemArray = valuesSection.AsEnumerable();

            foreach (var item in itemArray)
            {
                if (!string.IsNullOrEmpty(item.Value))
                {
                    Console.WriteLine($"Processing {item.Value}");
                    UpdateIpConfigurationForHost(item.Value,externalIp);
                }
            }
            LastPublicAddress = externalIp;
            Log($"I will send address that I updated the ip {externalIp}");
            
            SendEmail(string.Format($"[Changed!] external ip address new public address {externalIp}"));
        }
        private void UpdateIpConfigurationForHost(string host, string externalIp)
        {
            string hostAlibabaConfiguration = AlibabaGate.GetcurrentIpConfiguration(Domain, host);
            if (hostAlibabaConfiguration != externalIp)
            {
                AlibabaGate.UpdateDnsValue(Domain, host, externalIp);
                //var updatedAlibabaConfiguration = alibabaGate.GetcurrentIpConfiguration(Domain, host);
            }
        }

        //private void Check(string host)
        //{
        //    Log($"Starting check of the IP address for the host:${host}. Last remembered IP {(LastPublicAddressDictionary.ContainsKey(host) ? LastPublicAddressDictionary[host] : string.Empty)}");
        //    var currentExternalIp = Ifconfig.GetPublicIpAddress();
        //    if (LastPublicAddressDictionary.ContainsKey(host) == false || LastPublicAddressDictionary[host] != currentExternalIp)
        //    {
        //        Log($"It seems that for host {host} address is not up to date");
        //        string currentAlibabaConfiguration = AlibabaGate.GetcurrentIpConfiguration(Domain, host);
        //        if (currentAlibabaConfiguration != currentExternalIp)
        //        {
        //            AlibabaGate.UpdateDnsValue(Domain, host, currentExternalIp);
        //            //var updatedAlibabaConfiguration = alibabaGate.GetcurrentIpConfiguration(Domain, host);
        //            Log($"I will send address that I updated the ip in alibaba for host {host}");
        //            if (LastPublicAddressDictionary.ContainsKey(host))
        //            {
        //                SendEmail(string.Format($"[Changed!] Last public addres:{LastPublicAddressDictionary[host]}, new public address {currentExternalIp}. Value changed from: {currentAlibabaConfiguration}"));
        //            }
        //            else
        //            {
        //                SendEmail(string.Format($"First address setup for hsot {host}, new public address {currentExternalIp}. Value changed from: {currentAlibabaConfiguration}"));
        //            }
        //            this.LastPublicAddressDictionary[host] = currentExternalIp;
        //        }
        //        this.LastPublicAddressDictionary[host] = currentExternalIp;
        //    }
        //    else
        //    {
        //        if ((DateTime.Now.Hour % 6 == 0 && LastMonitorEmailSent.AddHours(6) < DateTime.Now)
        //            || this.LastMonitorEmailSent == DateTime.MinValue)
        //        {
        //            LastMonitorEmailSent = DateTime.Now;

        //            SendEmail($"No changes current ip:{currentExternalIp}");
        //        }
        //    }

        //    Log($"Waiting 1 minute:{ DateTime.Now} for host {host}");
        //}

        private void SendEmail(string body)
        {
            Log("Try to send email");
            try
            {
                Console.WriteLine(body);
                SentEmailGmail.Gmail.Send("productivitytools.tech@gmail.com", Configuration["GmailPassword"], "pwujczyk@gmail.com", "PT.AbibalaCloud", body);
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }
    }
}
