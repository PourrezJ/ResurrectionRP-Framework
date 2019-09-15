using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using AltV.Net;
using AltV.Net.Async;

namespace ResurrectionRP_Server.Business
{
    public partial class Market : Business
    {

        public static List<Market> MarketsList = new List<Market>();

        public int ID;
        public Vector3 StationPos;
        public float Range = 14f;
        public int EssencePrice = 1;
        public float Litrage = 0;
        public int LitrageMax = 1000;
        public StationService Station;

        public Market(int id, string businnessName, Models.Location location, uint blipSprite, int inventoryMax, Vector3 stationPos, PedModel pedhash = 0, string owner = null) : base(businnessName, location, blipSprite, inventoryMax, pedhash, owner)
        {
            ID = id;
            Buyable = true;
            StationPos = stationPos;
        }

        public override async Task Init()
        {
            this.Station = new StationService(this.ID, this.Range, this.StationPos);
            this.Station.StationBlip = Entities.Blips.BlipsManager.CreateBlip("Station essence", StationPos, 128, 361, 0.5f);
            this.Station.Colshape = Alt.CreateColShapeCylinder(StationPos, Range, 3f);

            this.Station.Colshape.SetData("FuelPumpID", this.Station.ID);
            this.Station.Colshape.SetData("FuelPumpRange", this.Station.Range);
            this.Station.Colshape.SetData("FuelPumpLocation", this.Station.location);

            EventHandlers.Events.OnPlayerEnterColShape += Events_PlayerEnterColshape;
            EventHandlers.Events.OnPlayerLeaveColShape += Events_PlayerExitColshape;

            this.Inventory.MaxSlot = 40;
            this.Inventory.MaxSize = 750;
            this.MaxEmployee = 5;
            await base.Init();
            MarketsList.Add(this);
        }

        private async Task Events_PlayerExitColshape(IColShape colShape, IPlayer client)
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
                        Menu RefuelMenu = new Menu("ID_RefuelMenu", "Station Service", "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true);
                        RefuelMenu.ItemSelectCallback = RefuelMenuCallBack;
                        RefuelMenu.Add(new MenuItem("Remplir la station", "", "", true));

                        await MenuManager.OpenMenu(client, RefuelMenu);
                    }
                }
                else
                {
                    client.DisplayHelp("Votre citerne est vide, vous avez rien à faire ici !", 15000);
                }
            }
        }

        private async Task Events_PlayerEnterColshape(IColShape colShape, IPlayer client)
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
                client.DisplayHelp("Vous venez de sortir de la zone de ravitaillement!", 30000);

            }
        }
    }
}
