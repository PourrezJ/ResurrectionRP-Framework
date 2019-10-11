using AltV.Net;
using AltV.Net.Elements.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using ResurrectionRP_Server.Bank;
using ResurrectionRP_Server.Colshape;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Society.Societies;
using ResurrectionRP_Server.Society.Societies.Bennys;
using ResurrectionRP_Server.Society.Societies.WildCustom;
using ResurrectionRP_Server.Society.Societies.WhiteWereWolf;
using ResurrectionRP_Server.Entities;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Society
{
    [BsonIgnoreExtraElements]
    [BsonKnownTypes(typeof(Bennys), typeof(Unicorn), typeof(Sandjob), typeof(Rhumerie), typeof(PetrolSociety), typeof(Tequilala), typeof(WhiteWereWolf), typeof(PawnCar), typeof(WildCustom), typeof(BlackStreetNation), typeof(YellowJack), typeof(Amphitheatre), typeof(Weazel))]
    public partial class Society
    {
        #region Static fields
        [JsonIgnore]
        public static List<Society> JobsList = new List<Society>();

        [JsonIgnore]
        public static int PriceNameChange = 10000;
        #endregion

        #region Fields and properties
        public BsonObjectId _id;

        [JsonIgnore]
        protected Entities.Blips.Blips Blip;

        public ConcurrentDictionary<string, string> Employees = new ConcurrentDictionary<string, string>(); // Liste des noms RP

        [BsonIgnore]
        public ConcurrentDictionary<string, string> InService = new ConcurrentDictionary<string, string>();

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
        public IColshape ServiceColshape;
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

        #region Init
        public virtual void Init()
        {
            if (ServicePos != null)
            {
                // Blip
                Blip = Entities.Blips.BlipsManager.CreateBlip(SocietyName, ServicePos, BlipColor,(int) BlipSprite, 1);

                ServiceColshape = ColshapeManager.CreateCylinderColshape(ServicePos - new Vector3(0.0f, 0.0f, 1f), 1f, 2f);
                ServiceColshape.OnPlayerEnterColshape += OnPlayerEnterServiceColshape;
                ServiceColshape.OnPlayerLeaveColshape += OnPlayerLeaveServiceColshape;
                Marker = Marker.CreateMarker(MarkerType.VerticalCylinder, ServicePos - new Vector3(0.0f, 0.0f, 1f), new Vector3(1, 1f, 1f), Color.FromArgb(128, 255, 255, 255));
            }

            if (Parking != null)
            {
                Parking.Init();
                Parking.ParkingType = ParkingType.Society;
                Parking.OnPlayerEnterParking += OnPlayerEnterParking;
                Parking.OnVehicleEnterParking += OnVehicleEnterParking;
                Parking.OnVehicleOut += OnVehicleOut;
                Parking.OnVehicleStored += OnVehicleStored;
            }

            if (BankAccount == null)
                BankAccount = new BankAccount(AccountType.Society, BankAccount.GenerateNewAccountNumber(), 0);

            Inventory.MaxSlot = 40;
            InService = new ConcurrentDictionary<string, string>();
            BankAccount.Owner = this;

            SocietyManager.SocietyList.Add(this);
        }
        #endregion

        #region Event handlers
        public virtual void OnPlayerEnterServiceColshape(IColshape colshape, IPlayer client)
        {
            if (client == null || !client.Exists)
                return;

            Task.Run(async () => { await OpenSocietyMainMenu(client); }).Wait();
        }

        public virtual void OnPlayerLeaveServiceColshape(IColshape colshape, IPlayer client)
        {
            if (client == null || !client.Exists)
                return;

            PlayerHandler player = client.GetPlayerHandler();

            if (player != null && player.HasOpenMenu())
                MenuManager.CloseMenu(client);
        }

        public void OnPlayerEnterParking(PlayerHandler player, Parking parking)
        {
            OpenParkingMenu(player?.Client);
        }

        public void OnVehicleEnterParking(VehicleHandler vehicle, Parking parking)
        {
            OpenParkingMenu(vehicle?.Vehicle?.Driver);
        }

        public virtual Task OnVehicleOut(IPlayer client, VehicleHandler vehicle, Location location = null)
        {
            vehicle.Vehicle.Rotation = location.Rot.ConvertRotationToRadian();
            client.SetPlayerIntoVehicle(vehicle.Vehicle);
            UpdateInBackground();
            return Task.CompletedTask;
        }

        public virtual Task OnVehicleStored(IPlayer client, VehicleHandler vehicle)
        {
            vehicle.ParkingName = SocietyName;
            UpdateInBackground();
            return Task.CompletedTask;
        }
        #endregion

        #region Methods
        public virtual Task PriseService(IPlayer client)
        {
            if (client == null || !client.Exists)
                return Task.CompletedTask;

            PlayerHandler ph = client.GetPlayerHandler();

            if (ph == null)
                return Task.CompletedTask;

            InService.TryAdd(client.GetSocialClub(), ph.Identite.Name);
            client.SendNotificationSuccess("Vous avez pris votre service");
            return Task.CompletedTask;
        }

        public virtual Task QuitterService(IPlayer client)
        {
            if (client == null || !client.Exists)
                return Task.CompletedTask;

            PlayerHandler ph = client.GetPlayerHandler();

            if (ph == null)
                return Task.CompletedTask;

            InService.TryRemove(client.GetSocialClub(), out _);

            client.ApplyCharacter();
            client.SendNotificationSuccess("Vous avez quitté votre service");
            return Task.CompletedTask;
        }

        public virtual bool IsEmployee(IPlayer client)
        {
            if (Employees == null)
                return false;

            var social = client.GetSocialClub();

            return Employees.ContainsKey(client.GetSocialClub()) || Owner == social;
        }

        public int NbEmployeesOnline()
        {
            int employees = 0;

            foreach (IPlayer player in GameMode.PlayerList)
            {
                if (player == null || !player.Exists)
                    continue;

                if (IsEmployee(player))
                    employees++;
            }

            return employees;
        }
        #endregion
    }
}
