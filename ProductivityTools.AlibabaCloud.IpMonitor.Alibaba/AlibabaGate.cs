﻿using Aliyun.Acs.Alidns.Model.V20150109;
using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProductivityTools.MasterConfiguration;
using static Aliyun.Acs.Alidns.Model.V20150109.DescribeDomainRecordsResponse;

namespace ProductivityTools.AlibabaCloud.IpMonitor.Alibaba
{
    public class AlibabaGate
    {
        private string Region
        {
            get
            {

                var r = string.Empty;
                return r;
            }
        }

        IClientProfile ClientProfile
        {
            get
            {

                IClientProfile clientProfile = DefaultProfile.GetProfile(
                    Region, // region ID
                    " ", //  AccessKey ID of RAM account
                    " "); // AccessKey Secret of RAM account

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

        private DescribeDomainRecords_Record GetCurrentConfiguration(string domain, string host)
        {
            DescribeDomainRecords_Record x = null;
            try
            {
                var d = new DescribeDomainRecordsRequest();
                d.DomainName = domain;
                DefaultAcsClient.DoAction(d, ClientProfile);
                var response3 = DefaultAcsClient.GetAcsResponse(d);
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
                //client.DoAction(requestdomain, clientProfile);
                var response2 = DefaultAcsClient.GetAcsResponse(requestdomain);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }
        }
    }
}
