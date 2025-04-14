using Microsoft.Extensions.Configuration;
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
        private readonly string ConfigurationFileName;

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
                JsonConfigurationProvider jsonConfigurationProvider = provider as JsonConfigurationProvider;
                this.ConfigurationFileName = jsonConfigurationProvider.Source.Path;
                if (jsonConfigurationProvider != null)
                {
                    var fileProvider = jsonConfigurationProvider.Source.FileProvider as Microsoft.Extensions.FileProviders.PhysicalFileProvider;
                    if (fileProvider != null)
                    {
                        var pathToWatch = fileProvider.Root;
                        this.FileSystemWatcher = new FileSystemWatcher(pathToWatch);
                    }
                }
            }
        }


        public async Task Run(CancellationToken cancellationToken)
        {
            Log("XXXXX");
            EnableFileWatcher();
            while (!cancellationToken.IsCancellationRequested)
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
                Thread.Sleep(TimeSpan.FromMinutes(1));
            }
        }


        private void EnableFileWatcher()
        {
            Log($"EnableFileWatcher start", EventLogEntryType.SuccessAudit);
            Log($"EnableFileWatcher start", EventLogEntryType.Warning);
            FileSystemWatcher.NotifyFilter = NotifyFilters.Attributes |
    NotifyFilters.CreationTime |
    NotifyFilters.FileName |
    NotifyFilters.LastAccess |
    NotifyFilters.LastWrite |
    NotifyFilters.Size |
    NotifyFilters.Security;
            //FileSystemWatcher.Changed += OnChanged;
            FileSystemWatcher.Created += OnChanged;
            FileSystemWatcher.Deleted += OnChanged;
            FileSystemWatcher.Renamed += OnChanged;
            //FileSystemWatcher.Filter = "*.json";// this.ConfigurationFileName;

            //var path = Path.Join(FileSystemWatcher.Path, FileSystemWatcher.Filter);
            //if (!File.Exists(path))
            //{
            //    throw new Exception($"Path {path} does not exists");
            //}
           
            FileSystemWatcher.EnableRaisingEvents = true;
            //Log($"EnableFileWatcher end filter:{FileSystemWatcher.Filter}, path:{FileSystemWatcher.Path}, full path: {path}", EventLogEntryType.Warning);
        }


        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Log($"File watcher OnChanged invoked", EventLogEntryType.Warning);
            Log($"ChangeType:{e.ChangeType.ToString()}", EventLogEntryType.Warning);
            Log($"File watcher OnChanged, file changed: {e.FullPath}", EventLogEntryType.Warning);
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            UpdateAlibabaFromFile();
            Log($"File watcher OnChanged, file changed: {e.FullPath}", EventLogEntryType.Warning);
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
            if (ExternalIpChanged(externalIp))
            {
                UpdateIpConfigurationForHosts(externalIp);
            }
            Log($"Waiting 1 minute:{DateTime.Now}");
        }

        private void Log(string log)
        {
            Log(log, EventLogEntryType.Information);
        }

        private void Log(string log, EventLogEntryType eventLogEntryType)
        {
            string name = "PT.AlibabaCloud.Core";

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
            //Log($"Starting check of the IP address. Last remembered IP {LastPublicAddress}");
            if (LastPublicAddress != externalIp)
            {
                Log($"It seems that external IP changed, let us update all hosts from config. Last Public address {LastPublicAddress} externalIP: {externalIp}", EventLogEntryType.Warning);
                return true;
            }
            else
            {
                //Log($"Last remembered IP {LastPublicAddress} and current extrenal Ip {externalIp} are the same");
                return false;
            }

        }

        private void UpdateIpConfigurationForHosts(string externalIp)
        {
            var hosts = Configuration.GetSection("Hosts").Get<HostConfig[]>();
            AlibabaGate.UpdateAlibabaConfiguration(hosts, externalIp);
            LastPublicAddress = externalIp;
            SendEmail(string.Format($"[Changed!] external ip address new public address {externalIp}"));
        }

        private void SendEmail(string body)
        {
            Log("Try to send email");
            try
            {
                var hostname = System.Environment.MachineName;
                Console.WriteLine(body);
                var bodyWithhostname = $"hostname: {hostname}, Message:{body}";
                SentEmailGmail.Gmail.Send("productivitytools.tech@gmail.com", Configuration["GmailPassword"], "pwujczyk@gmail.com", "PT.AbibalaCloud", bodyWithhostname);
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }
    }
}
