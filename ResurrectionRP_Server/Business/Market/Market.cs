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
        public float Range = 3f;
        public StationService Station;

        public Market(int id, string businnessName, Models.Location location, uint blipSprite, int inventoryMax, Vector3 stationPos, PedModel pedhash = 0, string owner = null) : base(businnessName, location, blipSprite, inventoryMax, pedhash, owner)
        {
            ID = id;
            Buyable = true;
            StationPos = stationPos;
        }

        public override async Task Init()
        {
            Station = new StationService(this.ID, this.Range, this.StationPos);
            Station.StationBlip = Entities.Blips.BlipsManager.CreateBlip("Station essence", StationPos, 128, 361, 0.5f);
            Station.Colshape = Alt.CreateColShapeCylinder(StationPos - new Vector3(0,0,2), 14f, 6f);

            Station.Colshape.SetOnPlayerEnterColShape(Events_PlayerEnterColshape);
            Station.Colshape.SetOnPlayerLeaveColShape(Events_PlayerExitColshape);
            Station.Colshape.SetOnVehicleEnterColShape(Events_VehicleEnterColshape);
            Station.Colshape.SetOnVehicleLeaveColShape(Events_VehicleExitColshape);

            Inventory.MaxSlot = 40;
            Inventory.MaxSize = 750;
            MaxEmployee = 5;
            await base.Init();
            MarketsList.Add(this);
        }

        private async Task Events_PlayerExitColshape(IColShape colShape, IPlayer client)
        {
            if (!await client.ExistsAsync())
                return;

            await MenuManager.CloseMenu(client);
        }

        private async Task Events_PlayerEnterColshape(IColShape colShape, IPlayer client)
        {
            if (!client.Exists)
                return;

            if (!colShape.IsEntityInColShape(client)) return;

            // On vérifie que ce soit un camion citerne qui rentre dans la zone
            if (await client.IsInVehicleAsync() && await (await client.GetVehicleAsync()).GetModelAsync() == 4097861161)
            {
                IVehicle fueltruck = await client.GetVehicleAsync();
                // Si il posséde du carburant raffiné
                if (fueltruck.GetVehicleHandler().OilTank.Traite > 0 )
                {
                    Menu RefuelMenu = new Menu("ID_RefuelMenu", "Station Service", "", 0, 0, Menu.MenuAnchor.MiddleRight, false, true, true);
                    RefuelMenu.ItemSelectCallbackAsync = RefuelMenuCallBack;
                    RefuelMenu.Add(new MenuItem("Remplir la station", "", "", true));

                    await MenuManager.OpenMenu(client, RefuelMenu);
                }
                else
                    client.DisplayHelp("~r~Votre citerne est vide.", 10000);
            }
        }

        private Task Events_VehicleEnterColshape(IColShape colshape, IVehicle vehicle)
        {
            if (!vehicle.Exists)
                return Task.CompletedTask;

            if (!Station.VehicleInStation.ContainsKey(vehicle.Id))
                Station.VehicleInStation.TryAdd(vehicle.Id, vehicle);

            return Task.CompletedTask;
        }

        private async Task Events_VehicleExitColshape(IColShape colshape, IVehicle vehicle)
        {
            if (!vehicle.Exists)
                return;

            if (this.Station.VehicleInStation.ContainsKey(vehicle.Id))
                this.Station.VehicleInStation.TryRemove(vehicle.Id, out IVehicle veh);

            if (vehicle.Driver == null)
                return;
            IPlayer client = vehicle.Driver;

            if (!client.Exists)
                return;


            if (_utilisateurRavi == client && _ravitaillement )
            {
                _ravitaillement = false;
                _utilisateurRavi = null;
                // API.Shared.OnProgressBar(client, false);
                await Update();
                client.DisplayHelp("~r~Vous êtes sorti de la zone de ravitaillement", 12000);
            }
        }
    }
}
