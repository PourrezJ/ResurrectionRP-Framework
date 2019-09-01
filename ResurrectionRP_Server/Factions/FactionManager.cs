﻿using AltV.Net.Elements.Entities;
using MongoDB.Driver;
using ResurrectionRP_Server.XMenuManager;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Factions
{
    public class FactionManager
    {
        public List<Faction> FactionList = new List<Faction>();

        public ONU Onu { get; private set; }
        public LSPD Lspd { get; private set; }
        public LSCustom LSCustom { get; private set; }
        public Division Rebelle { get; private set; }
        public Gouv Gouvernement { get; private set; }
        public Dock Dock { get; private set; }

        public Nordiste Nordiste { get; private set; }

        public async Task InitAllFactions()
        {
            FactionManager fm = GameMode.Instance.FactionManager;
            fm.Onu = (ONU)await (await LoadFaction<ONU>("ONU") ?? new ONU("ONU", FactionType.ONU)).OnFactionInit();
            fm.Lspd = (LSPD)await (await LoadFaction<LSPD>("LSPD") ?? new LSPD("LSPD", FactionType.LSPD)).OnFactionInit();
            fm.Rebelle = (Division)await (await LoadFaction<Division>("Division") ?? new Division("Division", FactionType.Division)).OnFactionInit();
            fm.LSCustom = (LSCustom)await (await LoadFaction<LSCustom>("LSCustom") ?? new LSCustom("LSCustom", FactionType.LSCustom)).OnFactionInit();
            fm.Gouvernement = (Gouv)await (await LoadFaction<Gouv>("Gouv") ?? new Gouv("Gouv", FactionType.Gouv)).OnFactionInit();
            fm.Dock = (Dock)await (await LoadFaction<Dock>("Dock") ?? new Dock("Dock", FactionType.Dock)).OnFactionInit();
            fm.Nordiste = (Nordiste)await (await LoadFaction<Nordiste>("Bureau du Shérif") ?? new Nordiste("Bureau du Shérif", FactionType.Nordiste)).OnFactionInit();

            Utils.Utils.Delay((int)TimeSpan.FromMinutes(10).TotalMilliseconds, false, async () =>
            {
                foreach (var faction in FactionList)
                {       
                    await faction.UpdateDatabase();
                    await Task.Delay(50);
                }
            });
        }

        public static void AddFactionTargetMenu(IPlayer client, IPlayer target, XMenu xMenu)
        {
            GameMode.Instance.FactionManager.Onu?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.BRIEFCASE_MEDICAL_SOLID);
            GameMode.Instance.FactionManager.Lspd?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
            GameMode.Instance.FactionManager.LSCustom?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.TOOLBOX_SOLID);
            GameMode.Instance.FactionManager.Rebelle?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.REBEL_BRAND);
            GameMode.Instance.FactionManager.Gouvernement?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
            GameMode.Instance.FactionManager.Dock?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
            GameMode.Instance.FactionManager.Nordiste?.AddFactionTargetMenu(client, target, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
        }

        public static void AddFactionVehicleMenu(IPlayer client, IVehicle vehicle, XMenu xMenu)
        {
            GameMode.Instance.FactionManager.Onu?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.BRIEFCASE_MEDICAL_SOLID);
            GameMode.Instance.FactionManager.Lspd?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
            GameMode.Instance.FactionManager.LSCustom?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.TOOLBOX_SOLID);
            GameMode.Instance.FactionManager.Rebelle?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.REBEL_BRAND);
            GameMode.Instance.FactionManager.Gouvernement?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
            GameMode.Instance.FactionManager.Dock?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
            GameMode.Instance.FactionManager.Nordiste?.AddFactionVehicleMenu(client, vehicle, xMenu, XMenuItemIcons.USER_SHIELD_SOLID);
        }

        public static async Task<T> LoadFaction<T>(string faction)
        {
            var filter = Builders<T>.Filter.Eq("FactionName", faction);
            return await Database.MongoDB.GetCollectionSafe<T>("factions").FindAsync<T>(filter).Result.FirstOrDefaultAsync();
        }

        public static async Task<bool> IsMedic(IPlayer client)
        {  
            if (GameMode.Instance.FactionManager.Onu != null)
                return await GameMode.Instance.FactionManager.Onu.HasPlayerIntoFaction(client);
            return false;
        }

        public static async Task<bool> IsLspd(IPlayer client)
        {
            if (GameMode.Instance.FactionManager.Lspd != null)
                return await GameMode.Instance.FactionManager.Lspd.HasPlayerIntoFaction(client);
            return false;
        }

        public static async Task<bool> IsRebelle(IPlayer client)
        {
            if (GameMode.Instance.FactionManager.Rebelle != null)
                return await GameMode.Instance.FactionManager.Rebelle.HasPlayerIntoFaction(client);
            return false;
        }

        public static async Task<bool> IsLSCustom(IPlayer client)
        {
            if (GameMode.Instance.FactionManager.LSCustom != null)
                return await GameMode.Instance.FactionManager.LSCustom.HasPlayerIntoFaction(client);
            return false;
        }
        public static async Task<bool> IsDock(IPlayer client)
        {
            if (GameMode.Instance.FactionManager.Dock != null)
                return await GameMode.Instance.FactionManager.Dock.HasPlayerIntoFaction(client);
            return false;
        }

        public static async Task<bool> IsGouv(IPlayer client)
        {
            if (GameMode.Instance.FactionManager.Gouvernement != null)
                return await GameMode.Instance.FactionManager.Gouvernement.HasPlayerIntoFaction(client);
            return false;
        }

        public static async Task<bool> IsNordiste(IPlayer client)
        {
            if (GameMode.Instance.FactionManager.Gouvernement != null)
                return await GameMode.Instance.FactionManager.Nordiste.HasPlayerIntoFaction(client);
            return false;
        }

        public void OnPlayerConnected(IPlayer client)
        {
            Onu?.OnPlayerConnected(client);
            Lspd?.OnPlayerConnected(client);
            LSCustom?.OnPlayerConnected(client);
            Rebelle?.OnPlayerConnected(client);
            Dock?.OnPlayerConnected(client);
            Gouvernement?.OnPlayerConnected(client);
            Nordiste?.OnPlayerConnected(client);
        }

        public void OnPlayerDisconnected(IPlayer client, DisconnectReason type, string reason)
        {
            Onu?.OnPlayerDisconnected(client, type, reason);
            Lspd?.OnPlayerDisconnected(client, type, reason);
            LSCustom?.OnPlayerDisconnected(client, type, reason);
            Rebelle?.OnPlayerDisconnected(client, type, reason);
            Dock?.OnPlayerDisconnected(client, type, reason);
            Nordiste?.OnPlayerConnected(client);
        }

        public async Task Update()
        {
            for (int i = 0; i < GameMode.Instance.FactionManager.FactionList.Count; i++)
            {
                await GameMode.Instance.FactionManager.FactionList[i].PayCheck();
                await GameMode.Instance.FactionManager.FactionList[i].UpdateDatabase();
            }
        }

            public async Task OnEnterColShape(IPlayer client, IColShape colshape)
        {
            for (int i = 0; i < FactionList?.Count; i++) await FactionList[i].OnEnterColshape(client, colshape);

            var faction = FactionList.Find(f => f?.Parking_colShape == colshape || f?.Heliport_colShape == colshape || f?.Shop_colShape == colshape || f?.Vestiaire_colShape == colshape);

            if (colshape == faction?.Parking_colShape)
                await faction?.OpenConcessMenu(client, Faction.ConcessType.Vehicle, faction.ParkingLocation, faction.FactionName);
            else if (colshape == faction?.Heliport_colShape)
                await faction?.OpenConcessMenu(client, Faction.ConcessType.Helico, faction.HeliportLocation, faction.FactionName);
            else if (colshape == faction?.Shop_colShape)
                await faction?.OpenShopMenu(client);
            else if (colshape == faction?.Vestiaire_colShape)
                await faction?.PriseServiceMenu(client);
        }

        public static async Task OnExitColShape(IPlayer player, IColShape colshapePointer)
        {
            for (int i = 0; i < GameMode.Instance.FactionManager.FactionList?.Count; i++)
                await GameMode.Instance.FactionManager.FactionList[i].OnExitColshape(player, colshapePointer);
        }
    }
}