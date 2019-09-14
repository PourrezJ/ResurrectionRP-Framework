using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;

namespace ResurrectionRP_Server.Models
{
    public class ResuPickupManager
    {
        #region Private static properties
        public static List<ResuPickup> ResuPickupList = new List<ResuPickup>();
        #endregion

        public ResuPickupManager()
        {
            AltAsync.OnClient("ObjectManager_InteractPickup", ObjectManager_InteractPickup);
        }

        private Task ResuPickup_Take(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return Task.CompletedTask;

            if (int.TryParse(args[0].ToString(), out int netID))
            {
                /*var resupickup = GetResuPickup(netID);
                if (resupickup != null)
                    await resupickup.Take(client);*/
            }
            return Task.CompletedTask;
        }
        public async Task ObjectManager_InteractPickup(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;
            
            int oid = int.Parse(args[0].ToString());
            var resupickup = GetResuPickup(oid);
            if (resupickup != null)
                await resupickup.Take(client);
        }
        public static ResuPickup GetResuPickup(int netID)
        {
            return ResuPickupList.Find(r => r.Object.id == netID) ?? null;
        }
    }

    public class ResuPickup
    {
        [JsonIgnore]
        public Entities.Objects.Object Object;
        [JsonIgnore]
        public Streamer.Data.TextLabel Label;

        public uint Hash;
        public Item Item;
        public int Quantite;
        public Vector3 Position;
        public bool Hide;

        public delegate Task TakeDelegate(IPlayer client, ResuPickup pickup);
        [JsonIgnore]
        public TakeDelegate OnTakePickup { get; set; }

        public ResuPickup()
        {
        }

        public static ResuPickup CreatePickup(string model, Item item, int quantite, Vector3 position, bool hide, TimeSpan endlife, uint dimension = ushort.MaxValue)
        {
            Entities.Objects.Object obje = Entities.Objects.ObjectManager.CreateObject(model, position, new Vector3(), true, true, dimension);
            var obj = new ResuPickup()
            {
                Hash = Alt.Hash(model),
                Item = item,
                Quantite = quantite,
                Position = position,
                Hide = hide,
                Object = obje
            };

            string str = $"{item.name} x{quantite}";
            if (!hide) obj.Label =
                    GameMode.Instance.Streamer.AddEntityTextLabel(str, obj.Position + new Vector3(0, 0, 0.5f), 0, 255, 255, 255, 120, 3);

            ResuPickupManager.ResuPickupList.Add(obj);

            Utils.Utils.Delay((int)endlife.TotalMilliseconds, true, () =>
            {
                obj.Delete();
            });

            return obj;
        }
        public static string GenerateRandomID()
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] stringChars = new char[4];
            Random random = new Random();
            string generatedPlate = "";

            
            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            return generatedPlate = new string(stringChars);
        }
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

        public void Delete()
        {
            AltAsync.Do(() =>
            {
                if (Object != null)
                {
                    Object.Destroy();
                    Label.Destroy(); 

                    if (ResuPickupManager.ResuPickupList.Contains(this))
                        ResuPickupManager.ResuPickupList.Remove(this);
                    
                }
            });
        }
    }
}
