using AltV.Net;
using AltV.Net.Elements.Entities;
using MongoDB.Driver;
using ResurrectionRP_Server.XMenuManager;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Factions
{
    public static class FactionManager
    {
        #region Fields and properties
        public static List<Faction> FactionList = new List<Faction>();

        public static EMS EMS { get; private set; }

        public static LSPD Lspd { get; private set; }

        public static LSCustom LSCustom { get; private set; }

        public static Gouv Gouvernement { get; private set; }

        public static Dock Dock { get; private set; }

        public static Sheriff Sheriff { get; private set; }
        #endregion

        #region Init
        public static void InitAllFactions()
        { 
            EMS = (EMS)(LoadFaction<EMS>("EMS") ?? new EMS("EMS", FactionType.EMS)).Init();
            Lspd = (LSPD)(LoadFaction<LSPD>("LSPD") ?? new LSPD("LSPD", FactionType.LSPD)).Init();
            LSCustom = (LSCustom)(LoadFaction<LSCustom>("LSCustom") ?? new LSCustom("LSCustom", FactionType.LSCustom)).Init();
            Gouvernement = (Gouv)(LoadFaction<Gouv>("Gouv") ?? new Gouv("Gouv", FactionType.Gouv)).Init();
            Dock = (Dock)(LoadFaction<Dock>("Dock") ?? new Dock("Dock", FactionType.Dock)).Init();
            Sheriff = (Sheriff)(LoadFaction<Sheriff>("Bureau du Shérif") ?? new Sheriff("Bureau du Shérif", FactionType.Nordiste)).Init();
            
            Utils.Util.Delay((int)TimeSpan.FromMinutes(10).TotalMilliseconds, async () =>
            {
                foreach (var faction in FactionList)
                {       
                    faction.UpdateInBackground();
                    await Task.Delay(50);
                }
            });

            FactionList.AddRange(new List<Faction>() { EMS, Lspd, LSCustom, Gouvernement, Dock, Sheriff });
        }
        #endregion

        #region Event handlers
        public static void OnPlayerConnected(IPlayer client)
        {
            EMS?.OnPlayerConnected(client);
            Lspd?.OnPlayerConnected(client);
            LSCustom?.OnPlayerConnected(client);
            Dock?.OnPlayerConnected(client);
            Gouvernement?.OnPlayerConnected(client);
            Sheriff?.OnPlayerConnected(client);
        }

        public static void OnPlayerDisconnected(IPlayer client)
        {
            EMS?.OnPlayerDisconnected(client);
            Lspd?.OnPlayerDisconnected(client);
            LSCustom?.OnPlayerDisconnected(client);
            Dock?.OnPlayerDisconnected(client);
            Sheriff?.OnPlayerConnected(client);
        }
        #endregion

        #region Methods
        public static void AddFactionTargetMenu(IPlayer client, IPlayer target, XMenu xMenu)
        {
            EMS?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.BRIEFCASE_MEDICAL_SOLID);
            Lspd?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
            LSCustom?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.TOOLBOX_SOLID);
            Gouvernement?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
            Dock?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
            Sheriff?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
        }

        public static void AddFactionVehicleMenu(IPlayer client, IVehicle vehicle, XMenu xMenu)
        {
            EMS?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.BRIEFCASE_MEDICAL_SOLID);
            Lspd?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
            LSCustom?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.TOOLBOX_SOLID);
            Gouvernement?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
            Dock?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
            Sheriff?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
        }

        public static T LoadFaction<T>(string faction)
        {
            try
            {
                var filter = Builders<T>.Filter.Eq("FactionName", faction);
                return Database.MongoDB.GetCollectionSafe<T>("factions").Find<T>(filter).FirstOrDefault<T>();
            }
            catch (Exception ec)
            {
                Alt.Server.LogError(ec.ToString());
                throw;
            }
        }

        public static bool IsMedic(IPlayer client)
        {  
            if (EMS != null)
                return EMS.HasPlayerIntoFaction(client);
            return false;
        }

        public static bool IsLspd(IPlayer client)
        {
            if (Lspd != null)
                return Lspd.HasPlayerIntoFaction(client);
            return false;
        }
        
        public static bool IsLSCustom(IPlayer client)
        {
            if (LSCustom != null)
                return LSCustom.HasPlayerIntoFaction(client);
            return false;
        }
        
        public static bool IsDock(IPlayer client)
        {
            if (Dock != null)
                return Dock.HasPlayerIntoFaction(client);
            return false;
        }

        public static bool IsGouv(IPlayer client)
        {
            if (Gouvernement != null)
                return Gouvernement.HasPlayerIntoFaction(client);
            return false;
        }
        
        public static bool IsNordiste(IPlayer client)
        {
            if (Sheriff != null)
                return Sheriff.HasPlayerIntoFaction(client);
            return false;
        }

        public static async Task Update()
        {
            for (int i = 0; i < FactionList.Count; i++)
            {
                await FactionList[i].PayCheck();
                await Task.Delay(50);
                FactionList[i].UpdateInBackground();
            }
        }
        #endregion
    }
}
