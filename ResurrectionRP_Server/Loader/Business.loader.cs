using AltV.Net;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Business = ResurrectionRP_Server.Business.Business;
using MongoDB.Driver;

namespace ResurrectionRP_Server.Loader
{
    public class BusinessesLoader
    {
        public List<Business.Business> BusinessesList = new List<Business.Business>();
        public static async Task LoadAllBusinesses()
        {
            Alt.Server.LogColored("--- Start loading all businesses in database ---");

            var _businessesList = await Database.MongoDB.GetCollectionSafe<Business>("businesses").AsQueryable().ToListAsync();
            foreach (var _businesses in _businessesList)
            {
                await _businesses.Init();
            }


            Utils.Utils.Delay((int)TimeSpan.FromMinutes(5).TotalMilliseconds, false, async () =>
            {
                foreach (var _businesses in _businessesList)
                {
                    await _businesses.Update();
                    await Task.Delay(50);
                }
            });

            Alt.Server.LogColored($"--- Finish loading all businesses in database: {_businessesList.Count} ---");
        }

    }
}
