using System;
using System.Collections.Generic;
using System.Text;
using AltV.Net;

namespace ResurrectionRP_Server.Streamer.Data
{
    public struct Object
    {
        public int id;
        public uint model;
        public Object(string model, int entityId)
        {
            this.model = Alt.Hash(model);
            this.id = entityId;
        }

        public Dictionary<string, object> export()
        {
            var data = new Dictionary<string, object>();
            data["model"] = this.model;
            data["entityType"] = (int)EntityType.Object;
            data["id"] = this.id;
            return data;
        }
    }
}
