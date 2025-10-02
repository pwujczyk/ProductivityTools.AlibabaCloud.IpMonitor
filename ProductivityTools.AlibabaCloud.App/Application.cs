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
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    try
                    {
                        MeasureExecutionTime(CheckFile);
                        MeasureExecutionTime(CheckIP);
                        ExceptionsCount = 0;
                    }
                    catch (Exception ex)
                    {
                        ExceptionsCount++;
                        Console.WriteLine(ex.ToString());

                        Thread.Sleep(TimeSpan.FromMinutes(1));
                        if (ExceptionsCount > 10)
                        {
                            SendEmail(string.Format($"10 times exception was thrown in the row {ex.ToString()}"));
                            Thread.Sleep(TimeSpan.FromHours(1));
                        }
                    }
                    //Thread.Sleep(TimeSpan.FromMinutes(1));
                    Log($"Waiting 10 seconds minute:{DateTime.Now}");
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                }
                catch (Exception)
                {
                    Thread.Sleep(TimeSpan.FromHours(5));
                }
               
            }
        }

        public static void MeasureExecutionTime(Action action)
        {
            // Start the stopwatch.
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Execute the method.
            action.Invoke();

            // Stop the stopwatch.
            stopwatch.Stop();

            // Get the elapsed time.
            TimeSpan elapsedTime = stopwatch.Elapsed;

            // Get the method name using reflection.
            string methodName = action.Method.Name;

            // Print the elapsed time to the console, including the method name.
            Console.WriteLine($"Method '{methodName}' execution time: {elapsedTime.TotalMilliseconds} ms");
        }

        private void CheckFile()
        {
            var sha = CalculateJsonFileHash(this.ConfigurationFullPath);
            if (this.FileHash != sha)
            {
                this.FileHash = sha;
                UpdateAlibabaFromFile();
            }
        }


        public string CalculateJsonFileHash(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Log($"Error: File not found at path: {filePath}", EventLogEntryType.Warning);
                return null;
            }

            try
            {
                string jsonString = File.ReadAllText(filePath, Encoding.UTF8);
                return CalculateSha256Hash(jsonString);
            }
            catch (Exception ex)
            {
                Log($"Error reading file: {ex.Message}", EventLogEntryType.Error);
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

        private void UpdateAlibabaFromFile()
        {
            Log($"Hash changed so updating the alibaba", EventLogEntryType.Warning);

            var externalIp = Ifconfig.GetPublicIpAddress();
            Configuration.Reload();
            var hosts = Configuration.GetSection("Hosts").Get<HostConfig[]>();
            AlibabaGate.UpdateAlibabaConfiguration(hosts, externalIp);
        }

        private void CheckIP()
        {
            var externalIp = Ifconfig.GetPublicIpAddress();
            if (ExternalIpChanged(externalIp))
            {
                UpdateIpConfigurationForHosts(externalIp);
            }
      
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
