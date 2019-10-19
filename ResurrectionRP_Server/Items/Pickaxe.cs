using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using ResurrectionRP_Server.Models;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Utils.Enums;
using System.Numerics;

namespace ResurrectionRP_Server.Items
{
    class Pickaxe : Item
    {
        public float Health = 1000;
        public float Speed = 1.0f;
        public int MiningRate = 1;
        public Pickaxe(Models.InventoryData.ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = false, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "pickaxe", string icon = "pickaxe", string classes = "weapon", int miningrate = 1, float speed = 1.0f, int health = 1000) : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
            Health = health;
            Speed = speed;
            MiningRate = miningrate;
        }

        public override Task Give(IPlayer sender, IPlayer recever, int quantite)
        {
            return base.Give(sender, recever, quantite);
        }

        public void MarteauPiqueurSetWalkingStyle(IPlayer client, Entities.Objects.WorldObject prop)
        {
            client.PlayAnimation("weapons@heavy@rpg@f", "idle", 8, -1, 6000000, (AnimationFlags)49);
            prop.SetAttachToEntity(client, "PH_R_Hand", new Vector3(0.1f, 0.2f, 0.02f), new Vector3(-0, 80, 170));
        }
    }
}
