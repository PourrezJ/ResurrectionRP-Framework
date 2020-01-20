using AltV.Net.Data;
using AltV.Net.NetworkingEntity;
using AltV.Net.NetworkingEntity.Elements.Entities;
using ResurrectionRP_Server.Streamer.Data;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace ResurrectionRP_Server.Entities
{
    public class Marker : Entity
    {
        public MarkerType Type;

        public Vector3 Scale;

        public Color Color;

        public Marker(MarkerType type, Vector3 position, Vector3 scale, Color? color, int dimension = GameMode.GlobalDimension) : base(position, dimension)
        {
            Type = type;
            Scale = scale;
            Color = (color != null) ? color.Value : Color.FromArgb(60, 255, 255, 255);
            NetworkEntity = AltNetworking.CreateEntity(position.ConvertToEntityPosition(), dimension, GameMode.StreamDistance, Export(), StreamingType.EntityStreaming);
        }

        public static Marker CreateMarker(MarkerType type, Vector3 pos, Vector3 scale, Color? color = null, int dimension = GameMode.GlobalDimension)
        {
            var marker = new Marker(type, pos, scale, color);
            return marker;
        }

        public override Dictionary<string, object> Export()
        {
            var data = new Dictionary<string, object>();
            data["type"] = (int)Type;
            data["scalex"] = Scale.X;
            data["scaley"] = Scale.Y;
            data["scalez"] = Scale.Z;
            data["r"] = (int)Color.R;
            data["b"] = (int)Color.B;
            data["g"] = (int)Color.G;
            data["a"] = (int)Color.A;
            data["entityType"] = (int)EntityType.Marker;
            return data;
        }

        internal void SetColor(Color color)
        {
            Color = color;

            NetworkEntity.SetData("r", (int)color.R);
            NetworkEntity.SetData("g", (int)color.G);
            NetworkEntity.SetData("b", (int)color.B);
            NetworkEntity.SetData("a", (int)color.A);
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
