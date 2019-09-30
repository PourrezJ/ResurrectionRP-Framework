
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
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Factions;

namespace ResurrectionRP_Server.Services
{
    public class Pound
    {
        #region Fields
        private Location _poundSpawn = new Location(new Vector3(408.7641f, -1638.905f, 28.26795f), new Vector3(0.07774173f, 0.05821822f, 137.2628f));
        [BsonIgnore]
        private IColShape _colshape = null;

        public List<ParkedCar> PoundVehicleList = null;
        public int Price = 0;
        #endregion

        #region Init
        public Task Init()
        {
            if (PoundVehicleList == null)
                PoundVehicleList = new List<ParkedCar>();

            Alt.Server.LogInfo("--- [POUND] Starting pound ---");

            Entities.Blips.BlipsManager.CreateBlip("Fourrière", _poundSpawn.Pos, 0, 88,1,true);
            _colshape = Alt.CreateColShapeCylinder(_poundSpawn.Pos, 3f, 3f);
            Marker.CreateMarker(MarkerType.VerticalCylinder, _poundSpawn.Pos, new Vector3(1, 1, 1));

            //_colshape.OnEntityEnterColShape += _colshape_OnEntityEnterColShape;
            //_colshape.OnEntityExitColShape += _colshape_OnEntityExitColShape;
            EventHandlers.Events.OnPlayerEnterColShape += OnPlayerEnterColShape;

            /*
            List<string> checkedPlate = new List<string>();
            foreach (var vehicle in PoundVehicleList.ToList())
            {
                await VehiclesManager.DeleteVehicleFromAllParkings(vehicle.Plate);

                if (VehiclesManager.GetVehicleWithPlate(vehicle.Plate) != null)
                {
                    checkedPlate.Add(vehicle.Plate);
                }

                if (!checkedPlate.Contains(vehicle.Plate))
                {
                    checkedPlate.Add(vehicle.Plate);
                    if (DateTime.Now > vehicle.LastUse.AddMonths(1)) // Vérification si horodatage est dépassé, mise en occasion.
                    {
                        vehicle.LastUse = DateTime.Now;
                        // TODO POUR L'ENTREPRISE D'OCCASION
                    }
                }
                else
                {
                    await GameMode.Instance.Save();
                    PoundVehicleList.Remove(vehicle);
                    Alt.Server.LogError($"POUND | Vehicle duplicated plate: {vehicle.Plate} Owner: {vehicle.OwnerID} ");
                }
            }
            */

            Entities.Peds.Ped _npc = Entities.Peds.Ped.CreateNPC(PedModel.Gardener01SMM, new Vector3(409.1505f, -1622.874f, 29.29193f), 227.5882f);

            _npc.NpcInteractCallBack += (IPlayer client, Entities.Peds.Ped ped) =>
            {
                OpenPoundMenu(client);
            };

            Alt.Server.LogInfo($"--- [POUND] Finish loading all pounds in database: {PoundVehicleList.Count} ---");
            return Task.CompletedTask;
        }
        #endregion

        #region Event handlers
        public void OnPlayerEnterColShape(IColShape colShape, IPlayer client)
        {
            if (colShape != this._colshape)
                return;
            IVehicle vehicle = client.Vehicle;

            if (vehicle != null && vehicle.Model == (int)VehicleModel.Flatbed && FactionManager.IsLSCustom(client))
            {
                VehicleHandler _vh = vehicle.GetVehicleHandler();

                if (_vh.TowTruck != null)
                {
                    AcceptMenu _accept = AcceptMenu.OpenMenu(client, "Fourrière", "Voulez-vous mettre en fourrière le véhicule?");
                    _accept.AcceptMenuCallBack = AcceptMenuCallBack;
                }
            }
        }
        #endregion

        #region Methods
        private async Task AcceptMenuCallBack(IPlayer client, bool reponse)
        {
            if (reponse)
            {
                IVehicle vehicle = client.Vehicle;
                VehicleHandler _vh = vehicle.GetVehicleHandler();
                if (_vh != null)
                {
                    VehicleHandler _towedvehicle = VehiclesManager.GetVehicleHandler(_vh.TowTruck.VehPlate);

                    if (_towedvehicle != null)
                    {
                        var ph = client.GetPlayerHandler();

                        if (ph != null)
                        {
                            if (VehiclesManager.VehicleHandlerList.Remove(vehicle, out _))
                            {
                                await GameMode.Instance.FactionManager.LSCustom.BankAccount.AddMoney(750, $"Mise en fourrière {_towedvehicle.Plate} par {ph.Identite.Name}");
                                ph.AddMoney(250);

                                _vh.TowTruck = null;
                                PoundVehicleList.Add(new ParkedCar(_towedvehicle.Plate, DateTime.Now));
                                _towedvehicle.IsInPound = true;
                                _vh.UpdateProperties();
                                _towedvehicle.UpdateInBackground(false);
                                await _towedvehicle.Delete(true);
                                MenuManager.CloseMenu(client);
                            }
                        }
                    }
                }
            }
        }

        public void OpenPoundMenu(IPlayer client)
        {
            Menu _menu = new Menu("ID_PoundMenu", "Fourrière", $"Sortir un véhicule pour la somme: ${Price}", backCloseMenu: true);
            _menu.ItemSelectCallbackAsync = PoundMenuCallBack;

            var _poundList = GetVehicleInPound(client);
            if (_poundList.Count <= 0)
            {
                client.SendNotificationPicture(Utils.Enums.CharPicture.DIA_GARDENER, "Fourrière", "","~r~Vous n'avez aucun véhicule ici!" );
                return;
            }

            var social = client.GetSocialClub();

            foreach (var veh in _poundList)
            {
                try
                {
                    if (veh.OwnerID != social)
                        continue;

                    string _vehName = VehicleInfoLoader.VehicleInfoLoader.Get(veh.Model)?.DisplayName ?? "Inconnu";
                    var _item = new MenuItem((_vehName != null) ? _vehName : "INCONNU", "", "ID_Get", true, rightLabel: veh.Plate);
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

        private async Task PoundMenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menu.Id == "ID_PoundMenu" && menuItem.Id == "ID_Get" && menuItem.HasData("Vehicle"))
            {
                if (client.GetPlayerHandler().HasMoney(Price))
                {
                    VehicleHandler veh = menuItem.GetData("Vehicle");
                    veh.IsInPound = false;
                    await veh.SpawnVehicleAsync(new Location(_poundSpawn.Pos, _poundSpawn.Rot.ConvertRotationToRadian()));
                    veh.UpdateInBackground();
                    PoundVehicleList.Remove(PoundVehicleList.Find(v => v.Plate == veh.Plate));
                    await GameMode.Instance.Save();
                    client.SendNotificationPicture(Utils.Enums.CharPicture.DIA_GARDENER,"Fourrière", "","~g~Votre véhicule vous attend sur le parking." );
                    MenuManager.CloseMenu(client);
                }
                else
                {
                    client.SendNotificationError("Vous n'avez pas assez d'argent sur vous.");
                }
            }
        }

        public List<VehicleHandler> GetVehicleInPound(IPlayer client)
        {
            List<ParkedCar> List = PoundVehicleList.FindAll(v => VehiclesManager.GetVehicleHandler(v.Plate)?.OwnerID == client.GetSocialClub() || client.GetPlayerHandler().HasKey(v.Plate)).ToList();
            List<VehicleHandler> endup = new List<VehicleHandler>();

            foreach(ParkedCar car in List)
                endup.Add(VehiclesManager.GetVehicleHandler(car.Plate));

            return endup;
        }

        public async Task AddVehicleInPound(VehicleHandler veh)
        {
            Alt.Server.LogInfo ($"Mise en fourrière véhicule {veh.Plate}");

            if (!PoundVehicleList.Exists(p => p.Plate == veh.Plate))
                PoundVehicleList.Add(new ParkedCar(veh.Plate, DateTime.Now));

            veh.IsInPound = true;
            veh.ParkingName = "Fourrière";
            veh.UpdateInBackground(false, true);
            await veh.Delete();
        }
        #endregion
    }
}
