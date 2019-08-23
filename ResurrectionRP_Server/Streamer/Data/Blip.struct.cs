using System;
using System.Collections.Generic;
using System.Text;
using AltV.Net;
using System.Numerics;

namespace ResurrectionRP_Server.Streamer.Data
{
    public struct Blips
    {
        public int id;
        public string name;
        public int color;
        public int sprite;
        public float posx;
        public float posy;
        public float posz;

        public Blips(string name, Vector3 pos, int color, int sprite, int entityId)
        {
            this.id = entityId;
            this.name = name;
            this.color = color;
            this.sprite = sprite;
            this.posx = pos.X;
            this.posy = pos.Y;
            this.posz = pos.Z;

        }

        public Dictionary<string, object> export()
        {
            var data = new Dictionary<string, object>();
            data["name"] = this.name;
            data["posx"] = this.posx;
            data["posy"] = this.posy;
            data["posz"] = this.posz;
            data["color"] = this.color;
            data["sprite"] = this.sprite;
            data["entityType"] = (int)EntityType.Blip;
            data["id"] = this.id;
            return data;
        }
    }
}
