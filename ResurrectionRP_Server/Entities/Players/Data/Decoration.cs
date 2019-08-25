using System;
using System.Collections.Generic;
using System.Text;

namespace ResurrectionRP_Server.Entities.Players.Data
{
    public class Decoration
    {
        public int Collection { get; set; }
        public int Overlay { get; set; }

        public Decoration(int collection, int overlay)
        {
            Collection = collection;
            Overlay = overlay;
        }
    }
}
