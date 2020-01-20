using AltV.Net.NetworkingEntity;
using ResurrectionRP_Server.Streamer.Data;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace ResurrectionRP_Server.Entities
{
    public class TextLabel : Entity
    {
        private string _text;
        public string Text
        {
            set
            {
                _text = value;
                if (NetworkEntity.Exists)
                    NetworkEntity.SetData("text", value);
            }
            get => _text;
        }

        public int Font;

        public Color Color;
        
        public TextLabel(string text, int font, Color color, Vector3 position, int drawdistance = 5, int dimension = GameMode.GlobalDimension) : base(position, dimension)
        {
            _text = text;
            Font = font;
            Color = color;
            NetworkEntity = AltNetworking.CreateEntity(position.ConvertToEntityPosition(), dimension, drawdistance, Export(), StreamingType.Default);
        }

        public override Dictionary<string, object> Export()
        {
            var data = new Dictionary<string, object>();
            data["text"] = Text;
            data["font"] = Font;
            data["r"] = (int)Color.R;
            data["b"] = (int)Color.B;
            data["g"] = (int)Color.G;
            data["a"] = (int)Color.A;
            data["entityType"] = (int)EntityType.TextLabel;
            return data;
        }

        public static TextLabel CreateTextLabel(string label, Vector3 pos, Color? color = null, int font = 0, int drawDistance = 5, int dimension = GameMode.GlobalDimension)
        {
            var data = new TextLabel(label, font, (color != null) ? color.Value : Color.FromArgb(128,255,255,255), pos, drawDistance, dimension);
            return data;
        }
    }
}
