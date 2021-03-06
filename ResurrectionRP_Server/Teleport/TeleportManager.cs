﻿using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using Newtonsoft.Json;
using ResurrectionRP_Server.Colshape;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using ResurrectionRP_Server.Entities.Vehicles;

namespace ResurrectionRP_Server.Teleport
{
    public enum TeleportState
    {
        Enter,
        Out
    }

    public static class TeleportManager
    {
        #region Fields
        public static List<Teleport> Teleports = new List<Teleport>();
        #endregion

        #region Init
        public static void Init()
        {
            ColshapeManager.OnPlayerInteractInColshape += OnTeleportColshape;
            ColshapeManager.OnPlayerLeaveColshape += OnPlayerLeaveColshape;
        }
        #endregion

        #region Event handlers
        private static void OnTeleportColshape(IColshape colshape, IPlayer client)
        {
            if (!client.Exists)
                return;

            colshape.GetData("Teleport", out string datad);

            if (datad == null)
                return;

            var definition = new { ID = 0, State = new TeleportState() };
            var data = JsonConvert.DeserializeAnonymousType(datad, definition);
            Teleport teleport = GetTeleport(data.ID);

            if (teleport != null)
            {
                if (teleport.IsWhitelisted && !teleport.Whileliste.Contains(client.GetSocialClub()))
                {
                    client.SendNotificationError("Vous n'êtes pas autorisé à utiliser cette porte.");
                    return;
                }

                if (teleport.Sortie.Count > 1)
                {
                    if (!teleport.VehicleAllowed && client.Vehicle != null)
                        return;

                    Menu _menu = new Menu("ID_TeleportMenu", teleport.MenuTitle, "Sélectionnez une destination :", backCloseMenu: true);
                    _menu.ItemSelectCallback = MenuCallBack;

                    if (data.State == TeleportState.Out)
                    {
                        var item = new MenuItem("Sortie", executeCallback: true);
                        item.SetData("Location", teleport.Entree);
                        _menu.Add(item);
                    }

                    foreach (var etage in teleport.Sortie)
                    {
                        var item = new MenuItem(etage.Name, executeCallback: true);
                        item.SetData("Location", etage.Location);
                        _menu.Add(item);
                    }

                    _menu.OpenMenu(client);
                }
                else
                {
                    client.Emit("FadeOut", 1000);
                    Location location = (data.State == TeleportState.Enter) ? teleport.Sortie[0].Location : teleport.Entree;
                    client.RequestCollisionAtCoords(location.Pos);

                    VehicleHandler vehicle = client.Vehicle as VehicleHandler;

                    if (vehicle != null)
                        vehicle.Freeze(true);

                    Utils.Util.Delay(2000, async () =>
                    {
                        await AltAsync.Do(() =>
                        {
                            if (!client.Exists)
                                return;

                            if (teleport.VehicleAllowed && vehicle != null)
                            {
                                vehicle.WasTeleported = true;
                                
                                foreach(IPlayer player in client.GetNearestPlayers(150, false))
                                {
                                    player.EmitLocked("SetVehiclePosition", vehicle, location.Pos.ConvertToVector3Serialized());
                                }

                                vehicle.Position= location.Pos;
                                //vehicle.SetVehicleOnGroundProperly(client);
                                vehicle.Rotation = location.Rot;
                                vehicle.Freeze(false);
                            }
                            else
                            {
                                client.Position = location.Pos;
                                client.Rotation = location.Rot;
                            }
                            client.EmitLocked("FadeIn", 1000);
                        });
                    });
                }
            }
        }

        private static void OnPlayerLeaveColshape(IColshape colshape, IPlayer client)
        {
            if (!client.Exists)
                return;

            colshape.GetData("Teleport", out string datad);

            if (datad == null)
                return;

            PlayerHandler ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            if (ph.HasOpenMenu())
                MenuManager.CloseMenu(client);
        }
        #endregion

        #region Methods
        private static void MenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (!menuItem.HasData("Location"))
                return;

            Location etage = menuItem.GetData("Location");
            client.RequestCollisionAtCoords(etage.Pos);
            client.Emit("FadeOut", 1000);

            Utils.Util.Delay(2000, async () =>
                 {
                    await client.SetPositionAsync(etage.Pos);

                    // BUG v801: Set rotation when player in game not working
                    await client.SetHeadingAsync(etage.Rot.Z);
                    // client.Rotation = etage.Rot;
                    client.EmitLocked("FadeIn", 1000);
                 });
        }

        public static Teleport GetTeleport(int id) => Teleports.Find(t => t.ID == id);
        #endregion
    }
}
