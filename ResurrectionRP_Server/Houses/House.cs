using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Entities.Blips;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Houses
{
    public class House
    {
        #region Constants
        private const ushort DIMENSION_START = 1000;
        #endregion

        #region Fields and properties
        [BsonId]
        public int ID { get; set; }

        private string _owner;
        public string Owner {
            get => _owner;
            private set {
                if (string.IsNullOrEmpty(value))
                {
                    if (Marker != null) Marker.SetColor(Color.FromArgb(80, 255, 0, 0));
                }
                else
                {
                    if (Marker != null) Marker.SetColor(Color.FromArgb(80, 255, 255, 255));
                }

                _owner = value;
            }
        }

        public int Type;
        public Vector3 Position;
        public int Price;
        public bool Locked;
        public Parking Parking;

        // customization
        public string Name;

        // storage
        public Inventory.Inventory Inventory;

        // entities
        private Marker Marker;
        private IColShape ColShapeEnter;
        private IColShape ColShapeOut;
        private IPlayer OwnerHandle;

        // misc
        [JsonIgnore, BsonIgnore]
        public List<IPlayer> PlayersInside = new List<IPlayer>();
        #endregion

        #region Constructor
        public House(int id, string owner, int type, Vector3 position, int price, bool locked, string name = "", Parking parking = null)
        {
            ID = id;
            Owner = owner;
            Type = type;
            Position = position;
            Price = price;
            Locked = locked;

            Name = (string.IsNullOrEmpty(name) ? "Logement" : name);
            Parking = parking;

            if (Inventory == null)
                Inventory = new Inventory.Inventory(HouseTypes.HouseTypeList[Type].InventorySize, 40);
        
            OwnerHandle = null;
        }
        #endregion

        #region Init
        public void Init()
        {
            // create marker
            Marker = Marker.CreateMarker(MarkerType.VerticalCylinder, Position - new Vector3(0.0f, 0.0f, 1.0f), new Vector3(1.0f, 1.0f, 1.0f), Color.FromArgb(80, 255, 0, 0));

            // create colshape
            ColShapeEnter = Alt.CreateColShapeCylinder(Position - new Vector3(0, 0, 1), 1f, 3f);
            ColShapeEnter.Dimension = GameMode.GlobalDimension;
            ColShapeEnter.SetData("House", ID);
            ColShapeEnter.SetOnPlayerEnterColShape(OnPlayerEnterColshape);
            ColShapeEnter.SetOnPlayerLeaveColShape(OnPlayerLeaveColshape);

            ColShapeOut = Alt.CreateColShapeCylinder(HouseTypes.HouseTypeList[Type].Position.Pos - new Vector3(0.0f, 0.0f, 1.0f), 1f, 3f);
            ColShapeOut.Dimension = (short)(DIMENSION_START + ID);
            ColShapeOut.SetData("House", ID);
            ColShapeOut.SetOnPlayerEnterColShape(OnPlayerEnterColshape);
            ColShapeOut.SetOnPlayerInteractInColShapeAsync(OnPlayerInteractInColShape);

            InitParking();

            if (!string.IsNullOrEmpty(Owner))
                Marker.SetColor(Color.FromArgb(80, 255, 255, 255));

            if (GameMode.Instance.IsDebug)
                BlipsManager.CreateBlip(Name, Position, 4, 1);

            this.Inventory.MaxSlot = 40;

            if (Inventory.InventoryList.Length != Inventory.MaxSlot)
                Array.Resize(ref Inventory.InventoryList, Inventory.MaxSlot);

            PlayersInside = new List<IPlayer>();
        }
        #endregion

        #region Event handlers
        private void OnPlayerEnterParking(PlayerHandler player, Parking parking)
        {
            OpenParkingMenu(player?.Client);
        }

        private void OnVehicleEnterParking(VehicleHandler vehicle, Parking parking)
        {
            OpenParkingMenu(vehicle?.Vehicle?.Driver);
        }

        private void OnPlayerEnterColshape(IColShape colShape, IPlayer client)
        {
            if (colShape == ColShapeEnter)
                HouseManager.OpenHouseMenu(client, this);
            else if (colShape == ColShapeOut)
                client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~ pour intéragir", 5000);
        }

        private void OpenParkingMenu(IPlayer player)
        {
            if (player == null || !player.Exists)
                return;

            if (Owner == player.GetSocialClub())
                Parking.OpenParkingMenu(player, "", (player.GetPlayerHandler()?.StaffRank > AdminRank.Player) ? $"Logement {ID.ToString()}" : "Choisissez une option :", true);
            else
                player.SendNotificationError("Vous n'êtes pas autorisé à utiliser ce parking.");
        }

        private void OnPlayerLeaveColshape(IColShape colShape, IPlayer client)
        {
            PlayerHandler ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            if (ph.HasOpenMenu())
                MenuManager.CloseMenu(client);
        }

        private async Task OnPlayerInteractInColShape(IColShape colShape, IPlayer client)
        {
            await RemovePlayer(client, true);
        }

        private async Task OnParkingSaveNeeded()
        {
            await Save();
        }

        private async Task OnVehicleStored(IPlayer client, VehicleHandler vehicle)
        {
            vehicle.ParkingName = $"{Name} {ID}";
            await Save();
        }

        private async Task OnVehicleOutParking(IPlayer client, VehicleHandler vehicle, Location location)
        {
            await Save();
            client.SetPlayerIntoVehicle(vehicle.Vehicle);
        }
        #endregion

        #region Methods
        public void InitParking()
        {
            if (Parking != null)
            {
                Parking.Location = Parking.Spawn1.Pos;
                Parking.Init();
                Parking.Owner = Owner;
                Parking.ParkingType = ParkingType.House;
                Parking.OnSaveNeeded = OnParkingSaveNeeded;
                Parking.OnPlayerEnterParking += OnPlayerEnterParking;
                Parking.OnVehicleEnterParking += OnVehicleEnterParking;
                Parking.OnVehicleStored += OnVehicleStored;
                Parking.OnVehicleOut += OnVehicleOutParking;
            }
        }
        public async Task SetOwner(string owner)
        {
            if (OwnerHandle != null) OwnerHandle.EmitLocked("ResetHouseBlip", ID);
            if (!string.IsNullOrEmpty(owner))
            {
                Owner = owner;
                var player = PlayerManager.GetPlayerBySCN(owner);

                if (player == null)
                    return;

                SetOwnerHandle(player.Client);
            }
            else
            {
                Owner = null;
            }

            await Save();
        }

        public void SetOwnerHandle(IPlayer player)
        {
            OwnerHandle = player;   
            if (player != null)
                player.CreateBlip(411, Position, Name, 1, 69, 255, true);
        }

        public async Task SetName(string new_name)
        {
            Name = new_name;
            await Save();
        }

        public async Task SetLock(bool locked)
        {
            Locked = locked;
            await Save();
        }

        public async Task SetType(int new_type)
        {
            Type = new_type;
            await Save();
        }

        public async Task SetPrice(int new_price)
        {
            Price = new_price;
            await Save();
        }

        public bool SetIntoHouse(IPlayer client) 
            => HouseManager.SetIntoHouse(client, this);

        public bool RemoveFromHouse(IPlayer client) 
            => HouseManager.RemoveClientHouse(client);

        public async Task SendPlayer(IPlayer player)
        {
            if (!player.Exists)
                return;

            if (!SetIntoHouse(player))
                Alt.Server.LogWarning($"Player {player.GetPlayerHandler().Identite.Name} trying to enter house {ID} but already registered in another house");
            else
            {
                await player.SetDimensionAsync((short)(DIMENSION_START + ID));
                await player.SetPositionAsync(HouseTypes.HouseTypeList[Type].Position.Pos);

                // BUG v801: Set rotation when player in game not working
                await player.EmitAsync("setEntityHeading", player, HouseTypes.HouseTypeList[Type].Position.Rot.Z);
                // await player.SetRotationAsync(HouseTypes.HouseTypeList[Type].Position.Rot);
            }
        }

        public async Task RemovePlayer(IPlayer player, bool set_pos = true)
        {
            if (!await player.ExistsAsync())
                return;

            if (!RemoveFromHouse(player))
                Alt.Server.LogWarning($"Exiting unregistered player {player.GetPlayerHandler().Identite.Name} from house {ID}");

            if (set_pos)
            {
                await player.SetPositionAsync(Position);
                await player.SetDimensionAsync(GameMode.GlobalDimension);
            }
        }

        public async Task RemoveAllPlayers()
        {
            foreach (IPlayer player in PlayersInside)
            {
                if (!RemoveFromHouse(player))
                    Alt.Server.LogWarning($"Exiting unregistered player {player.GetPlayerHandler().Identite.Name} from house {ID}");
                else
                {
                    await player.SetPositionAsync(Position);
                    await player.SetDimensionAsync(GameMode.GlobalDimension);
                }         
            }
        }

        public async Task InsertHouse() => await Database.MongoDB.Insert<House>("houses", this);

        public async Task Save() => await Database.MongoDB.Update(this, "houses", ID);

        public async Task RemoveInDatabase() => await Database.MongoDB.Delete<House>("houses", ID);

        public async Task Destroy()
        {
            if (Marker != null || ColShapeEnter != null)
            {
                await RemoveAllPlayers();
                Marker.Destroy();
                ColShapeEnter.Remove();
            }
        }

        public static async Task<int> GetID()
        {
            var housesList = await Database.MongoDB.GetCollectionSafe<House>("houses").AsQueryable().ToListAsync();
            int id = 0;
            for(int b = 0; b < housesList.Count; b++)
            {
                if (housesList[b].ID > id) id = housesList[b].ID;
            }

            return id +1;
        }
        #endregion
    }
}