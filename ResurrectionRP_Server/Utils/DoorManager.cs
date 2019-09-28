using System.Collections.Generic;
using Newtonsoft.Json;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using AltV.Net;
using ResurrectionRP_Server.Streamer.Data;

namespace ResurrectionRP_Server.Utils
{
    public class Door
    {
        public int ID;
        public int Hash;
        public Vector3 Position;
        public bool Locked;
        public bool Hide;
        private TextLabel TextLabel;

        public delegate void InteractDelegate(IPlayer client, Door door);

        [JsonIgnore]
        public InteractDelegate Interact { get; set; }

        public static Door CreateDoor(int hash, Vector3 position, bool locked = false, bool hide = false)
        {
            var door = new Door()
            {
                ID = Utils.RandomNumber(int.MinValue, int.MaxValue),
                Hash = hash,
                Position = position,
                Locked = locked,
                Hide = hide
            };
            door.TextLabel = GameMode.Instance.Streamer.AddEntityTextLabel($"Porte: {((door.Locked) ? "Verrouillée" : "Deverrouillée")}", door.Position, 1, drawDistance:2);
            GameMode.Instance.DoorManager?.DoorList.Add(door);
            door.SetDoorLockState(locked);
            return door;
        }

        public static Door CreateDoor(uint hash, Vector3 position, bool locked = false, bool hide = false)
        {
            var door = new Door()
            {
                ID = Utils.RandomNumber(int.MinValue, int.MaxValue),
                Hash = (int)hash,
                Position = position,
                Locked = locked,
                Hide = hide
            };
            door.TextLabel = GameMode.Instance.Streamer.AddEntityTextLabel($"Porte: {((door.Locked) ? "Verrouillée" : "Deverrouillée")}", door.Position, 1, drawDistance: 2);
            GameMode.Instance.DoorManager?.DoorList.Add(door);
            door.SetDoorLockState(locked);
            return door;
        }


        public void SetDoorLockState(bool lockStatut)
        {
            Locked = lockStatut;
            GameMode.Instance.Streamer.UpdateEntityTextLabel(this.TextLabel.id, $"Porte: {((Locked) ? "Verrouillée" : "Deverrouillée")}");

            Alt.EmitAllClients("SetDoorLockState", ID, Locked);
        }

        public void SetDoorOpenState(float angle)
        {
            Alt.EmitAllClients("SetDoorLockState", ID, angle);
        }
    }
    public class DoorManager
    {
        public List<Door> DoorList = new List<Door>();

        public void OnPlayerConnected(IPlayer client)
        {
            client.Emit("SetAllDoorStatut", JsonConvert.SerializeObject(DoorList));
        }
    }
}
