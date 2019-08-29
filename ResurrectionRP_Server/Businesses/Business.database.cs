﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Businesses
{
    public partial class Business
    {

        public async Task Insert()
        {
            await Database.MongoDB.Insert("businesses", this);
        }

        public async Task Delete()
        {
            GameMode.Instance.BusinessesManager.BusinessesList.Remove(this);
            Entities.Blips.BlipsManager.Destroy(Blip);
            Inventory = null;
            Owner = null;
            Employees = null;
        }

        public async Task Update()
        {
            await Database.MongoDB.Update(this, "businesses", _id);
        }
    }
}
