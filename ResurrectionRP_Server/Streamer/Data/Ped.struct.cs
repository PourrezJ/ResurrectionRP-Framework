using System;
using System.Collections.Generic;
using System.Text;
using AltV.Net;

namespace ResurrectionRP_Server.Streamer.Data
{
    public struct Ped
    {
        public int ID;
        public uint Model;
        public PedType Type;
        public float Heading;
        public bool Freeze;
        public bool Invicible;
        public string CallbackName;
        public Ped(string model, PedType type, float heading, int entityId, string callbackName = null, bool freeze = true, bool invicible = true)
        {
            this.Model = Alt.Hash(model);
            this.Type = type;
            this.Heading = heading;
            this.Freeze = freeze;
            this.Invicible = invicible;
            this.CallbackName = callbackName;
            this.ID = entityId;
        }

        public Dictionary<string, object> export()
        {
            var data = new Dictionary<string, object>();
            data["model"] = this.Model;
            data["type"] = (int)this.Type;
            data["heading"] = this.Heading;
            data["freeze"] = this.Freeze;
            data["callbackName"] = this.CallbackName;
            data["invicible"] = this.Invicible;
            data["entityType"] = (int)EntityType.Ped;
            data["id"] = this.ID;
            return data;
        }
    }
}
