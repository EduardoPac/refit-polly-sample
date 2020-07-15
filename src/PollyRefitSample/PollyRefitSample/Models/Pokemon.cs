using Newtonsoft.Json;

namespace PollyRefitSample.Models
{
    public class Pokemon
    {
        [JsonProperty("id")] 
        public long Id { get; set; }

        [JsonProperty("name")] 
        public string Name { get; set; }

        [JsonProperty("height")] 
        public long Height { get; set; }

        [JsonProperty("sprites")] 
        public Sprites Sprites { get; set; }

        public byte[] Image { get; set; }
    }
}