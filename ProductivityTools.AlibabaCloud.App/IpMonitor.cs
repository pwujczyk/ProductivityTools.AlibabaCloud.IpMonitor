using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProductivityTools.AlibabaCloud.App
{
    public static class Ifconfig
    {
        public static string GetPublicIpAddress()
        {
            //var request = (HttpWebRequest)WebRequest.Create("http://ifconfig.me/ip");
            var request = (HttpWebRequest)WebRequest.Create("https://api.ipify.org");

            request.UserAgent = "curl"; // this will tell the server to return the information as if the request was made by the linux "curl" command

            string publicIPAddress;

            request.Method = "GET";
            using (WebResponse response = request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    publicIPAddress = reader.ReadToEnd();
                }
            }
            //return "128.0.0.1";
            return publicIPAddress.Replace("\n", "");
        }
    }
}
