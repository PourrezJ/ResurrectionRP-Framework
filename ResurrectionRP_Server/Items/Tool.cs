using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using ResurrectionRP_Server.Models;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Utils.Enums;
using System.Numerics;

namespace ResurrectionRP_Server.Items
{
    class Tool : Item
    {
        public float Health = 1000;
        public float Speed = 1.0f;
        public int MiningRate = 1;
        public Tool(Models.InventoryData.ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = false, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "tool", string icon = "pickaxe", string classes = "weapon", int miningrate = 1, float speed = 1.0f, int health = 1000) : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
            Health = health;
            Speed = speed;
            MiningRate = miningrate;
        }

        public override Task Give(IPlayer sender, IPlayer recever, int quantite)
        {
            return base.Give(sender, recever, quantite);
        }

        public void JackHammerSetWalkingStyle(IPlayer client, Entities.Objects.WorldObject prop)
        {
            client.PlayAnimation("weapons@heavy@rpg@f", "idle", 8, -1, 6000000, (AnimationFlags)49);
            prop.SetAttachToEntity(client, "PH_R_Hand", new Vector3(0.1f, 0.2f, 0.02f), new Vector3(-0, 80, 170));
        }

        public void SetPickaxeAnimation(IPlayer client, int durationms)
        {
            client.PlayAnimation("melee@large_wpn@streamed_core", "ground_attack_on_spot", 8, -1, durationms, (AnimationFlags)49);
        }

        public void SetJackHammerAnimation(IPlayer client, int durationms)
        {

            client.GetPlayerHandler()?.OutfitInventory.prop.SetAttachToEntity(client, "PH_R_Hand", new Vector3(0.1f, -0.1f, -0.02f), new Vector3(0, 0, 170));
            client.PlayAnimation("AMB@WORLD_HUMAN_CONST_DRILL@MALE@DRILL@BASE", "base", 8, -1, durationms, (AnimationFlags)49);
        }
    }
}
