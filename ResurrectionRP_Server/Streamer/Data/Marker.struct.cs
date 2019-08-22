using System;
using System.Collections.Generic;
using System.Text;
using AltV.Net;
using System.Numerics;
using Newtonsoft.Json;

namespace ResurrectionRP_Server.Streamer.Data
{
    public struct Marker
    {
        public int id;
        public MarkerType type;
        public float scalex;
        public float scaley;
        public float scalez;
        public int r;
        public int g;
        public int b;
        public int a;

        public Marker(MarkerType type, Vector3 scale, int r, int g, int b, int a, int entityId)
        {
            this.type = type;
            this.scalex = scale.X;
            this.scaley = scale.Y;
            this.scalez = scale.Z;
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
            this.id = entityId;
        }

        public Dictionary<string, object> export()
        {
            var data = new Dictionary<string, object>();
            data["type"] = (int)this.type;
            data["scalex"] = this.scalex ;
            data["scaley"] = this.scaley;
            data["scalez"] = this.scalez ;
            data["r"] = r;
            data["b"] = b;
            data["g"] = g;
            data["a"] = a;
            data["entityType"] = (int)EntityType.Marker;
            data["id"] = this.id;
            return data;
        }
    }
}
