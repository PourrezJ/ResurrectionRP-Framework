using System;
using System.Collections.Generic;
using System.Text;
using AltV.Net;
using System.Numerics;

namespace ResurrectionRP_Server.Entities.Blips
{
    public class Blips
    {
        public int id;
        public string name;
        public int color;
        public int sprite;
        public float posx;
        public float posy;
        public float posz;
        public float scale;
        public bool shortRange;
        public Streamer.Data.EntityType type = Streamer.Data.EntityType.Blip;

        public Blips(string name, Vector3 pos, int color, int sprite, float scale, bool shortRange, int entityId)
        {
            this.id = entityId;
            this.name = name;
            this.color = color;
            this.sprite = sprite;
            this.posx = pos.X;
            this.posy = pos.Y;
            this.posz = pos.Z;
            this.scale = scale;
            this.shortRange = shortRange;

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
            data["scale"] = this.scale;
            data["shortRange"] = this.shortRange;
            data["entityType"] = (int)this.type;
            data["id"] = this.id;
            return data;
        }

        public void setColorAsyc(int color)
        {
            BlipsManager.SetColor(this, color);
        }
    }
}
