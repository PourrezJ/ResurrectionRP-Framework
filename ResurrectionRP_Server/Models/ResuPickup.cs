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
            Alt.On("ResuPickup_Take", (IPlayer client, object[] args) =>ResuPickup_Take(client, args));
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
        /*
        public static ResuPickup CreatePickup(Vector3 pos, Item item, int quantite, int hash = 1108364521, bool hide = false)
        {
            ResuPickup pickup = new ResuPickup(hash, item, quantite, pos, hide);
            ResuPickupList.Add(pick up);
            return pickup;
        }*/

/*        public static ResuPickup GetResuPickup(int netID)
        {
            return ResuPickupList.Find(r => r.Object.NetHandle == netID) ?? null;
        }*/
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
        public string Id;

        public delegate Task TakeDelegate(IPlayer client, ResuPickup pickup);
        [JsonIgnore]
        public TakeDelegate OnTakePickup { get; set; }

        public ResuPickup()
        {
        }

        public static ResuPickup CreatePickup(string model, Item item, int quantite, Vector3 position, bool hide, TimeSpan endlife, uint dimension = ushort.MaxValue)
        {
            Entities.Objects.Object obje = Entities.Objects.ObjectManager.CreateObject(model, position, new Vector3(), true, true, dimension, GenerateRandomID());
            var obj = new ResuPickup()
            {
                Hash = Alt.Hash(model),
                Item = item,
                Quantite = quantite,
                Position = position,
                Hide = hide,
                Object = obje,
                Id = obje.pickup
            };

            string str = $"{item.name} x{quantite}";
            if (!hide) obj.Label =
                    GameMode.Instance.Streamer.AddEntityTextLabel(str, obj.Position + new Vector3(0,0,0.5f), 0, 255,255,255,120, 3);
            //obj.Object.IObject.SetSharedData("ItemDrop", obj);

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
        {/**
            AltAsync.Do(() =>
            {
                if (Object != null)
                {
                    if (Object.IObject != null && Object.IObject.Exists)
                    {
                        Object.IObject.Destroy();
                        Label.Destroy(); TODO

                        if (ResuPickupManager.ResuPickupList.Contains(this))
                            ResuPickupManager.ResuPickupList.Remove(this);
                    }
                }
            });**/
        }
    }
}
