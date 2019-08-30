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
        public BlipColor BlipColor;

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
                Vestiaire_colShape = await MP.Colshapes.NewTubeAsync(ServiceLocation, 1.0f, 1.0f);
                await MP.Markers.NewAsync(MarkerType.VerticalCylinder, ServiceLocation - new Vector3(0.0f, 0.0f, 1f), new Vector3(), new Vector3(), 1f, Color.FromArgb(128, 255, 255, 255), true, MP.GlobalDimension);
            }

            if (ParkingLocation != null)
            {
                if (Parking == null) Parking = new Parking(ParkingLocation.Pos, ParkingLocation);
                Parking_colShape = await MP.Colshapes.NewTubeAsync(ParkingLocation.Pos, 3.0f, 1.0f);
                await MP.Markers.NewAsync(MarkerType.VerticalCylinder, ParkingLocation.Pos - new Vector3(0.0f, 0.0f, 3f), new Vector3(), new Vector3(), 3f, Color.FromArgb(128, 255, 255, 255), true, MP.GlobalDimension);

                Parking.OnVehicleStored = OnVehicleStore;
                Parking.OnVehicleOut = OnVehicleOut;
                Parking.ParkingType = ParkingType.Faction;
                Parking.Limite = 3;
                Parking.Spawn1 = ParkingLocation;

                // TEMPORARY CODE TO REMOVE DUPLICATE VEHICLES
                List<VehicleHandler> vehicleList = new List<VehicleHandler>();

                foreach (VehicleHandler veh in Parking.ListVehicleStored)
                {
                    bool duplicate = false;

                    foreach (VehicleHandler vehicle in vehicleList)
                    {
                        if (veh.Plate == vehicle.Plate)
                        {
                            duplicate = true;
                            break;
                        }
                    }

                    if (!duplicate)
                        vehicleList.Add(veh);
                }

                if (Parking.ListVehicleStored.Count != vehicleList.Count)
                {
                    Parking.ListVehicleStored = vehicleList;
                    await UpdateDatabase();
                }
                // END TEMPORARY CODE
            }

            if (HeliportLocation != null)
            {
                if (Parking == null) Parking = new Parking(HeliportLocation.Pos, HeliportLocation);
                Heliport_colShape = await MP.Colshapes.NewTubeAsync(HeliportLocation.Pos, 3.0f, 1.0f);
                await MP.Markers.NewAsync(MarkerType.VerticalCylinder, HeliportLocation.Pos - new Vector3(0.0f, 0.0f, 3f), new Vector3(), new Vector3(), 3f, Color.FromArgb(128, 255, 255, 255), true, MP.GlobalDimension);
            }

            if (ShopLocation != null)
            {
                Shop_colShape = await MP.Colshapes.NewTubeAsync(ShopLocation, 1.0f, 2.0f);
                await MP.Markers.NewAsync(MarkerType.VerticalCylinder, ShopLocation, new Vector3(), new Vector3(), 1f, Color.FromArgb(128, 255, 255, 255), true, MP.GlobalDimension);
            }

            if (BlipPosition != Vector3.Zero) await MP.Blips.NewAsync(BlipSprite, BlipPosition, 1f, (uint)BlipColor, FactionName, 255, 10, true);

            ServicePlayerList = new List<string>();
            BankAccount.Owner = this;

            if (!string.IsNullOrEmpty(FactionName)) { MP.Logger.Info(FactionName + " is started."); }
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
            if (ServicePlayerList.Contains(client.GetSocialClubName()))
            {
                FactionPlayerList[client.GetSocialClubName()].LastPayCheck = (DateTime.Now).AddMinutes(PayCheckMinutes);
                await OnPlayerServiceEnter(client, GetRangPlayer(client));
            }
        }

        public virtual void OnPlayerDisconnected(IPlayer client, DisconnectReason type, string reason)
        {
            string socialClub = client.GetSocialClubName();
            Utils.Utils.Delay(60000 * 10, true, () =>
            {
                if (!MP.Players.Any(p => p.GetSocialClubName() == socialClub))
                    ServicePlayerList.Remove(socialClub);
            });
        }

        public virtual Task OnEnterColshape(IPlayer player, IColshape colshapePointer)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnExitColshape(IPlayer player, IColshape colshapePointer)
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
                    double salaire = FactionRang[await GetRangPlayer(ph.Client)].Salaire;

                    if (salaire == 0)
                        return;

                    if (await BankAccount.GetBankMoney(salaire, $"Salaire {ph.Identite.Name}", save: false))
                    {
                        FactionPlayerList[socialClub].LastPayCheck = (DateTime.Now).AddMinutes(PayCheckMinutes);
                        ph.BankAccount.AddMoney(salaire, $"Salaire {FactionName}");
                        await ph.Client.NotifyAsync($"Vous avez touché votre salaire ~g~${salaire}~w~.");
                    }
                    else
                        await ph.Client.SendNotificationError("Vous n'avez pas reçu votre salaire, les caisses sont vide!");
                }
            }
        }

        #endregion

        #region Method
        public virtual async Task<bool> TryAddIntoFaction(IPlayer client, int rang = 1)
        {
            bool add = FactionPlayerList.TryAdd(client.GetSocialClubName(), new FactionPlayer(client.GetSocialClubName(), rang));
            if (add)
            {
                await client.NotifyAsync($"Vous êtes désormais membre de {FactionName}");
                await client.GetPlayerHandler()?.Update();
                await UpdateDatabase();
                await PlayerFactionAdded(client);
            }
            else if (FactionPlayerList.ContainsKey(client.GetSocialClubName()))
            {
                await client.SendNotificationError($"Vous êtes déjà dans la faction {FactionName}");
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
            if (ServicePlayerList.Contains(client.GetSocialClubName()))
            {
                await client.SendNotificationSuccess("Vous avez quitté votre service");
                ServicePlayerList.Remove(client.GetSocialClubName());
                await OnPlayerServiceQuit(client, GetRangPlayer(client));
            }
            else
            {
                await client.SendNotificationSuccess("Vous avez pris votre service");
                FactionPlayerList[client.GetSocialClubName()].LastPayCheck = (DateTime.Now).AddMinutes(PayCheckMinutes);
                ServicePlayerList.Add(client.GetSocialClubName());
                await OnPlayerServiceEnter(client, GetRangPlayer(client));
            }
        }

        public bool IsOnService(IPlayer client)
            => ServicePlayerList.Contains(client.GetSocialClubName());

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
                if (ServicePlayerList.Contains(client.GetSocialClubName()))
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
            => FactionPlayerList[client.GetSocialClubName()] = new FactionPlayer(client.GetSocialClubName(), rang);

        public int GetRangPlayer(IPlayer client)
            => FactionPlayerList[client.GetSocialClubName()].Rang;

        public bool HasPlayerIntoFaction(IPlayer client)
            => FactionPlayerList.ContainsKey(client.GetSocialClubName());

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
