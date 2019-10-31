using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Entities.Worlds
{
    public class TrainManager
    {
        public static bool TrainLoaded = false;
        public static List<Train> TrainsList = new List<Train>();

        public static void LoadTrains()
        {
            TrainsList.Add(new Train((byte)Utils.Utils.RandomNumber(0, 22), new Vector3(1838.104f, 3528.820f, 38.384f), "Train", 0));
            TrainsList.Add(new Train(24, new Vector3(40.2f, -1201.3f, 31.0f), "Métro", 0)); // metro
            TrainsList.Add(new Train(24, new Vector3(-618.0f, -1476.8f, 16.2f), "Métro", 0)); // metro
            TrainLoaded = true;

            Alt.OnClient("TrainManager_PosUpdate", PosUpdate);
        }

        public static void OnPlayerConnected(IPlayer player)
        {
            while (!TrainLoaded)
                Task.Delay(20);

            // On envoi au joueur la liste des trains
            player.EmitLocked("LoadsAllTrains", JsonConvert.SerializeObject(TrainsList));

            // Check si ping élevé
            if (player.Ping > 60)
                return;

            // Attribution d'un train qui ne serai pas déjà gérer par un joueur
            foreach (Train train in TrainsList)
            {
                if (train.Owner == null)
                {
                    TrainAttribute(player, train);
                    break;
                }   
            }
        }

        private static void PosUpdate(IPlayer player, object[] args)
        {
            // reception par le owner de la nouvelle position du train
            int networkID = Convert.ToInt32(args[0]);
            Vector3 pos = new Vector3(Convert.ToSingle(args[1]), Convert.ToSingle(args[2]), Convert.ToSingle(args[3]));

            Train train = TrainsList.Find(t => t.NetworkID == networkID);
            lock (train)
            {
                if (train == null)
                    return;

                train.CurrentPos = pos;

                foreach (IPlayer client in Alt.GetAllPlayers())
                {
                    if (!client.Exists)
                        continue;

                    if (client.Position.Distance(train.CurrentPos) > 500)
                        continue;

                    if (client != train.Owner) // ne pas renvoyé la nouvelle pos au owner
                        client.EmitLocked("Train_PosUpdate", networkID, pos.ConvertToVector3Serialized());
                }
            }
        }

        public static void OnPlayerDisconnected(IPlayer player)
        {
            // recherche si un train est attribuer au joueur qui déconnecte pour le rendre libre
            foreach (Train train in TrainsList)
            {
                if (train.Owner == player)
                {
                    // On nullifie si aucun joueur n'est encore présent
                    train.Owner = null;
                    // et on recherche un nouveau owner
                    foreach(IPlayer otherClient in Alt.GetAllPlayers())
                    {
                        if (!HasOwnTrain(otherClient))
                            train.Owner = otherClient;
                    }
                }
            }
        }

        public static bool HasOwnTrain(IPlayer player) 
            => TrainsList.Exists(t => t.Owner == player);

        public static void TrainAttribute(IPlayer player, Train train)
        {
            train.Owner = player;
            Console.WriteLine($"Train Manager: attribution du train ID: {train.NetworkID} à {player.GetSocialClub()}");
            player.EmitLocked("Train_OwnUpdate", train.NetworkID, true);
        }
    }

    public class Train
    {
        public int NetworkID;
        public Vector3 CurrentPos;
        public byte Type;
        public int Speed;
        public string Name;

        [JsonIgnore]
        public IPlayer Owner;

        public Train(byte type, Vector3 currentPos, string name, int speed)
        {
            NetworkID = TrainManager.TrainsList.Count + 1;
            Type = type;
            CurrentPos = currentPos;
            Speed = speed;
            Name = name;
        }
    }
}
