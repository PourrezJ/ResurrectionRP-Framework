using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Farms;
using ResurrectionRP_Server.Inventory;
using ResurrectionRP_Server.Radio;
using ResurrectionRP_Server.Utils;
using System;
using System.Threading.Tasks;
using ResurrectionRP_Server.Entities.Players.Data;
using ResurrectionRP_Server.Bank;
using System.Linq;
using AltV.Net.Data;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Houses;
using ResurrectionRP_Server.Utils.Enums;
using ResurrectionRP_Server.Items;

namespace ResurrectionRP_Server.Entities.Players
{
    public partial class PlayerHandler
    {
        public delegate Task KeyPressedDelegate(IPlayer client, ConsoleKey Keycode, RaycastData raycastData, IVehicle vehicle, IPlayer playerDistant);
        public delegate Task KeyReleasedDelegate(IPlayer client, ConsoleKey Keycode);

        [BsonIgnore]
        public KeyPressedDelegate OnKeyPressed { get; set; }
        [BsonIgnore]
        public KeyReleasedDelegate OnKeyReleased { get; set; }

        private async Task OnKeyPressedCallback(IPlayer client, ConsoleKey Keycode, RaycastData raycastData, IVehicle vehicleDistant, IPlayer playerDistant)
        {
            if (!client.Exists)
                return;

 
            PlayerHandler ph = client.GetPlayerHandler();
            IVehicle vehicle = vehicleDistant ?? await client.GetVehicleAsync();
            VehicleHandler vh = vehicle?.GetVehicleHandler();
            Position playerPos = Position.Zero;

            client.GetPositionLocked(ref playerPos);

            if (ph == null)
                return;

            switch (Keycode)
            {
                case ConsoleKey.NumPad0:
                    if (ph.HasOpenMenu())
                        return;

                    if (ph.IsCuff())
                    {
                        client.SendNotificationError("Vous ne pouvez pas faire cette action, vous êtes menotté.");
                        return;
                    }

                    if (client.HasData("VehicleLockPicking"))
                    {
                        //await LockPick.StopLockPicking(client);
                    }
                    else if (client.HasData("VehicleRepair"))
                    {
                        //await CrateTools.StopRepair(client);
                    }
                    else
                    {
                        client.StopAnimation();
                    }
                    break;
            
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
                    if (ph.IsCuff())
                    {
                        client.SendNotificationError("Vous ne pouvez pas faire cette action, vous êtes menotté.");
                        return;
                    }

                    if (vehicle != null && vehicle.Exists)
                        await vh.OpenXtremMenu(client);
                    else if (HouseManager.IsInHouse(Client))
                    {
                        House house = HouseManager.GetHouse(Client);

                        if (house != null)
                            await HouseMenu.OpenHouseMenu(client, house);
                    }
                    break;

                case ConsoleKey.F5:
                    if (!ph.HasOpenMenu())
                        await ph.OpenAdminMenu();
                    break;

                case ConsoleKey.Backspace:
                    await XMenuManager.XMenuManager.CloseMenu(client);
                    await RPGInventoryManager.CloseMenu(client);
                    break;

                case ConsoleKey.E:
                    if (ph.HasOpenMenu())
                        return;

                    Farm farm = FarmManager.PlayerInFarmZone(client);

                    if (farm != null)
                    {
                        await farm.StartFarming(client);
                        return;
                    }

                    if (IsAtm(raycastData.entityHash) && client.Position.Distance(raycastData.pos) <= Globals.MAX_INTERACTION_DISTANCE)
                    {
                        await BankMenu.OpenBankMenu(ph, ph.BankAccount);
                        return;
                    }

                    if (vh != null && !client.IsInVehicle)
                    {
                        await vh.OpenXtremMenu(client);
                        return;
                    }

                    if (playerDistant != null)
                    {
                        await ph.OpenXtremPlayer(playerDistant);
                        return;
                    }

                    Objects.Object pickup = GameMode.Instance.ObjectManager.ListObject.FirstOrDefault(o => o.Value.position.Distance(playerPos) <= 1).Value;
                    if (pickup != null)
                    {
                        var resupickup = ResuPickupManager.GetResuPickup(pickup.id);
                        if (resupickup != null)
                            await resupickup.Take(client);
                        return;
                    }

                    if (raycastData.isHit && IsPump(raycastData.entityHash) && client.Position.Distance(raycastData.pos) <= Globals.MAX_INTERACTION_DISTANCE)
                    {
                        await Business.Market.OpenGasPumpMenu(client);
                        return;
                    }

                    Door door = GameMode.Instance.DoorManager.DoorList.Find(p => p.Position.DistanceTo2D(raycastData.pos) <= 1 && p.Hash == raycastData.entityHash && raycastData.isHit);
                    if (door != null)
                        await door.Interact?.Invoke(client, door);

                    break;

                case ConsoleKey.U:
                    if (vh != null)
                        await vh.LockUnlock(client);

                    break;
                    
                case ConsoleKey.M:
                    if (ph.HasOpenMenu())
                        return;

                    await AltAsync.Do(() =>
                    {
                        if (!client.Exists)
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

                            client.SetSyncedMetaData(SaltyShared.SharedData.Voice_VoiceRange, voiceRange);
                        }
                    });
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

                    await vehicle.SetSyncedMetaDataAsync("sirenDisabled", vh.SirenSound);

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

                    RadioManager.OpenRadio(client, ph.RadioSelected);
                    break;

                case ConsoleKey.PageDown:
                    RadioManager.Close(client);
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
                    Phone.PhoneManager.ClosePhone(client);
                    break;

                case (ConsoleKey)20:
                    if (ph.HasOpenMenu())
                        return;
                    await ph.RadioSelected?.UseRadio(client);
                    break;

                case ConsoleKey.D1:
                    if (ph.HasOpenMenu())
                        return;

                    await SwitchWeapon(1);
                    break;

                case ConsoleKey.D2:
                    if (ph.HasOpenMenu())
                        return;

                    await SwitchWeapon(2);
                    break;

                case ConsoleKey.D3:
                    if (ph.HasOpenMenu())
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
                   OnAnimationKeyPressed(Keycode);
                    break;
            }
        }

        #region Misc
        public async Task SwitchWeapon(int slot)
        {
            switch (slot)
            {
                case 1:
                    if (OutfitInventory.Slots[16] != null)
                    {
                        var weaponItem = (OutfitInventory.Slots[16].Item) as Items.Weapons;
                        if (weaponItem != null)
                        {
                            await Client.GiveWeaponAsync((uint)weaponItem.Hash, 99999, true);
                        }
                    }
                    else await Client.RemoveAllWeaponsAsync();
                    break;

                case 2:
                    if (OutfitInventory.Slots[17] != null)
                    {
                        var weaponItem = (OutfitInventory.Slots[17].Item) as Items.Weapons;
                        if (weaponItem != null)
                        {
                            await Client.GiveWeaponAsync((uint)weaponItem.Hash, 99999, true);
                        }
                    }
                    else await Client.RemoveAllWeaponsAsync();
                    break;
                case 3:
                    await Client.RemoveAllWeaponsAsync();
                    break;
            }

        }
        #endregion

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

        public void OnAnimationKeyPressed(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.NumPad1:
                    if (AnimSettings[0] != null) PlayAnimation(AnimSettings[0].AnimDict, AnimSettings[0].AnimName, 8, -1, -1, (AnimationFlags)49);
                    break;

                case ConsoleKey.NumPad2:
                    if (AnimSettings[1] != null) PlayAnimation(AnimSettings[1].AnimDict, AnimSettings[1].AnimName, 8, -1, -1, (AnimationFlags)49);
                    break;

                case ConsoleKey.NumPad3:
                    if (AnimSettings[2] != null) PlayAnimation(AnimSettings[2].AnimDict, AnimSettings[2].AnimName, 8, -1, -1, (AnimationFlags)49);
                    break;

                case ConsoleKey.NumPad4:
                    if (AnimSettings[3] != null) PlayAnimation(AnimSettings[3].AnimDict, AnimSettings[3].AnimName, 8, -1, -1, (AnimationFlags)49);
                    break;

                case ConsoleKey.NumPad5:
                    if (AnimSettings[4] != null) PlayAnimation(AnimSettings[4].AnimDict, AnimSettings[4].AnimName, 8, -1, -1, (AnimationFlags)49);
                    break;

                case ConsoleKey.NumPad6:
                    if (AnimSettings[5] != null) PlayAnimation(AnimSettings[5].AnimDict, AnimSettings[5].AnimName, 8, -1, -1, (AnimationFlags)49);
                    break;

                case ConsoleKey.NumPad7:
                    if (AnimSettings[6] != null) PlayAnimation(AnimSettings[6].AnimDict, AnimSettings[6].AnimName, 8, -1, -1, (AnimationFlags)49);
                    break;

                case ConsoleKey.NumPad8:
                    if (AnimSettings[7] != null) PlayAnimation(AnimSettings[7].AnimDict, AnimSettings[7].AnimName, 8, -1, -1, (AnimationFlags)49);
                    break;

                case ConsoleKey.NumPad9:
                    if (AnimSettings[8] != null) PlayAnimation(AnimSettings[8].AnimDict, AnimSettings[8].AnimName, 8, -1, -1, (AnimationFlags)49);
                    break;
            }
        }

        public void PlayAnimation(string animDict, string animName, float blendInSpeed = 8f, float blendOutSpeed = -8f, int duration = -1, AnimationFlags flags = (ResurrectionRP_Server.Utils.Enums.AnimationFlags)49, float playbackRate = 0f)
        {
            if (Client == null)
                return;

            if (!Client.Exists)
                return;

            Client.PlayAnimation(animDict, animName, blendInSpeed, blendOutSpeed, duration, flags, playbackRate);
        }

        private static bool IsAtm(uint entityHash) {
            switch (entityHash) {
                case 3424098598:
                case 506770882:
                case 2930269768:
                case 3168729781:
                    return true;
            }

            return false;
        }

        private static bool IsPump(uint entityHash) {
            switch (entityHash) {
                case 1339433404:
                case 1933174915:
                case unchecked((uint)-2007231801):
                case unchecked((uint)-462817101):
                case unchecked((uint)-469694731):
                case unchecked((uint)1694452750):
                case 1694:
                case 750:
                    return true;
            }

            return false;
        }
    }
}
