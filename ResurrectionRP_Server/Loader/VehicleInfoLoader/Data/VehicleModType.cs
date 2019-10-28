using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace VehicleInfoLoader.Data
{
    public sealed class VehicleModType
    {
        [JsonProperty("amount")]
        public int Amount { get; internal set; }
        
        [JsonProperty("list")]
        private Dictionary<int, VehicleMod> List { get; set; }

        public IEnumerable<int> GetModIds()
        {
            return List == null ? Enumerable.Empty<int>() : List.Keys;
        }

        public IEnumerable<VehicleMod> GetMods()
        {
            return List == null ? Enumerable.Empty<VehicleMod>() : List.Values;
        }
        
        public IReadOnlyDictionary<int, VehicleMod> Mods()
        {
            return List ?? new Dictionary<int, VehicleMod>();
        }

        public VehicleMod Mod(int mod)
        {
            if (HasMod(mod) == false)
            {
                return null;
            }
            
            return List[mod];
        }

        public bool HasMod(int mod)
        {
            return List != null && List.ContainsKey(mod);
        }
    }
}
