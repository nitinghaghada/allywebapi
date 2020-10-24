using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BusinessModel.Models
{
    public class Checklist
    {
      [JsonProperty("description")]
      public string description { get; set; }
      [JsonProperty("checked")]
      public bool Checked { get; set; }
      [JsonProperty("key")]
      public string key { get; set; }
      [JsonProperty("optional")]
      public bool optional { get; set; }
      [JsonProperty("automatic")]
      public bool automatic { get; set; }
  }
}
