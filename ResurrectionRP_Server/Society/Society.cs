
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Inventory;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Entities.Peds;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Bank;
using ResurrectionRP_Server.Society.Societies;
using ResurrectionRP_Server.Society.Societies.Bennys;
using ResurrectionRP_Server.Society.Societies.WildCustom;
using ResurrectionRP_Server.Society.Societies.WhiteWereWolf;
using ResurrectionRP_Server.Streamer.Data;
using ResurrectionRP_Server.Entities;

namespace ResurrectionRP_Server.Society
{
    [BsonIgnoreExtraElements]
    [BsonKnownTypes(typeof(Bennys), typeof(Unicorn), typeof(Sandjob), typeof(Rhumerie), typeof(PetrolSociety), typeof(Tequilala), typeof(WhiteWereWolf), typeof(PawnCar), typeof(WildCustom), typeof(BlackStreetNation), typeof(YellowJack))]
    public partial class Society
    {
        #region Static Variables
        [JsonIgnore]
        public static List<Society> JobsList = new List<Society>();

        [JsonIgnore]
        public static int PriceNameChange = 10000;
        #endregion

        #region Variables
        public BsonObjectId _id;

        [JsonIgnore]
        protected Entities.Blips.Blips Blip;

        public Dictionary<string, string> Employees = new Dictionary<string, string>(); // Liste des noms RP

        [BsonIgnore]
        public List<string> InService = new List<string>();

        public string SocietyName { get; set; }
        public Vector3 ServicePos { get; set; }
        public bool Resell { get; private set; }
        public int ResellPrice { get; private set; } = 150000;
        public uint BlipSprite { get; private set; }
        public string Owner { get; private set; } // Social Club du proprio
        public int BlipColor { get; private set; }
        public int MaxEmployee { get; private set; } = 15;
        public Inventory.Inventory Inventory { get; private set; } = null;
        public Parking Parking { get; set; }

        [BsonIgnore]
        public IColShape ServiceColshape;
        [BsonIgnore]
        public IColShape ParkingColshape;
        [BsonIgnore]
        public Marker Marker;

        public BankAccount BankAccount;
        #endregion

        #region Constructor
        public Society(string societyName, Vector3 servicePos, uint blipSprite, int blipColor, string owner = null, Inventory.Inventory inventory = null, Parking parking = null)
        {
            SocietyName = societyName;
            ServicePos = servicePos;
            BlipSprite = blipSprite;
            BlipColor = blipColor;

            if (owner != null)
                Owner = owner;

            if (inventory != null)
                Inventory = inventory;

            if (parking != null)
                Parking = parking;

            JobsList.Add(this);
        }
        #endregion

        #region Static Load
        public static async Task LoadAllSociety()
        {
            Console.WriteLine("--- Start loading all society in database ---");
            var _societyList = await Database.MongoDB.GetCollectionSafe<Society>("society").AsQueryable().ToListAsync();
            foreach (var _society in _societyList)
                await _society.Load();

            Console.WriteLine($"--- Finish loading all society in database: {_societyList.Count} ---");
        }
        #endregion

        #region Load
        public virtual async Task Load()
        {
            if (Employees == null)
                Employees = new Dictionary<string, string>();

            if (ServicePos != null)
            {
                // Blip
                Blip = Entities.Blips.BlipsManager.CreateBlip(SocietyName, ServicePos, BlipColor,(int) BlipSprite, 1);

                ServiceColshape = Alt.CreateColShapeCylinder(ServicePos - new Vector3(0.0f, 0.0f, 1f), 1f, 2f);
                Marker = Entities.Marker.CreateMarker(MarkerType.VerticalCylinder, ServicePos - new Vector3(0.0f, 0.0f, 1f), new Vector3(1, 1f, 1f), Color.FromArgb(128, 255, 255, 255));
            }

            if (Parking != null)
            {
                Parking.Load();
                InitParking(Parking.ParkingColshape);
            }

            if (BankAccount == null)
                BankAccount = new BankAccount(AccountType.Society, await BankAccount.GenerateNewAccountNumber(), 0);

            Inventory.MaxSlot = 40;
            InService = new List<string>();
            BankAccount.Owner = this;

            GameMode.Instance.SocietyManager.SocietyList.Add(this);
        }
        #endregion

        #region Events
        public virtual async Task OnPlayerEnterColshape(IColShape colShape, IPlayer client)
        {
            if (client == null || !client.Exists)
                return;

            if (colShape == ServiceColshape)
                await OpenServerJobMenu(client);
            else if (colShape == ParkingColshape)
                await OpenParkingMenu(client);
        }

        public virtual async Task OnVehicleOut(IPlayer client, VehicleHandler vehicle, Location location = null)
        {
            client.SetPlayerIntoVehicle(vehicle.Vehicle);
            await Update();
        }

        public virtual async Task OnVehicleStored(IPlayer client, VehicleHandler vehicle)
        {
            vehicle.ParkingName = SocietyName;
            await Update();
        }
        #endregion

        #region Methods
        public virtual Task PriseService(IPlayer client)
        {
            InService.Add(client.GetSocialClub());
            client.SendNotificationSuccess("Vous avez pris votre service");
            return Task.CompletedTask;
        }

        public virtual Task QuitterService(IPlayer client)
        {
            InService.Remove( client.GetSocialClub());
            client.GetPlayerHandler()?.Character?.ApplyCharacter(client);
            client.SendNotificationSuccess("Vous avez quitté votre service");
            return Task.CompletedTask;
        }

        public virtual bool IsEmployee(IPlayer client)
        {
            if (Employees == null)
                return false;

            var social = client.GetSocialClub();

            return Employees.ContainsKey( client.GetSocialClub()) || Owner == social;
        }

        public int GetEmployeeOnline()
        {
            int a = 0;

            foreach (var player in GameMode.Instance.PlayerList)
            {
                if (!player.Exists)
                    continue;

                if (IsEmployee(player))
                    a++;
            }
            return a;
        }

        public void InitParking(IColShape parkingColshape)
        {
            ParkingColshape = parkingColshape;
            Parking.ParkingType = ParkingType.Society;
            Parking.OnVehicleOut += OnVehicleOut;
            Parking.OnVehicleStored += OnVehicleStored;
        }

        public async Task Insert()
            => await Database.MongoDB.Insert("society", this);

        public async Task Update()
            => await Database.MongoDB.Update(this, "society", _id);

        #endregion
    }
}
