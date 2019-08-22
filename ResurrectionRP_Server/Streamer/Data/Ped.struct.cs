using System;
using System.Collections.Generic;
using System.Text;
using AltV.Net;

namespace ResurrectionRP_Server.Streamer.Data
{
    public struct Ped
    {
        public int id;
        public uint model;
        public PedType type;
        public float heading;
        public Ped(string model, PedType type, float heading, int entityId)
        {
            this.model = Alt.Hash(model);
            this.type = type;
            this.heading = heading;
            this.id = entityId;
        }

        public Dictionary<string, object> export()
        {
            var data = new Dictionary<string, object>();
            data["model"] = this.model;
            data["type"] = (int)this.type;
            data["heading"] = this.heading;
            data["entityType"] = (int)EntityType.Ped;
            data["id"] = this.id;
            return data;
        }
    }
}
