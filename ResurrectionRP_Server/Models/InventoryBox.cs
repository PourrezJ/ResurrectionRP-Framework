using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Numerics;
using AltV.Net;
using ResurrectionRP_Server.Entities.Objects;

namespace ResurrectionRP_Server.Models
{
    public class InventoryBox
    {
        public string ID;
        public Inventory.Inventory Inventory;
        public int Model;

        [BsonIgnore, JsonIgnore]
        public Entities.Objects.WorldObject Obj { get; private set; }

        private Location _location;
        public Location Location
        {
            get => _location;
            set
            {
                _location = value;
                if (Obj != null)
                {
                    Obj.Position = value.Pos;
                    Obj.Rotation = value.Rot;
                }
            }
        }

        public static InventoryBox CreateInventoryBox(string id, Location location,Inventory.Inventory inventory = null, int model = 0, int taille = 200)
        {
            if (model == 0)
                model = (int)Alt.Hash("prop_box_wood07a");
            InventoryBox inv = new InventoryBox()
            {
                ID = id,
                Location = location,
                Model = model,
                Inventory = (inventory == null) ? new Inventory.Inventory(taille, 20) : inventory
            };

            inv.Spawn();
            return inv;
        }

        public void Spawn() =>
            Obj = WorldObject.CreateObject(Model, new Vector3(Location.Pos.X, Location.Pos.Y, Location.Pos.Z ), Location.Rot, true);

        public void Destruct() =>
            Obj?.Destroy();
    }
}
