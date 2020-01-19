using System;
using System.Collections.Generic;
using System.Text;

namespace ResurrectionRP_Server.Streamer.Data
{
    public class TextLabel
    {
        public ulong id;

        private string _text;
        public string text
        {
            set
            {
                _text = value;
                Streamer.UpdateEntityTextLabel(this.id, value);
            }
            get => _text;
        }

        public int font;
        public int r;
        public int g;
        public int b;
        public int a;

        public TextLabel(string text, int font, int r, int g, int b, int a)
        {
            this._text = text;
            this.font = font;
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public Dictionary<string, object> export()
        {
            var data = new Dictionary<string, object>();
            data["text"] = this.text;
            data["font"] = this.font;
            data["r"] = r;
            data["b"] = b;
            data["g"] = g;
            data["a"] = a;
            data["entityType"] = (int)EntityType.TextLabel;
            data["id"] = this.id;
            return data;
        }

        public void Destroy()
        {
            Streamer.DestroyEntity(this.id);
        }
    }
}
