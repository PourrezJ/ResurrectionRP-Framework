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

        public Teleport()
        {
            lock (TeleportManager.Teleports)
            {
                ID = TeleportManager.Teleports.Count + 1;
            }
        }

        public static Teleport CreateTeleport(Location entree, List<TeleportEtage> sorti, Vector3 scale, bool vehicleAllowed = false, byte opacite = 128, short dimensionIN = 0, short dimensionOUT = 0, string menutitle = "Ouvrir la porte", bool iswhitelisted = false, List<string> whitelist = null, bool hide = false)
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

            IColshape enterColshape = ColshapeManager.CreateCylinderColshape(entree.Pos - new Vector3(0,0,1), scale.X, 3f);
            Marker.CreateMarker(MarkerType.VerticalCylinder, entree.Pos - new Vector3(0.0f, 0.0f, 1f), scale, Color.FromArgb(opacite, 255, 255, 255));
            if (!hide) 
                TextLabel.CreateTextLabel("~o~Appuyez sur ~w~E \n ~o~pour intéragir", entree.Pos, Color.White);
            enterColshape.SetData("Teleport", JsonConvert.SerializeObject(new
            {
                teleport.ID,
                State = TeleportState.Enter
            }));

            // Multiple Sorti
            foreach (var sortipos in sorti)
            {
                IColshape sortiColshape = ColshapeManager.CreateCylinderColshape(sortipos.Location.Pos - new Vector3(0, 0, 1), scale.X, 3f);
                Marker.CreateMarker(MarkerType.VerticalCylinder, sortipos.Location.Pos - new Vector3(0, 0, 1f), scale, Color.FromArgb(opacite, 255, 255, 255));
                if (!hide) 
                    TextLabel.CreateTextLabel("~o~Appuyez sur ~w~E \n ~o~pour intéragir", sortipos.Location.Pos, Color.White);
                sortiColshape.SetData("Teleport", JsonConvert.SerializeObject( new
                {
                    teleport.ID,
                    State = TeleportState.Out
                }) );
            }

            TeleportManager.Teleports.Add(teleport);

            return teleport;
        }
    }
}
