using ResurrectionRP_Server.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResurrectionRP_Server.AutoBusiness
{
    public class Bijouterie : AutoBusiness
    {

        public Bijouterie()
        {
            name = "Bijoutie Khartier";
            desc = " Bijouterie juive";
            
            pedPosition = new System.Numerics.Vector3(-1463.18f, -179.364f ,48.817f);
            pedHeading = 0;
            pedModel = (AltV.Net.Enums.PedModel)0x6BD9B68C;

            blipSprite = 617;
            blipColor = Entities.Blips.BlipColor.LightPurple;


            sellItems.TryAdd(Inventory.Inventory.ItemByID(Models.InventoryData.ItemID.PepiteOr), 10000 + new Random().Next(-15, 15) * 100);
            sellItems.TryAdd(Inventory.Inventory.ItemByID(Models.InventoryData.ItemID.LingotOr), 20000 + new Random().Next(-15, 15) * 200);

            // PEPITE ( 10k +- 15% ) | LINGOT 20k +- 15% |

            buyItems.TryAdd(Inventory.Inventory.ItemByID(Models.InventoryData.ItemID.LingotOr), 20000 + new Random().Next(-15, 15) * 200);
            // LINGO 20k +- 15%
            tradeItems.TryAdd(
                    new ItemStack( Inventory.Inventory.ItemByID(Models.InventoryData.ItemID.SacArgent), 20 ),
                    new ItemStack( Inventory.Inventory.ItemByID(Models.InventoryData.ItemID.LingotOr), 1)
                );
            // 20 sac pour 1 lingot

            

        }

    }
}
