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
using ResurrectionRP_Server.Entities.Peds;
using System.Collections.Generic;
using ResurrectionRP_Server.Factions;

namespace ResurrectionRP_Server.Entities.Players
{
    public partial class PlayerHandler
    {
        public delegate Task KeyPressedDelegateAsync(IPlayer client, ConsoleKey Keycode, RaycastData raycastData, IVehicle vehicle, IPlayer playerDistant, int streamedID);
        public delegate Task KeyReleasedDelegateAsync(IPlayer client, ConsoleKey Keycode);

        public delegate void KeyPressedDelegate(IPlayer client, ConsoleKey Keycode, RaycastData raycastData, IVehicle vehicle, IPlayer playerDistant, int streamedID);
        public delegate void KeyReleasedDelegate(IPlayer client, ConsoleKey Keycode);

        [BsonIgnore]
        public KeyPressedDelegateAsync OnKeyPressedAsync { get; set; }
        [BsonIgnore]
        public KeyReleasedDelegateAsync OnKeyReleasedAsync { get; set; }

        [BsonIgnore]
        public KeyPressedDelegate OnKeyPressed { get; set; }
        [BsonIgnore]
        public KeyReleasedDelegate OnKeyReleased { get; set; }

        private void OnKeyPressedCallback(IPlayer client, ConsoleKey Keycode, RaycastData raycastData, IVehicle vehicleDistant, IPlayer playerDistant, int streamedID)
        {
            try
            {
                if (!client.Exists)
                    return;

                PlayerHandler ph = client.GetPlayerHandler();
                IVehicle vehicle = vehicleDistant ?? client.Vehicle;
                VehicleHandler vh = vehicle?.GetVehicleHandler();
                Position playerPos = Position.Zero;

                client.GetPositionLocked(ref playerPos);

                if (ph == null)
                    return;

                Ped pnj = null;

                if (raycastData.entityType == 1)
                    pnj = Ped.NPCList.Find(p => p.Position.DistanceTo(raycastData.pos) <= Globals.MAX_INTERACTION_DISTANCE && p.Model == (AltV.Net.Enums.PedModel)raycastData.entityHash);

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
                            ph.OpenPlayerMenu();

                        break;

                    case ConsoleKey.F3:
                        if (ph.HasOpenMenu())
                            return;

                        if (ph.IsCuff())
                        {
                            client.SendNotificationError("Vous ne pouvez pas faire cette action, vous êtes menotté.");
                            return;
                        }

                        if (vehicle != null && vehicle.Exists)
                            vh.OpenXtremMenu(client);
                        else if (HouseManager.IsInHouse(Client))
                        {
                            if (Client.Dimension == GameMode.GlobalDimension)
                                return;
                            House house = HouseManager.GetHouse(Client);

                            if (house != null)
                                HouseMenu.OpenHouseMenu(client, house);
                        }
                        break;

                    case ConsoleKey.F5:
                        if (!ph.HasOpenMenu())
                            ph.OpenAdminMenu();
                        break;

                    case ConsoleKey.Backspace:
                        XMenuManager.XMenuManager.CloseMenu(client);
                        RPGInventoryManager.CloseMenu(client);
                        break;

                    case ConsoleKey.E:
                        if (ph.HasOpenMenu())
                            return;

                        if (IsSitting)
                        {
                            client.StopAnimation();
                            IsSitting = false;
                            return;
                        }

                        Farm farm = FarmManager.PlayerInFarmZone(client);

                        if (farm != null)
                        {
                            farm.StartFarming(client);
                            return;
                        }

                        PlayerHandler distantPh = null;
                        if (playerDistant == null && vehicle == null)
                            distantPh = PlayerManager.GetPlayersList().Find(p => p.Location.Pos.DistanceTo(raycastData.pos) < Globals.MAX_INTERACTION_DISTANCE);

                        if (playerDistant != null || distantPh != null && distantPh != this)
                        {
                            ph.OpenXtremPlayer(playerDistant ?? distantPh.Client);
                            return;
                        }

                        Door door = Door.DoorList.Find(p => p.Position.DistanceTo2D(raycastData.pos) <= 1 && p.Hash == raycastData.entityHash && raycastData.isHit);
                        
                        if (door != null)
                            door.Interact?.Invoke(client, door);

                        if (raycastData.entityType == 1)
                        {
                            if (pnj == null)
                                return;

                            if (pnj.Position.DistanceTo(client.Position) > 3)
                                return;

                            if (pnj.NpcInteractCallBackAsync != null)
                                Task.Run(async () => await pnj.NpcInteractCallBackAsync.Invoke(client, pnj));
                            else if (pnj.NpcInteractCallBack != null)
                                pnj.NpcInteractCallBack.Invoke(client, pnj);
                        }
                        else if (raycastData.entityType == 3)
                        {
                            if (raycastData.entityHash == 307713837)
                            {
                                List<Rack> rackList = new List<Rack>();
                                rackList.AddRange(GameMode.Instance.FactionManager.Dock.Racks);
                                rackList.Add(GameMode.Instance.FactionManager.Dock.Importation);
                                rackList.Add(GameMode.Instance.FactionManager.Dock.Quai);

                                var rack = rackList.Find(p => p.InventoryBox.Obj.ID == streamedID);
                                if (rack == null)
                                    return;

                                RPGInventoryMenu rackmenu = new RPGInventoryMenu(ph.PocketInventory, ph.OutfitInventory, ph.BagInventory, rack.InventoryBox.Inventory);

                                Task.Run(async () => await rackmenu.OpenMenu(client));
                            }
                            else if (Chair.IsChair(raycastData.entityHash))
                            {
                                var data = Chair.GetChairData(raycastData.entityHash);
                                if (data == null)
                                    return;

                                client.TaskStartScenarioAtPosition(data.task, new System.Numerics.Vector3(Convert.ToSingle(raycastData.pos.X + data.x), Convert.ToSingle(raycastData.pos.Y + data.y), Convert.ToSingle(raycastData.pos.Z + data.z)), (float)data.h, 0, true, true);
                                client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~ pour vous relevez.", 5000);
                                IsSitting = true;
                            }
                        }

                        if (IsAtm(raycastData.entityHash) && client.Position.Distance(raycastData.pos) <= Globals.MAX_INTERACTION_DISTANCE)
                        {
                            BankMenu.OpenBankMenu(ph, ph.BankAccount);
                            return;
                        }

                        if (vh != null && !client.IsInVehicle)
                        {
                            vh.OpenXtremMenu(client);
                            return;
                        }

                        Objects.WorldObject pickup = Objects.WorldObject.ListObject.FirstOrDefault(o => o.Value.Position.Distance(playerPos) <= Globals.MAX_INTERACTION_DISTANCE && o.Value.Model == AltV.Net.Alt.Hash("prop_money_bag_01")).Value;

                        if (pickup != null)
                        {
                            ResuPickup resupickup = ResuPickup.GetResuPickup(pickup.ID);

                            if (resupickup != null)
                                Task.Run(async () => await resupickup.Take(client));

                            return;
                        }

                        if (raycastData.isHit && IsPump(raycastData.entityHash) && client.Position.Distance(raycastData.pos) <= Globals.MAX_INTERACTION_DISTANCE)
                        {
                            Business.Market.OpenGasPumpMenu(client);
                            return;
                        }

                        break;

                    case ConsoleKey.U:
                        if (ph.HasOpenMenu())
                            return;

                        if (vh != null)
                            vh.LockUnlock(client);

                        break;

                    case ConsoleKey.M:
                        if (ph.HasOpenMenu())
                            return;

                        if (!client.Exists)
                            return;

                        if (client.GetSyncedMetaData(SaltyShared.SharedData.Voice_VoiceRange, out object voiceRange))
                        {
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
                        break;

                    case ConsoleKey.G:
                        if (ph.HasOpenMenu())
                            return;

                        if (vehicle == null)
                            return;

                        if (!vehicle.Exists)
                            return;

                        if (client.Seat != 1)
                            return;

                        vh.SirenSound = !vh.SirenSound;

                        vehicle.SetSyncedMetaData("SirenDisabled", vh.SirenSound);

                        break;

                    case ConsoleKey.X:
                        if (ph.HasOpenMenu())
                            return;

                        if (ph.IsCuff())
                        {
                            client.SendNotificationError("Vous ne pouvez pas faire cette action, vous êtes menotté.");
                            return;
                        }

                        ph.PlayerSync.Crounch = !ph.PlayerSync.Crounch;
                        Client.SetSyncedMetaData("Crounch", ph.PlayerSync.Crounch);
                        break;


                    case ConsoleKey.W:
                        if (raycastData.entityType != 1)
                            return;

                        if (pnj == null)
                            return;

                        if (pnj.Position.DistanceTo(client.Position) > 3)
                            return;

                        if (pnj.NpcSecInteractCallBackAsync != null)
                            Task.Run(async () => await pnj.NpcSecInteractCallBackAsync.Invoke(client, pnj));
                        else if (pnj.NpcSecInteractCallBack != null)
                            pnj.NpcSecInteractCallBack.Invoke(client, pnj);
                        break;


                    case ConsoleKey.I:
                        if (ph.HasOpenMenu())
                            return;

                        if (ph.IsCuff())
                        {
                            client.SendNotificationError("Vous ne pouvez pas faire cette action, vous êtes menotté.");
                            return;
                        }

                        RPGInventoryMenu menu = new RPGInventoryMenu(ph.PocketInventory, ph.OutfitInventory, ph.BagInventory, null);

                        Task.Run(async () => await menu.OpenMenu(client));
                        break;

                    case ConsoleKey.R:

                        if (!Client.IsDead)
                            return;

                        Client.Revive();

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

                        Phone.PhoneManager.OpenPhone(client, ph.PhoneSelected);
                        break;


                    case ConsoleKey.DownArrow:
                        Phone.PhoneManager.ClosePhone(client);
                        break;

                    case (ConsoleKey)20:
                        if (ph.HasOpenMenu())
                            return;

                        if (ph.RadioSelected != null)
                            ph.RadioSelected.UseRadio(client);

                        break;

                    case ConsoleKey.D1:
                        if (ph.HasOpenMenu())
                            return;

                        SwitchWeapon(1);
                        break;

                    case ConsoleKey.D2:
                        if (ph.HasOpenMenu())
                            return;

                        SwitchWeapon(2);
                        break;

                    case ConsoleKey.D3:
                        if (ph.HasOpenMenu())
                            return;

                        SwitchWeapon(3);
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
                        if (ph.HasOpenMenu())
                            return;
                        OnAnimationKeyPressed(Keycode);
                        break;
                }
            }
            catch(Exception ex)
            {
                AltV.Net.Alt.Server.LogError(ex.ToString());
            }
        }

        #region Misc
        public void SwitchWeapon(int slot)
        {
            switch (slot)
            {
                case 1:
                    if (OutfitInventory.Slots[16] != null)
                    {
                        var weaponItem = (OutfitInventory.Slots[16].Item) as Items.Weapons;
                        if (weaponItem != null)
                        {
                            Client.GiveWeapon((uint)weaponItem.Hash, 99999, true);
                        }
                    }
                    else Client.RemoveAllWeapons();
                    break;

                case 2:
                    if (OutfitInventory.Slots[17] != null)
                    {
                        var weaponItem = (OutfitInventory.Slots[17].Item) as Items.Weapons;
                        if (weaponItem != null)
                        {
                            Client.GiveWeapon((uint)weaponItem.Hash, 99999, true);
                        }
                    }
                    else Client.RemoveAllWeapons();
                    break;
                case 3:
                    Client.RemoveAllWeapons();
                    break;
            }

        }
        #endregion

        private void OnKeyReleasedCallback(IPlayer client, ConsoleKey Keycode)
        {
            try
            {
                if (!client.Exists)
                    return;

                PlayerHandler ph = client.GetPlayerHandler();

                if (ph == null)
                    return;


                switch (Keycode)
                {
                    case (ConsoleKey)20:
                        if (ph.HasOpenMenu())
                            return;

                        ph.RadioSelected?.DontUse(client);
                        break;
                }
            }
            catch(Exception ex)
            {
                AltV.Net.Alt.Server.LogError(ex.ToString());
            }
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
