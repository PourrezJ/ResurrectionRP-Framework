using System.Linq;
using Newtonsoft.Json;

namespace VehicleInfoLoader.Data
{
    public sealed class VehicleMod
    {
        [JsonProperty("name")]
        public string Name             { get; internal set; }
        
        [JsonProperty("localizedName")]
        public string LocalizedName    { get; internal set; }
        
        [JsonProperty("flags")]
        public string[] Flags          { get; internal set; }

        public bool HasFlag(string flag)
        {
            return Flags != null && Flags.Contains(flag);
        }
    }
}
