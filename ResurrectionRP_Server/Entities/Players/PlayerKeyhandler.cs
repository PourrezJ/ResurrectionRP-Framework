using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using ResurrectionRP_Server.Bank;
using ResurrectionRP_Server.Entities.Peds;
using ResurrectionRP_Server.Entities.Players.Data;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Factions;
using ResurrectionRP_Server.Farms;
using ResurrectionRP_Server.Houses;
using ResurrectionRP_Server.Inventory;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Radio;
using ResurrectionRP_Server.Utils;
using ResurrectionRP_Server.Utils.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Entities.Players
{
    public static class PlayerKeyHandler
    {
        public static void Init()
        {
            Alt.OnClient("OnKeyPress", OnKeyPress);
            Alt.OnClient("OnKeyUp", OnKeyReleased);
        }

        private static void OnKeyReleased(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            PlayerHandler ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            ConsoleKey key = (ConsoleKey)(Int64)args[0];

            switch (key)
            {
                case (ConsoleKey)20:
                    if (ph.HasOpenMenu())
                        return;

                    ph.RadioSelected?.DontUse(client);
                    break;
            }
        }

        private static void OnKeyPress(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            var ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            RaycastData raycastData = JsonConvert.DeserializeObject<RaycastData>(args[1].ToString());
            ConsoleKey key          = (ConsoleKey)(Int64)args[0];
            IVehicle vehicle        = (IVehicle)args[2] ?? client.Vehicle;
            IPlayer playerDistant   = (IPlayer)args[3] ?? null;
            int streamedID          = Convert.ToInt32(args[4]);

            VehicleHandler vh       = vehicle?.GetVehicleHandler();
            Position playerPos      = client.Position;


            Ped pnj = null;

            if (raycastData.entityType == 1)
                pnj = Ped.NPCList.Find(p => p.Position.DistanceTo(raycastData.pos) <= Globals.MAX_INTERACTION_DISTANCE && p.Model == (AltV.Net.Enums.PedModel)raycastData.entityHash);

            switch (key)
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
                    else if (HouseManager.IsInHouse(client))
                    {
                        if (client.Dimension == GameMode.GlobalDimension)
                            return;
                        House house = HouseManager.GetHouse(client);

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

                    if (ph.IsSitting)
                    {
                        client.StopAnimation();
                        ph.IsSitting = false;
                        return;
                    }

                    PlayerHandler distantPh = null;
                    if (playerDistant == null && vehicle == null)
                        distantPh = PlayerManager.GetPlayersList().Find(p => p.Location.Pos.DistanceTo(raycastData.pos) < Globals.MAX_INTERACTION_DISTANCE);

                    if (playerDistant != null || distantPh != null && distantPh != ph)
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
                            rackList.AddRange(FactionManager.Dock.Racks);
                            rackList.Add(FactionManager.Dock.Importation);
                            rackList.Add(FactionManager.Dock.Quai);

                            var rack = rackList.Find(p => p?.InventoryBox.Obj.ID == streamedID);
                            if (rack == null)
                                return;

                            RPGInventoryMenu rackmenu = new RPGInventoryMenu(ph.PocketInventory, ph.OutfitInventory, ph.BagInventory, rack.InventoryBox.Inventory);
                            rackmenu.OpenMenu(client);
                        }
                        else if (Chair.IsChair(raycastData.entityHash))
                        {
                            var data = Chair.GetChairData(raycastData.entityHash);
                            if (data == null)
                                return;

                            Vector3 pos = new Vector3(Convert.ToSingle(raycastData.entityPos.X + data.x), Convert.ToSingle(raycastData.entityPos.Y + data.y), Convert.ToSingle(raycastData.entityPos.Z + data.z));
                            client.TaskStartScenarioAtPosition(data.task, pos, raycastData.entityHeading + (float)data.h, 0, true, true); ;
                            client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~ pour vous relevez.", 5000);
                            ph.IsSitting = true;
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

                    Farm farm = FarmManager.PlayerInFarmZone(client);

                    if (farm != null && !farm.NewFarm)
                    {
                        farm.StartFarming(client);
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
                    client.SetSyncedMetaData("Crounch", ph.PlayerSync.Crounch);
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
                    menu.OpenMenu(client);
                    break;

                case ConsoleKey.R:

                    if (!client.IsDead)
                        return;

                    client.Revive();

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

                    SwitchWeapon(1, client, ph);
                    break;

                case ConsoleKey.D2:
                    if (ph.HasOpenMenu())
                        return;

                    SwitchWeapon(2, client, ph);
                    break;

                case ConsoleKey.D3:
                    if (ph.HasOpenMenu())
                        return;

                    SwitchWeapon(3, client, ph);
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
                    OnAnimationKeyPressed(client, ph, key);
                    break;
            }
        }

        public static void SwitchWeapon(int slot, IPlayer client, PlayerHandler ph)
        {
            switch (slot)
            {
                case 1:
                    if (ph.OutfitInventory.Slots[16] != null)
                    {
                        var weaponItem = (ph.OutfitInventory.Slots[16].Item) as Items.Weapons;
                        if (weaponItem != null)
                        {
                            client.GiveWeapon((uint)weaponItem.Hash, 99999, true);
                        }
                    }
                    else client.RemoveAllWeapons();
                    break;

                case 2:
                    if (ph.OutfitInventory.Slots[17] != null)
                    {
                        var weaponItem = (ph.OutfitInventory.Slots[17].Item) as Items.Weapons;
                        if (weaponItem != null)
                        {
                            client.GiveWeapon((uint)weaponItem.Hash, 99999, true);
                        }
                    }
                    else client.RemoveAllWeapons();
                    break;
                case 3:
                    client.RemoveAllWeapons();
                    break;
            }

        }

        public static void OnAnimationKeyPressed(IPlayer client, PlayerHandler ph, ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.NumPad1:
                    if (ph.AnimSettings[0] != null) client.PlayAnimation(ph.AnimSettings[0].AnimDict, ph.AnimSettings[0].AnimName, 8, -1, -1, (AnimationFlags)1);
                    break;

                case ConsoleKey.NumPad2:
                    if (ph.AnimSettings[1] != null) client.PlayAnimation(ph.AnimSettings[1].AnimDict, ph.AnimSettings[1].AnimName, 8, -1, -1, (AnimationFlags)1);
                    break;

                case ConsoleKey.NumPad3:
                    if (ph.AnimSettings[2] != null) client.PlayAnimation(ph.AnimSettings[2].AnimDict, ph.AnimSettings[2].AnimName, 8, -1, -1, (AnimationFlags)1);
                    break;

                case ConsoleKey.NumPad4:
                    if (ph.AnimSettings[3] != null) client.PlayAnimation(ph.AnimSettings[3].AnimDict, ph.AnimSettings[3].AnimName, 8, -1, -1, (AnimationFlags)1);
                    break;

                case ConsoleKey.NumPad5:
                    if (ph.AnimSettings[4] != null) client.PlayAnimation(ph.AnimSettings[4].AnimDict, ph.AnimSettings[4].AnimName, 8, -1, -1, (AnimationFlags)1);
                    break;

                case ConsoleKey.NumPad6:
                    if (ph.AnimSettings[5] != null) client.PlayAnimation(ph.AnimSettings[5].AnimDict, ph.AnimSettings[5].AnimName, 8, -1, -1, (AnimationFlags)1);
                    break;

                case ConsoleKey.NumPad7:
                    if (ph.AnimSettings[6] != null) client.PlayAnimation(ph.AnimSettings[6].AnimDict, ph.AnimSettings[6].AnimName, 8, -1, -1, (AnimationFlags)1);
                    break;

                case ConsoleKey.NumPad8:
                    if (ph.AnimSettings[7] != null) client.PlayAnimation(ph.AnimSettings[7].AnimDict, ph.AnimSettings[7].AnimName, 8, -1, -1, (AnimationFlags)1);
                    break;

                case ConsoleKey.NumPad9:
                    if (ph.AnimSettings[8] != null) client.PlayAnimation(ph.AnimSettings[8].AnimDict, ph.AnimSettings[8].AnimName, 8, -1, -1, (AnimationFlags)1);
                    break;
            }
        }

        private static bool IsAtm(uint entityHash)
        {
            switch (entityHash)
            {
                case 3424098598:
                case 506770882:
                case 2930269768:
                case 3168729781:
                    return true;
            }

            return false;
        }

        private static bool IsPump(uint entityHash)
        {
            switch (entityHash)
            {
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
