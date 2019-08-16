using System.Collections.Generic;
using System.Collections.ObjectModel;
using AltV.Net.Enums;
using Newtonsoft.Json;

namespace VehicleInfoLoader.Data
{
    public sealed partial class VehicleManifest
    {
        [JsonProperty("hash")]
        public VehicleModel Hash              { get; internal set; }
        
        [JsonProperty("name")]
        public string Name                   { get; internal set; }
        
        [JsonProperty("displayName")]
        public string DisplayName            { get; internal set; }
        
        [JsonProperty("localizedName")]
        public string LocalizedName          { get; internal set; }
        
        [JsonProperty("manufacturerName")]
        public string ManufacturerName       { get; internal set; }
        
        [JsonProperty("localizedManufacturer")]
        public string LocalizedManufacturer  { get; internal set; }
        
        [JsonProperty("vehicleClass")]
        public int VehicleClass              { get; internal set; }
        
        [JsonProperty("vehicleClassName")]
        public string VehicleClassName       { get; internal set; }
        
        [JsonProperty("localizedVehicleClass")]
        public string LocalizedVehicleClass  { get; internal set; }
        
        [JsonProperty("wheelType")]
        public int WheelType                 { get; internal set; }
        
        [JsonProperty("wheelTypeName")]
        public string WheelTypeName          { get; internal set; }
        
        [JsonProperty("localizedWheelType")]
        public string LocalizedWheelType     { get; internal set; }

        [JsonProperty("convertible")]
        public bool Convertible              { get; internal set; }
        
        [JsonProperty("electric")]
        public bool Electric                 { get; internal set; }
        
        [JsonProperty("trailer")]
        public bool Trailer                  { get; internal set; }
        
        [JsonProperty("neon")]
        public bool Neon                     { get; internal set; }
        
        [JsonProperty("dimensions")]
        public VehicleDimensions Dimensions  { get; internal set; }

        [JsonProperty("bones")]
        public ReadOnlyDictionary<string, int> Bones { get; internal set; }
        
        [JsonProperty("maxSpeed")]
        public float MaxSpeed                { get; internal set; }
        
        [JsonProperty("maxBraking")]
        public float MaxBraking              { get; internal set; }
        
        [JsonProperty("maxTraction")]
        public float MaxTraction             { get; internal set; }
        
        [JsonProperty("maxAcceleration")]
        public float MaxAcceleration         { get; internal set; }
        
        [JsonProperty]
        public float _0xBFBA3BA79CFF7EBF     { get; internal set; }
        
        [JsonProperty]
        public float _0x53409B5163D5B846     { get; internal set; }
        
        [JsonProperty]
        public float _0xC6AD107DDC9054CC     { get; internal set; }
        
        [JsonProperty]
        public float _0x5AA3F878A178C4FC     { get; internal set; }
        
        [JsonProperty("maxNumberOfPassengers")]
        public int MaxNumberOfPassengers     { get; internal set; }
        
        [JsonProperty("maxOccupants")]
        public int MaxOccupants              { get; internal set; }
        
        [JsonProperty("rewards")]
        public string[] Rewards              { get; internal set; }
        
        [JsonProperty("mods")]
        internal Dictionary<int, VehicleModType> ModList;
        
        [JsonProperty("liveries")]
        internal LiveryCollection LiveryList;
        
    }
}
