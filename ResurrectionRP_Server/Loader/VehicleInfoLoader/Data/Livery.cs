using Newtonsoft.Json;

namespace VehicleInfoLoader.Data
{
    public class Livery
    {
        [JsonProperty("id")]
        public int Id { get; internal set; }
        
        [JsonProperty("name")]
        public string Name { get; internal set; }
        
        [JsonProperty("localizedName")]
        public string LocalizedName { get; internal set; }
    }
}
