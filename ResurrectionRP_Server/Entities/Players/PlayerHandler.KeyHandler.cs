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
using System.Numerics;

namespace ResurrectionRP_Server.Entities.Players
{
    public partial class PlayerHandler
    {
        public delegate Task KeyPressedDelegateAsync(IPlayer client, ConsoleKey Keycode, RaycastData raycastData, IVehicle vehicle, IPlayer playerDistant, int streamedID);
        public delegate Task KeyReleasedDelegateAsync(IPlayer client, ConsoleKey Keycode);

        public delegate void KeyPressedDelegate(IPlayer client, ConsoleKey Keycode, RaycastData raycastData, IVehicle vehicle, IPlayer playerDistant, int streamedID);
        public delegate void KeyReleasedDelegate(IPlayer client, ConsoleKey Keycode);

        [BsonIgnore]
        public KeyPressedDelegate OnKeyPressed { get; set; }
        [BsonIgnore]
        public KeyReleasedDelegate OnKeyReleased { get; set; }

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
