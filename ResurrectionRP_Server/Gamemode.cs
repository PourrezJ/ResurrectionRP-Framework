using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using ResurrectionRP_Server.Models;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ResurrectionRP_Server;

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
        public List<IPlayer> PlayerList = new List<IPlayer>();

        [BsonIgnore]
        public short GlobalDimension = 3;

        public List<string> PlateList = new List<string>();

        #region Static
        public static Location FirstSpawn = new Location(new Vector3(-1072.886f, -2729.607f, 0.8148939f), new Vector3(0, 0, 313.7496f));
        #endregion

        #endregion

        #region Events
        public async Task OnStartAsync()
        {
            IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            Alt.Server.LogInfo("Création des controlleurs...");
            Alt.Server.LogInfo("Création des controlleurs terminée");
            
            
            Alt.OnPlayerConnect += OnPlayerConnected;
            Alt.OnPlayerDisconnect += OnPlayerDisconnected;
            Alt.OnPlayerEnterVehicle += OnPlayerEnterVehicle;
            Alt.OnPlayerLeaveVehicle += OnPlayerLeaveVehicle;

            Chat.Initialize();
            Chat.RegisterCmd("veh", CommandVeh);
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
            player.Emit("OnPlayerEnterVehicle", vehicle.Id, seat, 50, 50);
        }

        private void OnPlayerLeaveVehicle(IVehicle vehicle, IPlayer player, byte seat)
        {
            player.Emit("OnPlayerLeaveVehicle");
        }
        #endregion

        #region methods
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
