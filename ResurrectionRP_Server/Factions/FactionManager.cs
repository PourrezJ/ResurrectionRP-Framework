using AltV.Net;
using AltV.Net.Elements.Entities;
using MongoDB.Driver;
using ResurrectionRP_Server.XMenuManager;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Factions
{
    public class FactionManager
    {
        #region Fields and properties
        public List<Faction> FactionList = new List<Faction>();

        public ONU Onu { get; private set; }

        public LSPD Lspd { get; private set; }

        public LSCustom LSCustom { get; private set; }

        //public Division Rebelle { get; private set; }

        public Gouv Gouvernement { get; private set; }

        public Dock Dock { get; private set; }

        public Nordiste Nordiste { get; private set; }
        #endregion

        #region Init
        public async Task InitAllFactions()
        {
            FactionManager fm = GameMode.Instance.FactionManager;
            fm.Onu = (ONU)(await LoadFaction<ONU>("ONU") ?? new ONU("ONU", FactionType.ONU)).Init();
            fm.Lspd = (LSPD)(await LoadFaction<LSPD>("LSPD") ?? new LSPD("LSPD", FactionType.LSPD)).Init();
            // fm.Rebelle = (Division)(await LoadFaction<Division>("Division") ?? new Division("Division", FactionType.Division)).Init();
            fm.LSCustom = (LSCustom)(await LoadFaction<LSCustom>("LSCustom") ?? new LSCustom("LSCustom", FactionType.LSCustom)).Init();
            fm.Gouvernement = (Gouv)(await LoadFaction<Gouv>("Gouv") ?? new Gouv("Gouv", FactionType.Gouv)).Init();
            fm.Dock = (Dock)(await LoadFaction<Dock>("Dock") ?? new Dock("Dock", FactionType.Dock)).Init();
            fm.Nordiste = (Nordiste)(await LoadFaction<Nordiste>("Bureau du Shérif") ?? new Nordiste("Bureau du Shérif", FactionType.Nordiste)).Init();

            Utils.Utils.Delay((int)TimeSpan.FromMinutes(10).TotalMilliseconds, false, async () =>
            {
                foreach (var faction in FactionList)
                {       
                    await faction.UpdateDatabase();
                    await Task.Delay(50);
                }
            });

            FactionList.AddRange(new List<Faction>() { Onu, Lspd, LSCustom, Gouvernement, Dock, Nordiste });
        }
        #endregion

        #region Event handlers
        public void OnPlayerConnected(IPlayer client)
        {
            Onu?.OnPlayerConnected(client);
            Lspd?.OnPlayerConnected(client);
            LSCustom?.OnPlayerConnected(client);
            //Rebelle?.OnPlayerConnected(client);
            Dock?.OnPlayerConnected(client);
            Gouvernement?.OnPlayerConnected(client);
            Nordiste?.OnPlayerConnected(client);
        }

        public void OnPlayerDisconnected(IPlayer client)
        {
            Onu?.OnPlayerDisconnected(client);
            Lspd?.OnPlayerDisconnected(client);
            LSCustom?.OnPlayerDisconnected(client);
            //Rebelle?.OnPlayerDisconnected(client);
            Dock?.OnPlayerDisconnected(client);
            Nordiste?.OnPlayerConnected(client);
        }
        #endregion

        #region Methods
        public static void AddFactionTargetMenu(IPlayer client, IPlayer target, XMenu xMenu)
        {
            GameMode.Instance.FactionManager.Onu?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.BRIEFCASE_MEDICAL_SOLID);
            GameMode.Instance.FactionManager.Lspd?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
            GameMode.Instance.FactionManager.LSCustom?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.TOOLBOX_SOLID);
            //GameMode.Instance.FactionManager.Rebelle?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.REBEL_BRAND);
            GameMode.Instance.FactionManager.Gouvernement?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
            GameMode.Instance.FactionManager.Dock?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
            GameMode.Instance.FactionManager.Nordiste?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
        }

        public static void AddFactionVehicleMenu(IPlayer client, IVehicle vehicle, XMenu xMenu)
        {
            GameMode.Instance.FactionManager.Onu?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.BRIEFCASE_MEDICAL_SOLID);
            GameMode.Instance.FactionManager.Lspd?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
            GameMode.Instance.FactionManager.LSCustom?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.TOOLBOX_SOLID);
            //GameMode.Instance.FactionManager.Rebelle?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.REBEL_BRAND);
            GameMode.Instance.FactionManager.Gouvernement?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
            GameMode.Instance.FactionManager.Dock?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
            GameMode.Instance.FactionManager.Nordiste?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
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
            if (GameMode.Instance.FactionManager.Onu != null)
                return GameMode.Instance.FactionManager.Onu.HasPlayerIntoFaction(client);
            return false;
        }

        public static bool IsLspd(IPlayer client)
        {
            if (GameMode.Instance.FactionManager.Lspd != null)
                return GameMode.Instance.FactionManager.Lspd.HasPlayerIntoFaction(client);
            return false;
        }
        
/*        public static async Task<bool> IsRebelle(IPlayer client)
        {
            if (GameMode.Instance.FactionManager.Rebelle != null)
                return await GameMode.Instance.FactionManager.Rebelle.HasPlayerIntoFaction(client);
            return false;
        }
*/

        public static bool IsLSCustom(IPlayer client)
        {
            if (GameMode.Instance.FactionManager.LSCustom != null)
                return GameMode.Instance.FactionManager.LSCustom.HasPlayerIntoFaction(client);
            return false;
        }
        
        public static bool IsDock(IPlayer client)
        {
            if (GameMode.Instance.FactionManager.Dock != null)
                return GameMode.Instance.FactionManager.Dock.HasPlayerIntoFaction(client);
            return false;
        }

        public static bool IsGouv(IPlayer client)
        {
            if (GameMode.Instance.FactionManager.Gouvernement != null)
                return  GameMode.Instance.FactionManager.Gouvernement.HasPlayerIntoFaction(client);
            return false;
        }
        
        public static bool IsNordiste(IPlayer client)
        {
            if (GameMode.Instance.FactionManager.Nordiste != null)
                return GameMode.Instance.FactionManager.Nordiste.HasPlayerIntoFaction(client);
            return false;
        }

        public async Task Update()
        {
            for (int i = 0; i < GameMode.Instance.FactionManager.FactionList.Count; i++)
            {
                await GameMode.Instance.FactionManager.FactionList[i].PayCheck();
                await GameMode.Instance.FactionManager.FactionList[i].UpdateDatabase();
            }
        }
        #endregion
    }
}
