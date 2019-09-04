﻿using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using ResurrectionRP_Server.Models;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ResurrectionRP_Server.EventHandlers;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Utils.Extensions;
using AltV.Net.Enums;
using SaltyServer;
using ResurrectionRP_Server.Radio;

namespace ResurrectionRP_Server
{
    public class GameMode
    {
        #region Variables
        public ObjectId _id;

        [BsonIgnore]
        public static GameMode Instance { get; private set; }

        [BsonIgnore]
        public static bool IsLinux { get; private set; }

        [BsonIgnore]
        public bool IsDebug { get; private set; } = true;

        [BsonIgnore]
        public bool ServerLoaded = false;

        [BsonIgnore]
        public Streamer.Streamer Streamer { get; private set; }

        [BsonIgnore]
        public float StreamDistance { get; private set; } = 500;

        [BsonIgnore]
        public BanManager BanManager { get; private set; }

        [BsonIgnore]
        public PlayerManager PlayerManager { get; private set; }

        [BsonIgnore]
        public List<IPlayer> PlayerList = new List<IPlayer>();

        [BsonIgnore]
        public IVoiceChannel GlobalVoiceChannel { get; private set; }

       // [BsonIgnore]
       // public FactionManager FactionManager { get; private set; }

        public static short GlobalDimension = short.MaxValue;

        public List<string> PlateList = new List<string>();


        #region Pools

        [BsonIgnore]
        public Economy.Economy Economy { get; private set; }

        [BsonIgnore]
        public VehiclesManager VehicleManager { get; private set; }
        public Services.Pound PoundManager { get; set; }

        [BsonIgnore]
        public Jobs.JobsManager JobsManager { get; private set; }

        // Menus
        [BsonIgnore]
        public MenuManager MenuManager { get; private set; }
        [BsonIgnore]
        public XMenuManager.XMenuManager XMenuManager { get; private set; }

        [BsonIgnore]
        public Entities.Peds.PedsManager PedManager { get; private set; }

        [BsonIgnore]
        public Entities.Blips.BlipsManager BlipsManager { get; private set; }

        [BsonIgnore]
        public Loader.BusinessesLoader BusinessesManager { get; private set; }

        [BsonIgnore]
        public DrivingSchool.DrivingSchoolManager DrivingSchoolManager { get; private set; }

        [BsonIgnore]
        public Utils.DoorManager DoorManager { get; private set; }

        [BsonIgnore]
        public Weather.WeatherManager WeatherManager { get; private set; }
        [BsonIgnore]
        public Inventory.RPGInventoryManager RPGInventory { get; private set; }
        [BsonIgnore]
        public Phone.PhoneManager PhoneManager { get; private set; }

        [BsonIgnore]
        public Voice VoiceController { get; private set; }
        [BsonIgnore]
        public RadioManager RadioManager { get; private set; }

        public static bool ServerLock;

        public Time Time { get; set; }
        public bool ModeAutoFourriere { get; internal set; }
        
        #endregion

        #region Static
        public static Location FirstSpawn = new Location(new Vector3(-1072.886f, -2729.607f, 0.8148939f), new Vector3(0, 0, 313.7496f));
        #endregion

        #endregion

        #region Constructor
        public GameMode()
        {
            if (Instance != null) return;
            Instance = this;
            var ci = new CultureInfo("fr-FR");
            CultureInfo.DefaultThreadCurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;

            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => OnServerStop();
        }
        #endregion

        #region Events
        private async void OnServerStop()
        {
            var players = GameMode.Instance.PlayerList;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] != null && players[i].Exists)
                    players[i].Kick("Server stop");
            }

            //await HouseManager.House_Exit();
        }

        public async Task OnStartAsync()
        {
            Alt.OnPlayerConnect += OnPlayerConnected;
            Alt.OnPlayerDisconnect += OnPlayerDisconnected;

            IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            Alt.Server.LogColored("~g~Création des controlleurs...");
            Streamer = new Streamer.Streamer();
            Economy = new Economy.Economy();
            DoorManager = new Utils.DoorManager();
            PlayerManager = new PlayerManager();
            BanManager = new BanManager();
            VehicleManager = new VehiclesManager();
            PedManager = new Entities.Peds.PedsManager();
            BlipsManager = new Entities.Blips.BlipsManager();
            PhoneManager = new Phone.PhoneManager();
            RPGInventory = new Inventory.RPGInventoryManager();
            MenuManager = new MenuManager();
            BusinessesManager = new Loader.BusinessesLoader();
            XMenuManager = new XMenuManager.XMenuManager();
            WeatherManager = new Weather.WeatherManager();
            DrivingSchoolManager = new DrivingSchool.DrivingSchoolManager();
            JobsManager = new Jobs.JobsManager();
            VoiceController = new Voice();

            RadioManager = new RadioManager();

            //FactionManager = new FactionManager();
            Alt.Server.LogColored("~g~Création des controlleurs terminée");

            if (Time == null)
                Time = new Time();

            if (PoundManager == null)
                PoundManager = new Services.Pound();

            Alt.Server.LogColored("~g~Initialisations des controlleurs...");
            await Loader.CarParkLoader.LoadAllCarPark();
            await Loader.CarDealerLoaders.LoadAllCardealer();
            await Loader.VehicleRentLoaders.LoadAllVehicleRent();
            await Loader.TattooLoader.TattooLoader.LoadAllTattoo();
            await VehicleManager.LoadAllVehiclesActive();
            await Loader.ClothingLoader.LoadAllCloth();
            await Loader.BusinessesLoader.LoadAllBusinesses();
            await WeatherManager.InitWeather();
            await JobsManager.Init();
            await PoundManager.LoadPound();
            DrivingSchoolManager.InitAll();

            VoiceController.OnResourceStart();
            Alt.Server.LogColored("~g~Initialisation des controlleurs terminé");


            ModeAutoFourriere = Config.GetSetting<bool>("ModeAutoFourriere");
            if (ModeAutoFourriere)
                PoundManager.Price = 0;
            Events.Initialize();

            Chat.Initialize();
            Chat.RegisterCmd("veh", CommandVeh);
            Chat.RegisterCmd("coords", (IPlayer player, string[] args) =>
            {
                if (player.Vehicle != null)
                {
                    Chat.SendChatMessage(player, "X: " + player.Vehicle.Position.X + " Y: " + player.Vehicle.Position.Y + " Z: " + player.Vehicle.Position.Z);
                    Chat.SendChatMessage(player, "RX: " + player.Vehicle.Rotation.Roll + " RY: " + player.Vehicle.Rotation.Pitch + " RZ: " + player.Vehicle.Rotation.Yaw);
                }
                else
                {
                    Chat.SendChatMessage(player, "X: " + player.Position.X + " Y: " + player.Position.Y + " Z: " + player.Position.Z);
                    Chat.SendChatMessage(player, "RX: " + player.Rotation.Roll + " RY: " + player.Rotation.Pitch + " RZ: " + player.Rotation.Yaw);
                }
            });
            Chat.RegisterCmd("getCoords", (IPlayer player, string[] args) =>
            {
                Alt.Server.LogColored($" X: {player.Position.X}  Y: {player.Position.Y} Z: {player.Position.Z} ");
                Alt.Server.LogColored($" RX: {player.Rotation.Roll}  RY: {player.Rotation.Pitch} RZ: {player.Rotation.Yaw} ");
            });
            Chat.RegisterCmd("task", async (IPlayer player, string[] args) =>
            {
            });
            ServerLoaded = true;
        }


        private void OnPlayerConnected(IPlayer player, string reason)
        {
            if (PlayerList.Find(b => b == player) == null)
                PlayerList.Add(player);

            Alt.Log($"==> {player.Name} has connected.");
        }

        private void OnPlayerDisconnected(IPlayer player, string reason)
        {
            if (PlayerList.Find(b => b == player) != null)
                PlayerList.Remove(player);

            RPGInventory.OnPlayerQuit(player);
            PlayerHandler.PlayerHandlerList.TryRemove(player, out _);
            Alt.Log($"==> {player.Name} has disconnected.");
        }
        #endregion

        #region Methods
        private void CommandVeh(IPlayer player, string[] args)
        {
            if (args == null)
            {
                player.SendChatMessage("{FF0000}Usage: /veh [vehicle name]");
                return;
            }

            VehicleHandler vh = new VehicleHandler(player.GetSocialClub(), Alt.Hash(args[0]), new Vector3(player.Position.X+5, player.Position.Y, player.Position.Z), player.Rotation, locked:false);

            Task.Run(async () =>
            {
                await vh.SpawnVehicle(null);
                PlayerHandler ph = player.GetPlayerHandler();

                if (ph != null)
                {
                    ph.ListVehicleKey.Add(new VehicleKey(vh.VehicleManifest.DisplayName, vh.Plate));

                    if (vh.Vehicle != null)
                        player.Emit("SetPlayerIntoVehicle", vh.Vehicle, -1);

                    await vh.InsertVehicle();
                    await ph.Update();
                }
            });
        }

        public async Task Save()
        {
            await Database.MongoDB.Update(this, "gamemode", _id);
        }
        #endregion
    }
}
