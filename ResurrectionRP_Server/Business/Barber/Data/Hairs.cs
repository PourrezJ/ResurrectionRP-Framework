﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ResurrectionRP_Server.Business.Barber
{
    public class Hairs
    {
        public byte ID;
        public string Name;
        public int Price;

        public static List<Hairs> HairsMenList = new List<Hairs>
        {
            new Hairs {ID = 0, Name = "Rasage de prés", Price = 50},
            new Hairs {ID = 1, Name = "Boule à zéro", Price = 50},
            new Hairs {ID = 2, Name = "Crête avec côtés rasés", Price = 50},
            new Hairs {ID = 3, Name = "Hipster", Price = 50},
            new Hairs {ID = 4, Name = "Raie d'un côté", Price = 50},
            new Hairs {ID = 5, Name = "Coupe courte", Price = 50},
            new Hairs {ID = 6, Name = "Biker", Price = 50},
            new Hairs {ID = 7, Name = "Queue de cheval", Price = 50},
            new Hairs {ID = 8, Name = "Tresse africaine", Price = 50},
            new Hairs {ID = 9, Name = "Lissé", Price = 50},
            new Hairs {ID = 10, Name = "Court peigné", Price = 50},
            new Hairs {ID = 11, Name = "Punk", Price = 50},
            new Hairs {ID = 12, Name = "Caesar", Price = 50},
            new Hairs {ID = 13, Name = "Coupé", Price = 50},
            new Hairs {ID = 14, Name = "Dreads", Price = 50},
            new Hairs {ID = 15, Name = "Cheveux longs", Price = 50},
            new Hairs {ID = 16, Name = "Frisé", Price = 50},
            new Hairs {ID = 17, Name = "Coupe de surfer", Price = 50},
            new Hairs {ID = 18, Name = "Court sur le côté", Price = 50},
            new Hairs {ID = 19, Name = "Haut et luisant", Price = 50},
            new Hairs {ID = 20, Name = "Long Slicked", Price = 50},
            new Hairs {ID = 21, Name = "Jeune Hipster", Price = 50},
            new Hairs {ID = 22, Name = "Mullet", Price = 50},
            new Hairs {ID = 24, Name = "Tresse classique", Price = 50},
            new Hairs {ID = 25, Name = "Tresse palmé", Price = 50},
            new Hairs {ID = 26, Name = "Tresse éclairée", Price = 50},
            new Hairs {ID = 27, Name = "Tresse spéciale", Price = 50},
            new Hairs {ID = 28, Name = "Tresse en zig zag", Price = 50},
            new Hairs {ID = 29, Name = "Tresse en escargot", Price = 50},
            new Hairs {ID = 30, Name = "80's Will Smith", Price = 50},
            new Hairs {ID = 31, Name = "Coupe playmobil", Price = 50},
            new Hairs {ID = 32, Name = "Coupe balayé", Price = 50},
            new Hairs {ID = 33, Name = "Coupe balayé sur le côté", Price = 50},
            new Hairs {ID = 34, Name = "Punk", Price = 50},
            new Hairs {ID = 35, Name = "Mod", Price = 50},
            new Hairs {ID = 72, Name = "Coupe plateau", Price = 50},
            new Hairs {ID = 73, Name = "Boule à zéro millitaire", Price = 50},
            /*
            new Hairs {ID = 75, Name = "ModHair_18", Price = 50},
            new Hairs {ID = 78, Name = "ModHair_17", Price = 50},
            new Hairs {ID = 79, Name = "ModHair_16", Price = 50},
            new Hairs {ID = 80, Name = "ModHair_15", Price = 50},
            new Hairs {ID = 81, Name = "ModHair_14", Price = 50},
            new Hairs {ID = 82, Name = "ModHair_13", Price = 50},
            new Hairs {ID = 87, Name = "ModHair_12", Price = 50},
            new Hairs {ID = 89, Name = "ModHair_11", Price = 50},
            new Hairs {ID = 95, Name = "ModHair_10", Price = 50},
            new Hairs {ID = 96, Name = "ModHair_09", Price = 50},
            new Hairs {ID = 97, Name = "ModHair_08", Price = 50},
            new Hairs {ID = 98, Name = "ModHair_07", Price = 50},
            new Hairs {ID = 99, Name = "ModHair_06", Price = 50},
            new Hairs {ID = 100, Name = "ModHair_05", Price = 50},
            new Hairs {ID = 101, Name = "ModHair_04", Price = 50},
            new Hairs {ID = 103, Name = "ModHair_03", Price = 50},
            new Hairs {ID = 104, Name = "ModHair_02", Price = 50},
            new Hairs {ID = 105, Name = "ModHair_01", Price = 50}*/
        };

        public static List<Hairs> HairsGirlList = new List<Hairs>
        {
            new Hairs { ID= 0, Name= "Rasage de prés", Price = 50},
            new Hairs { ID= 1, Name= "Court", Price = 50},
            new Hairs { ID= 2, Name= "Layered Mod", Price = 50},
            new Hairs { ID= 3, Name= "Queue de cochon", Price = 50},
            new Hairs { ID= 4, Name= "Queue de cheval", Price = 50},
            new Hairs { ID= 5, Name= "Coupe tresse", Price = 50},
            new Hairs { ID= 6, Name= "Tresses", Price = 50},
            new Hairs { ID= 7, Name= "Bob", Price = 50},
            new Hairs { ID= 8, Name= "Faux Hawk", Price = 50},
            new Hairs { ID= 9, Name= "Twist Francais", Price = 50},
            new Hairs { ID= 10, Name= "Long Bob", Price = 50},
            new Hairs { ID= 11, Name= "En vrac", Price = 50},
            new Hairs { ID= 12, Name= "Pixie", Price = 50},
            new Hairs { ID= 13, Name= "Frange coupée", Price = 50},
            new Hairs { ID= 14, Name= "Noeud Haut", Price = 50},
            new Hairs { ID= 15, Name= "Bob wavy", Price = 50},
            new Hairs { ID= 16, Name= "Bun désordonné", Price = 50},
            new Hairs { ID= 17, Name= "Pin Up Girl", Price = 50},
            new Hairs { ID= 18, Name= "Bun serré", Price = 50},
            new Hairs { ID= 19, Name= "Bob tordu", Price = 50},
            new Hairs { ID= 20, Name= "Bob clapet", Price = 50},
            new Hairs { ID= 21, Name= "Big Bangs", Price = 50},
            new Hairs { ID= 22, Name= "Tresse avec un haut noeud", Price = 50},
            new Hairs { ID= 23, Name= "Mullet", Price = 50},
            new Hairs { ID= 25, Name= "Tresse pincé", Price = 50},
            new Hairs { ID= 26, Name= "Tresse en feuille", Price = 50},
            new Hairs { ID= 27, Name= "Tresse en zig zag", Price = 50},
            new Hairs { ID= 28, Name= "Coupe petite fille", Price = 50},
            new Hairs { ID= 29, Name= "Tresse vagues", Price = 50},
            new Hairs { ID= 30, Name= "Tresse chaines", Price = 50},
            new Hairs { ID= 31, Name= "Coupe vintage", Price = 50},
            new Hairs { ID= 32, Name= "Coupe playmobil", Price = 50},
            new Hairs { ID= 33, Name= "Coupe balayé", Price = 50},
            new Hairs { ID= 34, Name= "Coupe balayé sur le côté", Price = 50},
            new Hairs { ID= 35, Name= "Punk",Price = 50},
            new Hairs { ID= 36, Name= "Bandana et tresse", Price = 50},
            new Hairs { ID= 37, Name= "Layered Mod", Price = 50},
            new Hairs { ID= 38, Name= "Skinbyrd", Price = 50},
            new Hairs { ID= 76, Name= "Bun soigné", Price = 50},
            new Hairs { ID= 77, Name= "Bob court", Price = 50},/*
            new Hairs { ID= 113, Name= "ModHair_01", Price = 50},
            new Hairs { ID= 111, Name= "ModHair_02", Price = 50},
            new Hairs { ID= 106, Name= "ModHair_03", Price = 50},
            new Hairs { ID= 105, Name= "ModHair_04", Price = 50},
            new Hairs { ID= 103, Name= "ModHair_05", Price = 50},
            new Hairs { ID= 102, Name= "ModHair_06", Price = 50},
            new Hairs { ID= 101, Name= "ModHair_07", Price = 50},
            new Hairs { ID= 100, Name= "ModHair_08", Price = 50},
            new Hairs { ID= 99, Name= "ModHair_09", Price = 50},
            new Hairs { ID= 98, Name= "ModHair_010", Price = 50},
            new Hairs { ID= 95, Name= "ModHair_011", Price = 50},
            new Hairs { ID= 94, Name= "ModHair_012", Price = 50},
            new Hairs { ID= 92, Name= "ModHair_013", Price = 50},
            new Hairs { ID= 91, Name= "ModHair_014", Price = 50},
            new Hairs { ID= 90, Name= "ModHair_015", Price = 50},
            new Hairs { ID= 89, Name= "ModHair_016", Price = 50},
            new Hairs { ID= 88, Name= "ModHair_017", Price = 50},
            new Hairs { ID= 87, Name= "ModHair_018", Price = 50},
            new Hairs { ID= 86, Name= "ModHair_019", Price = 50},
            new Hairs { ID= 85, Name= "ModHair_020", Price = 50},
            new Hairs { ID= 84, Name= "ModHair_021", Price = 50},
            new Hairs { ID= 83, Name= "ModHair_022", Price = 50},
            new Hairs { ID= 82, Name= "ModHair_023", Price = 50},
            new Hairs { ID= 81, Name= "ModHair_024", Price = 50},
            new Hairs { ID= 80, Name= "ModHair_025", Price = 50},
            new Hairs { ID= 79, Name= "ModHair_026", Price = 50},
            new Hairs { ID= 96, Name= "ModHair_027", Price = 50}*/
        };
    }
}

