using Aliyun.Acs.Alidns.Model.V20150109;
using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Profile;
using ProductivityTools.AlibabaCloud.IpMonitor.Alibaba;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public AlibabaGate(string region, string accessKeyId, string accessKeySecret)
        {
            this.Region = region;
            this.AccessKeyId = accessKeyId;
            this.AccessKeySecret = accessKeySecret;
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

        public string GetcurrentIpConfiguration(string domain, string host)
        {
            DescribeDomainRecords_Record result = GetCurrentConfiguration(domain, host);
            return result._Value;
        }

        public void CreateDomain(HostConfig[] hosts)
        {
            string domainName = "productivitytools.top";
            var records=GetCurrentDomainRecords(domainName);
            foreach(var host in hosts)
            {
                var record = records.FirstOrDefault(x => x.RR == host.RR);
                if (record==null)
                {
                    CreateNewRecord(domainName, host);
                }
                else
                {
                    ValidateRecordData(record, host);
                }
            }
            Console.WriteLine(records);



        }

        private void CreateNewRecord(string domainName, HostConfig local)
        {
            var newDomainRecordRequest = new Aliyun.Acs.Alidns.Model.V20150109.AddDomainRecordRequest();
            newDomainRecordRequest.DomainName = domainName;
            newDomainRecordRequest.RR = local.RR;
            newDomainRecordRequest._Value = local.Target;
            newDomainRecordRequest.Type = local.Type;
            var actionResult=DefaultAcsClient.DoAction(newDomainRecordRequest, ClientProfile);
            //var response3= DefaultAcsClient.GetAcsResponse(newDomainRecordRequest);

        }

        private void ValidateRecordData(DescribeDomainRecords_Record alibaba, HostConfig host)
        {
            if (alibaba.Type == host.Type && alibaba._Value == host.Target && alibaba.RR == host.RR)
            {
                Console.Write("Do nothing");
            }
            else
            {
                Console.WriteLine("update value");
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

        private DescribeDomainRecords_Record GetCurrentConfiguration(string domain, string host)
        {
            DescribeDomainRecords_Record x = null;
            try
            {
                var request = new DescribeDomainRecordsRequest();
                request.PageSize = 100;
                request.DomainName = domain;
                DefaultAcsClient.DoAction(request, ClientProfile);
                var response3 = DefaultAcsClient.GetAcsResponse(request);
                x = response3.DomainRecords.SingleOrDefault(d1 => d1.RR == host);
                return x;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }
            throw new Exception("Unknown exception");
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
    }
}
