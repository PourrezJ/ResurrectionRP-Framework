using MongoDB.Bson.Serialization;
using System;
using System.Drawing;

namespace ResurrectionRP_Server.Database
{
    public class ColorBsonSerializer : IBsonSerializer
    {
        public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var value = context.Reader.ReadInt32();
            return Color.FromArgb(value);
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            context.Writer.WriteInt32(((Color)value).ToArgb());
        }

        public Type ValueType
        {
            get { return typeof(Color); }
        }
    }
}