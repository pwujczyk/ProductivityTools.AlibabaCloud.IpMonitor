﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using ProductivityTools.AlibabaCloud.Alibaba;
using ProductivityTools.AlibabaCloud.IpMonitor.Alibaba;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProductivityTools.AlibabaCloud.App
{
    public class Application
    {
        private string LastPublicAddress = "";
        // private Dictionary<string, string> LastPublicAddressDictionary = new Dictionary<string, string>();
        private string Domain = "productivitytools.top";
        private DateTime LastMonitorEmailSent = DateTime.MinValue;
        private int ExceptionsCount = 0;
        private readonly IConfigurationRoot Configuration;
        private readonly FileSystemWatcher FileSystemWatcher;

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

                    alibabaGate = new AlibabaGate(region, accessKeyId, accessKeySecret, Log);
                }
                return alibabaGate;
            }
        }


        public Application(IConfigurationRoot configuration)
        {
            this.Configuration = configuration;
            foreach (var provider in this.Configuration.Providers)
            {
                JsonConfigurationProvider JsonConfigurationProvider = provider as JsonConfigurationProvider;
                if (JsonConfigurationProvider != null)
                {
                    var fileProvider = JsonConfigurationProvider.Source.FileProvider as Microsoft.Extensions.FileProviders.PhysicalFileProvider;
                    if (fileProvider != null)
                    {
                        var pathToWatch = fileProvider.Root;
                        this.FileSystemWatcher = new FileSystemWatcher(pathToWatch);
                    }
                }
            }
        }


        public void Run()
        {
            EnableFileWatcher();
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


        private void EnableFileWatcher()
        {
            FileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite;
            FileSystemWatcher.Changed += OnChanged;

            FileSystemWatcher.Filter = "ProductivityTools.AlibabaCloud.json";
            FileSystemWatcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            UpdateAlibabaFromFile();
            Log($"File changed: {e.FullPath}", EventLogEntryType.Information);
        }

        private void UpdateAlibabaFromFile()
        {
            var externalIp = Ifconfig.GetPublicIpAddress();
            Configuration.Reload();
            var hosts = Configuration.GetSection("Hosts").Get<HostConfig[]>();
            AlibabaGate.UpdateAlibabaConfiguration(hosts, externalIp);
        }

        private void Check()
        {
            var externalIp = Ifconfig.GetPublicIpAddress();
            //var hosts = Configuration.GetSection("Hosts2").Get<HostConfig[]>();
            //AlibabaGate.UpdateAlibabaConfiguration(hosts, externalIp);


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
            var hosts = Configuration.GetSection("Hosts").Get<HostConfig[]>();

            AlibabaGate.UpdateAlibabaConfiguration(hosts, externalIp);
            //foreach (var host in hosts)
            //{
            //    if (host.MapToExternal)
            //    {
            //        UpdateIpConfigurationForHost(host, externalIp);
            //    }
            //}
            //LastPublicAddress = externalIp;
            //Log($"I will send address that I updated the ip {externalIp}");

            SendEmail(string.Format($"[Changed!] external ip address new public address {externalIp}"));
        }
        //private void UpdateIpConfigurationForHost(HostConfig host, string externalIp)
        //{
        //    string hostAlibabaConfiguration = AlibabaGate.GetcurrentIpConfiguration(Domain, host.RR);
        //    if (hostAlibabaConfiguration != externalIp)
        //    {
        //        AlibabaGate.UpdateRecord(hostAlibabaConfiguration, host, externalIp);
        //    }
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
