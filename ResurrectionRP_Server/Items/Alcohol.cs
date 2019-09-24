using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Models;
using System.Numerics;
using System.Threading.Tasks;
using Flags = ResurrectionRP_Server.Utils.Enums.AnimationFlags;

namespace ResurrectionRP_Server.Items
{
    class Alcohol : Item
    {
        public int Timer = 60000;
        public int Drink;

        public Alcohol(Models.InventoryData.ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "alcohol", string icon = "unknown-item", string classes = "basic", int timer = 60000, int drink = 0) : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
            Timer = timer;
            Drink = drink;
        }

        public override async Task Use(IPlayer client, string inventoryType, int slot)
        {
            Entities.Players.PlayerHandler ph = client.GetPlayerHandler();
            if (ph != null)
            {
                if (ph.DeleteItem(slot, inventoryType, 1))
                {
                    if (ph.Thirst + Drink > 100)
                        ph.UpdateHungerThirst(-1, 100);
                    else
                        ph.UpdateHungerThirst(-1, ph.Thirst + Drink);

                }

                switch (id)
                {
                    case Models.InventoryData.ItemID.Cafe:
                        await AnimateEatDrink(client, ph, "prop_food_coffee", new Vector3(), new Vector3());
                        break;

                    case Models.InventoryData.ItemID.JambonBeurre:
                        await AnimateEatDrink(client, ph, "prop_sandwich_01", new Vector3(), new Vector3());
                        break;

                    case Models.InventoryData.ItemID.Donuts:
                        await AnimateEatDrink(client, ph, "prop_donut_01", new Vector3(), new Vector3());
                        break;

                    case Models.InventoryData.ItemID.Eau:
                        await AnimateEatDrink(client, ph, "prop_ld_flow_bottle", new Vector3(), new Vector3());
                        break;

                    case Models.InventoryData.ItemID.Vin:
                        await AnimateEatDrink(client, ph, "prop_wine_bot_01", new Vector3(), new Vector3());
                        break;
                }
            }
        }

        public Task AnimateEatDrink(IPlayer client, Entities.Players.PlayerHandler ph, string props, Vector3 position, Vector3 rotation)
        {
            client.PlayAnimation("mp_player_intdrink", "loop_bottle", 4, -8, -1, (Flags.OnlyAnimateUpperBody | Flags.AllowPlayerControl));

            Utils.Utils.Delay(4000, true, () =>
            {
                if (!client.Exists)
                    return;

                client.PlayAnimation("mp_player_intdrink", "outro_bottle", 4, -8, -1, (Flags.OnlyAnimateUpperBody | Flags.AllowPlayerControl));
                /*
                if (obj != null)
                {
                    await ph.PlayAnimation("mp_player_intdrink", "outro_bottle", 4, -8, -1, (Flags.OnlyAnimateUpperBody | Flags.AllowPlayerControl));
                    await obj.Destroy();
                }*/  
            });

            return Task.CompletedTask;
        }
    }
}
