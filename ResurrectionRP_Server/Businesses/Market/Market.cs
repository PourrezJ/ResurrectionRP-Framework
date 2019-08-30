using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using AltV.Net;
using AltV.Net.Async;

namespace ResurrectionRP_Server.Businesses
{
    public partial class Market : Business
    {

        public static List<Market> MarketsList = new List<Market>();

        public int ID;
        public Vector3 StationPos;
        public float Range = 100f;
        public int EssencePrice = 1;
        public float Litrage = 0;
        public int LitrageMax = 1000;
        public int StationBlip;
        [BsonIgnore]
        public IColShape FuelPumpColshape { get; private set; }

        public Market(int id, string businnessName, Models.Location location, uint blipSprite, int inventoryMax, Vector3 stationPos, PedModel pedhash = 0, string owner = null) : base(businnessName, location, blipSprite, inventoryMax, pedhash, owner)
        {
            ID = id;
            Buyable = true;
            StationPos = stationPos;
        }

        public override async Task Init()
        {
            //await MP.Blips.NewAsync(361, StationPos, 0.5f, 1, "Station d'éssence", 128, 10, true);
            StationBlip = Entities.Blips.BlipsManager.CreateBlip("Station essence", StationPos, 128, 361, 0.5f);
            FuelPumpColshape = Alt.CreateColShapeCylinder(StationPos, Range, 3f);
            //FuelPumpColshape = await MP.Colshapes.NewTubeAsync(StationPos, Range, 3f);
            //FuelPumpColshape.SetSharedData("FuelPump", this);

            EventHandlers.Events.OnPlayerEnterColShape += Events_PlayerEnterColshape;
            EventHandlers.Events.OnPlayerLeaveColShape += Events_PlayerExitColshape;

            this.Inventory.MaxSlot = 40;
            this.Inventory.MaxSize = 750;
            this.MaxEmployee = 5;
            await base.Init();
            MarketsList.Add(this);
        }

        private async void Events_PlayerExitColshape(IColShape colShape, IPlayer client)
        {
            if (!client.Exists)
                return;

            if (colShape != FuelPumpColshape) return;
            // On vérifie que ce soit un camion citerne qui rentre dans la zone
            if (await client.IsInVehicleAsync() && await (await client.GetVehicleAsync()).GetModelAsync() == 4097861161)
            {
                IVehicle fueltruck = await client.GetVehicleAsync();
                // Si il posséde du carburant raffiné
                if (fueltruck.GetData("RefuelRaffine", out object data))
                {
                    if ((int)data > 0)
                    {
                        Menu RefuelMenu = new Menu("ID_RefuelMenu", "Station Service", "", 0, 0, Menu.MenuAnchor.MiddleRight, false, true, true);
                        RefuelMenu.ItemSelectCallback = RefuelMenuCallBack;
                        RefuelMenu.Add(new MenuItem("Remplir la station", "", "", true));

                        await MenuManager.OpenMenu(client, RefuelMenu);
                    }
                }
                else
                {
                    await client.displayHelp("Votre citerne est vide, vous avez rien à faire ici !", 15000);
                }
            }
        }

        private async void Events_PlayerEnterColshape(IColShape colShape, IPlayer client)
        {
            if (!client.Exists)
                return;

            if (colShape != FuelPumpColshape) return;
            if (_utilisateurRavi == client && _ravitaillement && await _utilisateurRavi.IsInVehicleAsync() && await (await _utilisateurRavi.GetVehicleAsync()).GetModelAsync() == 4097861161)
            {
                _ravitaillement = false;
                _utilisateurRavi = null;
                // API.Shared.OnProgressBar(client, false);
                await Update();
                await client.displayHelp("Vous venez de sortir de la zone de ravitaillement!", 30000);

            }
        }
    }
}
