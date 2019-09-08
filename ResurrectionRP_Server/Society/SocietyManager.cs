using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using AltV.Net;
namespace ResurrectionRP_Server.Society
{
    public class SocietyManager
    {
        public List<Society> SocietyList = new List<Society>();
        public static Dictionary<IPlayer, Society> AddParkingList = new Dictionary<IPlayer, Society>();
        public static async Task LoadAllSociety()
        {
            Alt.Server.LogInfo("--- Start loading all businesses in database ---");
            var societyList = await Database.MongoDB.GetCollectionSafe<Society>("society").AsQueryable().ToListAsync();
            foreach (var society in societyList)
            {
                await society.Load();
                EventHandlers.Events.OnPlayerEnterColShape += society.OnPlayerEnterColshape;
            }


            Utils.Utils.Delay((int)TimeSpan.FromMinutes(7).TotalMilliseconds, false, async () =>
            {
                foreach (var society in societyList)
                {
                    await society.Update();
                    await Task.Delay(50);
                }
            });
            Alt.Server.LogInfo($"--- Finish loading all businesses in database: {societyList.Count} ---");
        }

    }
}
