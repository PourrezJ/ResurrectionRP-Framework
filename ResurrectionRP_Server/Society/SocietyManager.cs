﻿using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using AltV.Net;
namespace ResurrectionRP_Server.Society
{
    public class SocietyManager
    {
        #region Public fields
        public List<Society> SocietyList = new List<Society>();
        public static Dictionary<IPlayer, Society> AddParkingList = new Dictionary<IPlayer, Society>();
        #endregion

        #region Init
        public static async Task LoadAllSociety()
        {
            Alt.Server.LogInfo("--- Start loading all society in database ---");
            var societyList = await Database.MongoDB.GetCollectionSafe<Society>("society").AsQueryable().ToListAsync();

            Utils.Utils.SetInterval(async () =>
            {
                foreach (var society in societyList)
                {
                    await society.Update();
                    await Task.Delay(50);
                }
            }, (int)TimeSpan.FromMinutes(7).TotalMilliseconds);

            await AltV.Net.Async.AltAsync.Do(() =>
            {
                foreach (var society in societyList)
                    society.Init();
            });

            Alt.Server.LogInfo($"--- Finish loading all society in database: {societyList.Count} ---");
        }
        #endregion
    }
}
