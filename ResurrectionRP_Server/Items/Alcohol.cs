using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Models;
using System.Numerics;
using Flags = ResurrectionRP_Server.Utils.Enums.AnimationFlags;

namespace ResurrectionRP_Server.Items
{
    class Alcohol : Item
    {
        public int Timer = 60000;
        public int Drink;
        public int Alcolhol;

        public Alcohol(Models.InventoryData.ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "alcohol", string icon = "unknown-item", string classes = "basic", int timer = 60000, int drink = 0, double alcohol = 0.1) : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
            Timer = timer;
            Drink = drink;
        }

        public override void Use(IPlayer client, string inventoryType, int slot)
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

                AnimateEatDrink(client, ph, "prop_wine_bot_01", new Vector3(), new Vector3());
                ph.AddAlcolhol(1);
            }
        }

        public void AnimateEatDrink(IPlayer client, Entities.Players.PlayerHandler ph, string props, Vector3 position, Vector3 rotation)
        {
            client.PlayAnimation("mp_player_intdrink", "loop_bottle", 4, -8, -1, (Flags.OnlyAnimateUpperBody | Flags.AllowPlayerControl));
            Utils.Utils.Delay(4000, async () =>
            {
                if (!await client.ExistsAsync())
                    return;

                await client.PlayAnimationAsync("mp_player_intdrink", "outro_bottle", 4, -8, -1, (Flags.OnlyAnimateUpperBody | Flags.AllowPlayerControl));
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
