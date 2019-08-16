using Newtonsoft.Json;
using VehicleInfoLoader.Models;

namespace VehicleInfoLoader.Data
{
    public class VehicleDimensions
    {
        [JsonProperty("min")]
        public Vector Min { get; internal set; }
        
        [JsonProperty("max")]
        public Vector Max { get; internal set; }
    }
}
