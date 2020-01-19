using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ResurrectionRP_Server.Streamer.Data
{
    public class TextLabel
    {
        public ulong ID;

        private string _text;
        public string Text
        {
            set
            {
                _text = value;
                Streamer.UpdateEntityTextLabel(ID, value);
            }
            get => _text;
        }

        public int Font;
        public Color Color;

        public TextLabel(string text, int font, Color color)
        {
            _text = text;
            Font = font;
            Color = color;
        }

        public TextLabel(string text, int font, int r, int g, int b, int a)
        {
            this._text = text;
            this.Font = font;
            this.Color = Color.FromArgb(a, r, g, b);
        }

        public Dictionary<string, object> export()
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

        public void Destroy()
        {
            Streamer.DestroyEntity(ID);
        }
    }
}
