using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Utils;

namespace ResurrectionRP_Server.Models
{
    public enum ParkingType
    {
        Public,
        House,
        Faction,
        Society
    }

    public class ParkedCar
    {
        #region Fields
        [BsonId]
        public string Plate;
        public DateTime ParkTime;
        #endregion

        #region Constructor
        public ParkedCar(string plate, DateTime date)
        {
            Plate = plate;
            ParkTime = date;
        }
        #endregion

        #region Methods
        public IVehicle GetVehicle() => VehiclesManager.GetVehicleByPlate(Plate);
        public VehicleHandler GetVehicleHandler() => VehiclesManager.GetVehicleByPlate(Plate)?.GetVehicleHandler();
        #endregion
    }

    public class Parking
    {
        #region Fields and Properties
        public static List<Parking> ParkingList = new List<Parking>();

        public int ID;
        public string Name;
        public int Price;
        public bool Hidden;
        public string Owner; // Social Club
        public Vector3 Location;
        public Location Spawn1;
        public Location Spawn2;
        public int MaxVehicles;
        public ParkingType ParkingType;

        private List<ParkedCar> _listvehiclestored = new List<ParkedCar>();
        public List<ParkedCar> ListVehicleStored
        {
            get
            {
                if (_listvehiclestored == null)
                    _listvehiclestored = new List<ParkedCar>();
                return _listvehiclestored;
            }
            set => _listvehiclestored = value;
        }
        [BsonIgnore]
        private List<string> _whitelist = new List<string>();
        public List<string> Whitelist
        {
            get
            {
                if (_whitelist == null) _whitelist = new List<string>();
                return _whitelist;
            }
            set => _whitelist = value;
        }

        public event OnVehicleStoredEvent OnVehicleStored;
        public event OnVehicleOutEvent OnVehicleOut;

        [BsonIgnore]
        public OnSaveNeededDelegate OnSaveNeeded { get; set; }
        [BsonIgnore]
        public IColShape ParkingColshape { get; private set; }
        #endregion

        #region Delegates
        public delegate Task OnPlayerEnterParkingEvent(IPlayer client);
        public delegate Task OnVehicleStoredEvent(IPlayer client, VehicleHandler vehicle);
        public delegate Task OnVehicleOutEvent(IPlayer client, VehicleHandler vehicle, Location Spawn);
        public delegate Task OnSaveNeededDelegate();
        #endregion

        #region Constructor
        public Parking(Vector3 location = new Vector3(), Location spawn1 = null, Location spawn2 = null, string name = "", string owner = "", int price = 0, int maxVehicles = 999, bool hidden = true)
        {
            ID = GenerateRandomID();
            Name = name;
            Price = price;
            Hidden = hidden;
            Location = location;
            Spawn1 = spawn1;
            Spawn2 = spawn2;
            Owner = owner;
            MaxVehicles = maxVehicles;
        }
        #endregion

        #region Event handlers
        private async Task OnPlayerEnterColShape(IColShape colShape, IPlayer client)
        {
            if (colShape != ParkingColshape || !client.Exists)
                return;

            if (ParkingType != ParkingType.Society)
                await OpenParkingMenu(client);
        }

        private async Task OnPlayerLeaveColShape(IColShape colShape, IPlayer client)
        {
            if (colShape != ParkingColshape || !client.Exists)
                return;

            PlayerHandler player = client.GetPlayerHandler();

            if (player != null && player.HasOpenMenu())
                await MenuManager.CloseMenu(client);
        }

        private async Task OnVehicleEnterColShape(IColShape colShape, IVehicle vehicle)
        {
            if (colShape != ParkingColshape || !vehicle.Exists || vehicle.Driver == null)
                return;

            await OnPlayerEnterColShape(colShape, vehicle.Driver);
        }

        private async Task OnVehicleLeaveColShape(IColShape colShape, IVehicle vehicle)
        {
            if (colShape != ParkingColshape || !vehicle.Exists || vehicle.Driver == null)
                return;

            await OnPlayerLeaveColShape(colShape, vehicle.Driver);
        }
        #endregion

        #region Private methods
        private int GenerateRandomID()
        {
            int number = Utils.Utils.RandomNumber(999999999);
            while (ParkingList.Exists(x => x.ID == number))
            {
                number = Utils.Utils.RandomNumber(999999999);
            }
            return number;
        }

        private async Task StoreVehicle(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            var vehicle = await client.GetVehicleAsync();
            await StoreVehicle(client, vehicle);
            await menu.CloseMenu(client);
        }

        private async Task GetVehicle(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            try
            {
                VehicleHandler veh = menuItem.GetData("Vehicle"); // choix du véhicule choisi dans la liste de nos véhicule dans le parking
                Location Spawn = null;

                if (menu.GetData("Location") != null)
                {
                    if (!VehiclesManager.IsVehicleInSpawn(menu.GetData("Location")))
                    {
                        Spawn = menu.GetData("Location");
                        await veh.SpawnVehicle(Spawn);
                    }
                }
                else
                {
                    // Attribution du spawn de libre
                    if (!VehiclesManager.IsVehicleInSpawn(Spawn1.Pos))
                    {
                        Spawn = Spawn1;
                        await veh.SpawnVehicle(Spawn1);
                    }
                    else if (Spawn2 != null && !VehiclesManager.IsVehicleInSpawn(Spawn2.Pos))
                    {
                        Spawn = Spawn2;
                        await veh.SpawnVehicle(Spawn2);
                    }
                    else
                    {
                        // Aucun spawn de libre on stop tout.
                        client.SendNotificationError("Aucune place de disponible pour sortir votre véhicule.");
                        return;
                    }
                }

                // if (ParkingType != ParkingType.Public)
                //     await client.PutIntoVehicleAsync(veh.Vehicle, -1);

                RemoveVehicle(veh); // retrait du véhicule dans la liste
                veh.ParkingName = string.Empty;

                if (OnVehicleOut != null)
                    await OnVehicleOut.Invoke(client, veh, Spawn); // callback (ex carpark)

                veh.UpdateFull();
                await MenuManager.CloseMenu(client);
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("Parking | " + ex.ToString());
            }
        }

        private async Task<VehicleHandler> GetVehicleInParking(string plate)
        {
            var filter = Builders<VehicleHandler>.Filter.Eq(x => x.Plate, plate);
            return await Database.MongoDB.GetCollectionSafe<VehicleHandler>("vehicles").FindAsync(filter).Result.SingleAsync();
        }
        #endregion

        #region Public methods
        public void Load(float markerscale = 3f, int opacite = 128, bool blip = false, uint sprite = 50, float scale = 1f, byte color = 0, uint alpha = 255, string name = "", short dimension = GameMode.GlobalDimension)
        {
            Marker.CreateMarker(MarkerType.VerticalCylinder, Location - new Vector3(0.0f, 0.0f, markerscale-1), new Vector3(3, 3, 3));
            ParkingColshape = Alt.CreateColShapeCylinder(new AltV.Net.Data.Position(Location.X, Location.Y, Location.Z -1), markerscale, 4);
            ParkingColshape.Dimension = dimension;
            ParkingColshape.SetOnPlayerEnterColShape(OnPlayerEnterColShape);
            ParkingColshape.SetOnPlayerLeaveColShape(OnPlayerLeaveColShape);
            ParkingColshape.SetOnVehicleEnterColShape(OnVehicleEnterColShape);
            ParkingColshape.SetOnVehicleLeaveColShape(OnVehicleLeaveColShape);
            GameMode.Instance.Streamer.AddEntityTextLabel(this.Name + "\n~o~Approchez pour interagir", Location, 4);

            if (blip)
                Entities.Blips.BlipsManager.CreateBlip(name, Location,color,(int) sprite);

            ParkingList.Add(this);
        }

        public async Task OpenParkingMenu(IPlayer client, string title = "", string description = "", bool canGetAllVehicle = false, Location location = null, int vehicleType = -1, Menu menu = null, Menu.MenuCallback menuCallback = null)
        {
            if (!client.Exists)
                return;

            if (Whitelist != null && Whitelist.Count > 0)
            {
                var social = client.GetSocialClub();

                if (!Whitelist.Contains(social))
                {
                    client.SendNotificationError("Vous n'êtes pas autorisé à utiliser ce parking.");
                    return;
                }
            }

            if (menu == null)
                menu = new Menu("ID_ParkingMenu", (string.IsNullOrEmpty(title) ? "Parking" : title), (string.IsNullOrEmpty(description)) ? "Choisissez une option :" : description, Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true);
            else
            {
                menu.ClearItems();
                menu.BackCloseMenu = false;
                menu.ItemSelectCallback = menuCallback;
            }

            if (client.IsInVehicle && await client.GetSeatAsync() == 1) // I store my vehicle
            {
                IVehicle vehplayer = client.Vehicle;

                if (!vehplayer.Exists)
                    return;

                VehicleHandler vehicle = vehplayer.GetVehicleHandler();

                if (vehicle == null)
                    return;

                if (vehicle.SpawnVeh)
                {
                    client.SendNotificationError("Vous ne pouvez pas garer un véhicule de location dans le parking!");
                    return;
                }

                if (ListVehicleStored.Count >= MaxVehicles)
                {
                    client.SendNotificationError("Le parking est plein!");
                    return;
                }
                
                var item = new MenuItem("Ranger votre voiture", "", "ID_StoreVehicle", true, rightLabel: $"{((Price > 0) ? "$" + Price.ToString() : "")}");
                item.OnMenuItemCallback = StoreVehicle;
                menu.Add(item);
            }
            else // I want my vehicle
            {
                menu.SubTitle = "Quel véhicule souhaitez-vous récupérer :";
                PlayerHandler ph = client.GetPlayerHandler();

                if (ph == null)
                    return;

                string social = client.GetSocialClub();
                List<ParkedCar> vehicleListParked = null;

                if (canGetAllVehicle)
                    vehicleListParked = ListVehicleStored;
                else if (vehicleType == -1)
                    vehicleListParked = ListVehicleStored.FindAll(p => VehiclesManager.GetVehicleHandler(p.Plate).OwnerID == social || ph.ListVehicleKey.Exists(v => v.Plate == p.Plate));
                else
                    vehicleListParked = ListVehicleStored.FindAll(p => (VehiclesManager.GetVehicleHandler(p.Plate).OwnerID == social || ph.ListVehicleKey.Exists(v => v.Plate == p.Plate) && VehiclesManager.GetVehicleHandler(p.Plate).VehicleManifest.VehicleClass == vehicleType));

                FilterDefinition<VehicleHandler> filter = Builders<VehicleHandler>.Filter.AnyIn("_id", vehicleListParked.Select(v => v.Plate).ToArray());
                List<VehicleHandler> vehicleList = await Database.MongoDB.GetCollectionSafe<VehicleHandler>("vehicles").Find(filter).ToListAsync();

                if (vehicleList.Count > 0)
                {
                    foreach (VehicleHandler veh in vehicleList)
                    {
                        try
                        {
                            if (veh.VehicleManifest == null)
                                veh.VehicleManifest = VehicleInfoLoader.VehicleInfoLoader.Get(veh.Model) ?? null;

                            string _description =
                                $"~g~Essence:~w~ {veh.Fuel} \n" +
                                $"~g~Etat: ~w~{(veh.BodyHealth * 0.1)} \n" +
                                $"{((ParkingType == ParkingType.Public) ? $"~g~Fin Horodateur:~w~ {veh.LastUse.AddMonths(1).ToShortDateString()}" : "")}";

                            MenuItem item = new MenuItem(string.IsNullOrEmpty(veh.VehicleManifest.LocalizedName) ? veh.VehicleManifest.DisplayName : veh.VehicleManifest.LocalizedName, _description, "", true, rightLabel: veh.Plate);
                            item.SetData("Vehicle", veh);
                            item.OnMenuItemCallback = GetVehicle;
                            menu.Add(item);
                        }
                        catch(Exception ex)
                        {
                            Alt.Server.LogError($"OpenParkingMenu: {veh.Plate} {this.ID} {this.Name} {client.GetSocialClub()} : " + ex);
                        }
                    }
                }
                else
                {
                    client.SendNotificationError("Vous n'avez aucun véhicule dans ce parking.");
                    return;
                }
            } 

            menu.SetData("Location", location);
            await menu.OpenMenu(client);
        }

        public async Task StoreVehicle(IPlayer client, IVehicle vh)
        {
            if (vh == null)
                return;

            try
            {
                if (!client.GetPlayerHandler().HasMoney(Price))
                {
                    client.SendNotificationError($"Vous n'avez pas les ${Price} en poche pour payer le ticket.");
                    return;
                }

                VehicleHandler veh = vh.GetVehicleHandler();

                if (veh != null)
                {
                    if (veh.IsParked)
                        return;

                    if (GameMode.Instance.IsDebug)
                        Alt.Server.LogColored($"~b~Parking ~w~| New vehicle in parking id {this.ID}");

                    await vh.SetEngineOnAsync(false);
                    veh.LockState = VehicleLockState.Locked;
                    veh.LastUse = DateTime.Now; // refresh the last use

                    lock (ListVehicleStored)
                    {
                        if (ListVehicleStored.Find(p => p.Plate == veh.Plate) == null)
                            ListVehicleStored.Add(new ParkedCar(veh.Plate, DateTime.Now));
                    }

                    veh.IsParked = true;
                    veh.ParkingName = Name;
                    veh.UpdateMilageAndFuel();

                    if (OnVehicleStored != null)
                        await OnVehicleStored.Invoke(client, veh); // call event for success storage

                    veh.UpdateFull();
                    await veh.Delete(false);
                }
                else
                {
                    Alt.Server.LogError("GetHandlerByVehicle fuck is null this shit! mother fucker!");
                }
                await MenuManager.CloseMenu(client);
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("StoreVehicle" + ex);
            }
        }

        public bool RemoveVehicle(VehicleHandler veh)
        {
            bool success = false;

            lock (ListVehicleStored)
            {
                ParkedCar vehicle = ListVehicleStored.FirstOrDefault(v => v.Plate == veh.Plate);

                if (vehicle == null)
                    success = false;
                else
                    success = ListVehicleStored.Remove(vehicle);
            }

            if (!success)
                Alt.Server.LogError($"Parking '{Name}': Error removing vehicle {veh.Plate}");

            return success;
        }
        #endregion
    }
}
