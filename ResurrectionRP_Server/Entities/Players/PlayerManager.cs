using System;
using System.Collections.Generic;
using System.Text;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using System.Threading.Tasks;
using MongoDB.Driver;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Async.Events;
using AltV.Net.Elements.Entities;

namespace ResurrectionRP_Server.Entities.Players
{
    class PlayerManager
    {
        #region Variables 
        private static uint Dimension = 2;
        public static int StartMoney = 0;
        public static int StartBankMoney = 0;

        private bool _whitelistOn = false;
        #endregion

        #region Constructor
        public PlayerManager()
        {
            AltAsync.OnPlayerDead += Events_PlayerDeath;

        }
        #endregion
        
        #region ServerEvents

        private async Task Events_PlayerDeath(IPlayer player, IEntity killer, uint weapon)
        {
            if (!player.Exists)
                return;
            
            //if (GetPlayerByClient(player) != null)
                //await GetPlayerByClient(player)?.SetDead(true); // For fix client.Dead is doesn't work actually 
        }
        #endregion

        #region Methods FindAsync(p => p.PID.ToLower() == socialClub.ToLower()).FirstOrDefaultAsync();
        public static async Task<PlayerHandler> GetPlayerHandlerDatabase(string socialClub) =>
            await Database.MongoDB.GetCollectionSafe<PlayerHandler>("players").Find(p => p.PID.ToLower() == socialClub.ToLower()).FirstOrDefaultAsync();


        public static PlayerHandler GetPlayerByClient(IPlayer player)
        {
            if (!player.Exists)
                return null;

            if (player.GetData("PlayerHandler", out object data))
            {
                return data as PlayerHandler;
            }
            else if (PlayerHandler.PlayerHandlerList.TryGetValue(player, out PlayerHandler value))
            {
                return value;
            }
            return null;
        }
        #endregion

    }
}
