using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net.Async;
using AltV.Net;

namespace ResurrectionRP_Server.Models
{
    public class InventoryBox
    {
        public string ID;
        public Inventory.Inventory Inventory;
        public string Model;

        [BsonIgnore, JsonIgnore]
        public Entities.Objects.Object Obj { get; private set; }

        private Location _location;
        public Location Location
        {
            get => _location;
            set
            {
                _location = value;
/*                AltAsync.Do(() =>
                {
                    if (Obj != null && Obj.IObject.Exists)
                    {
                        Obj.IObject.SetPosition(value.Pos);
                        Obj.IObject.SetRotation(value.Rot);
                    }
                });*/
            }
        }

        public async static Task<InventoryBox> CreateInventoryBox(string id, Location location,Inventory.Inventory inventory = null, string model = "prop_box_wood07a", int taille = 200)
        {
            InventoryBox inv = new InventoryBox()
            {
                ID = id,
                Location = location,
                Model = model,
                Inventory = (inventory == null) ? new Inventory.Inventory(taille, 20) : inventory
            };

            await inv.Spawn();
            return inv;
        }

        public async Task Spawn() 
        {
            Obj = Entities.Objects.ObjectManager.CreateObject(Model, new Vector3(Location.Pos.X, Location.Pos.Y, Location.Pos.Z - 1f), Location.Rot);
            
        }

        public void Destruct() =>
            Obj?.Destroy();
    }
}
