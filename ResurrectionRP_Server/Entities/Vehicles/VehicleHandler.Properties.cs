using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using Newtonsoft.Json;
using ResurrectionRP.Server.Entities.Vehicles.Data;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Vehicles.Data;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using VehicleInfoLoader.Data;

namespace ResurrectionRP_Server.Entities.Vehicles
{
    public class VehicleProperties
    {
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public ConcurrentDictionary<int, int> Mods { get; set; }
    = new ConcurrentDictionary<int, int>();

        public uint BodyHealth { get; set; } = 1000;
        public int EngineHealth { get; set; } = 1000;
        public int PetrolTankHealth { get; set; }


        //public List<bool> NeonState { get; set; } = new List<bool>();
        //public List<byte> NeonColor { get; set;  } = new List<byte>();
        public Tuple<bool, bool, bool, bool> NeonState { get; set; } = new Tuple<bool, bool, bool, bool>(false, false, false, false);
        public Tuple<byte, byte, byte, byte> NeonColor { get; set; } = new Tuple<byte, byte, byte, byte>(0,0,0,0);

        public byte Dirt { get; set; } = 0;
        public bool Engine { get; set; } = false;
        

        public byte PrimaryColor { get; set; } = 0;
        public byte SecondaryColor { get; set; } = 0;

        public byte WindowTint { get; set; } = 0;
        public bool ArmoredWindows { get; set; } = false;

        public byte SirenActive { get; set; } = 0;

        public byte FrontBumperDamage { get; set; } = 0;
        public byte RearBumperDamage { get; set; } = 0;
        


        public VehicleProperties() {

            
        }

    
    }



}

