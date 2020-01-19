
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Enums;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Colshape;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Factions;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Database;

namespace ResurrectionRP_Server.Services
{
    public static class Pound
    {
        #region Fields
        private static Location _poundSpawn = new Location(new Vector3(408.7641f, -1638.905f, 28.26795f), new Vector3(0.07774173f, 0.05821822f, 137.2628f));
        private static IColshape _colshape = null;
        public static int Price = 500;
        #endregion

        #region Init
        public static void Init()
        {
            Alt.Server.LogInfo("--- [POUND] Starting pound ---");

            if (Config.GetSetting<bool>("AutoPound"))
                Price = 0;

            Entities.Blips.BlipsManager.CreateBlip("Fourrière", _poundSpawn.Pos, 0, 88,1,true);
            _colshape = ColshapeManager.CreateCylinderColshape(_poundSpawn.Pos, 3f, 3f);
            Marker.CreateMarker(MarkerType.VerticalCylinder, _poundSpawn.Pos, new Vector3(1, 1, 1));

            //_colshape.OnEntityEnterColShape += _colshape_OnEntityEnterColShape;
            //_colshape.OnEntityExitColShape += _colshape_OnEntityExitColShape;
            ColshapeManager.OnPlayerEnterColshape += OnPlayerEnterColshape;

            Entities.Peds.Ped _npc = Entities.Peds.Ped.CreateNPC(PedModel.Gardener01SMM, new Vector3(409.1505f, -1622.874f, 29.29193f), 227.5882f);

            _npc.NpcInteractCallBack += (IPlayer client, Entities.Peds.Ped ped) =>
            {
                OpenPoundMenu(client);
            };

            Alt.Server.LogInfo($"--- [POUND] Finish loading Pound ---");
        }
        #endregion

        #region Event handlers
        public static void OnPlayerEnterColshape(IColshape colshape, IPlayer client)
        {
            if (colshape != _colshape)
                return;

            IVehicle vehicle = client.Vehicle;

            if (vehicle != null && vehicle.Model == (int)VehicleModel.Flatbed && FactionManager.IsLSCustom(client))
            {
                VehicleHandler _vh = vehicle.GetVehicleHandler();

                if (_vh.VehicleData.TowTruck != null)
                {
                    AcceptMenu _accept = AcceptMenu.OpenMenu(client, "Fourrière", "Voulez-vous mettre en fourrière le véhicule?");
                    _accept.AcceptMenuCallBack = AcceptMenuCallBack;
                }
            }
        }
        #endregion

        #region Methods
        private static void AcceptMenuCallBack(IPlayer client, bool reponse)
        {
            if (reponse)
            {
                IVehicle vehicle = client.Vehicle;
                VehicleHandler _vh = vehicle.GetVehicleHandler();
                if (_vh != null)
                {
                    VehicleHandler _towedvehicle = _vh.GetVehicleHandler();

                    if (_towedvehicle != null)
                    {
                        var ph = client.GetPlayerHandler();

                        if (ph != null)
                        {
                            Factions.FactionManager.LSCustom.BankAccount.AddMoney(750, $"Mise en fourrière {_towedvehicle.VehicleData.Plate} par {ph.Identite.Name}");
                            ph.AddMoney(250);

                            _vh.VehicleData.TowTruck = null;
                            _towedvehicle.VehicleData.IsInPound = true;
                            _vh.VehicleData.UpdateProperties();
                            _towedvehicle.UpdateInBackground(false, true);
                            Task.Run(async ()=> await _towedvehicle.DeleteAsync(false));
                            MenuManager.CloseMenu(client);
                        }
                    }
                }
            }
        }

        public static void OpenPoundMenu(IPlayer client)
        {
            Menu _menu = new Menu("ID_PoundMenu", "Fourrière", $"Sortir un véhicule pour la somme: ${Price}", backCloseMenu: true);
            _menu.ItemSelectCallback = PoundMenuCallBack;

            var _poundList = GetVehicleInPound(client);
            if (_poundList.Count() <= 0)
            {
                client.SendNotificationPicture(Utils.Enums.CharPicture.DIA_GARDENER, "Fourrière", "","~r~Vous n'avez aucun véhicule ici!" );
                return;
            }

            var social = client.GetSocialClub();

            foreach (var veh in _poundList)
            {
                try
                {

                    string _vehName = VehicleInfoLoader.VehicleInfoLoader.Get(veh.Model)?.DisplayName ?? "Inconnu";
                    var _item = new MenuItem(_vehName ?? "INCONNU", "", "ID_Get", true, rightLabel: veh.Plate);
                    _item.SetData("Vehicle", veh);
                    _menu.Add(_item);
                }
                catch (Exception ex)
                {
                    Alt.Server.LogError("OpenPoundMenu" + ex);
                }
            }

            _menu.OpenMenu(client);
        }

        private static void PoundMenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menu.Id == "ID_PoundMenu" && menuItem.Id == "ID_Get" && menuItem.HasData("Vehicle"))
            {
                PlayerHandler ph = client.GetPlayerHandler();

                if (ph == null)
                    return;

                if (ph.HasMoney(Price))
                {
                    VehicleData vehdata = menuItem.GetData("Vehicle");
                    vehdata.IsInPound = false;
                    vehdata.LastUse = DateTime.Now;
                    vehdata.SpawnVehicle(new Location(_poundSpawn.Pos, _poundSpawn.Rot.ConvertRotationToRadian()));
                    vehdata.Vehicle.UpdateInBackground();

                    var keyfind = ph.ListVehicleKey.FindLast(k => k.Plate == vehdata.Plate);
                    if (keyfind == null)
                        ph.ListVehicleKey.Add(new VehicleKey(vehdata.Vehicle?.VehicleManifest.LocalizedName, vehdata.Plate));

                    Task.Run(async ()=> await GameMode.Instance.Save());
                    client.SendNotificationPicture(Utils.Enums.CharPicture.DIA_GARDENER,"Fourrière", "","Votre véhicule vous attend sur le parking." );
                    MenuManager.CloseMenu(client);
                }
                else
                {
                    client.SendNotificationError("Vous n'avez pas assez d'argent sur vous.");
                }
            }
        }

        public static IEnumerable<VehicleData> GetVehicleInPound(IPlayer client)
        {
            return VehiclesManager.GetAllVehicleData().Where(v => (v.OwnerID == client.GetSocialClub() || client.GetPlayerHandler().HasKey(v.Plate)) && v.IsInPound);
        }

        public static void AddVehicleInPound(VehicleData veh)
        {
            if (veh == null)
                return;

            veh.IsInPound = true;
            veh.ParkingName = "Fourrière";
            veh.Vehicle.UpdateInBackground(false, true);
            Alt.Server.LogInfo($"Mise en fourrière véhicule {veh.Plate}");
            Task.Run(async ()=> await veh.DeleteAsync());
        }
        #endregion
    }
}
