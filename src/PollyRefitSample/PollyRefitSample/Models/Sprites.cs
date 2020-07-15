using System;
using Newtonsoft.Json;

namespace PollyRefitSample.Models
{
    public class Sprites
    {
        [JsonProperty("front_default")] 
        public Uri FrontDefault { get; set; }
    }
}