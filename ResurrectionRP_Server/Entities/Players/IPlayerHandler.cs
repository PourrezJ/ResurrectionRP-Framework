using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Bank;
using ResurrectionRP_Server.Entities.Players.Data;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Inventory;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Phone;
using ResurrectionRP_Server.Radio;
using ResurrectionRP_Server.Utils.Enums;

namespace ResurrectionRP_Server.Entities.Players
{
    public interface IPlayerHandler
    {
        Animation[] AnimSettings { get; set; }
        Inventory.Inventory BagInventory { get; set; }
        BankAccount BankAccount { get; set; }
        PlayerCustomization Character { get; set; }
        IPlayer Client { get; }
        Clothings Clothing { get; set; }
        int Hunger { get; set; }
        Identite Identite { get; set; }
        string IP { get; set; }
        bool IsOnline { get; set; }
        bool IsOnProgress { get; set; }
        bool Jailed { get; }
        DateTime LastUpdate { get; set; }
        List<License> Licenses { get; set; }
        List<VehicleKey> ListVehicleKey { get; }
        Location Location { get; set; }
        double Money { get; }
        OutfitInventory OutfitInventory { get; set; }
        Phone.Phone PhoneSelected { get; set; }
        string PID { get; set; }
        PlayerSync PlayerSync { get; set; }
        Inventory.Inventory PocketInventory { get; set; }
        Radio.Radio RadioSelected { get; set; }
        StaffRank StaffRank { get; set; }
        int Thirst { get; set; }
        int TimeSpent { get; set; }
        VehicleHandler Vehicle { get; set; }

        void AddAlcolhol(int quantity);
        bool AddItem(Item item, int quantity = 1);
        void AddKey(VehicleHandler veh, string keyVehicleName = null);
        void AddMoney(double somme);
        int CountItem(Item item);
        int CountItem(ItemID itemid);
        bool DeleteAllItem(ItemID itemID, int quantite = 1);
        bool DeleteItem(int slot, string inventoryType, int quantity);
        bool DeleteOneItemWithID(ItemID itemID);
        List<ItemStack> GetAllItems();
        Dictionary<string, ItemStack[]> GetStacksItems(ItemID itemID);
        bool HasBankMoney(double somme, string reason, bool save = true);
        bool HasItemID(ItemID id);
        Inventory.Inventory HasItemInAnyInventory(ItemID item);
        bool HasKey(string plate);
        bool HasKey(VehicleHandler veh);
        bool HasLicense(LicenseType type);
        bool HasMoney(double somme);
        bool HasOpenMenu();
        bool InventoryIsFull(double itemSize = 0);
        bool IsCuff();
        Task LoadPlayer(IPlayer client, bool firstspawn = false);
        void OpenAdminMenu(PlayerHandler playerSelected = null);
        void OpenPlayerMenu();
        void OpenXtremAdmin();
        void OpenXtremPlayer(IPlayer targetClient);
        bool PocketIsFull();
        void RemoveKey(VehicleHandler veh);
        Task ResetFacialAnim();
        void ResetWalkingStyle();
        void SetCuff(bool cuff);
        Task SetFacialAnim(string facial);
        void SetHealth(ushort health);
        void SetWalkingStyle(string walkingAnim);
        void UpdateClothing();
        void UpdateFull();
        void UpdateHungerThirst(int hunger = -1, int thirst = -1);
        void UpdateInBackground();
        double UpdateStat(Stats stat, double data);
        int UpdateStat(Stats stat, float data);
        int UpdateStat(Stats stat, int data);
        int UpdateStat(Stats stat, string data);
    }
}