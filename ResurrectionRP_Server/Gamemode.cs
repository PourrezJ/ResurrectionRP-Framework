using AltV.Net;
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
using ResurrectionRP_Server;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Utils.Extensions;
using AltV.Net.Enums;

namespace ResurrectionRP_Server
{
    public class GameMode
    {
        public ObjectId _id;

        #region Variables
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
        public Entities.Players.PlayerManager PlayerManager { get; private set; }

        [BsonIgnore]
        public List<IPlayer> PlayerList = new List<IPlayer>();


        public static short GlobalDimension = short.MaxValue;

        public List<string> PlateList = new List<string>();


        #region Pools
        [BsonIgnore]
        public VehiclesManager VehicleManager { get; private set; }


        // Menus
        [BsonIgnore]
        public XMenuManager.XMenuManager XMenuManager { get; private set; }




        //[BsonIgnore]
        //public Weather.WeatherManager WeatherManager { get; private set; }
        [BsonIgnore]
        public Inventory.RPGInventoryManager RPGInventory { get; private set; }
        [BsonIgnore]
        public Phone.PhoneManager PhoneManager { get; private set; }

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

            IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            Alt.Server.LogInfo("Création des controlleurs...");
            Streamer = new Streamer.Streamer();
            PlayerManager = new Entities.Players.PlayerManager();
            BanManager = new BanManager();
            VehicleManager = new VehiclesManager();
            PhoneManager = new Phone.PhoneManager();
            RPGInventory = new Inventory.RPGInventoryManager();
            XMenuManager = new XMenuManager.XMenuManager();
            Alt.Server.LogInfo("Création des controlleurs terminée");

            if (Time == null)
                Time = new Time();

            Alt.Server.LogInfo("Initialisations des controlleurs...");
            await VehicleManager.LoadAllVehiclesActive();
            await Loader.ClothingLoader.LoadAllCloth();
            Alt.Server.LogInfo("Initialisation des controlleurs terminé");

            Alt.OnPlayerConnect += OnPlayerConnected;
            Alt.OnPlayerDisconnect += OnPlayerDisconnected;

            Chat.Initialize();
            Chat.RegisterCmd("veh", CommandVeh);
            Chat.RegisterCmd("coords", (IPlayer player, string[] args) =>
            {
                Chat.SendChatMessage(player, "X: " + player.Position.X + " Y: " + player.Position.Y + " Z: " + player.Position.Z);
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
            Alt.Log($"==> {player.Name} has disconnected.");
            RPGInventory.OnPlayerQuit(player);

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

            VehicleHandler vh = new VehicleHandler(player.GetSocialClub(), Alt.Hash(args[0]), player.Position, player.Rotation, locked:false);
            Task.Run(async () =>
            {
                await vh.SpawnVehicle();
                var ph = player.GetPlayerHandler();

                if (ph != null)
                {
                    ph.ListVehicleKey.Add(new VehicleKey(vh.VehicleManifest.DisplayName, vh.Plate));
                    if (vh.Vehicle != null)
                        player.Emit("SetPlayerIntoVehicle", vh.Vehicle, -1);

                    await ph.UpdatePlayerInfo();
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
