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
using System.Security.Cryptography;
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
        private readonly string ConfigurationFullPath;
        private string FileHash { get; set; }

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
                var configurationFileName = jsonConfigurationProvider.Source.Path;
                if (jsonConfigurationProvider != null)
                {
                    var fileProvider = jsonConfigurationProvider.Source.FileProvider as Microsoft.Extensions.FileProviders.PhysicalFileProvider;
                    if (fileProvider != null)
                    {
                        var pathToWatch = fileProvider.Root;
                        this.ConfigurationFullPath = Path.Combine(pathToWatch, configurationFileName);
                    }
                }
            }
        }


        public async Task Run(CancellationToken cancellationToken)
        {
            Log("XXXXX");
            //EnableFileWatcher();
            CheckFile();
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

        private void CheckFile()
        {
           var sha = CalculateJsonFileHash(this.ConfigurationFullPath);
            if (this.FileHash!=sha)
            {
                this.FileHash = sha;
            }
        }


        public static string CalculateJsonFileHash(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File not found at path: {filePath}");
                return null;
            }

            try
            {
                string jsonString = File.ReadAllText(filePath, Encoding.UTF8);
                return CalculateSha256Hash(jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Calculates the SHA-256 hash of a given string.
        /// </summary>
        /// <param name="data">The string to hash.</param>
        /// <returns>The SHA-256 hash as a hexadecimal string.</returns>
        private static string CalculateSha256Hash(string data)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                byte[] hashBytes = sha256.ComputeHash(bytes);

                // Convert byte array to a hexadecimal string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2")); // "x2" formats as lowercase hexadecimal
                }
                return builder.ToString();
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
            Log($"File watcher OnChanged invoked, ChangeType:{e.ChangeType.ToString()}, file changed:{e.FullPath}, XXX:{e.Name} ", EventLogEntryType.Warning);
            FileInfo file = new FileInfo(e.FullPath);
            Log($"FileName: {file.Name}, {Path.GetFullPath(e.Name)}");
            
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            UpdateAlibabaFromFile();
            //Log($"File watcher OnChanged, file changed: {e.FullPath}", EventLogEntryType.Warning);
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
