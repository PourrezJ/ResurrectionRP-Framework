using System.Collections.Generic;
using Newtonsoft.Json;
using System.Numerics;
using AltV.Net.Elements.Entities;
using AltV.Net;
using ResurrectionRP_Server.Streamer.Data;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Drawing;
using ResurrectionRP_Server.Entities;

namespace ResurrectionRP_Server.Utils
{
    public class Door
    {
        #region Fields and properties
        public static List<Door> DoorList = new List<Door>();

        public int ID;
        [BsonRepresentation(BsonType.Int64, AllowOverflow = true)]
        public uint Hash;
        public Vector3 Position;
        public bool Locked;
        public bool Hide;
        private TextLabel TextLabel;

        public delegate void InteractDelegate(IPlayer client, Door door);

        [JsonIgnore]
        public InteractDelegate Interact { get; set; }
        #endregion

        #region Methods
        public static Door CreateDoor(uint hash, Vector3 position, bool locked = false, bool hide = false)
        {
            var door = new Door()
            {
                ID = Util.RandomNumber(int.MinValue, int.MaxValue),
                Hash = hash,
                Position = position,
                Locked = locked,
                Hide = hide
            };

            door.TextLabel = TextLabel.CreateTextLabel($"Porte: {((door.Locked) ? "Verrouillée" : "Deverrouillée")}", door.Position, Color.White ,1, 2);
            DoorList.Add(door);
            door.SetDoorLockState(locked);
            return door;
        }

        public void SetDoorLockState(bool lockStatut)
        {
            Locked = lockStatut;
            TextLabel.Text = $"Porte: {((Locked) ? "Verrouillée" : "Deverrouillée")}";
            Alt.EmitAllClients("SetDoorLockState", ID, Locked);
        }

        public static void OnPlayerConnected(IPlayer client)
        {
            client.Emit("SetAllDoorStatut", JsonConvert.SerializeObject(DoorList));
        }
        #endregion
    }
}
