using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Async;

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
            EventHandlers.Events.OnPlayerInteractTeleporter += OnTeleportColshape;
        }

        private async Task OnTeleportColshape(IColShape colshape, IPlayer player)
        {
            if (!player.Exists)
                return;


            var definition = new { ID = 0, State = new TeleportState() };

            colshape.GetData("Teleport", out string datad);
            var data = JsonConvert.DeserializeAnonymousType(datad, definition);
            Teleport teleport = GetTeleport(data.ID);

            if (teleport != null)
            {
                if (teleport.IsWhitelisted && !teleport.Whileliste.Contains(player.GetSocialClub()))
                {
                    player.SendNotificationError("Vous n'êtes pas autorisé à utiliser cette porte.");
                    return;
                }

                if (teleport.Sorti.Count > 1)
                {
                    if (!teleport.VehicleAllowed && await player.GetVehicleAsync() != null)
                        return;

                    Menu _menu = new Menu("ID_SellMenu", teleport.MenuTitle, backCloseMenu: true);
                    _menu.ItemSelectCallback = MenuCallBack;

                    if (data.State == TeleportState.Out)
                    {
                        var item = new MenuItem("Sorti", executeCallback: true);
                        item.SetData("Location", teleport.Entree);
                        _menu.Add(item);
                    }

                    foreach (var etage in teleport.Sorti)
                    {
                        var item = new MenuItem(etage.Name, executeCallback: true);
                        item.SetData("Location", etage.Location);
                        _menu.Add(item);
                    }

                    await _menu.OpenMenu(player);
                }
                else
                {
                    if (teleport.VehicleAllowed && await player.GetVehicleAsync() != null)
                    {
                        var vehicle = await player.GetVehicleAsync();

                        if (data.State == TeleportState.Enter)
                        {
                            var location = teleport.Sorti[0].Location;
                            player.RequestCollisionAtCoords(location.Pos);
                            await vehicle.SetPositionAsync(location.Pos);
                            await vehicle.SetRotationAsync(location.Rot);
                        }
                        else
                        {
                            player.RequestCollisionAtCoords(teleport.Entree.Pos);
                            await vehicle.SetPositionAsync(teleport.Entree.Pos);
                            await vehicle.SetRotationAsync(teleport.Entree.Rot);
                        }
                    }
                    else
                    {
                        if (data.State == TeleportState.Enter)
                        {
                            var location = teleport.Sorti[0].Location;
                            player.RequestCollisionAtCoords(location.Pos);
                            await player.SetPositionAsync(location.Pos);
                            await player.SetRotationAsync(location.Rot);
                        }
                        else
                        {
                            player.RequestCollisionAtCoords(teleport.Entree.Pos);
                            await player.SetPositionAsync(teleport.Entree.Pos);
                            await player.SetRotationAsync(teleport.Entree.Rot);
                        }
                    }

                     player.Freeze(true);
                    await Task.Delay(250);
                     player.Freeze(false);
                }
            }
        }

        private async Task MenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (!menuItem.HasData("Location")) return;
            Models.Location etage = menuItem.GetData("Location");

            client.RequestCollisionAtCoords( etage.Pos);

            await client.SetPositionAsync(etage.Pos);
            await client.SetRotationAsync(etage.Rot);
        }

        public Teleport GetTeleport(int id) => this.Teleports.Find(t => t.ID == id);
    }
}
