using System.Collections.Generic;
using Newtonsoft.Json;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using AltV.Net;
namespace ResurrectionRP_Server.Utils
{
    public class Door
    {
        public int ID;
        public int Hash;
        public Vector3 Position;
        public bool Locked;
        public bool Hide;
        private int TextLabel;

        public delegate Task InteractDelegate(IPlayer client, Door door);
        [JsonIgnore]
        public InteractDelegate Interact { get; set; }

        public Door()
        {

        }

        public async static Task<Door> CreateDoor(int hash, Vector3 position, bool locked = false, bool hide = false)
        {
            var door = new Door()
            {
                ID = Utils.RandomNumber(int.MinValue, int.MaxValue),
                Hash = hash,
                Position = position,
                Locked = locked,
                Hide = hide
            };
            door.TextLabel = GameMode.Instance.Streamer.AddEntityTextLabel($"Porte: {((door.Locked) ? "Verrouillée" : "Deverrouillée")}", door.Position, 1);
            GameMode.Instance.DoorManager?.DoorList.Add(door);
            await door.SetDoorLockState(locked);
            return door;
        }

        public async static Task<Door> CreateDoor(uint hash, Vector3 position, bool locked = false, bool hide = false)
        {
            var door = new Door()
            {
                ID = Utils.RandomNumber(int.MinValue, int.MaxValue),
                Hash = (int)hash,
                Position = position,
                Locked = locked,
                Hide = hide
            };
            door.TextLabel = GameMode.Instance.Streamer.AddEntityTextLabel($"Porte: {((door.Locked) ? "Verrouillée" : "Deverrouillée")}", door.Position, 1);
            GameMode.Instance.DoorManager?.DoorList.Add(door);
            await door.SetDoorLockState(locked);
            return door;
        }


        public async Task SetDoorLockState(bool lockStatut)
        {
            Locked = lockStatut;
            GameMode.Instance.Streamer.UpdateEntityTextLabel(this.TextLabel, $"Porte: {((Locked) ? "Verrouillée" : "Deverrouillée")}");
            foreach(IPlayer player in Alt.GetAllPlayers())
            {
                await player.EmitAsync("SetDoorLockState", ID, Locked);
            }
            //await MP.Players.CallAsync("SetDoorLockState", ID, Locked);
        }

        public async Task SetDoorOpenState(float angle)
        {
            foreach (IPlayer player in Alt.GetAllPlayers())
            {
                await player.EmitAsync("SetDoorLockState", ID, angle);
            }
            //await MP.Players.CallAsync("SetDoorOpenState", ID, angle);
        }
    }
    public class DoorManager
    {
        public List<Door> DoorList = new List<Door>();

        public DoorManager()
        {
            Alt.OnClient("DoorManager_Interact", DoorManager_Interact);
        }

        public async Task OnPlayerConnected(IPlayer client)
        {
            await client.EmitAsync("SetAllDoorStatut", JsonConvert.SerializeObject(DoorList));
        }

        private async void DoorManager_Interact(IPlayer client, object[] args)
        {
            Door door = DoorList.Find(d => d.ID == (int)args[0]);
            await door.Interact?.Invoke(client, door);
        }
    }
}
