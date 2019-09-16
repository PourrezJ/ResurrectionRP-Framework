using System;
using System.Collections.Generic;
using System.Text;

namespace ResurrectionRP_Server.Business.Barber
{
    public class Beards
    {
        public int ID;
        public string Name;
        public int Price;

        public static List<Beards> BeardsList = new List<Beards>
        {
            new Beards {ID = -1, Name = "Rasé précis", Price = 50},
            new Beards {ID = 0, Name = "Light Stubble", Price = 50},
            new Beards {ID = 1, Name = "Balbo", Price = 50},
            new Beards {ID = 2, Name = "Circle Beard", Price = 50},
            new Beards {ID = 3, Name = "Goatee", Price = 50},
            new Beards {ID = 4, Name = "Chin", Price = 50},
            new Beards {ID = 5, Name = "Chin Fuzz", Price = 50},
            new Beards {ID = 6, Name = "Pencil Chin Strap", Price = 50},
            new Beards {ID = 7, Name = "Scruffy", Price = 50},
            new Beards {ID = 8, Name = "Musketeer", Price = 50},
            new Beards {ID = 9, Name = "Mustache", Price = 50},
            new Beards {ID = 10, Name = "Trimmed Beard", Price = 50},
            new Beards {ID = 11, Name = "Stubble", Price = 50},
            new Beards {ID = 12, Name = "Thin Circle Beard", Price = 50},
            new Beards {ID = 13, Name = "Horseshoe", Price = 50},
            new Beards {ID = 14, Name = "Pencil and 'Chops", Price = 50},
            new Beards {ID = 15, Name = "Chin Strap Beard", Price = 50},
            new Beards {ID = 16, Name = "Balbo and Sideburns", Price = 50},
            new Beards {ID = 17, Name = "Mutton Chops", Price = 50},
            new Beards {ID = 18, Name = "Scruffy Beard", Price = 50},
            new Beards {ID = 19, Name = "Curly", Price = 50},
            new Beards {ID = 20, Name = "Curly & Deep Stranger", Price = 50},
            new Beards {ID = 21, Name = "Handlebar", Price = 50},
            new Beards {ID = 22, Name = "Faustic", Price = 50},
            new Beards {ID = 23, Name = "Otto & Patch", Price = 50},
            new Beards {ID = 24, Name = "Otto and Full Stranger", Price = 50},
            new Beards {ID = 25, Name = "Light Franz", Price = 50},
            new Beards {ID = 26, Name = "The Hampstead", Price = 50},
            new Beards {ID = 27, Name = "The Ambrose", Price = 50},
            new Beards {ID = 28, Name = "Lincoln Curtain", Price = 50}
        };
    }
}
