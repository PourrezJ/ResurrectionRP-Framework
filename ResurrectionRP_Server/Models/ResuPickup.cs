using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using ResurrectionRP_Server.Entities;
using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Models
{
    public class ResuPickup
    {
        #region Delegates
        public delegate Task TakeDelegate(IPlayer client, ResuPickup pickup);
        #endregion

        #region Fields
        public static ConcurrentDictionary<ulong, ResuPickup> ResuPickupList = new ConcurrentDictionary<ulong, ResuPickup>();


        [JsonIgnore]
        public Entities.Objects.WorldObject Object;
        [JsonIgnore]
        public TextLabel Label;

        public uint Hash;
        public Item Item;
        public int Quantite;
        public Vector3 Position;
        public bool Hide;
        #endregion

        #region Events
        public event TakeDelegate OnTakePickup;
        #endregion

        #region Constructor
        public ResuPickup()
        {
        }
        #endregion

        #region Static methods
        public static ResuPickup CreatePickup(string model, Item item, int quantite, Vector3 position, bool hide, TimeSpan endlife, short dimension = GameMode.GlobalDimension)
        {
            Entities.Objects.WorldObject worldObject = Entities.Objects.WorldObject.CreateObject((int)Alt.Hash(model), position, new Vector3(), true, true, dimension);

            ResuPickup pickup = new ResuPickup()
            {
                Hash = Alt.Hash(model),
                Item = item,
                Quantite = quantite,
                Position = position,
                Hide = hide,
                Object = worldObject
            };

            if (!hide)
            {
                string str = $"{item.name} x{quantite}";
                pickup.Label = TextLabel.CreateTextLabel(str, pickup.Position + new Vector3(0, 0, 0.5f), System.Drawing.Color.White);
            }

            ResuPickupList.TryAdd(worldObject.ID, pickup);

            Utils.Utils.Delay((int)endlife.TotalMilliseconds,() => AltAsync.Do(()=> pickup?.Delete()));

            return pickup;
        }
        #endregion

        #region Methods
        public async Task<bool> Take(IPlayer client)
        {
            try
            {
                await OnTakePickup?.Invoke(client, this);
                return true;
            }
            catch (Exception ex)
            {
                Alt.Server.LogError(ex.ToString() + ex);
                return true;
            }
        }

        public async Task Delete()
        {
            if (Object != null)
            {
                await Object.Destroy();

                if (Label != null)
                    await Label.Destroy();

                ResuPickupList.TryRemove(Object.ID, out _);
            }
        }

        public static ResuPickup GetResuPickup(ulong netID)
        {
            ResuPickupList.TryGetValue(netID, out ResuPickup pickup);
            return pickup;
        }
        #endregion
    }
}
