using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ResurrectionRP_Server.Entities.Worlds
{
    public class TrainManager
    {
        public static List<Train> TrainsList = new List<Train>();

        public static void LoadTrains()
        {
            TrainsList.Add(new Train(15, new Vector3(1838.1046142578125f, 3528.820556640625f, 38.384864807128906f)));
        }

        public static void OnPlayerConnected(IPlayer player)
        {
            player.Emit("LoadsAllTrains", JsonConvert.SerializeObject(TrainsList));
        }
    }

    public class Train
    {
        public Vector3 CurrentPos;
        public byte Type;

        [JsonIgnore]
        public IPlayer Owner;

        public Train(byte type, Vector3 currentPos)
        {
            Type = type;
            CurrentPos = currentPos;
        }
    }
}
