using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductivityTools.AlibabaCloud.IpMonitor.Alibaba
{
    public class HostConfig
    {
        /// <summary>
        /// Relative Record. The subdomain part of the DNS record (e.g., "www", "jenkins").
        /// </summary>
        public string RR { get; set; }

        /// <summary>
        /// DNS Record Type (e.g., "A", "CNAME", "AAAA").
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The value the DNS record should point to if <see cref="MapToExternal"/> is false.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// If true, the record value will be automatically updated with the current public IP of the server.
        /// If false, the value from the <see cref="Target"/> property will be used.
        /// </summary>
        public bool  MapToExternal { get; set; }
    }
}
