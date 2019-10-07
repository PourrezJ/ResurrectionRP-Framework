using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ResurrectionRP_Server.Bank;
using ResurrectionRP_Server.Colshape;
using ResurrectionRP_Server.Entities.Peds;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using VehicleInfoLoader.Data;

public enum GarageType
{
    Car,
    Bike
}

public class GarageData
{
    #region Structs
    public struct EsthetiqueData
    {
        public readonly byte ModID;
        public readonly string ModName;
        public readonly double ModPrice;

        public EsthetiqueData(byte modID, string modName, double modPrice)
        {
            ModID = modID;
            ModName = modName;
            ModPrice = modPrice;
        }
    }

    public struct PerformanceData
    {
        public readonly byte ModID;
        public readonly string ModName;
        public readonly double[] ModPrice;

        public PerformanceData(byte modID, string modName, double[] modPrice)
        {
            ModID = modID;
            ModName = modName;
            ModPrice = modPrice;
        }
    }
    #endregion

    #region Fields
    public EsthetiqueData[] EsthetiqueModList;
    public PerformanceData[] PerformanceModList;
    #endregion

    #region Constructor
    public GarageData(EsthetiqueData[] esthetiqueModList, PerformanceData[] performanceModList)
    {
        EsthetiqueModList = esthetiqueModList;
        PerformanceModList = performanceModList;
    }
    #endregion

    #region Static methods
    public static double CalculPrice(VehicleHandler vehicle, double originalPrice)
    {
        var multiplicator = 0;
        double price = 0;

        switch (vehicle.VehicleManifest.VehicleClass)
        {
            case 0: // Compact
                multiplicator = 155;
                break;

            case 1: // Sedans
                multiplicator = 77;
                break;

            case 2: // SUVs
                multiplicator = 285;
                break;

            case 3: // Coupes
                multiplicator = 285;
                break;

            case 4: // Muscle
                multiplicator = 285;
                break;

            case 5: // Sports Classics
                multiplicator = 800;
                break;

            case 6: // Sports
                multiplicator = 800;
                break;

            case 7: // Super
                multiplicator = 2000;
                break;

            case 9: // Off-Road
                multiplicator = 165;
                break;

            case 12: // Vans
                multiplicator = 165;
                break;
        }

        price = originalPrice * (multiplicator / 100);

        if (price == 0)
            return originalPrice;

        return price;
    }
    #endregion

    #region Methods
    public EsthetiqueData? GetEsthetiqueData(int modID)
    {
        for (int i = 0; i < EsthetiqueModList.Count(); i++)
        {
            if (EsthetiqueModList[i].ModID == modID)
                return EsthetiqueModList[i];
        }
        return null;
    }

    public PerformanceData? GetPerformanceData(int modID)
    {
        for (int i = 0; i < PerformanceModList.Count(); i++)
        {
            if (PerformanceModList[i].ModID == modID)
                return PerformanceModList[i];
        }
        return null;
    }
    #endregion
}

namespace ResurrectionRP_Server.Society.Societies
{
    public abstract class Garage : Society
    {
        #region Private fields
        private Ped _npc;
        private IColshape _workZone;
        private byte _modType;
        private string _subtitle;
        private double _price;
        private Color _neonColor;
        private int _red;
        private int _green;
        private int _blue;

        protected Door GarageDoor;
        #endregion

        #region Protected properties
        protected int[] BlackListCategories { get; set; }

        protected GarageData Data { get; set; }

        protected GarageType Type { get; set; }

        protected IVehicle VehicleBench { get; set; }

        protected Location PnjLocation { get; set; }

        protected Vector3 WorkZonePosition { get; set; }

        protected IPlayer ClientInMenu { get; set; }

        protected Banner MenuBanner { get; set; }
        #endregion

        #region Constructor
        public Garage(string societyName, Vector3 servicePos, uint blipSprite, int blipColor, string owner = null, Inventory.Inventory inventory = null, Models.Parking parking = null) : base(societyName, servicePos, blipSprite, blipColor, owner, inventory, parking)
        {
        }
        #endregion

        #region Init
        public override void Init()
        {
            _npc = Ped.CreateNPC(PedModel.Benny, PnjLocation.Pos, PnjLocation.Rot.Z, GameMode.GlobalDimension);
            _npc.NpcInteractCallBack = OnInteractWithPnj;

            _workZone = ColshapeManager.CreateCylinderColshape(new Vector3(WorkZonePosition.X, WorkZonePosition.Y, WorkZonePosition.Z - 1), 10, 5);
            _workZone.OnVehicleEnterColshape += OnVehicleEnterWorkzone;
            _workZone.OnVehicleLeaveColshape += OnVehicleLeaveWorkzone;

            base.Init();
        }
        #endregion

        #region Event handlers
        private void OnInteractWithPnj(IPlayer client, Entities.Peds.Ped npc)
        {
            if (VehicleBench != null)
                OpenMainMenu(client);
            else
                client.SendNotificationError("Aucun véhicule devant l'établi.");
        }

        private void OnVehicleEnterWorkzone(IColshape colshape, IVehicle vehicle)
        {
            if (vehicle == null || !vehicle.Exists)
                return;

            if (VehicleBench == null)
                VehicleBench = vehicle;
        }

        private void OnVehicleLeaveWorkzone(IColshape colshape, IVehicle vehicle)
        {
            if (vehicle == null || !vehicle.Exists)
                return;

            if (vehicle == VehicleBench)
                VehicleBench = null;

            if (ClientInMenu != null)
                MenuManager.CloseMenu(ClientInMenu);
        }
        #endregion

        #region Main menu
        protected virtual void OpenMainMenu(IPlayer client)
        {
            if (!(IsEmployee(client) || Owner == client.GetSocialClub()))
            {
                client.SendNotificationError("Hey mec tu veux quoi?!");
                return;
            }

            if (VehicleBench == null || !VehicleBench.Exists)
            {
                client.SendNotificationError("Aucun véhicule devant l'établi.");
                return;
            }

            try
            {
                var info = VehicleInfoLoader.VehicleInfoLoader.Get(VehicleBench.Model);

                if (info == null)
                {
                    client.SendNotificationError("Je ne touche pas à ce véhicule bordel.");
                    return;
                }
                else if (info.VehicleClass != 8 && Type == GarageType.Bike)
                {
                    client.SendNotificationError("Seules les motos sont autorisées mon pote!");
                    return;
                }
                else if (info.VehicleClass == 8 && Type == GarageType.Car)
                {
                    client.SendNotificationError("J'ai une geule à faire de la moto?!");
                    return;
                }
                else if (BlackListCategories.Contains(info.VehicleClass))
                {
                    client.SendNotificationError("OH! Je touche pas à ça, dégage!");
                    return;
                }
                else if (ClientInMenu != null && ClientInMenu != client)
                {
                    client.SendNotificationError("Un mécanicien utilise déjà la caisse à outils.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("OpenMainMenu" + ex);
                client.SendNotificationError("Erreur inconnue, contactez un membre du staff.");
                return;
            }

            Menu mainMenu = new Menu("ID_Main", "", SocietyName, Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true, MenuBanner);
            mainMenu.ItemSelectCallback = MainMenuCallback;
            mainMenu.Finalizer = Finalizer;

            MenuItem design = new MenuItem("Esthétique", "", "Design", true);
            mainMenu.Add(design);

            MenuItem performance = new MenuItem("Performance", "", "Perf", true);
            mainMenu.Add(performance);

            MenuItem historique = new MenuItem("Historique", "", "Histo", true);
            mainMenu.Add(historique);

            mainMenu.OpenMenu(client);
            ClientInMenu = client;
        }

        protected virtual void MainMenuCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (VehicleBench == null || !VehicleBench.Exists)
            {
                menu.CloseMenu(client);
                return;
            }

            if (menuItem.Id == "Design")
                OpenDesignMenu(client);
            else if (menuItem.Id == "Perf")
                OpenPerformanceMenu(client);
            else if (menuItem.Id == "Histo")
                OpenHistoricMenu(client);
        }
        #endregion

        #region Performance
        protected void OpenPerformanceMenu(IPlayer client)
        {
            if (VehicleBench == null || !VehicleBench.Exists)
            {
                MenuManager.CloseMenu(client);
                return;
            }

            var manifest = VehicleBench.GetVehicleHandler()?.VehicleManifest;

            if (manifest == null)
            {
                client.SendNotificationError("problème avec le véhicule.");
                return;
            }

            Menu menu = new Menu("ID_Perf", "", "Choisissez l'élément à installer :", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, banner: MenuBanner);
            menu.ItemSelectCallback = PerformanceMenuCallback;

            foreach (var mod in Data.PerformanceModList)
            {
                if (manifest.HasModType(mod.ModID))
                {
                    MenuItem item = new MenuItem(mod.ModName, executeCallback: true);
                    item.SetData("mod", mod.ModID);
                    item.OnMenuItemCallback = PerformanceItemMenuCallback;
                    menu.Add(item);
                }
            }

            menu.OpenMenu(client);
            ClientInMenu = client;
        }

        protected void PerformanceMenuCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (VehicleBench == null || !VehicleBench.Exists)
                menu.CloseMenu(client);
            else if (menuItem == null)
                OpenMainMenu(client);
        }

        protected void PerformanceItemMenuCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (VehicleBench == null || !VehicleBench.Exists)
            {
                menu.CloseMenu(client);
                return;
            }
            else if (!menuItem.HasData("mod"))
            {
                client.SendNotificationError("problème avec le véhicule.");
                OpenPerformanceMenu(client);
                return;
            }

            _modType = menuItem.GetData("mod");
            _subtitle = $"{menuItem.Text} :";
            OpenPerformanceChoiceMenu(client);
        }

        protected void OpenPerformanceChoiceMenu(IPlayer client, int selectedItem = 0)
        {
            if (VehicleBench == null || !VehicleBench.Exists)
            {
                MenuManager.CloseMenu(client);
                return;
            }

            var vh = VehicleBench.GetVehicleHandler();
            var manifest = vh.VehicleManifest;

            if (vh == null || !manifest.HasModType(_modType))
            {
                client.SendNotificationError("Je ne peux pas sur ce véhicule.");
                OpenPerformanceMenu(client);
            }

            Menu menu = new Menu("ID_Perfs", "", _subtitle, Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, banner: MenuBanner);
            menu.ItemSelectCallback = PerformanceChoiceMenuCallback;
            menu.IndexChangeCallback = ModPreview;
            menu.Finalizer = Finalizer;

            var mods = manifest.Mods(_modType);
            var perfdata = Data.GetPerformanceData(_modType).Value;
            vh.Mods.TryGetValue(_modType, out byte valueInstalled);

            for (int i = 0; i < mods.Count(); i++)
            {
                var mod = mods.ElementAt(i);

                if (mod == null || mod.Name == string.Empty)
                    continue;

                string modName = mod.LocalizedName;

                if (modName == string.Empty)
                    modName = mod.Name;

                if (_modType == 11 && mod.Name == "CMOD_ARM_0")
                    modName = "Gestion moteur de série";
                else if (_modType == 11 && mod.Name == "CMOD_ENG_6")
                    modName = "Reprog moteur Niv. 5";

                MenuItem item = null;

                if (i == valueInstalled)
                {
                    item = new MenuItem(modName, executeCallbackIndexChange: true);

                    if (Type == GarageType.Bike)
                        item.RightBadge = BadgeStyle.Bike;
                    else
                        item.RightBadge = BadgeStyle.Car;
                }
                else
                {
                    item = new MenuItem(modName, executeCallback: true, executeCallbackIndexChange: true);
                    var price = GarageData.CalculPrice(vh, perfdata.ModPrice[i]);

                    if (price != 0)
                        item.RightLabel = $"${price}";

                    item.SetData("price", price);
                    item.OnMenuItemCallback = ModChoice;
                }

                menu.Add(item);
            }

            menu.SelectedIndex = selectedItem;
            menu.OpenMenu(client);
            ClientInMenu = client;
            ModPreview(client, menu, selectedItem, menu.Items[selectedItem]);
        }

        protected void PerformanceChoiceMenuCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (VehicleBench == null || !VehicleBench.Exists)
            {
                menu.CloseMenu(client);
                return;
            }

            VehicleHandler vh = VehicleBench.GetVehicleHandler();

            if (menuItem == null)
            {
                if (vh.Mods.ContainsKey(_modType))
                    VehicleBench.SetMod(_modType, vh.Mods[_modType]);
                else
                    VehicleBench.SetMod(_modType, 0);

                OpenPerformanceMenu(client);
            }
        }
        #endregion

        #region Design
        protected void OpenDesignMenu(IPlayer client)
        {
            if (VehicleBench == null || !VehicleBench.Exists)
            {
                MenuManager.CloseMenu(client);
                return;
            }

            Menu menu = new Menu("ID_Design", "", "Choisissez l'élément à installer :", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, banner: MenuBanner);
            menu.ItemSelectCallback = DesignMenuCallback;
            menu.Finalizer = Finalizer;

            VehicleHandler vh = VehicleBench.GetVehicleHandler();
            var manifest = vh.VehicleManifest;

            foreach (var mod in Data.EsthetiqueModList)
            {
                if (manifest.HasModType(mod.ModID))
                {
                    var price = GarageData.CalculPrice(vh, Data.GetEsthetiqueData(mod.ModID)?.ModPrice ?? 0);

                    MenuItem item = new MenuItem(mod.ModName, rightLabel: $"${price}", executeCallback: true);
                    item.SetData("mod", mod.ModID);
                    item.SetData("price", price);
                    item.OnMenuItemCallback = DesignMenuItemCallback;
                    menu.Add(item);
                }
            }

            if (manifest.Neon)
            {
                double price = GarageData.CalculPrice(vh, 2500);
                MenuItem item = new MenuItem("Néons", "", "Neons", rightLabel: $"${price}", executeCallback: true, executeCallbackIndexChange: true);
                item.SetData("price", price);
                menu.Add(item);
            }

            menu.OpenMenu(client);
            ClientInMenu = client;
        }

        protected void DesignMenuCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (VehicleBench == null || !VehicleBench.Exists)
            {
                menu.CloseMenu(client);
                return;
            }
            else if (menuItem == null)
            {
                OpenMainMenu(client);
                return;
            }

            if (menuItem.Id == "Neons")
            {
                _price = menuItem.GetData("price");
                OpenNeonsMenu(client);
            }
        }

        protected void DesignMenuItemCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (VehicleBench == null || !VehicleBench.Exists)
            {
                menu.CloseMenu(client);
                return;
            }
            else if (!menuItem.HasData("mod"))
            {
                client.SendNotificationError("problème avec le véhicule.");
                OpenDesignMenu(client);
                return;
            }

            _modType = menuItem.GetData("mod");
            _price = menuItem.GetData("price");
            _subtitle = $"{menuItem.Text} :";
            OpenDesignChoiceMenu(client);
        }

        protected void OpenDesignChoiceMenu(IPlayer client, int selectedItem = 0)
        {
            if (VehicleBench == null || !VehicleBench.Exists)
            {
                MenuManager.CloseMenu(client);
                return;
            }

            VehicleHandler vh = VehicleBench.GetVehicleHandler();
            var manifest = vh.VehicleManifest;

            if (!manifest.HasModType(_modType)) // 666 = neons
            {
                client.SendNotificationError("Je ne peux pas sur ce véhicule.");
                OpenDesignMenu(client);
            }

            Menu menu = new Menu("ID_DesignChoice", "", _subtitle, Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, banner: MenuBanner);
            menu.IndexChangeCallback = ModPreview;
            menu.ItemSelectCallback = DesignChoiceCallback;
            menu.Finalizer = Finalizer;

            IEnumerable<VehicleMod> mods = manifest.Mods(_modType);
            vh.Mods.TryGetValue(_modType, out byte valueInstalled);

            for (int i = 0; i < mods.Count(); i++)
            {
                var mod = mods.ElementAt(i);

                if (mod == null || mod.Name == string.Empty)
                    continue;

                MenuItem item = null;
                string modName = mod.LocalizedName;

                if (modName == string.Empty)
                    modName = mod.Name;

                if (i == valueInstalled)
                {
                    item = new MenuItem(modName, executeCallbackIndexChange: true);

                    if (Type == GarageType.Bike)
                        item.RightBadge = BadgeStyle.Bike;
                    else
                        item.RightBadge = BadgeStyle.Car;
                }
                else
                {
                    item = new MenuItem(modName, executeCallback: true, executeCallbackIndexChange: true, rightLabel: $"${_price}");
                    item.SetData("price", _price);
                }

                item.OnMenuItemCallback = ModChoice;
                menu.Add(item);
            }

            menu.SelectedIndex = selectedItem;
            menu.OpenMenu(client);
            ClientInMenu = client;
            ModPreview(client, menu, selectedItem, menu.Items[selectedItem]);
        }

        protected void DesignChoiceCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (VehicleBench == null || !VehicleBench.Exists)
            {
                menu.CloseMenu(client);
                return;
            }

            VehicleHandler vh = VehicleBench.GetVehicleHandler();

            if (vh == null)
            {
                menu.CloseMenu(client);
                return;
            }

            if (menuItem == null)
            {
                if (_modType == 14 && vh.Mods.ContainsKey(_modType))
                    HornStop(VehicleBench, vh.Mods[_modType]);
                else if (_modType == 14)
                    HornStop(VehicleBench, 0);
                else if (_modType == 69 && vh.Mods.ContainsKey(_modType))
                    VehicleBench.SetWindowTint(Utils.Utils.GetWindowTint(vh.Mods[_modType]));
                else if (_modType == 69)
                    VehicleBench.SetWindowTint(WindowTint.None);
                else if (vh.Mods.ContainsKey(_modType))
                    VehicleBench.SetMod(_modType, vh.Mods[_modType]);
                else
                    VehicleBench.SetMod(_modType, 0);

                OpenDesignMenu(client);
            }
        }

        protected void OpenNeonsMenu(IPlayer client)
        {
            if (VehicleBench == null || !VehicleBench.Exists)
            {
                MenuManager.CloseMenu(client);
                return;
            }

            VehicleHandler vh = VehicleBench.GetVehicleHandler();

            Menu menu = new Menu("ID_Neons", "", "Néons :", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, banner: MenuBanner);
            menu.ItemSelectCallback = NeonsMenuCallback;
            menu.ListItemChangeCallback = NeonListItemChangeCallback;
            menu.Finalizer = Finalizer;

            _neonColor = vh.NeonColor;
            _red = vh.NeonColor.R / 17;
            _green = vh.NeonColor.G / 17;
            _blue = vh.NeonColor.B / 17;
            List<object> colorItems = GetColorListItems();
            menu.Add(new ListItem("Rouge", "", "Red", colorItems, _red, false, true));
            menu.Add(new ListItem("Vert", "", "Green", colorItems, _green, false, true));
            menu.Add(new ListItem("Bleu", "", "Blue", colorItems, _blue, false, true));

            MenuItem menuItem = new MenuItem("Valider", "", "Validate", true, false, $"${_price}");
            menuItem.SetData("price", _price);
            menu.Add(menuItem);

            vh.SetNeonState(true);
            vh.EngineOn = true;
            menu.OpenMenu(client);
            ClientInMenu = client;
        }

        protected void NeonsMenuCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (VehicleBench == null || !VehicleBench.Exists)
            {
                menu.CloseMenu(client);
                return;
            }

            VehicleHandler vh = VehicleBench.GetVehicleHandler();

            if (menuItem == null)
            {
                vh.NeonColor = _neonColor;

                OpenDesignMenu(client);
                return;
            }

            if (menuItem.Id == "Validate")
            {
                double price = menuItem.GetData("price");
                InstallNeon(client, price);
            }
        }

        protected void NeonListItemChangeCallback(IPlayer client, Menu menu, IListItem listItem, int listIndex)
        {
            if (VehicleBench == null || !VehicleBench.Exists)
            {
                menu.CloseMenu(client);
                return;
            }

            VehicleHandler vh = VehicleBench.GetVehicleHandler();

            if (listItem.Id == "Red")
                _red = int.Parse(listItem.Items[listIndex].ToString());
            else if (listItem.Id == "Green")
                _green = int.Parse(listItem.Items[listIndex].ToString());
            else if (listItem.Id == "Blue")
                _blue = int.Parse(listItem.Items[listIndex].ToString());

            vh.NeonColor = Color.FromArgb(_red * 17, _green * 17, _blue * 17);
        }
        #endregion

        #region Vehicle history
        protected void OpenHistoricMenu(IPlayer client)
        {
            if (VehicleBench == null || !VehicleBench.Exists)
            {
                MenuManager.CloseMenu(client);
                return;
            }

            VehicleHandler vh = VehicleBench.GetVehicleHandler();

            if (vh == null)
            {
                client.SendNotificationError("Problème avec le véhicule.");
                return;
            }

            if (vh.Mods.Count == 0)
            {
                client.SendNotificationError("Aucune amélioration n'est disponible sur le véhicule!");
                return;
            }
            else if (vh.Mods.Count >= 1)
            {
                Menu menu = new Menu("ID_Histo", "", "Historique :", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, false, MenuBanner);
                menu.ItemSelectCallback = HistoricMenuCallback;
                menu.Finalizer = Finalizer;

                foreach (var mod in vh.Mods)
                {
                    VehicleMod modData;

                    if (mod.Key == 22)
                        modData = vh.VehicleManifest.GetMod(mod.Key, mod.Value);
                    else
                        modData = vh.VehicleManifest.GetMod(mod.Key, mod.Value - 1);

                    if (modData != null)
                    {
                        string modName = modData.LocalizedName;

                        if (modData.Name == "CMOD_ARM_0")
                            modName = "Gestion moteur de série";
                        else if (modData.Name == "CMOD_ENG_6")
                            modName = "Reprog moteur Niv. 5";
                        else if (modData.LocalizedName == string.Empty)
                            modName = modData.Name;

                        menu.Add(new MenuItem(modName));
                    }
                }

                if (vh.NeonColor != null && !vh.NeonColor.IsEmpty)
                {
                    Color color = vh.NeonColor;
                    menu.Add(new MenuItem($"Néons : Rouge {color.R / 17} - Vert {color.G / 17} - Bleu {color.B / 17}"));
                }

                menu.OpenMenu(client);
                ClientInMenu = client;
            }
        }

        protected void HistoricMenuCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (VehicleBench == null || !VehicleBench.Exists)
            {
                menu.CloseMenu(client);
                return;
            }

            if (menuItem == null)
                OpenMainMenu(client);
        }
        #endregion

        #region Methods
        protected void ModPreview(IPlayer client, Menu menu, int itemIndex, IMenuItem menuItem)
        {
            if (VehicleBench == null || !VehicleBench.Exists)
            {
                MenuManager.CloseMenu(client);
                return;
            }

            byte selected = (byte)itemIndex;

            if (_modType == 14)
                HornPreview(VehicleBench, selected);
            else if (_modType == 23)
                VehicleBench.SetWheels(2, 10);
            else if (_modType == 69)
                VehicleBench.SetWindowTint(Utils.Utils.GetWindowTint(selected));
            else
                VehicleBench.SetMod(_modType, selected);
        }

        protected void InstallNeon(IPlayer client, double price)
        {
            if (VehicleBench == null || !VehicleBench.Exists)
            {
                MenuManager.CloseMenu(client);
                return;
            }

            VehicleHandler vh = VehicleBench.GetVehicleHandler();

            if (BankAccount.GetBankMoney(price, $"{SocietyName}: Néons"))
            {
                _neonColor = Color.FromArgb(_red * 17, _green * 17, _blue * 17);
                vh.NeonColor = _neonColor;
                vh.UpdateInBackground(false);

                client.SendNotificationSuccess($"Vous avez installé des Néons pour la somme de ${price}");
                OpenNeonsMenu(client);
            }
        }

        protected void ModChoice(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (VehicleBench == null || !VehicleBench.Exists)
            {
                MenuManager.CloseMenu(client);
                return;
            }

            byte selected = (byte)itemIndex;

            VehicleHandler vh = VehicleBench?.GetVehicleHandler();
            double price = menu.HasData("price") ? menu.GetData("price") : menuItem.GetData("price");

            string modName = string.Empty;

            // Bug with xenons
            if (_modType == 22)
                modName = vh.VehicleManifest.GetMod(_modType, selected).LocalizedName;
            else
                modName = vh.VehicleManifest.GetMod(_modType, selected - 1).LocalizedName;

            if (_modType == 11 && vh.VehicleManifest.GetMod(_modType, selected - 1).Name == "CMOD_ARM_0")
                modName = "Gestion moteur de série";

            bool hasMoney = true;

            if (price > 0)
                hasMoney = BankAccount.GetBankMoney(price, $"{SocietyName}: {modName}");

            if (hasMoney)
            {
                if (vh == null)
                {
                    MenuManager.CloseMenu(client);
                    return;
                }

                vh.Mods.AddOrUpdate(_modType, selected, (key, oldvalue) => selected);

                if (_modType != 69)
                    vh.WindowTint = Utils.Utils.GetWindowTint(selected);
                else
                    VehicleBench.SetMod(_modType, selected);

                vh.UpdateInBackground(false);
                string str = $"Vous avez installé {modName}";

                if (price != 0)
                    str += $" pour la somme de ${price}.";

                client.SendNotificationSuccess(str);
            }
            else
                client.SendNotificationError("Vous n'avez pas assez sur le compte de l'entreprise.");

            if (menu.Id == "ID_DesignChoice")
                OpenDesignChoiceMenu(client, itemIndex);
            else if (menu.Id == "ID_Perfs")
                OpenPerformanceChoiceMenu(client, itemIndex);
        }

        protected List<object> GetColorListItems()
        {
            List<object> items = new List<object>();

            for (int i = 0; i <= 15; i++)
                items.Add(i.ToString());

            return items;
        }

        protected void HornPreview(IVehicle vehicle, byte horn)
        {
            foreach (IPlayer client in vehicle.GetPlayersInRange(10f))
            {
                if (!client.Exists)
                    continue;

                client.EmitLocked("HornPreview", vehicle, horn - 1, true);
            }
        }

        protected void HornStop(IVehicle vehicle, byte horn)
        {
            foreach (IPlayer client in vehicle.GetPlayersInRange(58f))
            {
                if (!client.Exists)
                    continue;

                client.EmitLocked("HornPreview", vehicle, horn - 1, false);
            }
        }

        protected void Finalizer(IPlayer client, Menu menu)
        {
            ClientInMenu = null;
        }
        #endregion
    }
}
