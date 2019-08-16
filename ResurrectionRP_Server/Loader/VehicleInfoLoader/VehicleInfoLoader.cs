using AltV.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using VehicleInfoLoader.Data;

namespace VehicleInfoLoader
{
    public sealed class VehicleInfoLoader
    {
        private static string _basePath = $"vehicleinfo{Path.DirectorySeparatorChar}";
        private static bool _cache = true;
        private static readonly ConcurrentDictionary<uint, VehicleManifest> _vehicles = new ConcurrentDictionary<uint, VehicleManifest>();
        
        public static VehicleManifest Get(string vehicle)
        {

            try
            {
                string path = MakePath(vehicle + ".json");
                if (!File.Exists(path))
                {
                    Alt.Server.LogWarning($"Could not find '{path}'");
                }

                var vehicleManifest = JsonConvert.DeserializeObject<VehicleManifest>(File.ReadAllText(path));

                if (_cache && _vehicles.TryAdd((uint)vehicleManifest.Hash, vehicleManifest) == false)
                {
                    return null;
                }

                return vehicleManifest;
            }
            catch
            {
                Alt.Server.LogError("Erreur de manifest avec le véhicule: " + vehicle);
                return null;
            }
        }

        public static VehicleManifest Get(uint vehicle)
        {
            try
            {
                if (_cache && _vehicles.TryGetValue(vehicle, out var manifest))
                {
                    return manifest;
                }

                string path = MakePath(vehicle + ".json");
                if (!File.Exists(path))
                {
                    Alt.Server.LogWarning($"Could not find '{path}'");
                }

                var vehicleManifest = JsonConvert.DeserializeObject<VehicleManifest>(File.ReadAllText(path));

                if (_cache && _vehicles.TryAdd((uint)vehicleManifest.Hash, vehicleManifest) == false)
                {
                    return null;
                }

                return vehicleManifest;
            }
            catch
            {
                Alt.Server.LogError("Erreur de manifest avec le véhicule: " + vehicle);
                return null;
            }

        }

        public static async Task<VehicleManifest> GetAsync(string vehicle)
        {
            return await Task.Run(() => Get(vehicle));
        }

        public static async Task<VehicleManifest> GetAsync(uint vehicle)
        {
            return await Task.Run(() => Get(vehicle));
        }

        public static void Remove(VehicleManifest manifest)
        {
            if (manifest == null)
            {
                throw new ArgumentNullException(nameof(manifest));
            }
            
            Remove((uint) manifest.Hash);
        } 

        public static void Remove(uint vehicle)
        {
            _vehicles.TryRemove(vehicle, out _);
        } 

        public static void Clear()
        {
            _vehicles.Clear();
        }

        public static async Task LoadAllManifests()
        {
            string[] files = Directory.GetFiles(MakePath(), "*.json");
            
            foreach (var file in files)
            {
                await GetAsync(Convert.ToString(Path.GetFileNameWithoutExtension(file)));
            }
        }

        public static void Setup(string path, bool cache=true)
        {
            _basePath = path;
            _cache = cache;
        }

        private static string MakePath(string relativePath = "")
        {
            return Path.GetFullPath(Path.Combine(_basePath, relativePath));
        }

    }
}
