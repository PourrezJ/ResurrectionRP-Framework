using MongoDB.Bson.Serialization.Attributes;
namespace ResurrectionRP_Server.Entities.Players
{
    [BsonIgnoreExtraElements]
    public class AnimationsSync
    {
        public string AnimDict;
        public string AnimName;
        public float BlendInSpeed;
        public float BlendOutSpeed;
        public int Duraction;
        public int Flag;
        public float PlaybackRate;
    }
}
