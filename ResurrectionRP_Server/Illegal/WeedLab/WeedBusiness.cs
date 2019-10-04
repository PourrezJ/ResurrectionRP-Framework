using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Illegal.WeedLab;
using ResurrectionRP_Server.Inventory;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.XMenuManager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Timers;

namespace ResurrectionRP_Server.Illegal
{
    public class Drying
    {
        public SeedType WeedType = SeedType.Aucune;
        public DateTime FinalDateTime { private set; get; } = DateTime.Now.AddMinutes(10);
        public Drying(SeedType weedtype) { WeedType = weedtype; }
    }

    public class WeedBusiness : IllegalSystem
    {
        #region Static Variables
        public static List<WeedBusiness> WeedlabsList = new List<WeedBusiness>();
        #endregion

        #region Private Variables
        private Vector3 Position = new Vector3(1744.833f, -1600.628f, 112.639f);
        private Vector3 InventoryPos = new Vector3(1758.7f, -1616.392f, 113.6451f);
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
        [BsonIgnore]
        public IColShape InventoryColshape { get; private set; }
        #endregion

        #region C4tor
        public WeedBusiness()
        {
            Enabled = true;
            Inventory = new Inventory.Inventory(1000, 40);
        }
        #endregion

        #region Load
        public override async Task Load()
        {
            DealerPedHash = PedModel.Hippie01;

            if (LabEnter == null)
                Alt.Server.LogError("Aucune entrée pour le laboratoire de weed! Utilisez la commande /createweedlabs");

            

            foreach (WeedZone weedzone in WeedZoneList)
            {
                Marker.CreateMarker(MarkerType.VerticalCylinder, weedzone.Position,  new Vector3(1,1,1), Color.FromArgb(80, 0, 160, 0), GameMode.GlobalDimension);
                weedzone.Textlabel = GameMode.Instance.Streamer.AddEntityTextLabel(LabelRefresh(weedzone), weedzone.Position + new Vector3(0, 0, 0.25f));
                weedzone.Colshape = Alt.CreateColShapeCylinder(weedzone.Position, 1f, 1f);
                weedzone.Marker = Marker.CreateMarker(MarkerType.VerticalCylinder, weedzone.Position - new Vector3(0, 0, 1));

                weedzone.OnGrowingChange += OnGrowingChange;
                weedzone.OnGrowingClientEnter += OnGrowingClientEnter;

                weedzone.Timer = Utils.Utils.SetInterval(() => weedzone.GrowLoop(), 5000);
            }

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

            InventoryColshape = Alt.CreateColShapeCylinder(InventoryPos, 1f, 1f);
            Marker.CreateMarker(MarkerType.VerticalCylinder, InventoryPos - new Vector3(0, 0, 1));

            Timer.Elapsed += async (sender, e)
                => await DryingLoop();

            Timer.Start();
            WeedlabsList.Add(this);

           // Alt.OnColShape += Alt_OnColShape;

            await base.Load();
        }

        private void Alt_OnColShape(IColShape colShape, IEntity targetEntity, bool state)
        {
            if (!state)
                return;



            var client = (IPlayer)targetEntity;

            if (client == null)
                return;

            if (InventoryColshape == colShape)
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
                    weedzone.OnGrowingZoneEnter(colShape, client);
                }
            }
        }
        #endregion

        #region Events
        private void OnGrowingChange(WeedZone zone, bool changeGrowingState)
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
            Alt.EmitAllClients("Weedlabs_Update", JsonConvert.SerializeObject(Position), JsonConvert.SerializeObject(zone));
        }

        public void Recolte(WeedZone zone)
        {
            SeedType weedtype = zone.SeedUsed;
            zone.GrowingState = StateZone.Stage0;
            zone.Hydratation = 0;
            zone.Plant = false;
            InDryings.Add(new Drying(weedtype));
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

        #region Menu
        public void OpenMenuGrow(IPlayer client, WeedZone zone)
        {
            XMenu xmenu = new XMenu("ID_Weed");
            xmenu.SetData("Zone", zone);
            xmenu.Callback = GrowZoneMenuCallback;

            var ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            if (zone.GrowingState == 0 && !zone.Plant)
            {
                if (GameMode.Instance.FactionManager.Lspd.ServicePlayerList.Count < 2)
                {
                    client.SendNotificationError("[HRP] Pas assez de miliciens de présent sur le serveur.");
                    return;
                }

                if (!ph.HasItemID(ItemID.GSkunk) && !ph.HasItemID(ItemID.GSkunk) && ph.HasItemID(ItemID.GSkunk) && ph.HasItemID(ItemID.GSkunk))
                {
                    client.SendNotificationError("Vous n'avez pas de graine sur vous.");
                    return;
                }

                if (ph.HasItemID(ItemID.GSkunk))
                {
                    xmenu.Add(new XMenuItem("Planter Skunk", "Planter vos graines de Skunk", "ID_SeedSkunk", XMenuItemIcons.SEEDLING_SOLID));
                }

                if (ph.HasItemID(ItemID.GPurple))
                {
                    xmenu.Add(new XMenuItem("Planter Purple", "Planter vos graines de Purple", "ID_SeedPurple", XMenuItemIcons.SEEDLING_SOLID));
                }

                if (ph.HasItemID(ItemID.GOrange))
                {
                    xmenu.Add(new XMenuItem("Planter OrangeBud", "Planter vos graines d'orange bud", "ID_SeedOrange", XMenuItemIcons.SEEDLING_SOLID));
                }

                if (ph.HasItemID(ItemID.GWhite))
                {
                    xmenu.Add(new XMenuItem("Planter White Widow", "Planter vos graines de White Widow", "ID_SeedWhite", XMenuItemIcons.SEEDLING_SOLID));
                }
            }

            if (ph.HasItemID(ItemID.Hydro) && zone.Spray == Spray.Off)
            {
                xmenu.Add(new XMenuItem("Installer l'hydroponie", "Relier les pots au système d'hydroponie", "ID_Hydro", XMenuItemIcons.BRANDING_WATERMARK));
            }

            if (zone.Plant)
            {
                xmenu.Add(new XMenuItem("Arosser", "Arroser les pieds fait monter l'hydratation", "ID_Tint", XMenuItemIcons.TINT_SOLID));
            }

            if (zone.GrowingState == StateZone.Stage3 && ph.HasItemID(ItemID.Secateur))
            {
                xmenu.Add(new XMenuItem("Récolter", $"Récolter vos pieds de {zone.SeedUsed.ToString()} et les mettre a sécher (10minutes).", "ID_Recolte", XMenuItemIcons.CUT_SOLID));
            }
            xmenu.OpenXMenu(client);
        }

        private void GrowZoneMenuCallback(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            WeedZone zone = (WeedZone)menu.GetData("Zone");
            if (zone == null) return;

            var ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            switch (menuItem.Id)
            {
                case "ID_SeedSkunk":
                    if (ph.DeleteOneItemWithID(ItemID.GSkunk))
                    {
                        zone.SeedUsed = SeedType.Skunk;
                        zone.Plant = true;
                    }
                    break;
                case "ID_SeedPurple":
                    if (ph.DeleteOneItemWithID(ItemID.GPurple))
                    {
                        zone.SeedUsed = SeedType.Purple;
                        zone.Plant = true;
                    }
                    break;
                case "ID_SeedOrange":
                    if (ph.DeleteOneItemWithID(ItemID.GOrange))
                    {
                        zone.SeedUsed = SeedType.Orange;
                        zone.Plant = true;
                    }
                    break;
                case "ID_SeedWhite":
                    if (ph.DeleteOneItemWithID(ItemID.GWhite))
                    {
                        zone.SeedUsed = SeedType.WhiteWidow;
                        zone.Plant = true;
                    }
                    break;
                case "ID_Hydro":
                    if (ph.DeleteOneItemWithID(ItemID.Hydro))
                    {
                        zone.Spray = Spray.On;
                        zone.Hydratation = 100;
                    }
                    break;
                case "ID_Tint":
                    if (zone.Hydratation + 10 <= 100)
                        zone.Hydratation += 10;
                    else zone.Hydratation = 100;
                    break;
                case "ID_Recolte":
                    Recolte(zone);
                    break;
            }

            LabelRefresh(zone);
            Task.Run(async () => await Update());
            
            if (menuItem.Id == "ID_Tint")
                menu.OpenXMenu(client);
            else
                RefreshClientInLabs(zone);
        }

        //public override void OnPlayerConnected(IPlayer client)
        //{
        //    client.Emit("Weedlabs_UpdateAll", JsonConvert.SerializeObject(Position), AdvancedEquipement, JsonConvert.SerializeObject(WeedZoneList));
        //}
        
        private void OnMenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem.Id == "OpenInventory")
            {
                var ph = client.GetPlayerHandler();
                var inv = new RPGInventoryMenu(ph.PocketInventory, ph.OutfitInventory, ph.BagInventory, Inventory);
                inv.OnMove += async (cl, inventaire) =>
                {
                    ph.UpdateFull();
                    await Update();
                };
                menu.CloseMenu(client);
                Task.Run(async()=> await inv.OpenMenu(client));
            }
        }

        #endregion
    }
}