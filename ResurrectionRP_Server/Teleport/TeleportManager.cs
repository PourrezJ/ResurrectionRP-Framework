using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using Newtonsoft.Json;
using ResurrectionRP_Server.Colshape;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.EventHandlers;
using ResurrectionRP_Server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using AltV.Net;

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
                    if (teleport.VehicleAllowed && client.Vehicle != null)
                    {
                        var vehicle = client.Vehicle;
                        vehicle.GetVehicleHandler().WasTeleported = true;
                        if (data.State == TeleportState.Enter)
                        {
                            var location = teleport.Sortie[0].Location;
                            client.RequestCollisionAtCoords(location.Pos);
                            vehicle.Position = location.Pos;
                            vehicle.Rotation = location.Rot;
                        }
                        else
                        {
                            client.RequestCollisionAtCoords(teleport.Entree.Pos);
                            vehicle.Position = teleport.Entree.Pos;
                            vehicle.Rotation = teleport.Entree.Rot;
                        }
                    }
                    else
                    {
                        if (data.State == TeleportState.Enter)
                        {
                            var location = teleport.Sortie[0].Location;
                            client.RequestCollisionAtCoords(location.Pos);
                            client.Position = location.Pos;
                            client.Rotation = location.Rot;
                        }
                        else
                        {
                            client.RequestCollisionAtCoords(teleport.Entree.Pos);
                            client.Position = teleport.Entree.Pos;
                            client.Rotation = teleport.Entree.Rot;
                        }
                    }

                    client.Freeze(true);

                    Task.Run(async () =>
                    {
                        await Task.Delay(250);
                        client.Freeze(false);
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


            Utils.Utils.Delay(2000, () =>
                 {
                     client.Position = etage.Pos;

                    // BUG v801: Set rotation when player in game not working
                    client.SetHeading(etage.Rot.Z);
                    // client.Rotation = etage.Rot;
                    client.Emit("FadeIn", 1000);
                 });
        }

        public static Teleport GetTeleport(int id) => Teleports.Find(t => t.ID == id);
        #endregion
    }
}
