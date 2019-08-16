using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace VehicleInfoLoader.Data
{
    internal sealed class LiveryCollection
    {
        [JsonProperty("amount")]
        public int Amount { get; internal set; }

        [JsonProperty("list")]
        private Dictionary<int, Livery> List { get; set; }

        public Livery GetLivery(int id)
        {
            if (HasLivery(id) == false)
            {
                return null;
            }

            return List[id];
        }
        
        public bool HasLiveries()
        {
            return List != null && List.Any();
        }
        
        public bool HasLivery(int id)
        {
            return List != null && List.ContainsKey(id);
        }

        public IEnumerable<Livery> GetLiveries()
        {
            if (HasLiveries() == false)
            {
                return Enumerable.Empty<Livery>();
            }

            return List.Values;
        }
        
        public IEnumerable<int> GetLiveryIds()
        {
            if (HasLiveries() == false)
            {
                return Enumerable.Empty<int>();
            }

            return List.Keys;
        }
    }
}
