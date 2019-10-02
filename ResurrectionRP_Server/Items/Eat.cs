using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Models;
using System.Threading.Tasks;
using System.Numerics;

using Flags = ResurrectionRP_Server.Utils.Enums.AnimationFlags;

namespace ResurrectionRP_Server.Items
{
    public class Eat : Item
    {
        public int Food;
        public int Drink;

        public Eat(Models.InventoryData.ItemID id, string name, string description, double weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, double itemPrice = 0, string type = "eat", string icon = "unknown-item", string classes = "food", int drink = 0, int food = 0) : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
            Food = food;
            Drink = drink;
        }

        public override void Use(IPlayer client, string inventoryType, int slot)
        {
            Entities.Players.PlayerHandler ph = client.GetPlayerHandler();

            if (ph != null)
            {
                if (ph.DeleteItem(slot, inventoryType, 1))
                {
                    if (ph.Hunger + Food > 100)
                        ph.UpdateHungerThirst(100);
                    else
                        ph.UpdateHungerThirst(ph.Hunger + Food);

                    if (ph.Thirst + Drink > 100)
                         ph.UpdateHungerThirst(-1, 100);
                    else
                        ph.UpdateHungerThirst(-1, ph.Thirst + Drink);
                }

                switch (id)
                {
                    case Models.InventoryData.ItemID.Cafe:
                        AnimateEatDrink(client, ph, "prop_food_coffee", new Vector3(), new Vector3());
                        break;

                    case Models.InventoryData.ItemID.JambonBeurre:
                        AnimateEatDrink(client, ph, "prop_sandwich_01", new Vector3(), new Vector3());
                        break;

                    case Models.InventoryData.ItemID.Donuts:
                        AnimateEatDrink(client, ph, "prop_donut_01", new Vector3(), new Vector3());
                        break;

                    case Models.InventoryData.ItemID.Eau:
                        AnimateEatDrink(client, ph, "prop_ld_flow_bottle", new Vector3(), new Vector3());
                        break;

                    case Models.InventoryData.ItemID.Vin:
                        AnimateEatDrink(client, ph, "prop_wine_bot_01", new Vector3(), new Vector3());
                        break;
                }
            }
        }

        public void AnimateEatDrink(IPlayer client, Entities.Players.PlayerHandler ph, string props, Vector3 position, Vector3 rotation)
        {
            if (Food > 0)
            {
                client.PlayAnimation("mp_player_inteat@burger", "mp_player_int_eat_burger", 4, -8, -1);

                Utils.Utils.Delay(4000, async () =>
                {
                    if (!client.Exists)
                        return;

                    await client.PlayAnimationAsync("mp_player_inteat@burger", "mp_player_int_eat_exit_burger", 4, -8, -1);
                });
            }
            else
            {
                client.PlayAnimation("mp_player_intdrink", "loop_bottle", 4, -8, -1);

                Utils.Utils.Delay(4000, async () =>
                {
                    if (!client.Exists)
                        return;

                    await client.PlayAnimationAsync("mp_player_intdrink", "outro_bottle", 4, -8, -1);
                });
            }
        }
    }
}
