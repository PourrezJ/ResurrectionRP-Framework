using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using ResurrectionRP_Server.Colshape;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Utils;
using Newtonsoft.Json;
using ResurrectionRP_Server.Streamer.Data;
using ResurrectionRP_Server.Entities.Blips;
using ResurrectionRP_Server.Database;

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
        public VehicleData GetVehicleHandler() 
            => VehiclesManager.GetVehicleDataWithPlate(Plate);
        #endregion
    }

    public class Parking
    {
        #region Delegates
        public delegate Task OnPlayerEnterParkingEvent(IPlayer client);
        public delegate Task OnVehicleStoredEvent(IPlayer client, VehicleHandler vehicle);
        public delegate Task OnVehicleOutEvent(IPlayer client, VehicleHandler vehicle, Location Spawn);
        public delegate Task OnSaveNeededDelegate();
        public delegate void OnPlayerParkingEvent(PlayerHandler player, Parking parking);
        public delegate void OnVehicleParkingEvent(VehicleHandler vehicle, Parking parking);
        #endregion

        #region Fields and properties
        public static List<Parking> ParkingList = new List<Parking>();

        public int ID;
        public string Name;
        public int Price;
        public bool Hidden;
        public string Owner; // Social Club
        [BsonIgnore]
        public TextLabel EntityLabel = null;
        [BsonIgnore]
        public Blips EntityBlip = null;
        [BsonIgnore]
        public Marker EntityMarker = null;


        private Vector3 _location = new Vector3();
        public Vector3 Location
        {
            get => _location;
            set
            {
                //Alt.Log("Location Parking changed " + JsonConvert.SerializeObject(value));
                _location = value;
            }
        }


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
        public event OnPlayerParkingEvent OnPlayerEnterParking;
        public event OnPlayerParkingEvent OnPlayerLeaveParking;
        public event OnVehicleParkingEvent OnVehicleEnterParking;
        public event OnVehicleParkingEvent OnVehicleLeaveParking;

        [BsonIgnore]
        public OnSaveNeededDelegate OnSaveNeeded { get; set; }
        [BsonIgnore]
        public IColshape ParkingColshape { get; private set; }
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
        private void OnPlayerEnterColshape(IColshape colshape, IPlayer client)
        {
            if (!client.Exists)
                return;

            if (OnPlayerEnterParking != null)
                OnPlayerEnterParking.Invoke(client.GetPlayerHandler(), this);
            else
                OpenParkingMenu(client);
        }

        private void OnPlayerLeaveColshape(IColshape colshape, IPlayer client)
        {
            if (!client.Exists)
                return;

            if (OnPlayerLeaveParking != null)
                OnPlayerEnterParking.Invoke(client.GetPlayerHandler(), this);
            else
            {
                PlayerHandler player = client.GetPlayerHandler();

                if (player != null && player.HasOpenMenu())
                    MenuManager.CloseMenu(client);
            }
        }

        private void OnVehicleEnterColshape(IColshape colshape, IVehicle vehicle)
        {
            if (!vehicle.Exists || vehicle.Driver == null)
                return;

            if (OnVehicleEnterParking != null)
                OnVehicleEnterParking.Invoke(vehicle.GetVehicleHandler(), this);
            else
                OnPlayerEnterColshape(colshape, vehicle.Driver);
        }

        private void OnVehicleLeaveColshape(IColshape colshape, IVehicle vehicle)
        {
            if (!vehicle.Exists || vehicle.Driver == null)
                return;

            if (OnVehicleLeaveParking != null)
                OnVehicleEnterParking.Invoke(vehicle.GetVehicleHandler(), this);
            else
                OnPlayerLeaveColshape(colshape, vehicle.Driver);
        }
        #endregion

        #region Private methods
        private int GenerateRandomID()
        {
            int number = Utils.Utils.RandomNumber(999999999);

            while (ParkingList.Exists(x => x.ID == number))
                number = Utils.Utils.RandomNumber(999999999);

            return number;
        }

        private void StoreVehicle(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            StoreVehicle(client, client.Vehicle);
            menu.CloseMenu(client);
        }

        private void GetVehicle(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            try
            {
                VehicleData veh = menuItem.GetData("Vehicle"); // choix du véhicule choisi dans la liste de nos véhicule dans le parking
                Location Spawn = null;

                if (menu.GetData("Location") != null)
                {
                    if (!VehiclesManager.IsVehicleInSpawn(menu.GetData("Location")))
                    {
                        Spawn = menu.GetData("Location");
                        veh.SpawnVehicle(Spawn);
                    }
                }
                else
                {
                    // Attribution du spawn de libre
                    if (!VehiclesManager.IsVehicleInSpawn(Spawn1.Pos))
                    {
                        Spawn = Spawn1;
                        veh.SpawnVehicle(Spawn1);
                    }
                    else if (Spawn2 != null && !VehiclesManager.IsVehicleInSpawn(Spawn2.Pos))
                    {
                        Spawn = Spawn2;
                        veh.SpawnVehicle(Spawn2);
                    }
                    else
                    {
                        // Aucun spawn de libre on stop tout.
                        client.SendNotificationError("Aucune place de disponible pour sortir votre véhicule.");
                        return;
                    }
                }

                if (veh.Vehicle == null)
                {
                    client.SendNotificationError($"Erreur avec votre véhicule, contactez le staff: {veh.Plate}");
                    return;
                }

                veh.EngineOn = false;
                RemoveVehicle(veh); // retrait du véhicule dans la liste

                if (OnVehicleOut != null)
                    Task.Run(()=> OnVehicleOut.Invoke(client, veh.Vehicle, Spawn)); // callback (ex carpark)

                veh.Vehicle.UpdateInBackground();
                MenuManager.CloseMenu(client);
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("Parking | " + ex.ToString());
            }
        }
        #endregion

        #region Public methods
        public void Init(float markerscale = 3f, int opacite = 128, bool blip = false, uint sprite = 50, float scale = 1f, byte color = 0, uint alpha = 255, string name = "", short dimension = GameMode.GlobalDimension)
        {
            EntityMarker = Marker.CreateMarker(MarkerType.VerticalCylinder, Location - new Vector3(0.0f, 0.0f, markerscale - 1), new Vector3(3, 3, 3));
            ParkingColshape = ColshapeManager.CreateCylinderColshape(new Position(Location.X, Location.Y, Location.Z - 1), markerscale, 4, dimension);
            ParkingColshape.OnPlayerEnterColshape += OnPlayerEnterColshape;
            ParkingColshape.OnPlayerLeaveColshape += OnPlayerLeaveColshape;
            ParkingColshape.OnVehicleEnterColshape += OnVehicleEnterColshape;
            ParkingColshape.OnVehicleLeaveColshape += OnVehicleLeaveColshape;

            if (!string.IsNullOrEmpty(Name))
                EntityLabel = Streamer.Streamer.AddEntityTextLabel($"{Name}\n~o~Approchez pour intéragir", Location, 4);
            else
                EntityLabel = Streamer.Streamer.AddEntityTextLabel("~o~Approchez pour intéragir", Location, 4);

            if (blip)
                EntityBlip=  Entities.Blips.BlipsManager.CreateBlip(name, Location,color,(int) sprite);

            ParkingList.Add(this);
        }

        public void OpenParkingMenu(IPlayer client, string title = "", string description = "", bool canGetAllVehicle = false, Location location = null, int vehicleType = -1, Menu menu = null, Menu.MenuCallback menuCallback = null)
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
                menu = new Menu("ID_ParkingMenu", string.IsNullOrEmpty(title) ? "Parking" : title, (string.IsNullOrEmpty(description)) ? "Choisissez une option :" : description, Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true);
            else
            {
                menu.ClearItems();
                menu.BackCloseMenu = false;
                menu.ItemSelectCallback = menuCallback;
            }

            if (client.IsInVehicle && client.Seat == 1) // I store my vehicle
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
                List<ParkedCar> vehicleListParked = new List<ParkedCar>();

                try
                {
                    if (canGetAllVehicle)
                        vehicleListParked = ListVehicleStored;
                    else if (vehicleType == -1)
                        vehicleListParked = ListVehicleStored.FindAll(p => p.GetVehicleHandler().OwnerID == social || ph.ListVehicleKey.Exists(v => v?.Plate == p?.Plate));
                    else
                        vehicleListParked = ListVehicleStored.FindAll(p => (p.GetVehicleHandler().OwnerID == social || ph.ListVehicleKey.Exists(v => v?.Plate == p.Plate)) /* && p.GetVehicleHandler().VehicleManifest.VehicleClass == vehicleType*/);
                }
                catch (Exception ex)
                {
                    Alt.Server.LogError($"OpenParkingMenu player: {client.GetSocialClub()} | parking: {Name} {ID} | {ex}");
                }

                var vehicleList = VehiclesManager.GetAllVehicles().Where(v => vehicleListParked.Select(p => p.Plate).Contains(v.Plate)).ToList();

                if (vehicleList.Count() > 0)
                {
                    foreach (var vehdata in vehicleList)
                    {
                        try
                        {
                            var manifest = VehicleInfoLoader.VehicleInfoLoader.Get(vehdata.Model) ?? null;

                            string _description =
                                $"~g~Essence:~w~ {vehdata.Fuel} \n" +
                                $"~g~Etat: ~w~{(vehdata.BodyHealth * 0.1)} \n" +
                                $"{((ParkingType == ParkingType.Public) ? $"~g~Fin Horodateur:~w~ {vehdata.LastUse.AddMonths(1).ToShortDateString()}" : "")}";

                            MenuItem item = new MenuItem(string.IsNullOrEmpty(manifest.LocalizedName) ? manifest.DisplayName : manifest.LocalizedName, _description, "", true, rightLabel: vehdata.Plate);
                            item.SetData("Vehicle", vehdata);
                            item.OnMenuItemCallback = GetVehicle;
                            menu.Add(item);
                        }
                        catch(Exception ex)
                        {
                            Alt.Server.LogError($"OpenParkingMenu: {vehdata.Plate} {ID} {Name} {client.GetSocialClub()} : " + ex);
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
            menu.OpenMenu(client);
        }

        public void StoreVehicle(IPlayer client, IVehicle vh, Location location = null)
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
                    if (veh.VehicleData.IsParked)
                        return;

                    if (GameMode.IsDebug)
                        Alt.Server.LogColored($"~b~Parking ~w~| New vehicle in parking id {ID}");

                    veh.EngineOn = false;
                    veh.LockState = VehicleLockState.Locked;

                    if (location != null)
                        veh.VehicleData.Location = location;
                    else
                        veh.VehicleData.Location = new Location(Location, veh.VehicleData.Location.Rot);

                    lock (ListVehicleStored)
                    {
                        if (ListVehicleStored.Find(p => p.Plate == veh.VehicleData.Plate) == null)
                            ListVehicleStored.Add(new ParkedCar(veh.VehicleData.Plate, DateTime.Now));
                    }

                    veh.VehicleData.IsParked = true;
                    veh.VehicleData.ParkingName = Name;
                    veh.VehicleData.UpdateMilageAndFuel();

                    if (OnVehicleStored != null)
                        Task.Run(() => OnVehicleStored.Invoke(client, veh)); // call event for success storage

                    veh.UpdateInBackground(true, true);
                    Task.Run(()=> veh.DeleteAsync(false));
                }
                else
                    Alt.Server.LogError("GetHandlerByVehicle fuck is null this shit! mother fucker!");

                MenuManager.CloseMenu(client);
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("StoreVehicle" + ex);
            }
        }

        public bool RemoveVehicle(VehicleData veh)
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

        public void Destroy()
        {
            ParkingColshape.Delete();

            if (EntityMarker != null)
                Marker.DestroyMarker(EntityMarker);
            if (EntityLabel != null)
                Streamer.Streamer.ListEntities[EntityLabel.id].Remove();
            if (EntityBlip != null)
                BlipsManager.Destroy(EntityBlip);
        }
        #endregion
    }
}
