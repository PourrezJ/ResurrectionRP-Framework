using ClothData = ResurrectionRP_Server.Models.ClothData;
using AltV.Net;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System;

namespace ResurrectionRP_Server.Database
{
    class ClothDataSerializer : SerializerBase<ClothData>
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, ClothData value)
        {
            context.Writer.WriteStartDocument();
            context.Writer.WriteName("Drawable");
            context.Writer.WriteInt32(value.Drawable);
            context.Writer.WriteName("Palette");
            context.Writer.WriteInt32(value.Palette);
            context.Writer.WriteName("Texture");
            context.Writer.WriteInt32(value.Texture);
            context.Writer.WriteEndDocument();
        }

        public override ClothData Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            if (context.Reader.CurrentBsonType != BsonType.Document)
            {
                return new ClothData();
            }
            var rawDoc = context.Reader?.ReadRawBsonDocument();
            if (rawDoc == null) return new ClothData();
            var doc = new RawBsonDocument(rawDoc);

            Boolean providedDrawable = doc.Contains("Drawable");
            Boolean providedTexture = doc.Contains("Texture");
            Boolean providedPalette = doc.Contains("Palette");

            if (providedDrawable && providedTexture && providedPalette)
            {
                try
                {
                    var clothdata = new ClothData((byte)doc.GetElement("Drawable").Value.ToInt32(), (byte)doc.GetElement("Texture").Value.ToInt32(), (byte)doc.GetElement("Palette").Value.ToInt32());
                    return clothdata;
                }
                catch (Exception ex)
                {
                    Alt.Server.LogError("Vector Serializer" +  ex);
                    return new ClothData();
                }
            }
            else
            {
                Alt.Server.LogError("Deserialization Problem - Data Structure is not valid");
                throw new ApplicationException("Deserialization Problem - Data Structure is not valid");
            }

        }
    }
}