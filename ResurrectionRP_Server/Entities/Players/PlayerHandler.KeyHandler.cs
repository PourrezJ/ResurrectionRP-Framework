using AltV.Net.Async;
using AltV.Net.Async.Events;
using AltV.Net.Elements.Entities;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Inventory;
using ResurrectionRP_Server.Utils.Extensions;
using System;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Entities.Players
{
    public partial class PlayerHandler
    {
        public delegate Task KeyPressedDelegate(IPlayer client, ConsoleKey Keycode);
        [BsonIgnore]
        public KeyPressedDelegate OnKeyPressed { get; set; }

        private async Task OnKeyPressedCallback(IPlayer client, ConsoleKey Keycode)
        {
            if (!client.Exists)
                return;

            PlayerHandler ph = client.GetPlayerHandler();
            IVehicle vehicle = await client.GetVehicleAsync();
            VehicleHandler vh = vehicle?.GetVehicleHandler();

            if (ph == null)
                return;

            
            switch (Keycode)
            {/**
                case ConsoleKey.NumPad0:
                    if (MenuManager.HasOpenMenu(client))
                        return;

                    if (ph.IsCuff())
                    {
                        await client.SendNotificationError("Vous ne pouvez pas faire cette action, vous êtes menottés.");
                        return;
                    }

                    if (client.HasData("VehicleLockPicking"))
                    {
                        await LockPick.StopLockPicking(client);
                    }
                    else if (client.HasData("VehicleRepair"))
                    {
                        await CrateTools.StopRepair(client);
                    }
                    else
                    {
                        await client.StopAnimationAsync();
                    }
                    break;

                case ConsoleKey.F2:
                    if (ph.IsCuff())
                    {
                        await client.SendNotificationError("Vous ne pouvez pas faire cette action, vous êtes menottés.");
                        return;
                    }
                    await ph.OpenPlayerMenu();
                    break;
                    */
                case ConsoleKey.F3:
                    if (vehicle == null || !vehicle.Exists)
                        return;

                    if (!vehicle.Exists)
                        return;
                    /*
                    if (ph.IsCuff())
                    {
                        await client.SendNotificationError("Vous ne pouvez pas faire cette action, vous êtes menottés.");
                        return;
                    }*/
                    await vh.OpenXtremMenu(client);
                    break;
                    /*
                case ConsoleKey.F4:
                    if (ph.IsCuff())
                    {
                        await client.SendNotificationError("Vous ne pouvez pas faire cette action, vous êtes menottés.");
                        return;
                    }
                    await ph.OpenAdminMenu();
                    break;
                    */
                case ConsoleKey.Backspace:
                    await XMenuManager.XMenuManager.CloseMenu(client);
                    /*
                    if (MenuManager.HasOpenMenu(client))
                        await MenuManager.CloseMenu(client);*/
                    await RPGInventoryManager.CloseMenu(client);
                    break;
                    /*
                case ConsoleKey.M:
                    if (MenuManager.HasOpenMenu(client))
                        return;

                    await MP.Utility.Schedule(() =>
                    {
                        if (client.TryGetSharedData(SaltyShared.SharedData.Voice_VoiceRange, out object data))
                        {
                            string voiceRange = (string)data;

                            switch (voiceRange)
                            {
                                case "Parler":
                                    voiceRange = "Crier";
                                    break;
                                case "Crier":
                                    voiceRange = "Chuchoter";
                                    break;
                                case "Chuchoter":
                                    voiceRange = "Parler";
                                    break;
                            }

                            client.SetSharedData(SaltyShared.SharedData.Voice_VoiceRange, voiceRange);
                        }
                    });
                    break;

                case ConsoleKey.G:
                    if (MenuManager.HasOpenMenu(client))
                        return;

                    if (vehicle == null)
                        return;

                    if (!vehicle.Exists)
                        return;

                    if (await client.GetSeatAsync() != -1)
                        return;

                    var players = await MP.Players.GetInRangeAsync(await vehicle.GetPositionAsync(), MP.Config.GetInt("stream-distance"), vh.Dimension);

                    vh.VehicleSync.SirenSound = !vh.VehicleSync.SirenSound;

                    foreach (var player in players)
                    {
                        if (!player.Exists)
                            continue;
                        await player.CallAsync("VehStream_SetSirenSoundCTL", vehicle.Id, vh.VehicleSync.SirenSound);
                    }
                    break;

                case ConsoleKey.X:
                    if (MenuManager.HasOpenMenu(client))
                        return;

                    if (ph.IsCuff())
                    {
                        await client.SendNotificationError("Vous ne pouvez pas faire cette action, vous êtes menottés.");
                        return;
                    }

                    await ph.Crounch(!ph.PlayerSync.Crounch);

                    break;
                **/
                case ConsoleKey.I:
/*                    if (MenuManager.HasOpenMenu(client))
                        return;*/

/*                    if (ph.IsCuff())
                    {
                        await client.SendNotificationError("Vous ne pouvez pas faire cette action, vous êtes menottés.");
                        return;
                    }*/

                    Inventory.RPGInventoryMenu menu = new Inventory.RPGInventoryMenu(ph.PocketInventory, ph.OutfitInventory, ph.BagInventory, null);

                    await menu.OpenMenu(client);
                    break;

                /**case ConsoleKey.PageUp:
                    if (MenuManager.HasOpenMenu(client))
                        return;

                    if (ph.IsCuff())
                    {
                        await client.SendNotificationError("Vous ne pouvez pas faire cette action, vous êtes menottés.");
                        return;
                    }

                    if (ph.RadioSelected == null)
                    {
                        await client.SendNotificationError("Vous n'avez pas de radio d'équiper sur vous.");
                        return;
                    }

                    await RadioManager.OpenRadio(client, ph.RadioSelected);
                    break;

                case ConsoleKey.PageDown:
                    await RadioManager.Close(client);
                    break;

                case ConsoleKey.UpArrow:
                    if (MenuManager.HasOpenMenu(client))
                        return;

                    if (ph.IsCuff())
                    {
                        await client.SendNotificationError("Vous ne pouvez pas faire cette action, vous êtes menottés.");
                        return;
                    }

                    if (ph.PhoneSelected == null)
                    {
                        await client.SendNotificationError("Vous n'avez pas de téléphone d'équiper sur vous.");
                        return;
                    }

                    await PhoneManager.OpenPhone(client, ph.PhoneSelected);
                    break;


                case ConsoleKey.DownArrow:
                    await PhoneManager.ClosePhone(client);
                    break;


                case ConsoleKey.D1:
                    if (MenuManager.HasOpenMenu(client))
                        return;

                    await SwitchWeapon(1);
                    break;

                case ConsoleKey.D2:
                    if (MenuManager.HasOpenMenu(client))
                        return;

                    await SwitchWeapon(2);
                    break;

                case ConsoleKey.D3:
                    if (MenuManager.HasOpenMenu(client))
                        return;

                    await SwitchWeapon(3);
                    break;

                case ConsoleKey.NumPad1:
                case ConsoleKey.NumPad2:
                case ConsoleKey.NumPad3:
                case ConsoleKey.NumPad4:
                case ConsoleKey.NumPad5:
                case ConsoleKey.NumPad6:
                case ConsoleKey.NumPad7:
                case ConsoleKey.NumPad8:
                case ConsoleKey.NumPad9:
                    await OnAnimationKeyPressed(Keycode);
                    break;*/
            }
        }
    }
}
