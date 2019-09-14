using AltV.Net.Async;
using AltV.Net.Async.Events;
using AltV.Net.Elements.Entities;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Farms;
using ResurrectionRP_Server.Inventory;
using ResurrectionRP_Server.Radio;
using ResurrectionRP_Server.Utils;
using ResurrectionRP_Server.Utils.Extensions;
using System;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Entities.Players
{
    public partial class PlayerHandler
    {
        public delegate Task KeyPressedDelegate(IPlayer client, ConsoleKey Keycode);
        public delegate Task KeyReleasedDelegate(IPlayer client, ConsoleKey Keycode);
        [BsonIgnore]
        public KeyPressedDelegate OnKeyPressed { get; set; }
        [BsonIgnore]
        public KeyPressedDelegate OnKeyReleased { get; set; }

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
                    if (ph.HasOpenMenu())
                        return;

                    if (ph.IsCuff())
                    {
                        await client.SendNotificationError("Vous ne pouvez pas faire cette action, vous êtes menotté.");
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
            */
                case ConsoleKey.F2:
                    if (ph.IsCuff())
                    {
                        client.SendNotificationError("Vous ne pouvez pas faire cette action, vous êtes menotté.");
                        return;
                    }

                    if (!ph.HasOpenMenu())
                        await ph.OpenPlayerMenu();

                    break;
                    
                case ConsoleKey.F3:
                    if (vehicle == null || !vehicle.Exists)
                        return;

                    if (!vehicle.Exists)
                        return;
                    
                    if (ph.IsCuff())
                    {
                        client.SendNotificationError("Vous ne pouvez pas faire cette action, vous êtes menotté.");
                        return;
                    }
                    await vh.OpenXtremMenu(client);
                    break;
                    
                case ConsoleKey.F5:
                    if (ph.HasOpenMenu())
                        return;

                    await ph.OpenAdminMenu();
                    break;
                    
                case ConsoleKey.Backspace:
                    await XMenuManager.XMenuManager.CloseMenu(client);
                    await RPGInventoryManager.CloseMenu(client);
                    break;

                case ConsoleKey.E:

                    Farm farm = await FarmManager.PlayerInFarmZone(client);
                    if (farm != null)
                    {
                        await farm.StartFarming(client);
                        return;
                    }

                    Door door = GameMode.Instance.DoorManager.DoorList.Find(p => p.Position.DistanceTo2D(client.Position) <= 1.5f);
                    if (door != null)
                        await door.Interact?.Invoke(client, door);

                    break;
                    
                case ConsoleKey.M:
                    if (ph.HasOpenMenu())
                        return;

                    if (client.GetSyncedMetaData(SaltyShared.SharedData.Voice_VoiceRange, out object data))
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

                        await client.SetSyncedMetaDataAsync(SaltyShared.SharedData.Voice_VoiceRange, voiceRange);
                    }
                    break;

                case ConsoleKey.G:
                    if (ph.HasOpenMenu())
                        return;

                    if (vehicle == null)
                        return;

                    if (!vehicle.Exists)
                        return;

                    if (await client.GetSeatAsync() != 1)
                        return;

                    vh.SirenSound = !vh.SirenSound;

                    await client.EmitAsync("VehicleSetSirenSound", vehicle, vh.SirenSound);

                    break;
                    /*
                case ConsoleKey.X:
                    if (ph.HasOpenMenu())
                        return;

                    if (ph.IsCuff())
                    {
                        await client.SendNotificationError("Vous ne pouvez pas faire cette action, vous êtes menotté.");
                        return;
                    }

                    await ph.Crounch(!ph.PlayerSync.Crounch);

                    break;
                **/
                case ConsoleKey.I:
                    if (ph.HasOpenMenu())
                        return;

                    if (ph.IsCuff())
                    {
                        client.SendNotificationError("Vous ne pouvez pas faire cette action, vous êtes menotté.");
                        return;
                    }

                    RPGInventoryMenu menu = new RPGInventoryMenu(ph.PocketInventory, ph.OutfitInventory, ph.BagInventory, null);

                    await menu.OpenMenu(client);
                    break;

                case ConsoleKey.PageUp:
                    if (ph.HasOpenMenu())
                        return;

                    if (ph.IsCuff())
                    {
                        client.SendNotificationError("Vous ne pouvez pas faire cette action, vous êtes menotté.");
                        return;
                    }

                    if (ph.RadioSelected == null)
                    {
                        client.SendNotificationError("Vous n'avez pas de radio d'équipée sur vous.");
                        return;
                    }

                    await RadioManager.OpenRadio(client, ph.RadioSelected);
                    break;

                case ConsoleKey.PageDown:
                    if (ph.HasOpenMenu())
                        return;

                    await RadioManager.Close(client);
                    break;

                case ConsoleKey.UpArrow:
                    if (ph.HasOpenMenu())
                        return;

                    if (ph.IsCuff())
                    {
                        client.SendNotificationError("Vous ne pouvez pas faire cette action, vous êtes menotté.");
                        return;
                    }

                    if (ph.PhoneSelected == null)
                    {
                        client.SendNotificationError("Vous n'avez pas de téléphone d'équipé sur vous.");
                        return;
                    }

                    await Phone.PhoneManager.OpenPhone(client, ph.PhoneSelected);
                    break;


                case ConsoleKey.DownArrow:
                    if (ph.HasOpenMenu())
                        return;

                    Phone.PhoneManager.ClosePhone(client);
                    break;

                case (ConsoleKey)20:
                    if (ph.HasOpenMenu())
                        return;

                    await ph.RadioSelected?.UseRadio(client);
                    break;

                    /*

                case ConsoleKey.D1:
                    if (ph.HasOpenMenu())
                        return;

                    await SwitchWeapon(1);
                    break;

                case ConsoleKey.D2:
                    if (client.HasOpenMenu())
                        return;

                    await SwitchWeapon(2);
                    break;

                case ConsoleKey.D3:
                    if (client.HasOpenMenu())
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

        private Task OnKeyReleasedCallback(IPlayer client, ConsoleKey Keycode)
        {
            if (!client.Exists)
                return Task.CompletedTask;

            PlayerHandler ph = client.GetPlayerHandler();

            if (ph == null)
                return Task.CompletedTask;


            switch (Keycode)
            {
                case (ConsoleKey)20:
                    if (ph.HasOpenMenu())
                        return Task.CompletedTask;

                    ph.RadioSelected?.DontUse(client);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
