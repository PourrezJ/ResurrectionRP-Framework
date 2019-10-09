using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Illegal.WeedLab;
using ResurrectionRP_Server.Illegal.WeedLab.Data;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Timers;

namespace ResurrectionRP_Server.Illegal
{
    public partial class WeedBusiness : IllegalSystem
    {
        #region Private Variables
        private Vector3 InventoryPos = new Vector3(1043.7627f, -3194.756f, -39.16992f);
        private Location InsideBat = new Location(new Vector3(1066.032f, -3183.42f, -39.1635f), new Vector3(0, 0, 85.58551f));
        private Timer Timer = new Timer(10000);
        #endregion

        #region Public Variables
        public Location LabEnter { get; set; }
        public bool AdvancedEquipement { get; set; } = false;

        public WeedZone[] WeedZoneList = new WeedZone[9]
        {
            new WeedZone(0, StateZone.Stage0 , SeedType.Aucune, new Vector3(1053.894f, -3196.052f, -40.16129f)),
            new WeedZone(1, StateZone.Stage0 , SeedType.Aucune, new Vector3(1053.783f, -3189.879f, -40.16138f)),
            new WeedZone(2, StateZone.Stage0 , SeedType.Aucune, new Vector3(1056.23f, -3192.55f, -40.16134f)),
            new WeedZone(3, StateZone.Stage0 , SeedType.Aucune, new Vector3(1060.228f, -3193.428f, -40.16134f)),
            new WeedZone(4, StateZone.Stage0 , SeedType.Aucune, new Vector3(1060.289f, -3198.381f, -40.16125f)),
            new WeedZone(5, StateZone.Stage0 , SeedType.Aucune, new Vector3(1060.687f, -3203.671f, -40.16113f)),
            new WeedZone(6, StateZone.Stage0 , SeedType.Aucune, new Vector3(1057.476f, -3203.444f, -40.1539f)),
            new WeedZone(7, StateZone.Stage0 , SeedType.Aucune, new Vector3(1057.831f, -3196.909f, -40.12994f)),
            new WeedZone(8, StateZone.Stage0 , SeedType.Aucune, new Vector3(1051.53f, -3201.792f, -40.11644f))
        };

        public List<Drying> InDryings { get; set; } = new List<Drying>();
        [BsonIgnore, JsonIgnore]
        public IColShape InventoryColshape { get; private set; }

        [BsonIgnore, JsonIgnore]
        public List<IPlayer> PlayersInside = new List<IPlayer>();
        #endregion

        #region C4tor
        public WeedBusiness()
        {
            Enabled = true;
            Inventory = new Inventory.Inventory(1000, 40);
        }
        #endregion

        #region Load
        public override void Load()
        {
            DealerPedHash = PedModel.Hippie01;

            if (LabEnter == null)
                LabEnter = new Location(new Vector3(), new Vector3());

            DealerLocations = new Location[]
            {
                new Location(new Vector3(-1121.171f, 2712.382f, 18.86371f), new Vector3(0, 0, 33.02702f)),
                new Location(new Vector3(1525.506f, 1709.852f, 110.0081f), new Vector3(0, 0, 350.3264f)),
                new Location(new Vector3(2484.941f, 3718.431f, 43.4684f), new Vector3(0, 0, 224.6393f)),
                new Location(new Vector3(86.1665f, 4562.606f, 90.50889f), new Vector3(0, 0, 49.5118f)),
                new Location(new Vector3(-504.0669f, -1632.734f, 17.7978f), new Vector3(0, 0, 248.5665f)),
                new Location(new Vector3(-492.0018f, -1029.593f, 52.47616f), new Vector3(0, 0, 0.9144158f)),
                new Location(new Vector3(28.57924f, -637.2659f, 7.508431f), new Vector3(0, 0, 5.610692f))
            };

            IllegalPrice = new Dictionary<ItemID, double>()
            {
                { ItemID.BOrange, 600 },
                { ItemID.BSkunk, 600 },
                { ItemID.BWhiteWidow, 600 },
                { ItemID.BPurple, 600 }
            };

            if (Inventory == null)
                Inventory = new Inventory.Inventory(1000, 40);

            Marker.CreateMarker(MarkerType.VerticalCylinder, InventoryPos, new Vector3(1, 1, 0.1f));
            InventoryColshape = Alt.CreateColShapeCylinder(InventoryPos - new Vector3(0,0,1), 1f, 3f);
            InventoryColshape.SetOnPlayerInteractInColShape(ColshapeInteract);
            InventoryColshape.SetOnPlayerEnterColShape(OnEnterColshape);

            foreach (WeedZone weedzone in WeedZoneList)
            {
                weedzone.Textlabel = Streamer.Streamer.AddEntityTextLabel(LabelRefresh(weedzone), weedzone.Position + new Vector3(0, 0, 0.75f));
                weedzone.Colshape = Alt.CreateColShapeCylinder(weedzone.Position, 1f, 1f);
                weedzone.Marker = Marker.CreateMarker(MarkerType.VerticalCylinder, weedzone.Position, new Vector3(1, 1, 0.2f), Color.FromArgb(160, 0, 100, 0));

                weedzone.OnGrowingChange += OnGrowingChange;
                weedzone.OnGrowingClientEnter += OnGrowingClientEnter;

                weedzone.Timer = Utils.Utils.SetInterval(() => weedzone.GrowLoop(), 5000);

                weedzone.Colshape.SetOnPlayerInteractInColShape(ColshapeInteract);
                weedzone.Colshape.SetOnPlayerEnterColShape(OnEnterColshape);
            }

            if (LabEnter != null)
                MakeDoor(LabEnter);

            Marker.CreateMarker(MarkerType.VerticalCylinder, InsideBat.Pos - new Vector3(0, 0, 1), new Vector3(1, 1, 0.5f));
            var colShape = Alt.CreateColShapeCylinder(InsideBat.Pos - new Vector3(0, 0, 1), 1.0f, 3f);
            colShape.SetOnPlayerEnterColShape(OnEnterColshape);
            colShape.SetOnPlayerInteractInColShape(OnPlayerInteractInColShapeOut);

            Timer.Elapsed += async (sender, e)
                => await DryingLoop();

            Timer.Start();

            base.Load();
        }

        private void OnPlayerInteractInColShapeOut(IColShape colShape, IPlayer client)
        {
            client.Position = LabEnter.Pos;
            client.Rotation = LabEnter.Rot;
        }

        private void OnEnterColshape(IColShape colShape, IPlayer client)
        {
            client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~ pour intéragir", 5000);
        }

        private void ColshapeInteract(IColShape colShape, IPlayer client)
        {
            if (colShape == InventoryColshape)
            {
                Menu menu = new Menu("ID_WeedMenu", "Weed Labs", $"Plante en cours de séchage: {InDryings.Count}", backCloseMenu: true);
                menu.ItemSelectCallback = OnMenuCallBack;
                menu.Add(new MenuItem("Ouvrir le stockage", "Récolte de weed", "OpenInventory", true));
                menu.OpenMenu(client);
            }
            else
            {
                foreach (WeedZone weedzone in WeedZoneList)
                {
                    if (weedzone.Colshape == colShape)
                    {
                        weedzone.OnGrowingZoneEnter(colShape, client);
                        return;
                    }
                }
            }
        }
        #endregion

        #region Events

        public void MakeDoor(Location labenter)
        {
            if (LabEnter != labenter)
            {
                LabEnter = labenter;
                Task.Run(async () => await Update());
            }

            Marker.CreateMarker(MarkerType.VerticalCylinder, LabEnter.Pos - new Vector3(0, 0, 1), new Vector3(1, 1, 0.2f));
            var colShape = Alt.CreateColShapeCylinder(LabEnter.Pos - new Vector3(0, 0, 1), 1.0f, 3f);
            colShape.SetOnPlayerEnterColShape(OnEnterColshape);
            colShape.SetOnPlayerInteractInColShape(OnPlayerInteractInColShape);
        }

        private void OnPlayerInteractInColShape(IColShape colShape, IPlayer client)
        {
            client.Position = InsideBat.Pos;
            client.Rotation = InsideBat.Rot;
            PlayersInside.Add(client);

            client.Emit("Weedlabs_Enter", JsonConvert.SerializeObject(WeedZoneList), AdvancedEquipement);
        }

        private void OnGrowingChange(WeedZone zone, bool changeGrowingState)
        {
            lock (zone)
            {
                if (zone.Advert >= 15)
                {
                    zone.GrowingState = StateZone.Stage0;
                    zone.SeedUsed = SeedType.Aucune;
                    zone.Hydratation = 0;
                    zone.Plant = false;
                }
                else if (zone.GrowingState < StateZone.Stage3 && zone.Hydratation > 0 && zone.Spray == Spray.Off)
                    zone.Hydratation -= 5;

                LabelRefresh(zone);

                if (changeGrowingState && zone.GrowingState != 0)
                    RefreshClientInLabs(zone);
            }
        }

        private void OnGrowingClientEnter(IPlayer client, WeedZone zone)
        {
            OpenMenuGrow(client, zone);
        }
        #endregion

        #region Methods
        public static string LabelRefresh(WeedZone zone)
        {
            string croissanceSTR = "";
            switch (zone.GrowingState)
            {
                case StateZone.Stage0:
                    croissanceSTR = "Germination";
                    break;

                case StateZone.Stage1:
                    croissanceSTR = "Croissance";
                    break;

                case StateZone.Stage2:
                    croissanceSTR = "Floraison";
                    break;

                case StateZone.Stage3:
                    croissanceSTR = "Recolte";
                    break;
            }

            string str = $"Zone: {(zone.ID + 1)} \n" +
            $"Variete: {zone.SeedUsed} \n" +
            $"Taux d'humidite: {((zone.Spray == Spray.On) ? "Automatique" : zone.Hydratation.ToString() + "%")} \n" +
            $"Etape: {(zone.Plant ? croissanceSTR : "Aucune")}";

            if (zone.Textlabel != null)
                zone.Textlabel.text = str;
            return str;
        }

        public void RefreshClientInLabs(WeedZone zone)
        {
            foreach(var client in this.PlayersInside.ToList())
            {
                if (client.Exists)
                    client.Emit("Weedlabs_Update", JsonConvert.SerializeObject(zone));
                else
                    PlayersInside.Remove(client);
            }
        }

        public void Recolte(WeedZone zone)
        {
            SeedType weedtype = zone.SeedUsed;
            zone.GrowingState = StateZone.Stage0;
            zone.Hydratation = 0;
            zone.Plant = false;
            InDryings.Add(new Drying(weedtype));
            RefreshClientInLabs(zone);
        }

        private async Task DryingLoop()
        {
            if (InDryings.Count > 0)
            {
                foreach (Drying dry in InDryings.ToList())
                {
                    if (DateTime.Now >= dry.FinalDateTime)
                    {
                        switch (dry.WeedType)
                        {
                            case SeedType.Orange:
                                Inventory.AddItem(ResurrectionRP_Server.Inventory.Inventory.ItemByID(ItemID.BOrange), 10);
                                break;
                            case SeedType.Purple:
                                Inventory.AddItem(ResurrectionRP_Server.Inventory.Inventory.ItemByID(ItemID.BPurple), 10);
                                break;
                            case SeedType.Skunk:
                                Inventory.AddItem(ResurrectionRP_Server.Inventory.Inventory.ItemByID(ItemID.BSkunk), 10);
                                break;
                            case SeedType.WhiteWidow:
                                Inventory.AddItem(ResurrectionRP_Server.Inventory.Inventory.ItemByID(ItemID.BWhiteWidow), 10);
                                break;
                        }
                        InDryings.Remove(dry);
                        await Update();
                    }
                }
            }
        }
        #endregion

   
    }
}