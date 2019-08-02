using AltV.Net;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Models;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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

        public List<string> PlateList = new List<string>();

        #region Static
        public static Location FirstSpawn = new Location(new Vector3(-1072.886f, -2729.607f, 0.8148939f), new Vector3(0, 0, 313.7496f));
        #endregion

        #endregion

        public async Task OnStartAsync()
        {
            IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

            Alt.Server.LogInfo("Création des controlleurs...");

            Alt.Server.LogInfo("Création des controlleurs terminé");

        }
    }
}
