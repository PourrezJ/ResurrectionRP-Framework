using System.Collections.Generic;
using System.Threading.Tasks;
using System.Numerics;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using AltV.Net.Enums;
using VehicleHandler = ResurrectionRP_Server.Entities.Vehicles.VehicleHandler;
using VehicleManager = ResurrectionRP_Server.Entities.Vehicles.VehiclesManager;
using ResurrectionRP_Server.Streamer.Data;
using ResurrectionRP_Server.Entities;
using System.Drawing;

namespace ResurrectionRP_Server.Jobs
{

    public class Jobs
    {
        #region Variables
        public string Name;
        public Vector3 ServicePos;
        public uint BlipSprite;
        public int BlipColor;
        public Models.Location VehicleSpawnLocation;
        public VehicleModel VehicleSpawnHash;

        private Entities.Blips.Blips _blip;
        private Marker _marker;
        private IColShape _serviceColshape;
        private Dictionary<string, VehicleHandler> _vehicleList = new Dictionary<string, VehicleHandler>();
        private static Dictionary<string, Jobs> _inServiceList = new Dictionary<string, Jobs>();
        #endregion

        #region Constructor
        public Jobs()
        {

        }
        #endregion

        #region Load
        public virtual void Load()
        {
            if (ServicePos != null)
            {
                _blip = Entities.Blips.BlipsManager.CreateBlip(Name, ServicePos, 1, (int)BlipSprite, 1, true);
                _serviceColshape = Alt.CreateColShapeCylinder(ServicePos, 1f, 1f);
                _marker = Marker.CreateMarker(MarkerType.VerticalCylinder, ServicePos - new Vector3(0, 0, 1), new Vector3(1, 1, 1), Color.FromArgb(128, 255, 255, 255));
            }
        }
        #endregion

        #region Events
        public virtual async Task OnEntityEnterColShape(IColShape colShape, IEntity entity, bool state)
        {
            if (!state || !entity.Exists || entity.Type != BaseObjectType.Player)
                return;
            IPlayer client = entity as IPlayer;
            if (colShape == _serviceColshape)
            {
                //if (client != null) await OpenServerJobMenu(client);
                if (client != null && client.Exists)
                    await PriseService(client);
            }
        }

        public virtual Task OnPlayerEnterVehicleJob(IVehicle vehicle, IPlayer client, byte seat)
        {
            return Task.CompletedTask;
        }
        #endregion

        #region Menus
/*        public virtual async Task<Menu> OpenServerJobMenu(IPlayer client)
        {
            Menu serverJobMenu = new Menu("ID_ServiceMenu", Name, "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true);
            serverJobMenu.BannerColor = new MenuColor(0, 0, 0, 0);
            serverJobMenu.Callback += MenuJobCallback;

            if (!await IsInService(client)) serverJobMenu.Add(new MenuItem("Prendre votre service", "", "ID_GetService", executeCallback: true));
            else serverJobMenu.Add(new MenuItem("Quitter votre service", "", "ID_QuitService", executeCallback: true));


            await MenuManager.OpenMenu(client, serverJobMenu);
            return serverJobMenu;
        }*/

/*        private async Task MenuJobCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menu.Id == "ID_ServiceMenu")
            {
                if (menuItem.Id == "ID_GetService")
                {
                    if (await PriseService(client))
                    {
                        await client.SendNotificationSuccess("Vous avez pris votre service.");
                    }
                    if (client.HasOpenMenu()) await MenuManager.CloseMenu(client);
                }
                else if (menuItem.Id == "ID_QuitService")
                {
                    await QuitterService(client);
                    if (client.HasOpenMenu()) await MenuManager.CloseMenu(client);
                }
            }
        }*/
        #endregion

        #region Methods
        public virtual Task<bool> PriseService(IPlayer client)
        {
            /*
            PlayerHandler ph = PlayerManager.GetPlayerByClient(client);
           
            // Choix du sexe
            switch (ph.Character.Gender)
            {
                case 0: // Homme
                    NAPI.Player.SetPlayerClothes(client, 8, 59, 0);
                    break;
                case 1: // Femme
                    NAPI.Player.SetPlayerClothes(client, 8, 36, 0);
                    break;
                default: // Ped?
                    client.SendNotificationError("Vous ne pouvez pas avoir de tenue avec ce personnage");
                    break;
            }*/
            if (VehicleSpawnLocation != null && VehicleManager.IsVehicleInSpawn(VehicleSpawnLocation.Pos))
            {
                client.SendNotificationError($"Un véhicule gêne la sortie de votre véhicule de fonction");
                return Task.FromResult(false);
            }

            var social = client.GetSocialClub();

            if (_inServiceList.TryAdd(social, this))
            {
                if (VehicleSpawnLocation != null)
                {
                    var _veh = VehicleManager.SpawnVehicle(social, (uint)VehicleSpawnHash, VehicleSpawnLocation.Pos, VehicleSpawnLocation.Rot, spawnVeh: true);
                    _veh.SpawnVeh = true;
                    //_veh.OnPlayerEnterVehicle = OnPlayerEnterVehicleJob;
                    AltAsync.OnPlayerEnterVehicle += OnPlayerEnterVehicleJob;
                    client.GetPlayerHandler()?.AddKey(_veh, "JOB DustMan");
                    _vehicleList.TryAdd(social, _veh);
                }

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public virtual async Task QuitterService(IPlayer client)
        {
            client.ApplyCharacter();

            if (IsInService(client))
            {
                var job = GetJobService(client);


                if (job.VehicleSpawnLocation != null)
                {
                    await job.RemoveVehiclePlayer(client);
                }
                _inServiceList.Remove( client.GetSocialClub());
                client.SendNotificationSuccess("Vous avez quitté votre service.");
            }
        }

        public bool IsInService(IPlayer client)
        {
            if (_inServiceList.ContainsKey(client.GetSocialClub())) return true;
            return false;
        }

        public Jobs GetJobService(IPlayer client)
        {
            if (_inServiceList.TryGetValue( client.GetSocialClub(), out Jobs value))
            {
                return value;
            }
            return null;
        }

        public VehicleHandler GetJobVehiclePlayer(IPlayer client)
        {
            if (_vehicleList.TryGetValue(client.GetSocialClub(), out VehicleHandler value))
            {
                return value;
            }
            return null;
        }

        public async Task RemoveVehiclePlayer(IPlayer client)
        {
            VehicleHandler _veh = GetJobVehiclePlayer(client);
            client.GetPlayerHandler()?.RemoveKey(_veh);
            _vehicleList.Remove(client.GetSocialClub());
            await _veh.DeleteAsync();
        }
        #endregion
    }
}
