using AltV.Net.Data;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Numerics;

namespace ResurrectionRP_Server.Models
{
    public class Location
    {
        [JsonProperty("position"), BsonElement("pos")]
        public Vector3 Pos { get; set; }
        [JsonProperty("rotation"), BsonElement("rot")]
        public Vector3 Rot { get; set; }

        public Location(Vector3 pos, Vector3 rot)
        {
            this.Pos = pos;
            this.Rot = rot;
        }

        public bool IsZero()
        {
            if (Pos == Vector3.Zero && Rot == Vector3.Zero) return true;
            return false;
        }

        public Rotation GetRotation()
            => new Rotation(Rot.X, Rot.Y, Rot.Z);
        /*
                public Rotation GetRotationFromRadianToDegree()
                    => new Rotation(Rot.X * (180 / 3.14159f), Rot.Y * (180 / 3.14159f), Rot.Z * (180 / 3.14159f ));


                public Rotation GetRotation()
                    => new Rotation(Rot.X, Rot.Y, Rot.Z);

                public Rotation GetRotationFromDegreeToRadian()
                    => new Rotation(Rot.X * (3.14159f/180) , Rot.Y * (3.14159f / 180), Rot.Z * (3.14159f / 180));*/
    }
}