﻿using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.EventHandlers;
using ResurrectionRP_Server.Loader;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Utils;
using System.Drawing;

namespace ResurrectionRP_Server.Business
{
    public class ClothingStore : Business
    {
        #region Fields
        public Vector3 ClothingPos;

        [BsonIgnore]
        private IColShape _clothingColShape;

        public Banner BannerStyle;

        #region All
        public int[] Mask;
        public int[] BackPack;
        #endregion

        #region Men
        public int[] MenLegs;
        public int[] MenFeet;
        public int[] MenGlove;
        public int[] MenUnderShirt;
        public int[] MenTops;
        #endregion

        #region Girl
        public int[] GirlLegs;
        public int[] GirlFeet;
        public int[] GirlGlove;
        public int[] GirlUnderShirt;
        public int[] GirlTops;
        #endregion

        public int[] MenGlasses;
        public int[] Glasses;

        public int[] MenAccessories;
        public int[] GirlAccessories;

        public int[] MenHats;
        public int[] GirlHats;
        #endregion

        #region Constructor
        public ClothingStore(string businnessName, Location location, uint blipSprite, int inventoryMax, Vector3 clothingPos, PedModel pedhash = 0, string owner = null, int bannerStyle = 0) : base(businnessName, location, blipSprite, inventoryMax, pedhash, owner)
        {
            ClothingPos = clothingPos;

            switch (bannerStyle)
            {
                case 0:
                    BannerStyle = Banner.LowFashion;
                    break;

                case 1:
                    BannerStyle = Banner.LowFashion2;
                    break;

                case 2:
                    BannerStyle = Banner.MidFashion;
                    break;

                case 3:
                    BannerStyle = Banner.HighFashion;
                    break;
            }
        }
        #endregion

        #region Events
        public override async Task Init()
        {
            await base.Init();

            _clothingColShape = Alt.CreateColShapeCylinder(ClothingPos - new Vector3(0, 0, 1), 4f, 3f);
            Marker.CreateMarker(MarkerType.VerticalCylinder, ClothingPos - new Vector3(0, 0, 4f), new Vector3(0, 0, 3f), Color.FromArgb(80, 255, 255, 255));
            Entities.Blips.BlipsManager.SetColor(Blip, 25);

            _clothingColShape.SetOnPlayerEnterColShape(OnPlayerEnterColShape);
            _clothingColShape.SetOnPlayerLeaveColShape(OnPlayerLeaveColShape);
            _clothingColShape.SetOnPlayerInteractInColShape(OnPlayerInteractInColShape);
        }

        public Task OnPlayerEnterColShape(IColShape colShape, IPlayer client)
        {
            client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~ pour intéragir", 5000);
            return Task.CompletedTask;
        }

        public virtual async Task OnPlayerLeaveColShape(IColShape colShape, IPlayer client)
        {
            PlayerHandler player = client.GetPlayerHandler();

            if (player == null)
                return;

            if (player.HasOpenMenu())
                await MenuManager.CloseMenu(client);
        }

        private async Task OnPlayerInteractInColShape(IColShape colShape, IPlayer client)
        {
            if (!client.Exists)
                return;

            await OpenClothingMenu(client);
        }

        public override Task OpenMenu(IPlayer client, Entities.Peds.Ped npc)
        {
            return base.OpenMenu(client, npc);
        }

        public override async Task<Menu> OpenSellMenu(IPlayer client, Menu menu)
        {
            return await base.OpenSellMenu(client, menu);
        }

        private Task MenuClose(IPlayer client, Menu menu)
        {
            var ph = client.GetPlayerHandler();

            if (ph == null)
                return Task.CompletedTask;

            ph.Clothing.UpdatePlayerClothing();

            return Task.CompletedTask;
        }
        #endregion

        #region Private methods
        private async Task BuyCloth(IPlayer client, byte componentID, int drawable, int variation, double price, string clothName)
        {
            ClothItem item = null;
            ClothManifest? clothdata = ClothingLoader.FindCloths(client, componentID) ?? null;

            if (clothdata == null)
                return;

            switch (componentID)
            {
                case 1:
                    item = new ClothItem(ItemID.Mask, clothdata.Value.DrawablesList[drawable].Variations[variation].Gxt, "", new ClothData(drawable, variation, 0), 0, true, false, false, true, false, 0, classes: "mask", icon: "mask");
                    break;

                case 2:
                    break;

                case 3:
                    break;

                case 4:
                    item = new ClothItem(ItemID.Pant, clothdata.Value.DrawablesList[drawable].Variations[variation].Gxt, "", new ClothData(drawable, variation, 0), 0, true, false, false, true, false, 0, classes: "pants", icon: "pants");
                    break;

                case 5:
                    break;

                case 6:
                    item = new ClothItem(ItemID.Shoes, clothdata.Value.DrawablesList[drawable].Variations[variation].Gxt, "", new ClothData(drawable, variation, 0), 0, true, false, false, true, false, 0, classes: "shoes", icon: "shoes");
                    break;

                case 7:
                    // item = new ClothItem(ItemID., clothdata.Value.DrawablesList[drawable].Variations[(byte)variation].Gxt, "", new ClothData(drawable, variation, 0), 0, true, false, false, true, false, 0, classes: "", icon: "");
                    break;

                case 8:
                    item = new ClothItem(ItemID.TShirt, clothdata.Value.DrawablesList[drawable].Variations[variation].Gxt, "", new ClothData(drawable, variation, 0), 0, true, false, false, true, false, 0, classes: "shirt", icon: "shirt");
                    break;
            }

            await BuyCloth(client, item, price, clothName);
        }

        private async Task BuyCloth(IPlayer client, ClothItem item, double price, string clothName)
        {
            var ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            if (ph.BankAccount.Balance >= price)
            {
                if (await ph.AddItem(item, 1))
                {
                    if (await ph.HasBankMoney(price, $"Achat vêtement {clothName}"))
                    {
                        client.SendNotificationSuccess($"Vous avez acheté le vêtement {clothName} pour la somme de ${price}");
                        ph.Update();
                    }
                }
                else
                    client.SendNotificationError("Vous n'avez pas la place pour cette élément.");
            }
            else
                client.SendNotificationError("Vous n'avez pas assez d'argent sur votre compte en banque.");
        }
        #endregion

        #region Menu
        public async Task OpenClothingMenu(IPlayer client)
        {
            Menu menu = new Menu("ClothingMenu", "", "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true, BannerStyle);
            menu.ItemSelectCallback = MenuCallBack;
            menu.Finalizer = MenuClose;

            if (Mask != null && Mask.Length > 0)
            {
                await OpenComponentMenuWithoutCat(client, menu, 1, true);
                return;
            }

            if (client.Model == (uint)PedModel.FreemodeMale01)
            {
                if (MenTops != null && MenTops.Length > 0)
                    menu.Add(new MenuItem("Haut", "", "ID_Haut", true));

                if (MenUnderShirt != null && MenUnderShirt.Length > 0)
                    menu.Add(new MenuItem("T-Shirt", "", "ID_TShirt", true));

                if (MenLegs != null && MenLegs.Length > 0)
                    menu.Add(new MenuItem("Pantalon", "", "ID_Pantalon", true));

                if (MenFeet != null && MenFeet.Length > 0)
                    menu.Add(new MenuItem("Chaussure", "", "ID_Chaussure", true));

                if (MenAccessories != null && MenAccessories.Length > 0)
                    menu.Add(new MenuItem("Accessoire", "", "ID_Accessoire", true));
            }
            else if (client.Model == (uint)PedModel.FreemodeFemale01)
            {
                if (GirlTops != null && GirlTops.Length > 0)
                    menu.Add(new MenuItem("Haut", "", "ID_Haut", true));

                if (GirlUnderShirt != null && GirlUnderShirt.Length > 0)
                    menu.Add(new MenuItem("T-Shirt", "", "ID_TShirt", true));

                if (GirlLegs != null && GirlLegs.Length > 0)
                    menu.Add(new MenuItem("Pantalon", "", "ID_Pantalon", true));

                if (GirlFeet != null && GirlFeet.Length > 0)
                    menu.Add(new MenuItem("Chaussure", "", "ID_Chaussure", true));

                if (GirlAccessories != null && GirlAccessories.Length > 0)
                    menu.Add(new MenuItem("Accessoire", "", "ID_Accessoire", true));
            }
            else
                client.SendNotificationError("Vous n'êtes pas autorisé à utiliser la boutique de vêtements avec ce skin.");

            await menu.OpenMenu(client);
        }

        private async Task MenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            switch (menuItem.Id)
            {
                case "ID_Haut":
                    await OpenHautMenu(client, menu);
                    break;

                case "ID_Pantalon":
                    await OpenComponentMenu(client, menu, 4);
                    break;

                case "ID_TShirt":
                    await OpenComponentMenuWithoutCat(client, menu, 8, false);
                    break;

                case "ID_Chaussure":
                    await OpenComponentMenu(client, menu, 6);
                    break;

                case "ID_Accessoire":
                    await OpenComponentMenuWithoutCat(client, menu, 7, false);
                    break;
            }
        }
        #endregion

        #region Tops
        public async Task OpenHautMenu(IPlayer client, Menu menu)
        {
            menu.ClearItems();
            menu.BackCloseMenu = false;
            menu.SubTitle = null;
            var data = ClothingLoader.FindTops(client) ?? null;

            if (data == null)
                return;

            menu.ItemSelectCallback = OnTopsCategorieCallBack;
            menu.IndexChangeCallback = null;

            var compoList = (client.Model == (uint)PedModel.FreemodeMale01) ? MenTops : GirlTops;

            foreach (var compo in compoList)
            {
                if (compo == 220 || compo == 218 || compo == 157) // Les vêtements de la Milice sont blacklisteds + add vetement biker le cuir
                    continue;

                var drawables = data.Value.DrawablesList[compo];
                var price = drawables.Price / 3;

                for (int b = 0; b < drawables.Variations.Count; b++)
                {
                    if (!menu.Items.Exists(p => p.Text == drawables.Categorie))
                    {
                        if (drawables.Categorie == "NULL")
                            continue;

                        var ui = new MenuItem(drawables.Categorie, executeCallback: true);
                        menu.Add(ui);
                    }
                }
            }

            await menu.OpenMenu(client);
        }

        private async Task OnTopsCategorieCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                await OpenClothingMenu(client);
                return;
            }

            menu.ClearItems();
            var compoList = (client.Model == (uint)PedModel.FreemodeMale01) ? MenTops : GirlTops;
            menu.SubTitle = menuItem.Text.ToUpper();
            menu.ItemSelectCallback = OnTopsCallBack;
            menu.IndexChangeCallback = PreviewTopsItem;

            ClothManifest? clothdata = ClothingLoader.FindTops(client);

            if (clothdata == null)
                return;

            // compoList c'est l'int array correspondant aux items d'un certains type (short, pants ... ) disponible en boutique.
            foreach (var drawable in compoList)
            {
                var drawables = clothdata.Value.DrawablesList[drawable]; // recup des données sur ces tenues

                var price = (drawables.Price / 3);

                if (drawables.Categorie != menuItem.Text)
                    continue;

                for (int b = 0; b < drawables.Variations.Count; b++)
                {
                    var variation = drawables.Variations[b];

                    if (variation.Gxt == "NULL")
                        continue;

                    var ui = new MenuItem(variation.Gxt, executeCallback: true);
                    ui.RightLabel = $"${price}";
                    ui.SetData("drawable", drawable);
                    ui.SetData("variation", b);
                    ui.SetData("price", price);
                    ui.SetData("name", variation.Gxt);
                    ui.SetData("torso", drawables.Torso[0]);
                    menu.Add(ui);
                }
            }

            await menu.OpenMenu(client);
            await PreviewTopsItem(client, menu, 0, menu.Items[0]);
        }

        private Task PreviewTopsItem(IPlayer client, Menu menu, int itemIndex, IMenuItem menuItem)
        {
            int drawable = (int)menuItem.GetData("drawable");
            int variation = (int)menuItem.GetData("variation");
            int torso = (int)menuItem.GetData("torso");

            client.SetCloth(ClothSlot.Tops, drawable, variation, 0);
            client.SetCloth(ClothSlot.Torso, torso, 0, 0);
            return Task.CompletedTask;
        }

        private async Task OnTopsCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                await OpenHautMenu(client, menu);
                return;
            }

            int drawable = menuItem.GetData("drawable");
            int variation = menuItem.GetData("variation");
            double price = menuItem.GetData("price");

            ClothItem item = new ClothItem(ItemID.Jacket, menuItem.Text, "", new ClothData(drawable, variation, 0), 0.2, true, false, false, true, false, classes: "jacket", icon: "jacket");
            await BuyCloth(client, item, price, menuItem.Text);
        }
        #endregion

        #region WithCategorie
        public async Task OpenComponentMenu(IPlayer client, Menu menu, byte componentID)
        {
            ClothManifest? data = ClothingLoader.FindCloths(client, componentID) ?? null;

            if (data == null)
                return;

            menu.ClearItems();
            menu.SubTitle = null;
            menu.BackCloseMenu = false;
            menu.ItemSelectCallback = CategorieCallBack;
            menu.IndexChangeCallback = null;

            int[] compoList = null;

            switch (componentID)
            {
                case 1:
                    compoList = Mask;
                    break;

                case 3:
                    compoList = (client.Model == (uint)PedModel.FreemodeMale01) ? MenGlove : GirlGlove;
                    break;

                case 4:
                    compoList = (client.Model == (uint)PedModel.FreemodeMale01) ? MenLegs : GirlLegs;
                    break;

                case 5:
                    compoList = BackPack;
                    break;

                case 6:
                    compoList = (client.Model == (uint)PedModel.FreemodeMale01) ? MenFeet : GirlFeet;
                    break;

                case 11:
                    compoList = MenTops;
                    break;
            }

            if (compoList == null)
                return;

            foreach (int compo in compoList)
            {
                ClothDrawable drawables = data.Value.DrawablesList[compo];
                int price = drawables.Price / 3;

                for (int b = 0; b < drawables.Variations.Count; b++)
                {
                    if (!menu.Items.Exists(p => p.Text == drawables.Categorie))
                    {
                        if (drawables.Categorie == null || drawables.Categorie == "NULL")
                            continue;

                        MenuItem ui = new MenuItem(drawables.Categorie, executeCallback: true);
                        menu.Add(ui);
                    }
                }
            }

            menu.SetData("componentID", componentID);
            menu.SetData("Categorie", compoList);
            await menu.OpenMenu(client);
        }

        private async Task CategorieCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                await OpenClothingMenu(client);
                return;
            }

            menu.ClearItems();

            int[] compoList = menu.GetData("Categorie");
            menu.SubTitle = menuItem.Text.ToUpper();
            menu.BackCloseMenu = false;
            menu.ItemSelectCallback = OnCallBackWithCat;
            menu.IndexChangeCallback = OnCurrentItem;

            byte componentID = menu.GetData("componentID");

            ClothManifest? clothdata = ClothingLoader.FindCloths(client, componentID);

            // compoList c'est l'int array correspondant aux items d'un certains type (short, pants ... ) disponible en boutique.
            foreach (int drawable in compoList)
            {
                if (!clothdata.Value.DrawablesList.ContainsKey(drawable))
                    continue; 

                ClothDrawable drawables = clothdata.Value.DrawablesList[drawable]; // recup des données sur ces tenues
                int price = drawables.Price / 3;

                if (drawables.Categorie != menuItem.Text)
                    continue;

                foreach (KeyValuePair<int, ClothVariation> pair in drawables.Variations)
                {
                    ClothVariation variation = pair.Value;

                    if (variation.Gxt == "NULL" || variation.Gxt == null)
                        continue;

                    MenuItem ui = new MenuItem(variation.Gxt, executeCallback: true);
                    ui.RightLabel = $"${price}";
                    ui.SetData("drawable", drawable);
                    ui.SetData("variation", pair.Key);
                    ui.SetData("price", price);
                    menu.Add(ui);
                }
            }

            await menu.OpenMenu(client);
            await OnCurrentItem(client, menu, 0, menu.Items[0]);
        }

        private async Task OnCallBackWithCat(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                byte compID = menu.GetData("componentID");
                await OpenComponentMenu(client, menu, compID);
                return;
            }

            byte componentID = menu.GetData("componentID");
            int drawable = menuItem.GetData("drawable");
            int variation = menuItem.GetData("variation");
            double price = menuItem.GetData("price");
            string clothName = menuItem.Text;
            await BuyCloth(client, componentID, drawable, variation, price, clothName);
        }

        private Task OnCurrentItem(IPlayer client, Menu menu, int itemIndex, IMenuItem menuItem)
        {
            try
            {
                byte componentID = menu.GetData("componentID");
                int drawable = menuItem.GetData("drawable");
                int variation = menuItem.GetData("variation");
                client.SetCloth((ClothSlot)componentID, drawable, variation, 0);
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("OnCurrentItem" + ex);
            }

            return Task.CompletedTask;
        }
        #endregion

        #region WithoutCategorie
        private async Task OpenComponentMenuWithoutCat(IPlayer client, Menu menu, byte componentID, bool backCloseMenu)
        {
            ClothManifest? data = ClothingLoader.FindCloths(client, componentID) ?? null;

            if (data == null)
                return;

            menu.ClearItems();
            menu.SubTitle = null;
            menu.BackCloseMenu = backCloseMenu;
            menu.ItemSelectCallback = OnCallBackWithoutCat;
            menu.IndexChangeCallback = OnCurrentItem;

            int[] compoList = null;

            switch (componentID)
            {
                case 1:
                    compoList = Mask;
                    break;

                case 7:
                    compoList = (client.Model == (uint)PedModel.FreemodeMale01) ? MenAccessories : GirlAccessories;
                    break;

                case 8:
                    compoList = (client.Model == (uint)PedModel.FreemodeMale01) ? MenUnderShirt : GirlUnderShirt;
                    break;
            }

            if (compoList == null)
                return;

            foreach (int compo in compoList)
            {
                if (!data.Value.DrawablesList.ContainsKey(compo))
                    continue;

                ClothDrawable drawables = data.Value.DrawablesList[compo];
                int price = drawables.Price;

                foreach (KeyValuePair<int, ClothVariation> pair in drawables.Variations)
                {
                    string name = drawables.Variations[pair.Key].Gxt;

                    if (name == "NULL")
                        continue;

                    MenuItem ui = new MenuItem(drawables.Variations[pair.Key].Gxt, executeCallback: true);
                    ui.RightLabel = $"${price}";
                    ui.SetData("drawable", compo);
                    ui.SetData("variation", pair.Key);
                    ui.SetData("price", price);
                    menu.Add(ui);
                }
            }

            menu.SetData("componentID", componentID);
            await menu.OpenMenu(client);
            await OnCurrentItem(client, menu, 0, menu.Items[0]);
        }

        private async Task OnCallBackWithoutCat(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                await OpenClothingMenu(client);
                return;
            }

            byte componentID = menu.GetData("componentID");
            int drawable = menuItem.GetData("drawable");
            int variation = menuItem.GetData("variation");
            double price = menuItem.GetData("price");
            string clothName = menuItem.Text;

            await BuyCloth(client, componentID, drawable, variation, price, clothName);
        }
        #endregion
    }
}
