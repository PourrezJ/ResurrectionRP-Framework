using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System;
using MongoDB.Bson.Serialization.Attributes;
using System.Numerics;
using System.Drawing;
using MongoDB.Driver;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Inventory;
using ResurrectionRP_Server.Streamer.Data;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server;
using ResurrectionRP_Server.Utils.Enums;
using AltV.Net.Async;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Entities.Blips;
using MongoDB.Bson;

namespace ResurrectionRP_Server.Houses
{
    #region House Class
    public class House
    {
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

        public void Load()
        {
            // create marker
            Marker = Marker.CreateMarker(MarkerType.VerticalCylinder, Position - new Vector3(0.0f, 0.0f, 1.0f), new Vector3(1.0f, 1.0f, 1.0f), Color.FromArgb(80, 255, 0, 0));

            // create colshape
            ColShapeEnter = AltV.Net.Alt.CreateColShapeCylinder(Position - new Vector3(0, 0, 1), 1f, 3f);
            ColShapeEnter.Dimension = GameMode.GlobalDimension;
            ColShapeEnter.SetData("House", this.ID);
            ColShapeEnter.SetOnPlayerEnterColShape(OnPlayerEnterColshape);

            ColShapeOut = AltV.Net.Alt.CreateColShapeCylinder(HouseTypes.HouseTypeList[Type].Position.Pos - new Vector3(0.0f, 0.0f, 1.0f), 1f, 3f);
            ColShapeOut.Dimension = (short)ID;
            ColShapeOut.SetData("House", this.ID);
            ColShapeOut.SetOnPlayerEnterColShape(OnPlayerEnterColshape);

            IColShape parkingColshape = null;
            if (Parking != null)
            {
                parkingColshape = AltV.Net.Alt.CreateColShapeCylinder(Parking.Spawn1.Pos - new Vector3(0, 0, 1), 3f, 3f);
                Parking.OnSaveNeeded = OnParkingSaveNeeded;
                Parking.OnVehicleStored = OnVehicleStored;
                Parking.OnVehicleOut = OnVehicleOutParking;
                Parking.ParkingType = ParkingType.House;

                Parking.Location = Parking.Spawn1.Pos;
                Parking.Load();
            }

            if (!string.IsNullOrEmpty(Owner))
            {
                Marker.SetColor(Color.FromArgb(80, 255, 255, 255));
            }

            if (parkingColshape != null)
                parkingColshape.SetMetaData("House_Parking", ID);


            if (GameMode.Instance.IsDebug)
                BlipsManager.CreateBlip(Name, Position, 4, 1);

            this.Inventory.MaxSlot = 40;
            if (Inventory.InventoryList.Length != Inventory.MaxSlot)
            {
                Array.Resize<ItemStack>(ref Inventory.InventoryList, Inventory.MaxSlot);
            }

            PlayersInside = new List<IPlayer>();
        }

        private async Task OnPlayerEnterColshape(IColShape colShape, IPlayer client)
        {
            if (colShape == ColShapeEnter)
                await HouseManager.OpenHouseMenu(client, this);
            else
                client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~ pour intéragir", 5000);
        }

        private async Task OnParkingSaveNeeded()
        {
            await Save();
        }

        private async Task OnVehicleStored(IPlayer client, VehicleHandler vehicle)
        {
            await Save();
        }

        private async Task OnVehicleOutParking(IPlayer client, VehicleHandler vehicle, Location location)
        {
            await Save();
            await vehicle.PutPlayerInVehicle(client);
        }

        public async Task Parking_onEntityEnterColShape(IColShape colShape, IPlayer client)
        {
            if (client.GetSocialClub() == Owner && Parking != null)
            {
                await Parking.OpenParkingMenu(client, "", ((client.GetPlayerHandler()?.StaffRank > AdminRank.Player) ? ID.ToString() : ""));
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

        public async Task SetOwner(IPlayer player)
        {
            Owner = (player == null) ? string.Empty : player.GetSocialClub();
            SetOwnerHandle(player);
            await Save();
        }

        public void SetOwnerHandle(IPlayer player)
        {
            OwnerHandle = player;
            /*
            if (player != null)
                await player.CreateBlip(411, Position, Name, 1, 69, 255, 1, true);*/
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
        public bool RemoveIntoHouse(IPlayer client) 
            => HouseManager.RemoveClientHouse(client);

        public async Task SendPlayer(IPlayer player)
        {
            if (SetIntoHouse(player))
            {
                await player.SetPositionAsync(HouseTypes.HouseTypeList[Type].Position.Pos);
                await player.SetRotationAsync(HouseTypes.HouseTypeList[Type].Position.Rot);
                await player.SetDimensionAsync((short)ID);
            }
        }

        public async Task RemovePlayer(IPlayer player, bool set_pos = true)
        {
            if (RemoveIntoHouse(player) && set_pos)
            {
                await player.SetPositionAsync(Position);
                await player.SetDimensionAsync(GameMode.GlobalDimension);
            }
        }

        public async Task RemoveAllPlayers(bool exit = false)
        {
            for (int i = PlayersInside.Count - 1; i >= 0; i--)
            {
                IPlayer player = PlayersInside[i];
                if (player != null && RemoveIntoHouse(player))
                {
                    await player.SetPositionAsync(Position);
                    await player.SetDimensionAsync(GameMode.GlobalDimension);
                    PlayersInside.RemoveAt(i);
                }         
            }
        }

        public async Task InsertHouse() => await Database.MongoDB.Insert<House>("houses", this);

        public async Task Save() => await Database.MongoDB.Update(this, "houses", ID);

        public async Task RemoveInDatabase() => await Database.MongoDB.Delete<House>("houses", ID);

        public async Task Destroy(bool exit = false)
        {
            if (Marker != null || ColShapeEnter != null)
            {
                await RemoveAllPlayers(exit);
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
    }
    #endregion
}