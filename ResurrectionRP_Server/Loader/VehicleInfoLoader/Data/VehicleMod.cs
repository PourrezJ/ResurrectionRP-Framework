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

        public VehicleMod(string name, string localizedName, string[] flags)
        {
            Name = name;
            LocalizedName = localizedName;
            Flags = flags;
        }

        public bool HasFlag(string flag)
        {
            return Flags != null && Flags.Contains(flag);
        }
    }
}
