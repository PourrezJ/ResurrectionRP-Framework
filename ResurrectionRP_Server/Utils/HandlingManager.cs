using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson.IO;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Entities.Vehicles.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Utils
{
    public static class HandlingManager
    {
        private static string _basePath = $"handlings{Path.DirectorySeparatorChar}";
        private static readonly ConcurrentDictionary<uint, Handling> _handlings = new ConcurrentDictionary<uint, Handling>();

        public static void LoadAllHandling()
        {
            if (!Directory.Exists("handlings"))
            {
                Alt.Server.LogWarning("Le dossier handlings est manquant.");
                return;
            }

            string[] files = Directory.GetFiles(MakePath(), "*.json");

            foreach (var file in files)
            {
                Get(Convert.ToString(Path.GetFileNameWithoutExtension(file)));
            }
        }

        public static Handling Get(string vehicle)
            => Get(uint.Parse(vehicle));


        public static Handling Get(uint vehicle)
        {
            try
            {
                if (_handlings.TryGetValue(vehicle, out var manifest))
                {
                    return manifest;
                }

                string path = MakePath(vehicle + ".json");
                if (!File.Exists(path))
                {
                    Alt.Server.LogWarning($"Could not find '{path}'");
                }

                var handling =  Newtonsoft.Json.JsonConvert.DeserializeObject<Handling>(File.ReadAllText(path));

                if (_handlings.TryAdd(vehicle, handling))
                    return handling;
                return null;
            }
            catch
            {
                Alt.Server.LogError("Erreur de manifest avec le véhicule: " + vehicle);
                return null;
            }

        }

        private static string MakePath(string relativePath = "")
        {
            return Path.GetFullPath(Path.Combine(_basePath, relativePath));
        }

        #region GetAllHandling
        public static void GetAllHandling(IPlayer player)
        {
            Alt.OnClient<IPlayer, string, string>("CallbackGetHandling", CallbackGetHandling);

            if (player.Vehicle == null)
            {
                player.SendNotificationError("Vous devez être dans un véhicule.");
                return;
            }

            if (!Directory.Exists("handlings"))
                Directory.CreateDirectory("handlings");

            Task.Run(async () =>
            {
                foreach (uint model in Enum.GetValues(typeof(VehicleModel)))
                {
                    Alt.Server.LogInfo("Récupération du handling du véhicule model: " + model);

                    player.EmitLocked("GetHandling", model);
                    await Task.Delay(150);
                }
            });
        }

        private static void CallbackGetHandling(IPlayer client, string model, string data)
        { 
            Handling handling = Newtonsoft.Json.JsonConvert.DeserializeObject<Handling>(data);

            File.WriteAllText(Directory.GetCurrentDirectory() + "//handlings//" + model.ToString() + ".json", Newtonsoft.Json.JsonConvert.SerializeObject(handling, Newtonsoft.Json.Formatting.Indented));
        }
        #endregion
    }
}
