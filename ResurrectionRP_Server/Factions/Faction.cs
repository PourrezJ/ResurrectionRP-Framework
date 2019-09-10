using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Bank;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using ResurrectionRP_Server.Factions.Model;

namespace ResurrectionRP_Server.Factions
{
    public partial class Faction
    {
        #region Variables
        [BsonId]
        public string FactionName;

        [BsonIgnore]
        public List<string> ServicePlayerList = new List<string>(); // player online and take a faction service

        public FactionType FactionType;

        [BsonIgnore]
        public FactionRang[] FactionRang;

        [BsonIgnore]
        public Vector3 ServiceLocation;

        public Location ParkingLocation;
        public Location HeliportLocation;

        [BsonIgnore]
        public Vector3 ShopLocation;

        [BsonIgnore]
        public uint BlipSprite;

        [BsonIgnore]
        public Entities.Blips.BlipColor BlipColor;

        [BsonIgnore]
        public Vector3 BlipPosition;

        [BsonIgnore]
        public IColShape Vestiaire_colShape { get; private set; }
        [BsonIgnore]
        public IColShape Parking_colShape { get; private set; }
        [BsonIgnore]
        public IColShape Heliport_colShape { get; private set; }
        [BsonIgnore]
        public IColShape Shop_colShape { get; private set; }

        public double PayCheckMinutes { get; set; } = 30;

        private List<FactionShop> _itemShop = new List<FactionShop>();
        [BsonIgnore]
        public List<FactionShop> ItemShop
        {
            get
            {
                if (_itemShop == null) _itemShop = new List<FactionShop>();
                return _itemShop;
            }
            set => _itemShop = value;
        }

        private List<FactionVehicle> _vehicleAllowed = new List<FactionVehicle>();
        [BsonIgnore]
        public List<FactionVehicle> VehicleAllowed
        {
            get
            {
                if (_vehicleAllowed == null) _vehicleAllowed = new List<FactionVehicle>();
                return _vehicleAllowed;
            }
            set => _vehicleAllowed = value;
        }

        #region Persistent Variables
        public Parking Parking;

        public ConcurrentDictionary<string, FactionPlayer> FactionPlayerList
            = new ConcurrentDictionary<string, FactionPlayer>(); // all players into faction

        public BankAccount BankAccount { get; set; }

        #endregion
        #endregion

        #region Constructor
        public Faction(string FactionName, FactionType FactionType)
        {
            this.FactionName = FactionName;
            this.FactionType = FactionType;

            BankAccount = new BankAccount(AccountType.Faction, FactionName, 300000);
            BankAccount.Owner = this;

            Task.Run(async () => {
                await InsertDatabase();
            });
        }
        #endregion

        #region Start
        public virtual async Task<Faction> OnFactionInit()
        {
            if (ServiceLocation != null)
            {
                Vestiaire_colShape = Alt.CreateColShapeCylinder(ServiceLocation - new Vector3(0, 0, 1), 1.0f, 1f);
                GameMode.Instance.Streamer.AddEntityMarker(Streamer.Data.MarkerType.HorizontalCircleArrow, ServiceLocation - new Vector3(0, 0, 1), new Vector3(1, 1, 1), 128);
            }

            if (ParkingLocation != null)
            {
                if (Parking == null) Parking = new Parking(ParkingLocation.Pos, ParkingLocation);


                Parking_colShape = Alt.CreateColShapeCylinder(ParkingLocation.Pos - new Vector3(0, 0, 1), 3.0f, 3f);
                GameMode.Instance.Streamer.AddEntityMarker(Streamer.Data.MarkerType.VerticalCylinder, ParkingLocation.Pos - new Vector3(0, 0, 1), new Vector3(2, 2, 2), 128);

                Parking.OnVehicleStored = OnVehicleStore;
                Parking.OnVehicleOut = OnVehicleOut;
                Parking.ParkingType = ParkingType.Faction;
                Parking.Limite = 3;
                Parking.Spawn1 = ParkingLocation;

            }

            if (HeliportLocation != null)
            {
                if (Parking == null) Parking = new Parking(HeliportLocation.Pos, HeliportLocation);
                Parking_colShape = Alt.CreateColShapeCylinder(HeliportLocation.Pos, 3.0f, 1f);
                GameMode.Instance.Streamer.AddEntityMarker(Streamer.Data.MarkerType.VerticalCylinder, HeliportLocation.Pos - new Vector3(0, 0, 0), new Vector3(3, 3, 3), 128);
            }

            if (ShopLocation != null)
            {
                Parking_colShape = Alt.CreateColShapeCylinder(ShopLocation, 1.0f, 2f);
                GameMode.Instance.Streamer.AddEntityMarker(Streamer.Data.MarkerType.VerticalCylinder, ShopLocation - new Vector3(0, 0, 0), new Vector3(1, 1, 1), 128);
            }

            if (BlipPosition != Vector3.Zero)
                Entities.Blips.BlipsManager.CreateBlip(FactionName, BlipPosition, (int)BlipColor, (int)BlipSprite, 1);

            ServicePlayerList = new List<string>();
            BankAccount.Owner = this;

            if (!string.IsNullOrEmpty(FactionName)) { Alt.Server.LogInfo(FactionName + " is started."); }
            GameMode.Instance.FactionManager.FactionList.Add(this);
            return this;
        }


        public virtual async Task OnVehicleOut(IPlayer client, VehicleHandler vehicle, Location location = null)
        {
            await UpdateDatabase();
        }

        private async Task OnVehicleStore(IPlayer client, VehicleHandler vehicle)
        {
            await UpdateDatabase();
        }
        #endregion

        #region Event

        public virtual Task OnPlayerPromote(IPlayer client, int rang)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnPlayerServiceEnter(IPlayer client, int rang)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnPlayerServiceQuit(IPlayer client, int rang)
        {
            return Task.CompletedTask;
        }

        public virtual async Task OnPlayerConnected(IPlayer client)
        {
            if (ServicePlayerList.Contains(client.GetSocialClub()))
            {
                FactionPlayerList[client.GetSocialClub()].LastPayCheck = (DateTime.Now).AddMinutes(PayCheckMinutes);
                await OnPlayerServiceEnter(client, GetRangPlayer(client));
            }
        }

        public virtual void OnPlayerDisconnected(IPlayer client)
        {
            string socialClub = client.GetSocialClub();
            Utils.Utils.Delay(60000 * 10, true, () =>
            {
                if (!GameMode.Instance.PlayerList.Any(p => p.GetSocialClub() == socialClub))
                    ServicePlayerList.Remove(socialClub);
            });
        }

        public virtual Task OnPlayerEnterColShape(IColShape colShape, IPlayer player)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnPlayerExitColShape(IColShape colShape, IPlayer player)
        {
            return Task.CompletedTask;
        }

        public virtual async Task PayCheck()
        {
            foreach (string socialClub in ServicePlayerList.ToList())
            {
                var ph = await PlayerManager.GetPlayerBySCN(socialClub);

                if (ph != null && FactionPlayerList.ContainsKey(socialClub) && DateTime.Now >= FactionPlayerList[socialClub].LastPayCheck)
                {
                    double salaire = FactionRang[ GetRangPlayer(ph.Client)].Salaire;

                    if (salaire == 0)
                        return;

                    if (await BankAccount.GetBankMoney(salaire, $"Salaire {ph.Identite.Name}", save: false))
                    {
                        FactionPlayerList[socialClub].LastPayCheck = (DateTime.Now).AddMinutes(PayCheckMinutes);
                        ph.BankAccount.AddMoney(salaire, $"Salaire {FactionName}");
                        ph.Client.SendNotification($"Vous avez touché votre salaire ~g~${salaire}~w~.");
                    }
                    else
                        ph.Client.SendNotificationError("Vous n'avez pas reçu votre salaire, les caisses sont vide!");
                }
            }
        }

        #endregion

        #region Method
        public virtual async Task<bool> TryAddIntoFaction(IPlayer client, int rang = 1)
        {
            bool add = FactionPlayerList.TryAdd(client.GetSocialClub(), new FactionPlayer(client.GetSocialClub(), rang));
            if (add)
            {
                client.SendNotification($"Vous êtes désormais membre de {FactionName}");
                await client.GetPlayerHandler()?.Update();
                await UpdateDatabase();
                await PlayerFactionAdded(client);
            }
            else if (FactionPlayerList.ContainsKey(client.GetSocialClub()))
            {
                client.SendNotificationError($"Vous êtes déjà dans la faction {FactionName}");
            }
            return add;
        }

        public virtual Task PlayerFactionAdded(IPlayer client)
        {
            return Task.CompletedTask;
        }

        public virtual async Task PriseService(IPlayer client)
        {
            var ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            await MenuManager.CloseMenu(client);
            if (ServicePlayerList.Contains(client.GetSocialClub()))
            {
                client.SendNotificationSuccess("Vous avez quitté votre service");
                ServicePlayerList.Remove(client.GetSocialClub());
                await OnPlayerServiceQuit(client, GetRangPlayer(client));
            }
            else
            {
                client.SendNotificationSuccess("Vous avez pris votre service");
                FactionPlayerList[client.GetSocialClub()].LastPayCheck = (DateTime.Now).AddMinutes(PayCheckMinutes);
                ServicePlayerList.Add(client.GetSocialClub());
                await OnPlayerServiceEnter(client, GetRangPlayer(client));
            }
        }

        public bool IsOnService(IPlayer client)
            => ServicePlayerList.Contains(client.GetSocialClub());

        public List<IPlayer> GetEmployeeOnline()
        {
            List<IPlayer> _employeeOnline = new List<IPlayer>();

            if (ServicePlayerList == null)
                return new List<IPlayer>();

            if (ServicePlayerList.Count <= 0)
                return new List<IPlayer>();

            foreach (var client in Alt.GetAllPlayers().ToList())
            {
                if (!client.Exists)
                    continue;
                if (ServicePlayerList.Contains(client.GetSocialClub()))
                {
                    _employeeOnline.Add(client);
                }
            }

            return _employeeOnline;
        }

        public async Task UpdateDatabase()
        {
            try
            {
                await Database.MongoDB.Update(this, "factions", FactionName);
            }
            catch (Exception ex)
            {
                Alt.Server.LogError($"UpdateDatabase Faction: {FactionName}: " + ex);
            }
        }

        public async Task InsertDatabase()
            => await Database.MongoDB.Insert("factions", this);

        public List<FactionVehicle> GetVehicleAllowed(int rang)
        {
            return VehicleAllowed.FindAll(x => x.Rang == rang);
        }

        public bool CanDepositMoney(IPlayer client) => FactionRang[GetRangPlayer(client)].CanDepositMoney;
        public bool CanTakeMoney(IPlayer client) => FactionRang[GetRangPlayer(client)].CanTakeMoney;
        public bool IsRecruteur(IPlayer client) => FactionRang[GetRangPlayer(client)].Recrute;

        public void SetRangPlayer(IPlayer client, int rang)
            => FactionPlayerList[client.GetSocialClub()] = new FactionPlayer(client.GetSocialClub(), rang);

        public int GetRangPlayer(IPlayer client)
            => FactionPlayerList[client.GetSocialClub()].Rang;

        public bool HasPlayerIntoFaction(IPlayer client)
            => FactionPlayerList.ContainsKey(client.GetSocialClub());

        public ICollection<string> GetAllSocialClubName()
            => FactionPlayerList.Keys;
        #endregion

        #region Enums
        public enum ConcessType
        {
            Vehicle,
            Helico
        }
        #endregion
    }
}
