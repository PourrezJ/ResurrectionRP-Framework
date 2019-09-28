using AltV.Net;
using AltV.Net.NetworkingEntity;
using AltV.Net.NetworkingEntity.Elements.Entities;
using ResurrectionRP_Server.Streamer.Data;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace ResurrectionRP_Server.Entities
{
    public class Marker
    {
        public int id;
        public MarkerType type;
        public Vector3 pos;
        public float scalex = 1;
        public float scaley = 1;
        public float scalez = 1;
        public int r;
        public int g;
        public int b;
        public int a;
        public int dimension;

        public static Marker CreateMarker(MarkerType type, Vector3 pos, Vector3? scale = null, Color? color = null, int dimension = GameMode.GlobalDimension)
        {
            var marker = new Marker();

            marker.type = type;
            marker.pos = pos;

            if (scale != null)
            {
                marker.scalex = scale.Value.X;
                marker.scaley = scale.Value.Y;
                marker.scalez = scale.Value.Z;
            }

            if (color != null)
            {
                marker.r = color.Value.R;
                marker.g = color.Value.G;
                marker.b = color.Value.B;
                marker.a = color.Value.A;
            }
            else
            {
                marker.r = 255;
                marker.g = 255;
                marker.b = 255;
                marker.a = 60;
            }

            marker.id = GameMode.Instance.Streamer.EntityNumber++;
            marker.dimension = dimension;

            INetworkingEntity item = AltNetworking.CreateEntity(pos.ConvertToEntityPosition(), dimension, GameMode.Instance.StreamDistance, marker.Export());
            GameMode.Instance.Streamer.ListEntities.TryAdd(marker.id, item);
            return marker;
        }

        public static void DestroyMarker(Marker marker)
        {
            GameMode.Instance.Streamer.ListEntities[marker.id].Remove();
            GameMode.Instance.Streamer.ListEntities[marker.id] = null;
        }

        public Dictionary<string, object> Export()
        {
            var data = new Dictionary<string, object>();
            data["type"] = (int)this.type;
            data["scalex"] = this.scalex;
            data["scaley"] = this.scaley;
            data["scalez"] = this.scalez;
            data["r"] = this.r;
            data["b"] = this.g;
            data["g"] = this.b;
            data["a"] = this.a;
            data["entityType"] = (int)EntityType.Marker;
            data["id"] = this.id;
            return data;
        }

        internal void SetColor(Color color)
        {
            this.r = color.R;
            this.g = color.G;
            this.b = color.B;
            this.a = color.A;

            GameMode.Instance.Streamer.ListEntities[id].SetData("r", color.R);
            GameMode.Instance.Streamer.ListEntities[id].SetData("g", color.G);
            GameMode.Instance.Streamer.ListEntities[id].SetData("b", color.B);
            GameMode.Instance.Streamer.ListEntities[id].SetData("a", color.A);
        }

        internal void Destroy()
        {
            GameMode.Instance.Streamer.DestroyEntity(this.id);
        }
    }

    public enum MarkerType
    {
        UpsideDownCone = 0,
        VerticalCylinder = 1,
        ThickCevronUp = 2,
        ThinCevronUp = 3,
        CheckeredFlagRect = 4,
        CheckeredFlagCircle = 5,
        VerticalCircle = 6,
        PlaneModel = 7,
        ChevronUpX1 = 20,
        ChevronUpX2 = 21,
        ChevronUpX3 = 22,
        HorizontalCircleFlat = 23,
        orizontalCircleSkinny = 25,
        HorizontalCircleArrow = 26,
        HorizontalSplitArrowCircle = 27,
        MarkerTypeDebugSphere = 28,
        MarkerTypeDollarSign = 29,
        MarkerTypeHorizontalBars = 30,
        MarkerTypeWolfHead = 31,
        MarkerTypeQuestionMark = 32,
        MarkerTypePlaneSymbol = 33,
        MarkerTypeHelicopterSymbol = 34,
        MarkerTypeBoatSymbol = 35,
        MarkerTypeCarSymbol = 36,
        MarkerTypeMotorcycleSymbol = 37,
        MarkerTypeBikeSymbol = 38,
        MarkerTypeTruckSymbol = 39,
        MarkerTypeParachuteSymbol = 40,
        MarkerTypeSawbladeSymbol = 41
    }
}
