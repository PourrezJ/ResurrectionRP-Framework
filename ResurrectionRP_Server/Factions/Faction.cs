﻿using AltV.Net;
using AltV.Net.Elements.Entities;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Bank;
using ResurrectionRP_Server.Colshape;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Factions.Model;
using ResurrectionRP_Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Discord;
using System.Drawing;
using ResurrectionRP_Server.Streamer.Data;

namespace ResurrectionRP_Server.Factions
{
    public partial class Faction
    {
        #region Fields and properties
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
        public IColshape Vestiaire_colShape { get; private set; }
        [BsonIgnore]
        public IColshape Heliport_colShape { get; private set; }
        [BsonIgnore]
        public IColshape Shop_colShape { get; private set; }

        public double PayCheckMinutes { get; set; } = 30;

        private List<FactionShopItem> _itemShop = new List<FactionShopItem>();
        [BsonIgnore]
        public List<FactionShopItem> ItemShop
        {
            get
            {
                if (_itemShop == null) _itemShop = new List<FactionShopItem>();
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

        public Parking Parking;

        public ConcurrentDictionary<string, FactionPlayer> FactionPlayerList
            = new ConcurrentDictionary<string, FactionPlayer>(); // all players into faction

        public BankAccount BankAccount { get; set; }

        [BsonIgnore]
        protected EmergencyCall EmCall = null;
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

        #region Init
        public virtual Faction Init()
        {
            if (ServiceLocation != Vector3.Zero)
            {
                Vestiaire_colShape = ColshapeManager.CreateCylinderColshape(ServiceLocation - new Vector3(0, 0, 1), 1.0f, 2f);
                Vestiaire_colShape.OnPlayerEnterColshape += OnPlayerEnterVestiaire;
                Vestiaire_colShape.OnPlayerLeaveColshape += OnPlayerLeaveVestiaire;
                Marker.CreateMarker(MarkerType.HorizontalCircleArrow, ServiceLocation - new Vector3(0, 0, 0.95f), new Vector3(1, 1, 1));
            }

            if (ParkingLocation != null)
            {
                if (Parking == null)
                    Parking = new Parking(ParkingLocation.Pos, ParkingLocation);

                Parking.Location = ParkingLocation.Pos;
                Parking.Spawn1 = ParkingLocation;
                Parking.Spawn1.Rot = Parking.Spawn1.Rot.ConvertRotationToRadian();
                Parking.Init();
                Parking.ParkingType = ParkingType.Faction;
                Parking.OnPlayerEnterParking += OnPlayerEnterParking;
                Parking.OnVehicleEnterParking += OnVehicleEnterParking;
                Parking.OnVehicleStored += OnVehicleStored;
                Parking.OnVehicleOut += OnVehicleOut;
            }

            if (HeliportLocation != null)
            {
                if (Parking == null)
                    Parking = new Parking(HeliportLocation.Pos, HeliportLocation);

                Heliport_colShape = ColshapeManager.CreateCylinderColshape(HeliportLocation.Pos - new Vector3(0, 0, 1), 3.0f, 2f);
                Heliport_colShape.OnPlayerEnterColshape += OnPlayerEnterHeliport;
                Heliport_colShape.OnPlayerLeaveColshape += OnPlayerLeaveHeliport;
                Marker.CreateMarker(MarkerType.VerticalCylinder, HeliportLocation.Pos - new Vector3(0, 0, 3), new Vector3(3, 3, 0.2f));
                TextLabel.CreateTextLabel("~o~Approchez pour intéragir", HeliportLocation.Pos, System.Drawing.Color.White);
            }

            if (ShopLocation != null && ShopLocation != Vector3.Zero)
            {
                Shop_colShape = ColshapeManager.CreateCylinderColshape(ShopLocation, 1.0f, 2f);
                Shop_colShape.OnPlayerEnterColshape += OnPlayerEnterShop;
                Shop_colShape.OnPlayerLeaveColshape += OnPlayerLeaveShop;
                Marker.CreateMarker(MarkerType.VerticalCylinder, ShopLocation - new Vector3(0, 0, 0), new Vector3(1, 1, 0.2f));
                TextLabel.CreateTextLabel("~o~Approchez pour intéragir", ShopLocation, System.Drawing.Color.White);
            }

            if (BlipPosition != Vector3.Zero)
                Entities.Blips.BlipsManager.CreateBlip(FactionName, BlipPosition, (int)BlipColor, (int)BlipSprite, 1);

            ServicePlayerList = new List<string>();
            BankAccount.Owner = this;

            if (!string.IsNullOrEmpty(FactionName))
                Alt.Server.LogInfo(FactionName + " is started.");

            return this;
        }
        #endregion

        #region Event handlers
        public virtual void OnVehicleOut(IPlayer client, VehicleHandler vehicle, Location location = null)
        {
            client.SetPlayerIntoVehicle(vehicle);
            vehicle.LockState = AltV.Net.Enums.VehicleLockState.Unlocked;
            UpdateInBackground();
        }

        private void OnVehicleStored(IPlayer client, VehicleHandler vehicle)
        {
            vehicle.VehicleData.ParkingName = FactionName;
            UpdateInBackground();
        }

        public void OnPlayerEnterVestiaire(IColshape colshape, IPlayer client)
        {
            PriseServiceMenu(client);
        }

        public void OnPlayerLeaveVestiaire(IColshape colshape, IPlayer client)
        {
            PlayerHandler player = client.GetPlayerHandler();

            if (player == null)
                return;

            if (player.HasOpenMenu())
                MenuManager.CloseMenu(client);
        }

        public void OnPlayerEnterShop(IColshape colshape, IPlayer client)
        {
            OpenShopMenu(client);
        }

        public void OnPlayerLeaveShop(IColshape colshape, IPlayer client)
        {
            PlayerHandler player = client.GetPlayerHandler();

            if (player == null)
                return;

            if (player.HasOpenMenu())
                MenuManager.CloseMenu(client);
        }

        public void OnPlayerEnterParking(PlayerHandler player, Parking parking)
        {
            OpenConcessMenu(player?.Client, ConcessType.Vehicle, ParkingLocation, FactionName);
        }

        public void OnVehicleEnterParking(VehicleHandler vehicle, Parking parking)
        {
            OpenConcessMenu(vehicle.Driver, ConcessType.Vehicle, ParkingLocation, FactionName);
        }

        public void OnPlayerEnterHeliport(IColshape colshape, IPlayer client)
        {
            OpenConcessMenu(client, ConcessType.Helico, HeliportLocation, FactionName);
        }

        public void OnPlayerLeaveHeliport(IColshape colshape, IPlayer client)
        {
            PlayerHandler player = client.GetPlayerHandler();

            if (player == null)
                return;

            if (player.HasOpenMenu())
                MenuManager.CloseMenu(client);
        }

        public virtual void OnPlayerPromote(IPlayer client, int rang)
        {
        }

        public virtual void OnPlayerServiceEnter(IPlayer client, int rang)
        {
            if (EmCall != null)
                EventHandlers.Events.InvokeEmergencyCallState(client, FactionName,true);
        }

        public virtual void OnPlayerServiceQuit(IPlayer client, int rang)
        {
            if (EmCall != null)
                EventHandlers.Events.InvokeEmergencyCallState(client, FactionName, false);
        }

        public virtual void OnPlayerConnected(IPlayer client)
        {
            if (ServicePlayerList.Contains(client.GetSocialClub()))
            {
                FactionPlayerList[client.GetSocialClub()].LastPayCheck = DateTime.Now.AddMinutes(PayCheckMinutes);
                OnPlayerServiceEnter(client, GetRangPlayer(client));
            }
        }

        public virtual void OnPlayerDisconnected(IPlayer client)
        {
            if (IsOnService(client))
            {
                ServicePlayerList.Remove(client.GetSocialClub());
                OnPlayerServiceQuit(client, GetRangPlayer(client));
            }
        }

        public virtual void OnPlayerEnterColShape(IColshape colshape, IPlayer player)
        {
        }

        public virtual void OnPlayerExitColShape(IColshape colshape, IPlayer player)
        {
        }
        #endregion

        #region Methods
        public virtual Task PayCheck()
        {
            foreach (string socialClub in ServicePlayerList.ToList())
            {
                var ph = PlayerManager.GetPlayerBySCN(socialClub);

                if (ph != null && FactionPlayerList.ContainsKey(socialClub) && DateTime.Now >= FactionPlayerList[socialClub].LastPayCheck)
                {
                    double salaire = FactionRang[GetRangPlayer(ph.Client)].Salaire;

                    if (salaire == 0)
                        return Task.CompletedTask;

                    if (BankAccount.GetBankMoney(salaire, $"Salaire {ph.Identite.Name}", save: false))
                    {
                        FactionPlayerList[socialClub].LastPayCheck = DateTime.Now.AddMinutes(PayCheckMinutes);
                        ph.BankAccount.AddMoney(salaire, $"Salaire {FactionName}");
                        ph.Client.SendNotification($"Vous avez touché votre salaire ${salaire}.");
                    }
                    else
                        ph.Client.SendNotificationError("Vous n'avez pas reçu votre salaire, les caisses sont vide!");
                }
            }

            return Task.CompletedTask;
        }

        public virtual bool TryAddIntoFaction(IPlayer client, int rang = 1)
        {
            bool add = FactionPlayerList.TryAdd(client.GetSocialClub(), new FactionPlayer(client.GetSocialClub(), rang));

            if (add)
            {
                client.SendNotification($"Vous êtes désormais membre de {FactionName}");
                client.GetPlayerHandler()?.UpdateFull();
                UpdateInBackground();
                PlayerFactionAdded(client);
            }
            else if (FactionPlayerList.ContainsKey(client.GetSocialClub()))
                client.SendNotificationError($"Vous êtes déjà dans la faction {FactionName}");

            return add;
        }

        public virtual void PlayerFactionAdded(IPlayer client)
        {
        }

        public virtual void PriseService(IPlayer client)
        {
            var ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            if (ServicePlayerList.Contains(client.GetSocialClub()))
            {
                client.SendNotificationSuccess("Vous avez quitté votre service");
                ServicePlayerList.Remove(client.GetSocialClub());
                OnPlayerServiceQuit(client, GetRangPlayer(client));
            }
            else
            {
                client.SendNotificationSuccess("Vous avez pris votre service");
                FactionPlayerList[client.GetSocialClub()].LastPayCheck = DateTime.Now.AddMinutes(PayCheckMinutes);
                ServicePlayerList.Add(client.GetSocialClub());
                OnPlayerServiceEnter(client, GetRangPlayer(client));
            }

            MenuManager.CloseMenu(client);
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

            var players = Alt.GetAllPlayers();
            lock (players)
            {
                foreach (var client in players)
                {
                    if (!client.Exists)
                        continue;
                    if (ServicePlayerList.Contains(client.GetSocialClub()))
                    {
                        _employeeOnline.Add(client);
                    }
                }
            }

            return _employeeOnline;
        }

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
