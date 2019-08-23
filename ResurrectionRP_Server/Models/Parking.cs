
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Elements.Entities;
using VehicleHandler = ResurrectionRP_Server.Entities.Vehicles.VehicleHandler;
using VehicleManager = ResurrectionRP_Server.Entities.Vehicles.VehiclesManager;
using AltV.Net.ColShape;

namespace ResurrectionRP_Server.Models
{
    public enum ParkingType
    {
        Public,
        House,
        Society
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

        private List<string> _listvehiclestored = new List<string>();
        public List<string> ListVehicleStored
        {
            get
            {
                if (_listvehiclestored == null)
                    _listvehiclestored = new List<string>();
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
        public AltV.Net.ColShape.IColShape ParkingColshape { get; private set; }
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
            Alt.OnColShape += OnEnterColShape;
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
        /*        private async Task StoreVehicle(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex, dynamic data)
                {
                    var vehicle = await client.GetVehicleAsync();
                    await StoreVehicle(client, vehicle);
                }*/
        private async Task GetVehicle(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex, dynamic data)
        {
            try
            {
                VehicleHandler veh = menuItem.GetData("Vehicle"); // choix du véhicule choisi dans la liste de nos véhicule dans le parking

                Location Spawn = null;

                if (menu.GetData("Location") != null)
                {
                    if (!await VehicleManager.GetVehicleInSpawn(menu.GetData("Location")))
                    {
                        Spawn = menu.GetData("Location");
                        await veh.SpawnVehicle(client, Spawn);
                    }
                }
                else
                {
                    // Attribution du spawn de libre
                    if (!await VehicleManager.GetVehicleInSpawn(Spawn1.Pos))
                    {
                        Spawn = Spawn1;
                        await veh.SpawnVehicle(client, Spawn1);
                    }
                    else if (Spawn2 != null && !await VehicleManager.GetVehicleInSpawn(Spawn2.Pos))
                    {
                        Spawn = Spawn2;
                        await veh.SpawnVehicle(client, Spawn2);
                    }
                    else
                    {
                        // Aucun spawn de libre on stop tout.
                        await client.SendNotificationError("Aucune place de disponible pour sortir votre véhicule.");
                        return;
                    }
                }

                Spawn = Spawn2;
                if (ParkingType == ParkingType.House)
                    await client.PutIntoVehicleAsync(veh.Vehicle, -1);

                RemoveVehicle(veh); // retrait du véhicule dans la list.
                await OnVehicleOut?.Invoke(client, veh, Spawn); // callback (ex carpark)
                await MenuManager.CloseMenu(client);
                await veh.InsertVehicle();
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
        public static async void OnEnterColShape(AltV.Net.Elements.Entities.IColShape colshapePointer, IEntity client, bool state)
        {
            if (!client.Exists)
                return;
            if (client.Type != 0)
                return;
            var player = client as IPlayer;
            Parking parking = ParkingList.Find(p => p.ParkingColshape == colshapePointer);

            if (parking != null && parking.ParkingType != ParkingType.Society)
                await parking.OpenParkingMenu(player);
        }

        public async Task Load(float markerscale = 3f, int opacite = 128, bool blip = false, uint sprite = 50, float scale = 1f, byte color = 0, uint alpha = 255, string name = "", uint dimension = MP.GlobalDimension)
        {
            await MP.Markers.NewAsync(MarkerType.VerticalCylinder, Location - new Vector3(0.0f, 0.0f, markerscale), new Vector3(), new Vector3(), 3f, Color.FromArgb(180, 255, 255, 255), true, dimension);
            ParkingColshape = await MP.Colshapes.NewTubeAsync(Location - new Vector3(0.0f, 0.0f, markerscale), markerscale, dimension);

            if (blip)
                await MP.Blips.NewAsync(sprite, Location, scale, color, name, alpha, 10, true);
            ParkingList.Add(this);
        }

        public async Task OpenParkingMenu(IPlayer client, string title = "", string description = "", bool canGetAllVehicle = false, Location location = null, int vehicleType = -1)
        {
            if (!client.Exists)
                return;

            if (Whitelist != null && Whitelist.Count > 0)
            {
                var social = await client.GetSocialClubNameAsync();

                if (!Whitelist.Contains(social))
                {
                    await client.SendNotificationError("Vous n'êtes pas autorisé à utiliser ce parking.");
                    return;
                }
            }

            Menu ParkingMenu = new Menu("ID_ParkingMenu", (string.IsNullOrEmpty(title) ? "Parking" : title), (string.IsNullOrEmpty(description)) ? "" : description, 0, 0, Menu.MenuAnchor.MiddleLeft, false, true, false);
            if (await client.IsInVehicleAsync() && await client.GetSeatAsync() == -1) // i'm store my vehicle
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
                    await client.SendNotificationError("Vous ne pouvez pas garer dans le parking un véhicule de location!");
                    return;
                }

                if (ListVehicleStored.Count + 1 > Limite)
                {
                    await client.SendNotificationError("Le parking est plein!");
                    return;
                }
                // it's ok!
                var item = new MenuItem("Ranger votre voiture", "", "ID_StoreVehicle", true, rightLabel: $"{((Price > 0) ? "$" + Price.ToString() : "")}");
                item.OnMenuItemCallback = StoreVehicle;
                ParkingMenu.Add(item);
            }
            else // i'm want my vehicle
            {
                ParkingMenu.SubTitle = "Quel véhicule souhaitez-vous récupérer :";
                var ph = client.GetPlayerHandler();
                if (ph == null) return;

                var social = await client.GetSocialClubNameAsync();
                if (!canGetAllVehicle)
                {
                    List<VehicleHandler> vehicleList = null;

                    if (vehicleType == -1)
                        vehicleList = ListVehicleStored.FindAll(p => p.OwnerID == social || ph.ListVehicleKey.Exists(v => v.Plate == p.Plate));
                    else
                        vehicleList = ListVehicleStored.FindAll(p => (p.OwnerID == social || ph.ListVehicleKey.Exists(v => v.Plate == p.Plate)) && p.VehicleManifest?.VehicleClass == vehicleType);

                    if (vehicleList.Count > 0)
                    {
                        foreach (var veh in vehicleList)
                        {
                            try
                            {
                                if (veh.VehicleManifest == null)
                                {
                                    veh.VehicleManifest = VehicleInfoLoader.VehicleInfoLoader.Get(veh.Model) ?? null;
                                }

                                string _description =
                                    $"~g~Essence:~w~ {veh.VehicleSync.Fuel} \n" +
                                    $"~g~Etat: ~w~{(veh.VehicleSync.BodyHealth * 0.1)} \n" +
                                    $"{((ParkingType == ParkingType.Public) ? $"~g~Fin Horodateur:~w~ {veh.LastUse.AddMonths(1).ToShortDateString()}" : "")}";

                                MenuItem item = new MenuItem(string.IsNullOrEmpty(veh.VehicleManifest.LocalizedName) ? veh.VehicleManifest.DisplayName : veh.VehicleManifest.LocalizedName, _description, "", true, rightLabel: veh.Plate);
                                item.SetData("Vehicle", veh);
                                item.OnMenuItemCallback = GetVehicle;
                                ParkingMenu.Add(item);
                            }
                            catch (Exception ex)
                            {
                                MP.Logger.Error($"OpenParkingMenu: {veh.Plate} {this.ID} {this.Name} {await client.GetSocialClubNameAsync()}", ex);
                            }
                        }
                    }
                    else
                    {
                        await client.SendNotificationError("Vous n'avez aucun véhicule dans ce parking.");
                        return;
                    }
                }
                else
                {
                    if (ListVehicleStored.Count > 0)
                    {
                        foreach (var veh in ListVehicleStored)
                        {
                            try
                            {
                                if (veh.VehicleManifest == null)
                                {
                                    veh.VehicleManifest = VehicleInfoLoader.VehicleInfoLoader.Get(veh.Model) ?? null;
                                }

                                string _description =
                                    $"~g~Essence:~w~ {veh.VehicleSync.Fuel} \n" +
                                    $"~g~Etat: ~w~{(veh.VehicleSync.BodyHealth * 0.1)} \n" +
                                    $"{((ParkingType == ParkingType.Public) ? $"~g~Fin Horodateur:~w~ {veh.LastUse.AddMonths(1).ToShortDateString()}" : "")}";

                                MenuItem item = new MenuItem(string.IsNullOrEmpty(veh.VehicleManifest.LocalizedName) ? veh.VehicleManifest.DisplayName : veh.VehicleManifest.LocalizedName, _description, "", true, rightLabel: veh.Plate);
                                item.SetData("Vehicle", veh);
                                item.OnMenuItemCallback = GetVehicle;
                                ParkingMenu.Add(item);
                            }
                            catch (Exception ex)
                            {
                                MP.Logger.Error($"OpenParkingMenu: {veh.Plate} {this.ID} {this.Name} {await client.GetSocialClubNameAsync()}", ex);
                            }
                        }
                    }
                    else
                    {
                        await client.SendNotificationError("Vous n'avez aucun véhicule dans ce parking.");
                        return;
                    }
                }
            }

            ParkingMenu.SetData("Location", location);

            await ParkingMenu.OpenMenu(client);
        }

        public async Task StoreVehicle(IPlayer client, IVehicle vh)
        {
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
                    await veh.SetEngineState(false);
                    veh.Locked = true;
                    veh.LastUse = DateTime.Now; // refresh the last use

                    lock (ListVehicleStored)
                    {
                        if (ListVehicleStored.FirstOrDefault(v => v.Plate == veh.Plate) != null)
                            MP.Logger.Warn($"Parking '{Name}': Duplicate plate {veh.Plate}");
                        else
                            ListVehicleStored.Add(veh); // Store vehicle into a list
                    }

                    await veh.Delete(true);
                    await OnVehicleStored?.Invoke(client, veh); // call event for success storage
                }
                else
                {
                    MP.Logger.Info("GetHandlerByVehicle fuck is null this shit! mother fucker!");
                }
                await MenuManager.CloseMenu(client);
            }
            catch (Exception ex)
            {
                MP.Logger.Error("StoreVehicle", ex);
            }
        }

        public bool RemoveVehicle(VehicleHandler veh)
        {
            bool success = false;

            lock (ListVehicleStored)
            {
                VehicleHandler vehicle = ListVehicleStored.FirstOrDefault(v => v.Plate == veh.Plate);

                if (vehicle == null)
                    success = false;
                else
                    success = ListVehicleStored.Remove(vehicle);
            }

            if (!success)
                MP.Logger.Warn($"Parking '{Name}': Error removing vehicle {veh.Plate}");

            return success;
        }
        #endregion
    
    }
}
