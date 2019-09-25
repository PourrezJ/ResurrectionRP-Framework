using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using Newtonsoft.Json;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.EventHandlers;
using ResurrectionRP_Server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Teleport
{
    public enum TeleportState
    {
        Enter,
        Out
    }

    public class TeleportManager
    {
        public List<Teleport> Teleports = new List<Teleport>();

        public TeleportManager()
        {
            Events.OnPlayerInteractInColShape += OnTeleportColshape;
            Events.OnPlayerLeaveColShape += OnPlayerLeaveColShape;
        }

        private async Task OnTeleportColshape(IColShape colshape, IPlayer client)
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
                    if (!teleport.VehicleAllowed && await client.GetVehicleAsync() != null)
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

                    await _menu.OpenMenu(client);
                }
                else
                {
                    if (teleport.VehicleAllowed && await client.GetVehicleAsync() != null)
                    {
                        var vehicle = await client.GetVehicleAsync();

                        if (data.State == TeleportState.Enter)
                        {
                            var location = teleport.Sortie[0].Location;
                            client.RequestCollisionAtCoords(location.Pos);
                            await vehicle.SetPositionAsync(location.Pos);
                            await vehicle.SetRotationAsync(location.Rot);
                        }
                        else
                        {
                            client.RequestCollisionAtCoords(teleport.Entree.Pos);
                            await vehicle.SetPositionAsync(teleport.Entree.Pos);
                            await vehicle.SetRotationAsync(teleport.Entree.Rot);
                        }
                    }
                    else
                    {
                        if (data.State == TeleportState.Enter)
                        {
                            var location = teleport.Sortie[0].Location;
                            client.RequestCollisionAtCoords(location.Pos);
                            await client.SetPositionAsync(location.Pos);
                            await client.SetRotationAsync(location.Rot);
                        }
                        else
                        {
                            client.RequestCollisionAtCoords(teleport.Entree.Pos);
                            await client.SetPositionAsync(teleport.Entree.Pos);
                            await client.SetRotationAsync(teleport.Entree.Rot);
                        }
                    }

                    client.Freeze(true);
                    await Task.Delay(250);
                    client.Freeze(false);
                }
            }
        }

        private async Task OnPlayerLeaveColShape(IColShape colshape, IPlayer client)
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
                await MenuManager.CloseMenu(client);
        }

        private async Task MenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (!menuItem.HasData("Location"))
                return;

            Location etage = menuItem.GetData("Location");
            client.RequestCollisionAtCoords(etage.Pos);

            await client.SetPositionAsync(etage.Pos);
            await client.SetRotationAsync(etage.Rot);
        }

        public Teleport GetTeleport(int id) => Teleports.Find(t => t.ID == id);
    }
}
