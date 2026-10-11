using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugShield.Portal.Models
{
    [JsonObject(MemberSerialization.OptIn)]

    public class AttackByHost
    {
        [JsonProperty]
        public string label { get; set; }

        [JsonProperty]
        public int data { get; set; }
        //[JsonProperty]
        //public int count { get; set; }
    }
}