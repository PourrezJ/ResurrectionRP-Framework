using System.Collections.Generic;
using System.Linq;

namespace VehicleInfoLoader.Data
{
    public sealed partial class VehicleManifest
    {
        public IEnumerable<int> ModTypes
        {
            get
            {
                return ModList?.Keys ?? Enumerable.Empty<int>();
            }
        }

        public IEnumerable<int> LiveryIds
        {
            get
            {
                return HasLiveries() == false ? Enumerable.Empty<int>() : LiveryList.GetLiveryIds();
            }
        }

        public IEnumerable<Livery> Liveries
        {
            get
            {
                return HasLiveries() == false ? Enumerable.Empty<Livery>() : LiveryList.GetLiveries();
            }
        }

        public int LiveryCount
        {
            get
            {
                return LiveryList == null ? 0 : LiveryList.Amount;
            }
        }

        public bool HasLiveries()
        {
            return LiveryList != null && LiveryList.HasLiveries();
        }
        
        public bool HasMods()
        {
            return ModList != null && ModList.Any();
        }
        
        public bool HasMod(int type, int mod)
        {
            return HasModType(type) && GetModType(type).HasMod(mod);
        }

        public bool HasModType(int type)
        {
            return ModList != null && ModList.ContainsKey(type);
        }

        public VehicleMod GetMod(int type, int mod)
        {
            if (HasMod(type, mod) == false)
            {
                return null;
            }

            return GetModType(type).Mod(mod);
        }

        public VehicleModType GetModType(int type)
        {
            if (HasModType(type) == false)
            {
                return null;
            }

            return ModList[type];
        }

        public IEnumerable<int> GetModIds(int type)
        {
            var modType = GetModType(type);
            if (modType == null)
            {
                return Enumerable.Empty<int>();
            }

            return modType.GetModIds();
        }

        public IEnumerable<VehicleMod> Mods(int type)
        {
            var modType = GetModType(type);
            if (modType == null)
            {
                return Enumerable.Empty<VehicleMod>();
            }

            return modType.GetMods();
        }

        public Dictionary<int, Dictionary<int, string>> ValidMods()
        {
            return ModList.ToDictionary(m => m.Key, m => m.Value.Mods().ToDictionary(t => t.Key, t => t.Value.Name));
        }

        public bool HasLivery(int id)
        {
            return LiveryList != null && LiveryList.HasLivery(id);
        }

        public Livery Livery(int id)
        {
            if (LiveryList == null)
            {
                return null;
            }

            return LiveryList.GetLivery(id);
        }

        public bool HasBone(int boneIndex)
        {
            return Bones != null && Bones.Values.Any(b => b == boneIndex);
        }

        public bool HasBone(string boneName)
        {
            return Bones != null && Bones.ContainsKey(boneName);
        }

        public IEnumerable<string> GetBoneNames()
        {
            return Bones.Select(s => s.Key);
        }

        public IEnumerable<int> GetBoneIndexes()
        {
            return Bones.Select(s => s.Value);
        }
    }
}
