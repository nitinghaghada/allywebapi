using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace BusinessModel.Models
{
    public class Entity
    {
      [JsonProperty("count")]
      public int Count { get; set; }
      [JsonProperty("items")]
    public IList<Dictionary<string, object>> Items { get; set; }
    [JsonProperty("links")]
      public List<Link> Links { get; set; }
  }
  public class Link
  {
    [JsonProperty("rel")]
    public string Rel { get; set; }
    [JsonProperty("href")]
    public string HRef { get; set; }
  }
}
