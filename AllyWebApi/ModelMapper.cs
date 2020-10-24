using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AllyWebApi
{
    public class ModelMapper
    {
        [JsonProperty("DBFIELD")]
        public string Dbfield;
        [JsonProperty("PROPERTYNAME")]
        public string PropertyName;
    }
}
