using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Utility;
using System.Numerics;
using System.Threading.Tasks;
using Flags = ResurrectionRP.Server.AnimationFlags;

namespace ResurrectionRP_Server.Models.InventoryData.Items
{
    class Alcohol : Item
    {
        public int Timer = 60000;
        public int Drink;

        public Alcohol(ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "alcohol", string icon = "unknown-item", string classes = "basic", int timer = 60000, int drink = 0) : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
            Timer = timer;
            Drink = drink;
        }

        public override async Task Use(IPlayer client, string inventoryType, int slot)
        {
            PlayerHandler ph = PlayerManager.GetPlayerByClient(client);
            if (ph != null)
            {
                if (ph.DeleteItem(slot, inventoryType, 1))
                {
                    if (ph.Thirst + Drink > 100)
                        await ph.UpdateHungerThirst(-1, 100);
                    else
                        await ph.UpdateHungerThirst(-1, ph.Thirst + Drink);

                }

                switch (id)
                {
                    case ItemID.Cafe:
                        await AnimateEatDrink(client, ph, "prop_food_coffee", new Vector3(), new Vector3());
                        break;

                    case ItemID.JambonBeurre:
                        await AnimateEatDrink(client, ph, "prop_sandwich_01", new Vector3(), new Vector3());
                        break;

                    case ItemID.Donuts:
                        await AnimateEatDrink(client, ph, "prop_donut_01", new Vector3(), new Vector3());
                        break;

                    case ItemID.Eau:
                        await AnimateEatDrink(client, ph, "prop_ld_flow_bottle", new Vector3(), new Vector3());
                        break;

                    case ItemID.Vin:
                        await AnimateEatDrink(client, ph, "prop_wine_bot_01", new Vector3(), new Vector3());
                        break;
                }
            }
            await MenuManager.CloseMenu(client);
        }

        public async Task AnimateEatDrink(IPlayer client, PlayerHandler ph, string props, Vector3 position, Vector3 rotation)
        {
            await ph.PlayAnimation("mp_player_intdrink", "loop_bottle", 4, -8, -1, (Flags.OnlyAnimateUpperBody | Flags.AllowPlayerControl));

            Utils.Delay(4000, true, async () =>
            {
                await ph.PlayAnimation("mp_player_intdrink", "outro_bottle", 4, -8, -1, (Flags.OnlyAnimateUpperBody | Flags.AllowPlayerControl));
                /*
                if (obj != null)
                {
                    await ph.PlayAnimation("mp_player_intdrink", "outro_bottle", 4, -8, -1, (Flags.OnlyAnimateUpperBody | Flags.AllowPlayerControl));
                    await obj.Destroy();
                }*/
            });
        }
    }
}
