using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessModel.Models
{
    public class Task
    {
        //[JsonProperty("key")]
        public string ItemKey { get; set; }
        //[JsonProperty("text7")]
        public string ActionCode { get; set; }
        //[JsonProperty("text3")]
        public string Handler { get; set; }
        //[JsonProperty("date3")]
        public DateTime Duedate { get; set; }
        //[JsonProperty("mark")]
        public string Status { get; set; }
        //[JsonProperty("date2")]
        public DateTime HandleDate { get; set; }
        //[JsonProperty("date1")]
        public DateTime HandleBefore { get; set; }
        //[JsonProperty("text9")]
        public string Registration { get; set; }
        //[JsonProperty("processed")]
        public string Outcome { get; set; }
        //[JsonProperty("text4")]
        public string ActivityType { get; set; }
        //[JsonProperty("Checklist")]
        public Check[] Checklist { get; set; }
        //[JsonProperty("parentKey")]
        public string MainItemKey { get; set; }
        public string MainItemType { get; set; }
        public string ReferenceNumber { get; set; }
        public string Progress { get; set; }
        public int RemainingDays { get; set; }
  }

    public class Tasks
    {
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("tasks")]
        public List<Task> TaskItems { get; set; }
    }

}

