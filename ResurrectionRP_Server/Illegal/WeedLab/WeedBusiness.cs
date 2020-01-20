using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using ResurrectionRP_Server.Colshape;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Entities.Blips;
using ResurrectionRP_Server.Illegal.WeedLab;
using ResurrectionRP_Server.Illegal.WeedLab.Data;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Streamer.Data;
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
        private Vector3 InventoryPos = new Vector3(1044.2505f, -3194.677f, -39.16992f);
        private Location InsideBat = new Location(new Vector3(1066.032f, -3183.42f, -39.1635f), new Vector3(0, 0, 85.58551f));
        private Timer Timer = new Timer(5000);

        public static Location[] LocationLab = new Location[]
        {
            new Location(new Vector3(904.022f,3560.044f,33.795654f), new Vector3(0f,0f,-0.1978956f)),
            new Location(new Vector3(1395.389f,3623.6177f,35.00879f), new Vector3(0f,0f,-0.24736951f)),
            new Location(new Vector3(2310.435f,4884.8174f,41.799316f), new Vector3(0f,0f,2.473695f)),
            new Location(new Vector3(-55.186813f,6392.5845f,31.487183f), new Vector3(0f,0f,-0.7915824f)),
            new Location(new Vector3(144.13187f,-131.20879f,54.82422f), new Vector3(0f,0f,-2.6715908f)),
            new Location(new Vector3(399.77142f,66.72527f,97.97656f), new Vector3(0f,0f,-2.7210646f)),
            new Location(new Vector3(1961.0637f,5184.8438f,47.932617f), new Vector3(0f,0f,-1.3357954f)),
        };

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
        #endregion

        #region Public Variables
        public Location LabEnter { get; set; }
        public bool AdvancedEquipement { get; set; } = false;

        public List<Drying> InDryings { get; set; } = new List<Drying>();
        [BsonIgnore, JsonIgnore]
        public IColshape InventoryColshape { get; private set; }

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
                LabEnter = LocationLab[Utils.Utils.RandomNumber(LocationLab.Length)];


            if (GameMode.IsDebug)
            {
                BlipsManager.CreateBlip("Labo-weed", LabEnter.Pos, BlipColor.Black, (int)BlipType.Gallery, 1, true);
            }

            DealerLocations = new Location[]
            {
                new Location(new Vector3(-112.82637f,-994.61536f,54.25122f), new Vector3(0f,0f,-2.7210646f)),
                new Location(new Vector3(-286.9978f,6181.3584f,31.487183f), new Vector3(0f,0f,-2.424221f)),
                new Location(new Vector3(1447.6615f,3749.3801f,31.925293f), new Vector3(0f,0f,-1.1873736f)),
                new Location(new Vector3(-8.3076935f,6487.5693f,31.504028f), new Vector3(0f,0f,2.7210646f)),
                new Location(new Vector3(1433.0637f,1499.5912f,113.76489f), new Vector3(0f,0f,-1.7315865f)),
                new Location(new Vector3(-864.8571f,-1095.9297f,2.1516113f), new Vector3(0f,0f,0.0989478f)),
                new Location(new Vector3(1471.2263f,6551.512f,14.013916f), new Vector3(0f,0f,0.7421085f)),
                new Location(new Vector3(1639.5824f,4879.4507f,42.13623f), new Vector3(0f,0f,-1.6821126f)),
                new Location(new Vector3(2327.4592f,2569.8594f,46.668823f), new Vector3(0f,0f,1.3357954f)),
                new Location(new Vector3(2818.4438f,-741.33624f,2.623413f), new Vector3(0f,0f,1.6326387f))

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
            InventoryColshape = ColshapeManager.CreateCylinderColshape(InventoryPos - new Vector3(0,0,1), 1f, 3f);
            InventoryColshape.OnPlayerInteractInColshape += ColshapeInteract;
            InventoryColshape.OnPlayerEnterColshape += OnEnterColshape;

            foreach (WeedZone weedzone in WeedZoneList)
            {
                weedzone.Textlabel = TextLabel.CreateTextLabel(LabelRefresh(weedzone), weedzone.Position + new Vector3(0, 0, 0.75f), System.Drawing.Color.White);
                weedzone.Colshape = ColshapeManager.CreateCylinderColshape(weedzone.Position, 1f, 1f);
                weedzone.Marker = Marker.CreateMarker(MarkerType.VerticalCylinder, weedzone.Position, new Vector3(1, 1, 0.2f), Color.FromArgb(80, 0, 100, 0));

                weedzone.OnGrowingChange += OnGrowingChange;
                weedzone.OnGrowingClientEnter += OnGrowingClientEnter;

                weedzone.Timer = new Timer(5000);
                weedzone.Timer.Elapsed += (sender, e) => weedzone.GrowLoop();
                weedzone.Timer.Start();

                weedzone.Colshape.OnPlayerInteractInColshape += ColshapeInteract;
                weedzone.Colshape.OnPlayerEnterColshape += OnEnterColshape;
            }

            if (LabEnter != null)
                MakeDoor(LabEnter);

            Marker.CreateMarker(MarkerType.VerticalCylinder, InsideBat.Pos - new Vector3(0, 0, 1), new Vector3(1, 1, 0.5f));
            var colShape = ColshapeManager.CreateCylinderColshape(InsideBat.Pos - new Vector3(0, 0, 1), 1.0f, 3f);
            colShape.OnPlayerEnterColshape += OnEnterColshape;
            colShape.OnPlayerInteractInColshape += OnPlayerInteractInColShapeOut;

            Timer.Elapsed += async (sender, e)
                => await DryingLoop();

            Timer.Start();

            base.Load();
        }

        private void OnPlayerInteractInColShapeOut(IColshape colShape, IPlayer client)
        {
            client.Position = LabEnter.Pos;
            client.Rotation = LabEnter.Rot;
        }

        private void OnEnterColshape(IColshape colShape, IPlayer client)
        {
            client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~ pour intéragir", 5000);
        }

        private void ColshapeInteract(IColshape colShape, IPlayer client)
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
            var colShape = ColshapeManager.CreateCylinderColshape(LabEnter.Pos - new Vector3(0, 0, 1), 1.0f, 3f);
            colShape.OnPlayerEnterColshape += OnEnterColshape;
            colShape.OnPlayerInteractInColshape += OnPlayerInteractInColShape;
        }

        private void OnPlayerInteractInColShape(IColshape colShape, IPlayer client)
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
                zone.Textlabel.Text = str;
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
                                Inventory.AddItem(LoadItem.GetItemWithID(ItemID.BOrange), 10);
                                break;
                            case SeedType.Purple:
                                Inventory.AddItem(LoadItem.GetItemWithID(ItemID.BPurple), 10);
                                break;
                            case SeedType.Skunk:
                                Inventory.AddItem(LoadItem.GetItemWithID(ItemID.BSkunk), 10);
                                break;
                            case SeedType.WhiteWidow:
                                Inventory.AddItem(LoadItem.GetItemWithID(ItemID.BWhiteWidow), 10);
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