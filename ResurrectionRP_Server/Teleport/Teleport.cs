using AltV.Net;
using AltV.Net.Elements.Entities;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using ResurrectionRP_Server.Colshape;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Entities;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace ResurrectionRP_Server.Teleport
{
    public struct TeleportEtage
    {
        public string Name;
        public Location Location;
    }

    [BsonIgnoreExtraElements]
    public class Teleport
    {
        public delegate void OnTeleport(IPlayer client, TeleportState state);

        [JsonIgnore]
        public OnTeleport OnTeleportEvent { get; set; }

        public int ID { get; private set; }
        public bool VehicleAllowed { get; set; }
        public Location Entree { get; set; }
        public List<TeleportEtage> Sortie { get; set; }
        public string MenuTitle { get; set; }
        public bool IsWhitelisted { get; set; }

        private List<string> whitelist = new List<string>();
        public List<string> Whileliste
        {
            get
            {
                if (whitelist == null)
                    whitelist = new List<string>();
                return whitelist;
            }
            set => whitelist = value;
        }

        //public Teleport(Location Entree, Location Sorti, float Scale = 1f, bool VehicleAllowed = false, int opacite = 128, uint dimensionIN = 0, uint dimensionOUT = 0, string menutitle = "Ouvrir la porte", bool iswhitelisted = false, List<string> whitelist = null)
        public Teleport()
        {
            ID = GameMode.Instance.TeleportManager.Teleports.Count + 1;
        }

        public static Teleport CreateTeleport(Location entree, List<TeleportEtage> sorti, float scale = 1f, bool vehicleAllowed = false, byte opacite = 128, uint dimensionIN = 0, uint dimensionOUT = 0, string menutitle = "Ouvrir la porte", bool iswhitelisted = false, List<string> whitelist = null)
        {
            var teleport = new Teleport()
            {
                Entree = entree,
                Sortie = sorti,
                VehicleAllowed = vehicleAllowed,
                IsWhitelisted = iswhitelisted,
                MenuTitle = menutitle,
                Whileliste = whitelist
            };

            if (iswhitelisted && whitelist == null)
            {
                teleport.Whileliste = new List<string>();
            }

            // Entrée

            IColshape enterColshape = ColshapeManager.CreateCylinderColshape(entree.Pos - new Vector3(0,0,1), scale, 3f);
            Marker.CreateMarker(MarkerType.VerticalCylinder, entree.Pos - new Vector3(0.0f, 0.0f, 1f), new Vector3(1, 1, 1));
            GameMode.Instance.Streamer.AddEntityTextLabel("~o~Appuyez sur ~w~E \n ~o~pour intéragir", entree.Pos, 1, 255, 255, 255, 255, 10);
            enterColshape.SetData("Teleport", JsonConvert.SerializeObject(new
            {
                teleport.ID,
                State = TeleportState.Enter
            }));

            // Multiple Sorti
            foreach (var sortipos in sorti)
            {
                IColshape sortiColshape = ColshapeManager.CreateCylinderColshape(sortipos.Location.Pos - new Vector3(0, 0, 1), scale, 3f);
                Marker.CreateMarker(MarkerType.VerticalCylinder, sortipos.Location.Pos - new Vector3(0, 0, 1f), new Vector3(1, 1, 1), Color.FromArgb(opacite, 255, 255, 255));
                GameMode.Instance.Streamer.AddEntityTextLabel("~o~Appuyez sur ~w~E \n ~o~pour intéragir", sortipos.Location.Pos, 1, 255, 255, 255, 255, 10);
                sortiColshape.SetData("Teleport", JsonConvert.SerializeObject( new
                {
                    teleport.ID,
                    State = TeleportState.Out
                }) );
            }

            GameMode.Instance.TeleportManager.Teleports.Add(teleport);

            return teleport;
        }
    }
}
