
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.EventHandlers;
using VehicleHandler = ResurrectionRP_Server.Entities.Vehicles.VehicleHandler;
using VehicleManager = ResurrectionRP_Server.Entities.Vehicles.VehiclesManager;

namespace ResurrectionRP_Server.Models
{
    public enum ParkingType
    {
        Public,
        House,
        Society
    }

    public class ParkedCar
    {
        #region Fields
        [BsonId]
        public string Plate;
        public string Owner;
        public int VehicleClass;
        public DateTime ParkTime;
        #endregion

        #region Constructor
        public ParkedCar(string plate, string owner, int vehicleClass, DateTime date)
        {
            Plate = plate;
            Owner = owner;
            VehicleClass = vehicleClass;
            ParkTime = date;
        }
        #endregion

        #region Methods
        public IVehicle GetVehicle() => GameMode.Instance.VehicleManager.GetVehicleByPlate(Plate);
        public VehicleHandler GetVehicleHandler() => GameMode.Instance.VehicleManager.GetVehicleByPlate(Plate)?.GetVehicleHandler();
        #endregion
    }

    class Parking
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
        public int Limite;
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

        [BsonIgnore]
        public OnVehicleStoredEvent OnVehicleStored { get; set; }
        [BsonIgnore]
        public OnVehicleOutEvent OnVehicleOut { get; set; }
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
        public Parking(Vector3 location = new Vector3(), Location spawn1 = null, Location spawn2 = null, string name = "", string owner = "", int price = 0, int limite = 999, bool hidden = true)
        {
            ID = GenerateRandomID();
            Name = name;
            Price = price;
            Hidden = hidden;
            Location = location;
            Spawn1 = spawn1;
            Spawn2 = spawn2;
            Owner = owner;
            Limite = limite;
        }
        #endregion

        #region Event handlers
        private async void OnPlayerEnterColShape(IColShape colShape, IPlayer client)
        {
            if (colShape != ParkingColshape || !client.Exists)
                return;

            if (ParkingType != ParkingType.Society)
                await OpenParkingMenu(client);
        }

        private async void OnPlayerLeaveColShape(IColShape colShape, IPlayer client)
        {
            if (colShape != ParkingColshape || !client.Exists)
                return;

            PlayerHandler player = client.GetPlayerHandler();

            if (player != null && player.HasOpenMenu())
                await MenuManager.CloseMenu(client);
        }

        private void OnVehicleEnterColShape(IColShape colShape, IVehicle vehicle)
        {
            if (colShape != ParkingColshape || !vehicle.Exists || vehicle.Driver == null)
                return;

            OnPlayerEnterColShape(colShape, vehicle.Driver);
        }

        private void OnVehicleLeaveColShape(IColShape colShape, IVehicle vehicle)
        {
            if (colShape != ParkingColshape || !vehicle.Exists || vehicle.Driver == null)
                return;

            OnPlayerLeaveColShape(colShape, vehicle.Driver);
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
                    if (!await VehicleManager.IsVehicleInSpawn(menu.GetData("Location")))
                    {
                        Spawn = menu.GetData("Location");
                        await veh.SpawnVehicle(Spawn);
                    }
                }
                else
                {
                    // Attribution du spawn de libre
                    if (!VehicleManager.IsVehicleInSpawn(Spawn1.Pos))
                    {
                        Spawn = Spawn1;
                        await veh.SpawnVehicle(Spawn1);
                    }
                    else if (Spawn2 != null && !VehicleManager.IsVehicleInSpawn(Spawn2.Pos))
                    {
                        Spawn = Spawn2;
                        await veh.SpawnVehicle(Spawn2);
                    }
                    else
                    {
                        // Aucun spawn de libre on stop tout.
                        await client.SendNotificationError("Aucune place de disponible pour sortir votre véhicule.");
                        return;
                    }
                }

                // if (ParkingType != ParkingType.Public)
                //     await client.PutIntoVehicleAsync(veh.Vehicle, -1);

                RemoveVehicle(veh); // retrait du véhicule dans la liste
                await OnVehicleOut?.Invoke(client, veh, Spawn); // callback (ex carpark)
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
        public async Task Load(float markerscale = 3f, int opacite = 128, bool blip = false, uint sprite = 50, float scale = 1f, byte color = 0, uint alpha = 255, string name = "", uint dimension = (uint)short.MaxValue)
        {
            GameMode.Instance.Streamer.addEntityMarker(Streamer.Data.MarkerType.VerticalCylinder, Location - new Vector3(0.0f, 0.0f, markerscale-1), new Vector3(3,3,3), 180);
            ParkingColshape = Alt.CreateColShapeCylinder(new AltV.Net.Data.Position(Location.X, Location.Y, Location.Z -1), markerscale, 4);
            GameMode.Instance.Streamer.addEntityTextLabel(this.Name + "\n~o~Approchez pour interagir", Location, 4);

            if (blip)
                GameMode.Instance.Streamer.addStaticEntityBlip(name, Location,color,(int) sprite);

            Events.OnPlayerEnterColShape += OnPlayerEnterColShape;
            Events.OnPlayerLeaveColShape += OnPlayerLeaveColShape;
            Events.OnVehicleEnterColShape += OnVehicleEnterColShape;
            Events.OnVehicleLeaveColShape += OnVehicleLeaveColShape;

            ParkingList.Add(this);
            await Task.CompletedTask;
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
                    await client.SendNotificationError("Vous n'êtes pas autorisé à utiliser ce parking.");
                    return;
                }
            }

            if (menu == null)
                menu = new Menu("ID_ParkingMenu", (string.IsNullOrEmpty(title) ? "Parking" : title), (string.IsNullOrEmpty(description)) ? "" : description, 0, 0, Menu.MenuAnchor.MiddleRight, false, true, true);
            else
            {
                menu.ClearItems();
                menu.BackCloseMenu = false;
                menu.ItemSelectCallback = menuCallback;
            }

            if (await client.IsInVehicleAsync() && await client.GetSeatAsync() == 1 ) // I store my vehicle
            {
                var vehplayer = await client.GetVehicleAsync();

                if (!vehplayer.Exists)
                    return;

                VehicleHandler vehicle = VehicleManager.GetHandlerByVehicle(vehplayer);

                if (vehicle == null)
                    return;

                // some check
                if (vehicle.SpawnVeh)
                {
                    await client.SendNotificationError("Vous ne pouvez pas garer un véhicule de location dans le parking!");
                    return;
                }

                if (ListVehicleStored.Count + 1 >= Limite)
                {
                    await client.SendNotificationError("Le parking est plein!");
                    return;
                }
                // it's ok!
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
                    vehicleListParked = ListVehicleStored.FindAll(p => p.Owner == social || ph.ListVehicleKey.Exists(v => v.Plate == p.Plate));
                else
                    vehicleListParked = ListVehicleStored.FindAll(p => (p.Owner == social || ph.ListVehicleKey.Exists(v => v.Plate == p.Plate) && p.VehicleClass == vehicleType));

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
                    await client.SendNotificationError("Vous n'avez aucun véhicule dans ce parking.");
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

            await client.EmitAsync("toggleControl", false);

            try
            {
                if (!await client.GetPlayerHandler().HasMoney(Price))
                {
                    await client.SendNotificationError($"Vous n'avez pas les ${Price} en poche pour payer le ticket.");
                    return;
                }

                VehicleHandler veh = VehicleManager.GetHandlerByVehicle(vh);

                if (veh != null)
                {
                    if (veh.IsParked)
                        return;

                    if (GameMode.Instance.IsDebug)
                        Alt.Server.LogColored($"~b~Parking ~w~| New vehicle in parking id {this.ID}");

                    await vh.SetEngineOnAsync(false);
                    veh.Locked = true;
                    veh.LastUse = DateTime.Now; // refresh the last use

                    lock (ListVehicleStored)
                    {
                        ListVehicleStored.Add(new Models.ParkedCar(veh.Plate, veh.OwnerID, veh.VehicleManifest.VehicleClass, DateTime.Now)); // Store vehicle into a list
                        veh.IsParked = true;
                    }
                    
                    await veh.Update();
                    await veh.Delete(false);
                    await OnVehicleStored?.Invoke(client, veh); // call event for success storage
                }
                else
                {
                    Alt.Server.LogError("GetHandlerByVehicle fuck is null this shit! mother fucker!");
                }
                //await MenuManager.CloseMenu(client); TODO
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("StoreVehicle" + ex);
            }

            await client.EmitAsync("toggleControl", true);
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
