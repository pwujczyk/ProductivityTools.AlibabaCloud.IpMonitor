using Aliyun.Acs.Alidns.Model.V20150109;
using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Profile;
using ProductivityTools.AlibabaCloud.IpMonitor.Alibaba;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Aliyun.Acs.Alidns.Model.V20150109.DescribeDomainRecordsResponse;

namespace ProductivityTools.AlibabaCloud.Alibaba
{
    public class AlibabaGate
    {
        private readonly string Region;
        private readonly string AccessKeyId;
        private readonly string AccessKeySecret;
        private readonly Action<string> Log;

        public AlibabaGate(string region, string accessKeyId, string accessKeySecret, Action<string> log)
        {
            this.Region = region;
            this.AccessKeyId = accessKeyId;
            this.AccessKeySecret = accessKeySecret;
            this.Log = log;
        }


        IClientProfile ClientProfile
        {
            get
            {

                IClientProfile clientProfile = DefaultProfile.GetProfile(
                    Region, // region ID
                    AccessKeyId, //  AccessKey ID of RAM account
                    AccessKeySecret); // AccessKey Secret of RAM account

                return clientProfile;
            }
        }

        DefaultAcsClient DefaultAcsClient
        {
            get
            {
                DefaultAcsClient client = new DefaultAcsClient(ClientProfile);
                return client;
            }
        }

        public string GetcurrentIpConfiguration(string domain, string rr)
        {
            DescribeDomainRecords_Record result = GetCurrentConfiguration(domain, rr);
            return result._Value;
        }

        public void UpdateAlibabaConfiguration(HostConfig[] hosts, string externalIp)
        {
            string domainName = "productivitytools.top";
            var records = GetCurrentDomainRecords(domainName);
            foreach (var host in hosts)
            {
                var record = records.FirstOrDefault(x => x.RR == host.RR);
                if (record == null)
                {
                    CreateNewRecord(domainName, host, externalIp);
                }
                else
                {
                    ValidateRecordData(record, host, externalIp);
                }
                records.Remove(record);
            }


            //var rremove = records.FirstOrDefault(x => x.RR == "jenkins-tiny-px1");
            // RemoveRecord(rremove);
            foreach (var record in records)
            {
                RemoveRecord(record);
            }
        }

        private void RemoveRecord(DescribeDomainRecords_Record alibaba)
       {
            Log($"Removing record:{alibaba.RR} domain {alibaba.DomainName}, type:{alibaba.Type}, Value:{alibaba._Value} ");

            var deleteDomainRecordRequest = new Aliyun.Acs.Alidns.Model.V20150109.DeleteDomainRecordRequest();
            deleteDomainRecordRequest.RecordId = alibaba.RecordId;
            var actionResult = DefaultAcsClient.DoAction(deleteDomainRecordRequest, ClientProfile);


        }

        private void CreateNewRecord(string domainName, HostConfig local, string externalIp)
        {
            Log($"New record creation for  RR:{local.RR} domain {domainName}, type:{local.Type}, Value:{local.Target} ");
            var newDomainRecordRequest = new Aliyun.Acs.Alidns.Model.V20150109.AddDomainRecordRequest();
            newDomainRecordRequest.DomainName = domainName;
            newDomainRecordRequest.RR = local.RR;
            newDomainRecordRequest.Type = local.Type;

            if (local.MapToExternal)
            {
                newDomainRecordRequest._Value = externalIp;
            }
            else
            {
                newDomainRecordRequest._Value = local.Target;
            }
            var actionResult = DefaultAcsClient.DoAction(newDomainRecordRequest, ClientProfile);
            //var response3= DefaultAcsClient.GetAcsResponse(newDomainRecordRequest);

        }

        private void ValidateRecordData(DescribeDomainRecords_Record alibaba, HostConfig host, string ipaddress)
        {
            if (alibaba.Type == host.Type && alibaba.RR == host.RR &&
               ((host.MapToExternal && alibaba._Value == ipaddress) || (host.MapToExternal == false && alibaba._Value == host.Target)))
            {
                Log($"No change for record RR:{host.RR} type: {host.Type}, Value:{host.Target}");
            }
            else
            {
                UpdateRecord(alibaba, host, ipaddress);
            }

        }

        private void UpdateRecord(DescribeDomainRecords_Record alibaba, HostConfig local,string ipaddress)
        {
            var tragetValue = local.MapToExternal ? ipaddress : local.Target;
            Log($"Update record  rr:{local.RR} type:{local.Type}, value:{tragetValue} ");

            var updateDomainRecordRequest = new Aliyun.Acs.Alidns.Model.V20150109.UpdateDomainRecordRequest();
            updateDomainRecordRequest.RecordId = alibaba.RecordId;
            updateDomainRecordRequest.RR = local.RR;
            updateDomainRecordRequest._Value = tragetValue;
            updateDomainRecordRequest.Type = local.Type;
            var response = DefaultAcsClient.GetAcsResponse(updateDomainRecordRequest);
        }

        public void UpdateDnsValue(string domain, string host, string ipAddress)
        {
            var currentconfiguration = GetCurrentConfiguration(domain, host);

            var requestdomain = new Aliyun.Acs.Alidns.Model.V20150109.UpdateDomainRecordRequest();
            try
            {
                requestdomain.RecordId = currentconfiguration.RecordId;
                requestdomain.RR = currentconfiguration.RR;
                requestdomain.Type = currentconfiguration.Type;
                requestdomain._Value = ipAddress;
                var response2 = DefaultAcsClient.GetAcsResponse(requestdomain);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }
        }

        private List<DescribeDomainRecords_Record> GetCurrentDomainRecords(string domain)
        {

            var request = new Aliyun.Acs.Alidns.Model.V20150109.DescribeDomainRecordsRequest();
            request.DomainName = domain;
            request.PageSize = 100;
            var r = DefaultAcsClient.DoAction(request, ClientProfile);
            var response3 = DefaultAcsClient.GetAcsResponse(request);
            var records = response3.DomainRecords;
            return records;
        }

        private DescribeDomainRecords_Record GetCurrentConfiguration(string domain, string rr)
        {
            DescribeDomainRecords_Record x = null;
            try
            {
                var request = new DescribeDomainRecordsRequest();
                request.PageSize = 100;
                request.DomainName = domain;
                DefaultAcsClient.DoAction(request, ClientProfile);
                var response3 = DefaultAcsClient.GetAcsResponse(request);
                x = response3.DomainRecords.SingleOrDefault(d1 => d1.RR == rr);
                return x;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }
            throw new Exception("Unknown exception");
        }


    }
}
