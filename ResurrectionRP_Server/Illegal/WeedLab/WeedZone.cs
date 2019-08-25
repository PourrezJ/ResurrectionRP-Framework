using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Timers;
using AltV.Net;
using AltV.Net.Elements.Entities;

namespace ResurrectionRP_Server.Illegal.WeedLab
{
    #region Enumerator
    public enum Spray
    {
        Off,
        On
    }

    public enum StateZone
    {
        Stage0,
        Stage1,
        Stage2,
        Stage3
    }

    public enum SeedType
    {
        Aucune,
        Skunk,
        Purple,
        Orange,
        WhiteWidow
    }
    #endregion

    public class WeedZone
    {
        public delegate Task OnGrowingChangeDelegate(WeedZone zone, bool growingStateChange);
        public delegate Task OnGrowingClientEnterDelegate(IPlayer client, WeedZone zone);

        [JsonIgnore, BsonIgnore]
        public OnGrowingChangeDelegate OnGrowingChange { get; set; }
        [JsonIgnore, BsonIgnore]
        public OnGrowingClientEnterDelegate OnGrowingClientEnter { get; set; }
        [JsonIgnore, BsonIgnore]
        public IColShape Colshape = null;
/*        [JsonIgnore, BsonIgnore]
        public ITextLabel Textlabel = null;
        [JsonIgnore, BsonIgnore]
        public IMarker Marker = null;*/
        [JsonIgnore, BsonIgnore]
        DateTime ArrosageTime;
        [JsonIgnore, BsonIgnore]
        DateTime MaxGrowtimeEtape;
        [JsonIgnore, BsonIgnore]
        public Timer Timer = new Timer(5000);

        public int ID;
        public Spray Spray = Spray.Off;
        public StateZone GrowingState;
        public SeedType SeedUsed = SeedType.Aucune;
        public int Hydratation = 0;
        public Vector3 Position;
        public int Advert;

        private bool _plant;
        public bool Plant
        {
            get => _plant;
            set
            {
                _plant = value;
                if (value)
                {
                    ArrosageTime = DateTime.Now.AddMinutes(1);
                    MaxGrowtimeEtape = DateTime.Now.Add(new TimeSpan(0, 0, 3, 0));
                    if (Timer != null)
                        Timer.Start();
                }
                else Timer?.Stop();
            }
        }

        public WeedZone(int id, StateZone state, SeedType seedtype, Vector3 position)
        {

            ID = id;
            GrowingState = state;
            SeedUsed = seedtype;
            Position = position;
        }

        public async Task GrowLoop()
        {

            try
            {
                if (Spray == Spray.Off && DateTime.Now >= ArrosageTime)
                {
                    Advert = (Hydratation > 0) ? Advert = 0 : Advert++;

                    ArrosageTime = DateTime.Now.AddMinutes(1);

                    if (OnGrowingChange != null)
                        await OnGrowingChange.Invoke(this, false);
                }

                if (Hydratation > 0 && DateTime.Now > MaxGrowtimeEtape)
                {
                    GrowingState++;

                    if (OnGrowingChange != null)
                        await OnGrowingChange.Invoke(this, true);
                    MaxGrowtimeEtape = DateTime.Now.Add(new TimeSpan(0, 0, 15, 0));
                }

                if (GrowingState == StateZone.Stage3)
                {
                    if (Timer != null)
                        Timer.Stop();
                    Spray = Spray.Off;
                }
            }
            catch (Exception ex)
            {
                Alt.Server.LogError(ex.ToString());
            }
        }

        public async Task OnGrowingZoneEnter(IColShape colshape, IPlayer client)
        {
            colshape.GetData("id", out int colid);
            this.Colshape.GetData("id", out int Colid);
            if (OnGrowingClientEnter != null && colid == Colid)
                await OnGrowingClientEnter.Invoke(client, this);
        }
    }
}
