using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using AltV.Net;
using System.Linq;

namespace ResurrectionRP_Server.Society
{
    public static class SocietyManager
    {
        #region Public fields
        public static List<Society> SocietyList = new List<Society>();
        public static Dictionary<IPlayer, Society> AddParkingList = new Dictionary<IPlayer, Society>();
        #endregion

        #region Init
        public static void LoadAllSociety()
        {
            Alt.Server.LogInfo("--- Start loading all society in database ---");
            var societyList = Database.MongoDB.GetCollectionSafe<Society>("society").AsQueryable();

            Utils.Utils.Delay((int)TimeSpan.FromMinutes(7).TotalMilliseconds, (Action)(async() =>
            {
                foreach (var society in societyList)
                {
                    society.UpdateInBackground();
                    await Task.Delay(50);
                }
            }));

            foreach (var society in societyList)
                society.Init();

            Alt.Server.LogInfo($"--- Finish loading all society in database: {societyList.Count()} ---");
        }
        #endregion
    }
}
