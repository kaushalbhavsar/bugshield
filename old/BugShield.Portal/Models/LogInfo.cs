using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugShield.Portal.Models
{
    public  class LogInfo
    {
        public string id { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public Nullable<System.DateTime> Time { get; set; }
        public string SourceIp { get; set; }
        public string SourcePort { get; set; }
        public string DestIp { get; set; }
        public string DestPort { get; set; }
        public string Method { get; set; }
        public string Url { get; set; }
        public string Host { get; set; }
        public string UserAgent { get; set; }
        public string PostData { get; set; }
        public string ResponseCode { get; set; }
        public Nullable<int> AttackId { get; set; }


        public string Country { get; set; }
        public string CountryCode { get; set; }


        public virtual BugDB BugDB { get; set; }
    }
}