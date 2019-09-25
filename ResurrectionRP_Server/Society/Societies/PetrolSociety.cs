using System;
using System.Collections.Generic;
using System.Text;
using AltV.Net.Elements.Entities;
using System.Threading.Tasks;
using System.Numerics;
namespace ResurrectionRP_Server.Society.Societies
{
    class PetrolSociety : Society
    {
        public PetrolSociety(string societyName, Vector3 servicePos, uint blipSprite, int blipColor, string owner = null, Inventory.Inventory inventory = null, Models.Parking parking = null) : base(societyName, servicePos, blipSprite, blipColor, owner, inventory, parking)
        {
        }

        public override async Task Init()
        {
            await base.Init();

            //await Blip?.SetScaleAsync(1.2f);
        }

        public override Task OnPlayerEnterServiceColshape(IColShape colShape, IPlayer client)
        {
            return base.OnPlayerEnterServiceColshape(colShape, client);
        }
    }
}
