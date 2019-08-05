using AltV.Net;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Numerics;

namespace ResurrectionRP_Server.Database
{
    class VectorSerializer : SerializerBase<Vector3>
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Vector3 value)
        {
            context.Writer.WriteStartDocument();
            context.Writer.WriteName("X");
            context.Writer.WriteDouble(value.X);
            context.Writer.WriteName("Y");
            context.Writer.WriteDouble(value.Y);
            context.Writer.WriteName("Z");
            context.Writer.WriteDouble(value.Z);
            context.Writer.WriteEndDocument();
        }

        public override Vector3 Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            if (context.Reader.CurrentBsonType != BsonType.Document)
            {
                return new Vector3();
            }
            var rawDoc = context.Reader?.ReadRawBsonDocument();
            if (rawDoc == null) return new Vector3();
            var doc = new RawBsonDocument(rawDoc);

            Boolean providedX = doc.Contains("X");
            Boolean providedY = doc.Contains("Y");
            Boolean providedZ = doc.Contains("Z");

            if (providedX && providedY && providedZ)
            {
                try
                {
                    var vector = new Vector3(
                          (doc.GetElement("X").Value.IsDouble) ? (float)doc.GetElement("X").Value.AsDouble : doc.GetElement("X").Value.AsInt32,
                          (doc.GetElement("Y").Value.IsDouble) ? (float)doc.GetElement("Y").Value.AsDouble : doc.GetElement("Y").Value.AsInt32,
                          (doc.GetElement("Z").Value.IsDouble) ? (float)doc.GetElement("Z").Value.AsDouble : doc.GetElement("Z").Value.AsInt32
                         );
                    return vector;
                }
                catch (Exception ex)
                {
                    Alt.Server.LogError("Vector Serializer: " + ex);
                    return Vector3.Zero;
                }
            }
            else
            {
                Alt.Server.LogInfo("Deserialization Problem - Data Structure is not valid");
                throw new ApplicationException("Deserialization Problem - Data Structure is not valid");
            }
        }
    }
}