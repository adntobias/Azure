using System;
using Newtonsoft.Json;

namespace APICall.Models
{
    public class Quote
    {
        [JsonProperty(PropertyName="_id")]
        public string Id { get; set; }
        public string content {get; set;}
        public string author {get; set;}
       
    }
}
