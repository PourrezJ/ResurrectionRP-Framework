using System;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Loader;


namespace ResurrectionRP_Server.Businesses
{
    public class ClothingStore : Business
    {
        #region Variables
        public Vector3 ClothingPos;

        [BsonIgnore]
        public IColShape ColthingColshape;

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

        #region Ctor
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
            await AltAsync.Do(async () =>
            {
                ColthingColshape = Alt.CreateColShapeCylinder(ClothingPos, 4f, 3f);
                ColthingColshape.SetData("ClothingID", this._id);
                GameMode.Instance.Streamer.addEntityMarker(Streamer.Data.MarkerType.VerticalCylinder, ClothingPos - new Vector3(0, 0, 4f), new Vector3(0, 0, 3f), 80, 255, 255, 255);
                await Entities.Blips.BlipsManager.SetColor(Blip, 25);
            });


            AltAsync.OnClient("ClothingID_Open", ClothingID_Open);
        }

        private async Task ClothingID_Open(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            if (args[0] == null)
                return;

            if (args[0].ToString() != this._id.ToString())
                return;

            await OpenClothingMenu(client);
        }

        public override void OnPlayerEnterColShape(IColShape colShape, IPlayer client)
        {
            if (ColthingColshape == null)
                return;

             base.OnPlayerEnterColShape(colShape, client);
        }

        public override Task OpenMenu(IPlayer client, Entities.Peds.Ped npc)
        {
            return base.OpenMenu(client, npc);
        }

        public override async Task<Menu> OpenSellMenu(IPlayer client, Menu menu)
        {
            return await base.OpenSellMenu(client, menu);
        }

        private async Task MenuClose(IPlayer client, Menu menu)
        {
            var ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            await ph.Clothing.UpdatePlayerClothing();
        }
        #endregion

        #region Private methods
        private async void BuyCloth(IPlayer client, byte componentID, int drawable, int variation, double price, string clothName)
        {
            Item item = null;
            var clothdata = await ClothingLoader.FindCloths(client, componentID) ?? null;

            if (clothdata == null)
                return;

            switch (componentID)
            {
                case 1:
                    item = new ClothItem(ItemID.Mask, clothdata.Value.DrawablesList[drawable].Variations[(byte)variation].Gxt, "", new ClothData((byte)drawable, (byte)variation, 0), 0, true, false, false, true, false, 0, classes: "mask", icon: "mask");
                    break;

                case 2:
                    break;

                case 3:
                    break;

                case 4:
                    item = new ClothItem(ItemID.Pant, clothdata.Value.DrawablesList[drawable].Variations[(byte)variation].Gxt, "", new ClothData((byte)drawable, (byte)variation, 0), 0, true, false, false, true, false, 0, classes: "pants", icon: "pants");
                    break;

                case 5:
                    break;

                case 6:
                    item = new ClothItem(ItemID.Shoes, clothdata.Value.DrawablesList[drawable].Variations[(byte)variation].Gxt, "", new ClothData((byte)drawable, (byte)variation, 0), 0, true, false, false, true, false, 0, classes: "shoes", icon: "shoes");
                    break;

                case 7:
                    break;

                case 8:
                    item = new ClothItem(ItemID.TShirt, clothdata.Value.DrawablesList[drawable].Variations[(byte)variation].Gxt, "", new ClothData((byte)drawable, (byte)variation, 0), 0, true, false, false, true, false, 0, classes: "shirt", icon: "shirt");
                    break;
            }

            var ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            if (await ph.HasBankMoney(price, $"Achat vêtement {clothName}"))
            {
                if (await ph.AddItem(item, 1))
                {
                    await client.SendNotificationSuccess($"Vous avez acheté le vêtement {clothName} pour la somme de ${price}");
                    // await ph.Clothing.UpdatePlayerClothing();
                    await ph.Update();
                }
                else
                    await client.SendNotificationError("Vous n'avez pas la place pour cette élément.");
            }
            else
                await client.SendNotificationError("Vous n'avez pas assez d'argent sur votre compte en banque.");
        }
        #endregion

        #region Menu
        public async Task OpenClothingMenu(IPlayer client)
        {
            Menu menu = new Menu("ClothingMenu", "", "", 0, 0, Menu.MenuAnchor.MiddleRight, false, true, true, BannerStyle);
            menu.ItemSelectCallback = MenuCallBack;
            menu.Finalizer = MenuClose;
            //menu.Add(new MenuItem("Chapeau", "", "ID_Chapeau"));

            if (Mask != null && Mask.Length > 0)
            {
                await OpenComponentMenuWithoutCat(client, menu, 1, true);
                return;
            }

            if (await client.GetModelAsync() == (uint)PedModel.FreemodeMale01)
            {
                if (MenTops != null && MenTops.Length > 0)
                    menu.Add(new MenuItem("Haut", "", "ID_Haut", true));

                if (MenUnderShirt != null && MenUnderShirt.Length > 0)
                    menu.Add(new MenuItem("T-Shirt", "", "ID_TShirt", true));

                if (MenLegs != null && MenLegs.Length > 0)
                    menu.Add(new MenuItem("Pantalon", "", "ID_Pantalon", true));

                if (MenFeet != null && MenFeet.Length > 0)
                    menu.Add(new MenuItem("Chaussure", "", "ID_Chaussure", true));
            }
            else if (await client.GetModelAsync() == (uint)PedModel.FreemodeFemale01)
            {
                if (GirlTops != null && GirlTops.Length > 0)
                    menu.Add(new MenuItem("Haut", "", "ID_Haut", true));

                if (GirlUnderShirt != null && GirlUnderShirt.Length > 0)
                    menu.Add(new MenuItem("T-Shirt", "", "ID_TShirt", true));

                if (GirlLegs != null && GirlLegs.Length > 0)
                    menu.Add(new MenuItem("Pantalon", "", "ID_Pantalon", true));
                if (GirlFeet != null && GirlFeet.Length > 0)
                    menu.Add(new MenuItem("Chaussure", "", "ID_Chaussure", true));
            }
            else
            {
                await client.SendNotificationError("Vous n'êtes pas autorisé à utiliser la boutique de vêtements avec ce skin.");
            }


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
            }
        }
        #endregion

        #region Tops
        public async Task OpenHautMenu(IPlayer client, Menu menu)
        {
            menu.ClearItems();
            menu.BackCloseMenu = false;
            menu.SubTitle = null;
            var data = await ClothingLoader.FindTops(client) ?? null;

            if (data == null)
                return;

            menu.ItemSelectCallback = OnTopsCategorieCallBack;
            //menu.CallbackOnIndexChange = false; ???
            menu.IndexChangeCallback = null;

            var compoList = (await client.GetModelAsync() == (uint)PedModel.FreemodeMale01) ? MenTops : GirlTops;

            foreach (var compo in compoList)
            {
                if (compo == 220 || compo == 218 || compo == 157) // Les vêtements de la Milice sont blacklisteds + add vetement biker le cuir
                    continue;

                var drawables = data.Value.DrawablesList[compo];

                var price = (drawables.Price / 3);

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
            var compoList = (await client.GetModelAsync() == (uint)PedModel.FreemodeMale01) ? MenTops : GirlTops;
            menu.SubTitle = menuItem.Text.ToUpper();
            menu.ItemSelectCallback = OnTopsCallBack;
            //menu.CallbackCurrentItem = true;
            menu.IndexChangeCallback = PreviewTopsItem;

            ClothManifest? clothdata = await ClothingLoader.FindTops(client);

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
                    var variation = drawables.Variations[(byte)b];

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

        private async Task PreviewTopsItem(IPlayer client, Menu menu, int itemIndex, IMenuItem menuItem)
        {
            int drawable = (int)menuItem.GetData("drawable");
            int variation = (int)menuItem.GetData("variation");
            int torso = (int)menuItem.GetData("torso");

            // Fix for preview not working everytime
            await Task.Delay(10);
            await client.EmitAsync("ComponentVariation", 11, drawable, variation, 0);
            await client.EmitAsync("ComponentVariation", 3, torso, 0, 0);
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

            var ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            if (await ph.HasBankMoney(price, $"Achat vêtement {menuItem.Text}"))
            {
                if (await ph.AddItem(new ClothItem(ItemID.Jacket, menuItem.Text, "", new ClothData((byte)drawable, (byte)variation, 0), 0.2, true, false, false, true, false, classes: "jacket", icon: "jacket"), 1))
                {
                    await client.SendNotificationSuccess($"Vous avez acheté le vêtement {menuItem.Text} pour la somme de ${price}");
                    // await ph.Clothing.UpdatePlayerClothing();
                    await ph.Update();
                }
                else
                    await client.SendNotificationError("Vous n'avez pas la place pour cette élément.");
            }
            else
                await client.SendNotificationError("Vous n'avez pas assez d'argent sur votre compte en banque.");
        }
        #endregion

        #region WithCategorie
        public async Task OpenComponentMenu(IPlayer client, Menu menu, byte componentID)
        {
            var data = await ClothingLoader.FindCloths(client, componentID) ?? null;

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
                    compoList = (await client.GetModelAsync() == (uint)PedModel.FreemodeMale01) ? MenGlove : GirlGlove;
                    break;

                case 4:
                    compoList = (await client.GetModelAsync() == (uint)PedModel.FreemodeMale01) ? MenLegs : GirlLegs;
                    break;

                case 5:
                    compoList = BackPack;
                    break;

                case 6:
                    compoList = (await client.GetModelAsync() == (uint)PedModel.FreemodeMale01) ? MenFeet : GirlFeet;
                    break;

                case 11:
                    compoList = MenTops;
                    break;

            }

            if (compoList == null)
                return;

            foreach (var compo in compoList)
            {
                var drawables = data.Value.DrawablesList[compo];

                var price = drawables.Price / 3;

                for (int b = 0; b < drawables.Variations.Count; b++)
                {
                    if (!menu.Items.Exists(p => p.Text == drawables.Categorie))
                    {
                        if (drawables.Categorie == null || drawables.Categorie == "NULL")
                            continue;

                        var ui = new MenuItem(drawables.Categorie, executeCallback: true);
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

            var componentID = menu.GetData("componentID");

            ClothManifest? clothdata = await ClothingLoader.FindCloths(client, componentID);

            // compoList c'est l'int array correspondant aux items d'un certains type (short, pants ... ) disponible en boutique.
            foreach (int drawable in compoList)
            {
                if (drawable > 255 || !clothdata.Value.DrawablesList.ContainsKey(drawable))
                    continue; // RAGEMP super fix! ... 

                ClothDrawable drawables = clothdata.Value.DrawablesList[drawable]; // recup des données sur ces tenues
                int price = drawables.Price / 3;

                if (drawables.Categorie != menuItem.Text)
                    continue;

                foreach (KeyValuePair<byte, ClothVariation> pair in drawables.Variations)
                {
                    ClothVariation variation = pair.Value;

                    if (variation.Gxt == "NULL" || variation.Gxt == null)
                        continue;

                    var ui = new MenuItem(variation.Gxt, executeCallback: true);
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

            BuyCloth(client, componentID, drawable, variation, price, clothName);
        }

        private async Task OnCurrentItem(IPlayer client, Menu menu, int itemIndex, IMenuItem menuItem)
        {
            try
            {
                byte componentID = menu.GetData("componentID");
                int drawable = menuItem.GetData("drawable");
                int variation = menuItem.GetData("variation");

                // Fix for preview not working everytime
                await Task.Delay(10);
                await client.EmitAsync("ComponentVariation", componentID, drawable, variation, 0);
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("OnCurrentItem" + ex);
            }
        }
        #endregion

        #region WithoutCategorie
        private async Task OpenComponentMenuWithoutCat(IPlayer client, Menu menu, byte componentID, bool backCloseMenu)
        {
            var data = await ClothingLoader.FindCloths(client, componentID) ?? null;

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

                case 8:
                    compoList = (await client.GetModelAsync() == (uint)PedModel.FreemodeMale01) ? MenUnderShirt : GirlUnderShirt;
                    break;
            }

            if (compoList == null)
                return;

            foreach (int compo in compoList)
            {
                if (compo > 255 || !data.Value.DrawablesList.ContainsKey(compo))
                    continue; // RAGEMP super fix! ...

                ClothDrawable drawables = data.Value.DrawablesList[compo];
                int price = drawables.Price;

                foreach (KeyValuePair<byte, ClothVariation> pair in drawables.Variations)
                {
                    var name = drawables.Variations[pair.Key].Gxt;

                    if (name == "NULL")
                        continue;

                    var ui = new MenuItem(drawables.Variations[pair.Key].Gxt, executeCallback: true);
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

            BuyCloth(client, componentID, drawable, variation, price, clothName);
        }
        #endregion
    }
}
