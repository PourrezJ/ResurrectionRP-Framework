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
using ResurrectionRP_Server;
using ResurrectionRP_Server.Entities.Vehicles;

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
        public bool IsDebug { get; private set; } = false;

        [BsonIgnore]
        public bool ServerLoaded = false;

        [BsonIgnore]
        public float StreamDistance { get; private set; } = 500;

        [BsonIgnore]
        public BanManager BanManager { get; private set; }

        [BsonIgnore]
        public Entities.Players.PlayerManager PlayerManager { get; private set; }

        [BsonIgnore]
        public List<IPlayer> PlayerList = new List<IPlayer>();

        [BsonIgnore]
        public List<Models.Social> SocialList = new List<Models.Social>();

        [BsonIgnore]
        public short GlobalDimension = 3;

        public List<string> PlateList = new List<string>();

        public static bool ServerLock;

        #region Pools
        [BsonIgnore]
        public VehiclesManager VehicleManager { get; private set; }
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
            PlayerManager = new Entities.Players.PlayerManager();
            BanManager = new BanManager();
            VehicleManager = new VehiclesManager();
            Alt.Server.LogInfo("Création des controlleurs terminée");
            
            
            Alt.OnPlayerConnect += OnPlayerConnected;
            Alt.OnPlayerDisconnect += OnPlayerDisconnected;
            Alt.OnPlayerEnterVehicle += OnPlayerEnterVehicle;
            Alt.OnPlayerLeaveVehicle += OnPlayerLeaveVehicle;
            Chat.Initialize();
            Chat.RegisterCmd("veh", CommandVeh);
            ServerLoaded = true;
        }

        private void OnPlayerConnected(IPlayer player, string reason)
        {

            if (PlayerList.Find(b => b == player) == null)
                PlayerList.Add(player);
            Alt.Log($"==> {player.Name} has connected.");
            Chat.Broadcast($"==> {player.Name} has joined.");
            player.Model = (uint)AltV.Net.Enums.PedModel.FreemodeMale01;
            player.Spawn(new Position(813, -279, 66), 1000);


        }
        private void OnPlayerDisconnected(IPlayer player, string reason)
        {

            if (PlayerList.Find(b => b == player) != null)
                PlayerList.Remove(player);
            Alt.Log($"==> {player.Name} has disconnected.");
            Chat.Broadcast($"==> {player.Name} has joined.");

        }

        private void OnPlayerEnterVehicle(IVehicle vehicle, IPlayer player, byte seat)
        {
            player.Emit("OnPlayerEnterVehicle", vehicle.Id, Convert.ToInt32( seat ) , 50, 50);
        }

        private void OnPlayerLeaveVehicle(IVehicle vehicle, IPlayer player, byte seat)
        {
            player.Emit("OnPlayerLeaveVehicle");
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

            IVehicle vehicle = Alt.CreateVehicle(args[0], player.Position, player.Rotation);

            if (vehicle != null)
                player.Emit("SetPlayerIntoVehicle", vehicle, -1);
        }
        public async Task Save()
        {
            await Database.MongoDB.Update(this, "gamemode", _id);
        }
        #endregion
    }
}
