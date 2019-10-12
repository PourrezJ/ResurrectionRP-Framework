using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using ResurrectionRP_Server.Colshape;
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
    public partial class House
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
                    if (Marker != null)
                        Marker.SetColor(Color.FromArgb(80, 255, 0, 0));
                }
                else
                {
                    if (Marker != null)
                        Marker.SetColor(Color.FromArgb(80, 255, 255, 255));
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
        private IColshape ColshapeEnter;
        private IColshape ColshapeOut;
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
            ColshapeEnter = ColshapeManager.CreateCylinderColshape(Position - new Vector3(0, 0, 1), 1f, 3f);
            ColshapeEnter.SetData("House", ID);
            ColshapeEnter.OnPlayerEnterColshape += OnPlayerEnterColshape;
            ColshapeEnter.OnPlayerLeaveColshape += OnPlayerLeaveColshape;

            ColshapeOut = ColshapeManager.CreateCylinderColshape(HouseTypes.HouseTypeList[Type].Position.Pos - new Vector3(0.0f, 0.0f, 1.0f), 1f, 3f, (short)(DIMENSION_START + ID));
            ColshapeOut.SetData("House", ID);
            ColshapeOut.OnPlayerEnterColshape += OnPlayerEnterColshape;
            ColshapeOut.OnPlayerInteractInColshape += OnPlayerInteractInColshape;

            InitParking();

            if (!string.IsNullOrEmpty(Owner))
                Marker.SetColor(Color.FromArgb(80, 255, 255, 255));

            if (GameMode.IsDebug)
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

        private void OnPlayerEnterColshape(IColshape colshape, IPlayer client)
        {
            if (colshape == ColshapeEnter)
                HouseManager.OpenHouseMenu(client, this);
            else if (colshape == ColshapeOut)
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

        private void OnPlayerLeaveColshape(IColshape colshape, IPlayer client)
        {
            PlayerHandler ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            if (ph.HasOpenMenu())
                MenuManager.CloseMenu(client);
        }

        private void OnPlayerInteractInColshape(IColshape colshape, IPlayer client)
        {
            RemovePlayer(client, true);
        }

        private Task OnParkingSaveNeeded()
        {
            UpdateInBackground();
            return Task.CompletedTask;
        }

        private Task OnVehicleStored(IPlayer client, VehicleHandler vehicle)
        {
            vehicle.ParkingName = $"{Name} {ID}";
            UpdateInBackground();
            return Task.CompletedTask;
        }

        private Task OnVehicleOutParking(IPlayer client, VehicleHandler vehicle, Location location)
        {
            UpdateInBackground();
            client.SetPlayerIntoVehicle(vehicle.Vehicle);
            return Task.CompletedTask;
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
        public void SetOwner(string owner)
        {
            if (OwnerHandle != null)
                OwnerHandle.EmitLocked("ResetHouseBlip", ID);

            if (!string.IsNullOrEmpty(owner))
            {
                Owner = owner;
                var player = PlayerManager.GetPlayerBySCN(owner);

                if (player == null)
                    return;

                SetOwnerHandle(player.Client);
            }
            else
                Owner = null;

            UpdateInBackground();
        }

        public void SetOwnerHandle(IPlayer player)
        {
            OwnerHandle = player;   
            if (player != null)
                player.CreateBlip(411, Position, Name, 1, 69, 255, true);
        }

        public void SetName(string new_name)
        {
            Name = new_name;
            UpdateInBackground();
        }

        public void SetLock(bool locked)
        {
            Locked = locked;
            UpdateInBackground();
        }

        public void SetType(int new_type)
        {
            Type = new_type;
            UpdateInBackground();
        }

        public void SetPrice(int new_price)
        {
            Price = new_price;
            UpdateInBackground();
        }

        public bool SetIntoHouse(IPlayer client) 
            => HouseManager.SetIntoHouse(client, this);

        public bool RemoveFromHouse(IPlayer client) 
            => HouseManager.RemoveClientHouse(client);

        public void SendPlayer(IPlayer player)
        {
            AltAsync.Do(() =>
            {
                if (!player.Exists)
                    return;

                if (!SetIntoHouse(player))
                    Alt.Server.LogWarning($"Player {player.GetPlayerHandler().Identite.Name} trying to enter house {ID} but already registered in another house");
                else
                {
                    player.Dimension = (short)(DIMENSION_START + ID);
                    player.Position = HouseTypes.HouseTypeList[Type].Position.Pos;

                    // BUG v801: Set rotation when player in game not working
                    player.SetHeading(HouseTypes.HouseTypeList[Type].Position.Rot.Z);
                    // player.Rotation = HouseTypes.HouseTypeList[Type].Position.Rot.ConvertRotationToRadian();
                }
            }).Wait();
        }

        public void RemovePlayer(IPlayer player, bool set_pos = true)
        {
            AltAsync.Do(() =>
            {
                if (!player.Exists)
                    return;

                if (!RemoveFromHouse(player))
                    Alt.Server.LogWarning($"Exiting unregistered player {player.GetPlayerHandler().Identite.Name} from house {ID}");

                if (set_pos)
                {
                    player.Position = Position;
                    player.Dimension = GameMode.GlobalDimension;
                }
            }).Wait();
        }

        public void RemoveAllPlayers()
        {
            foreach (IPlayer player in PlayersInside)
            {
                if (!RemoveFromHouse(player))
                    Alt.Server.LogWarning($"Exiting unregistered player {player.GetPlayerHandler().Identite.Name} from house {ID}");
                else
                {
                    player.Position = Position;
                    player.Dimension = GameMode.GlobalDimension;
                }         
            }
        }

        public void Destroy()
        {
            if (Marker != null || ColshapeEnter != null)
            {
                RemoveAllPlayers();
                Marker.Destroy();
                // ColshapeEnter.Remove();
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