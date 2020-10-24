namespace BusinessModel.Models
{
    public class Check
    {
        public string Description { get; set; }
        public string key { get; set; }
        //[JsonProperty("bol3")]
        public bool Automatic { get; set; }
        //[JsonProperty("bol1")]
        public bool Optional { get; set; }
        //[JsonProperty("text7")]
        public bool checkd { get; set; }
    }
}
