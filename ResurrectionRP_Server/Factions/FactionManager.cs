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

        public static ONU Onu { get; private set; }

        public static LSPD Lspd { get; private set; }

        public static LSCustom LSCustom { get; private set; }

        public static Division Rebelle { get; private set; }

        public static Gouv Gouvernement { get; private set; }

        public static Dock Dock { get; private set; }

        public static Nordiste Nordiste { get; private set; }
        #endregion

        #region Init
        public static async Task InitAllFactions()
        {
            Onu = (ONU)(await LoadFaction<ONU>("ONU") ?? new ONU("ONU", FactionType.ONU)).Init();
            Lspd = (LSPD)(await LoadFaction<LSPD>("LSPD") ?? new LSPD("LSPD", FactionType.LSPD)).Init();
            Rebelle = (Division)(await LoadFaction<Division>("Division") ?? new Division("Division", FactionType.Division)).Init();
            LSCustom = (LSCustom)(await LoadFaction<LSCustom>("LSCustom") ?? new LSCustom("LSCustom", FactionType.LSCustom)).Init();
            Gouvernement = (Gouv)(await LoadFaction<Gouv>("Gouv") ?? new Gouv("Gouv", FactionType.Gouv)).Init();
            Dock = (Dock)(await LoadFaction<Dock>("Dock") ?? new Dock("Dock", FactionType.Dock)).Init();
            Nordiste = (Nordiste)(await LoadFaction<Nordiste>("Bureau du Shérif") ?? new Nordiste("Bureau du Shérif", FactionType.Nordiste)).Init();

            Utils.Utils.Delay((int)TimeSpan.FromMinutes(10).TotalMilliseconds, async () =>
            {
                foreach (var faction in FactionList)
                {       
                    faction.UpdateInBackground();
                    await Task.Delay(50);
                }
            });

            FactionList.AddRange(new List<Faction>() { Onu, Lspd, LSCustom, Gouvernement, Dock, Nordiste });
        }
        #endregion

        #region Event handlers
        public static void OnPlayerConnected(IPlayer client)
        {
            Onu?.OnPlayerConnected(client);
            Lspd?.OnPlayerConnected(client);
            LSCustom?.OnPlayerConnected(client);
            Rebelle?.OnPlayerConnected(client);
            Dock?.OnPlayerConnected(client);
            Gouvernement?.OnPlayerConnected(client);
            Nordiste?.OnPlayerConnected(client);
        }

        public static void OnPlayerDisconnected(IPlayer client)
        {
            Onu?.OnPlayerDisconnected(client);
            Lspd?.OnPlayerDisconnected(client);
            LSCustom?.OnPlayerDisconnected(client);
            Rebelle?.OnPlayerDisconnected(client);
            Dock?.OnPlayerDisconnected(client);
            Nordiste?.OnPlayerConnected(client);
        }
        #endregion

        #region Methods
        public static void AddFactionTargetMenu(IPlayer client, IPlayer target, XMenu xMenu)
        {
            Onu?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.BRIEFCASE_MEDICAL_SOLID);
            Lspd?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
            LSCustom?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.TOOLBOX_SOLID);
            Rebelle?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.REBEL_BRAND);
            Gouvernement?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
            Dock?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
            Nordiste?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
        }

        public static void AddFactionVehicleMenu(IPlayer client, IVehicle vehicle, XMenu xMenu)
        {
            Onu?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.BRIEFCASE_MEDICAL_SOLID);
            Lspd?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
            LSCustom?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.TOOLBOX_SOLID);
            Rebelle?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.REBEL_BRAND);
            Gouvernement?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
            Dock?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
            Nordiste?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
        }

        public static async Task<T> LoadFaction<T>(string faction)
        {
            try
            {
                var filter = Builders<T>.Filter.Eq("FactionName", faction);
                return await Database.MongoDB.GetCollectionSafe<T>("factions").FindAsync<T>(filter).Result.FirstOrDefaultAsync();
            }
            catch (Exception ec)
            {
                Alt.Server.LogError(ec.ToString());
                throw;
            }
        }

        public static bool IsMedic(IPlayer client)
        {  
            if (Onu != null)
                return Onu.HasPlayerIntoFaction(client);
            return false;
        }

        public static bool IsLspd(IPlayer client)
        {
            if (Lspd != null)
                return Lspd.HasPlayerIntoFaction(client);
            return false;
        }
        
        public static bool IsRebelle(IPlayer client)
        {
            if (Rebelle != null)
                return Rebelle.HasPlayerIntoFaction(client);
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
            if (Nordiste != null)
                return Nordiste.HasPlayerIntoFaction(client);
            return false;
        }

        public static async Task Update()
        {
            for (int i = 0; i < FactionList.Count; i++)
            {
                await FactionList[i].PayCheck();
                FactionList[i].UpdateInBackground();
            }
        }
        #endregion
    }
}
