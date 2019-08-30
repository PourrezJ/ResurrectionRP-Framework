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
        public uint Model;

/*        [BsonIgnore, JsonIgnore]
        public ObjectHandler Obj { get; private set; }*/

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

        public static async Task<InventoryBox> CreateInventoryBox(string id, Location location,Inventory.Inventory inventory = null, uint model = 307713837, int taille = 200)
        {
            InventoryBox inv = new InventoryBox()
            {
                ID = id,
                Location = location,
                Model = model,
                Inventory = (inventory == null) ? new Inventory.Inventory(taille, 20) : inventory
            };

            //await inv.Spawn();
            return inv;
        }

/*        public async Task Spawn() TODO OBJECT HANDLER
        {
            Obj = await ObjectHandlerManager.CreateObject(Model, new Vector3(Location.Pos.X, Location.Pos.Y, Location.Pos.Z - 1f), Location.Rot);
            Obj.IObject.SetSharedData("ID", ID);
        }

        public async Task Destruct()
        {
            await Obj?.IObject?.DestroyAsync();
        }*/
    }
}
