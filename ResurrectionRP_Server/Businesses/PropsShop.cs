﻿
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using System.Numerics;
using System;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net.Enums;

namespace ResurrectionRP_Server.Businesses
{
    public class PropsStore : Business
    {
        #region Variables
        public Vector3 ClothingPos;

        [BsonIgnore]
        public IColShape ColthingColshape;

        public Banner BannerStyle;

        public int[] MenHats;
        public int[] GirlHats;

        public int[] MenGlasses;
        public int[] GirlGlasses;

        public int[] MenEars;
        public int[] GirlEars;

        public int[] MenWatches;
        public int[] GirlWatches;

        public int[] MenBracelets;
        public int[] GirlBracelets;
        #endregion

        #region Ctor
        public PropsStore(string businnessName, Models.Location location, uint blipSprite, int inventoryMax, Vector3 clothingPos, PedModel pedhash = 0, string owner = null, int bannerStyle = 0) : base(businnessName, location, blipSprite, inventoryMax, pedhash, owner)
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
            this.MaxEmployee = 5;
            await base.Init();
            await AltAsync.Do(async () =>
            {
                ColthingColshape = Alt.CreateColShapeCylinder(ClothingPos, 4f, 3f);
                ColthingColshape.SetData("ClothingID", this._id);
                GameMode.Instance.Streamer.addEntityMarker(Streamer.Data.MarkerType.VerticalCylinder, ClothingPos - new Vector3(0,0,4f), new Vector3(0,0,3f), 80, 255, 255, 255);
                //MP.Markers.New(MarkerType.VerticalCylinder, ClothingPos - new Vector3(0, 0, 4f), new Vector3(0, 0, 180), new Vector3(), 3f, Color.FromArgb(80, 255, 255, 255), true, MP.GlobalDimension);
                
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

            await OpenPropsStoreMenu(client);
        }

        public override void OnPlayerEnterColShape(IColShape colShape, IPlayer client)
        {
            if (ColthingColshape == null)
                return;

            base.OnPlayerEnterColShape(colShape, client);
        }

        private async Task MenuClose(IPlayer client, Menu menu)
        {
            var ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            await ph.Clothing.UpdatePlayerClothing();
        }
        #endregion

        #region MainMenu
        public async Task OpenPropsStoreMenu(IPlayer client)
        {
            Menu menu = new Menu("ClothingMenu", "", "", 0, 0, Menu.MenuAnchor.MiddleRight, false, true, true, BannerStyle);
            menu.ItemSelectCallback = MenuCallBack;
            menu.Finalizer = MenuClose;

            if (await client.GetModelAsync() == (uint)PedModel.FreemodeMale01)
            {
                if (MenHats != null && MenHats.Length > 0)
                    menu.Add(new MenuItem("Chapeaux", "", "ID_Hats", true));

                if (MenGlasses != null && MenGlasses.Length > 0)
                    menu.Add(new MenuItem("Lunettes", "", "ID_Glasses", true));

                if (MenEars != null && MenEars.Length > 0)
                    menu.Add(new MenuItem("Boucle Oreilles", "", "ID_Ears", true));

                if (MenWatches != null && MenWatches.Length > 0)
                    menu.Add(new MenuItem("Montres", "", "ID_Watches", true));

                if (MenBracelets != null && MenBracelets.Length > 0)
                    menu.Add(new MenuItem("Bracelets", "", "ID_Bracelets", true));
            }
            else if (await client.GetModelAsync() == (uint)PedModel.FreemodeFemale01)
            {
                if (GirlHats != null && GirlHats.Length > 0)
                    menu.Add(new MenuItem("Chapeaux", "", "ID_Hats", true));

                if (GirlGlasses != null && GirlGlasses.Length > 0)
                    menu.Add(new MenuItem("Lunettes", "", "ID_Glasses", true));

                if (GirlEars != null && GirlEars.Length > 0)
                    menu.Add(new MenuItem("Boucle Oreilles", "", "ID_Ears", true));

                if (GirlWatches != null && GirlWatches.Length > 0)
                    menu.Add(new MenuItem("Montres", "", "ID_Watches", true));

                if (GirlBracelets != null && GirlBracelets.Length > 0)
                    menu.Add(new MenuItem("Bracelets", "", "ID_Bracelets", true));
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
                case "ID_Hats":
                    await OpenCompomentMenuWithoutCat(client, menu, 0);
                    break;

                case "ID_Glasses":
                    await OpenCompomentMenuWithoutCat(client, menu, 1);
                    break;

                case "ID_Ears":
                    await OpenCompomentMenuWithoutCat(client, menu, 2);
                    break;

                case "ID_Watches":
                    await OpenCompomentMenuWithoutCat(client, menu, 6);
                    break;

                case "ID_Bracelets":
                    await OpenCompomentMenuWithoutCat(client, menu, 7);
                    break;
            }
        }
        #endregion

        #region WithoutCategorie
        private async Task OpenCompomentMenuWithoutCat(IPlayer client, Menu menu, byte compomentID)
        {
            var data = await Loader.ClothingLoader.FindProps(client, compomentID) ?? null;

            if (data == null)
                return;

            menu.ClearItems();
            menu.SubTitle = null;
            menu.BackCloseMenu = false;
            menu.ItemSelectCallback = OnCallBack;
            menu.IndexChangeCallback = OnCurrentItem;

            int[] compoList = null;

            switch (compomentID)
            {
                case 0:
                    compoList = (await client.GetModelAsync() == (uint)PedModel.FreemodeMale01) ? MenHats : GirlHats;
                    break;

                case 1:
                    compoList = (await client.GetModelAsync() == (uint)PedModel.FreemodeMale01) ? MenGlasses : GirlGlasses;
                    break;

                case 2:
                    compoList = (await client.GetModelAsync() == (uint)PedModel.FreemodeMale01) ? MenEars : GirlEars;
                    break;

                case 6:
                    compoList = (await client.GetModelAsync() == (uint)PedModel.FreemodeMale01) ? MenWatches : GirlWatches;
                    break;

                case 7:
                    compoList = (await client.GetModelAsync() == (uint)PedModel.FreemodeMale01) ? MenBracelets : GirlBracelets;
                    break;
            }

            if (compoList == null)
                return;

            foreach (int compo in compoList)
            {
                if (compo > 255 || !data.Value.DrawablesList.ContainsKey(compo))
                    continue; // RAGEMP super fix! ...

                Loader.ClothDrawable drawables = data.Value.DrawablesList[compo];
                int price = drawables.Price;

                foreach (KeyValuePair<byte, Loader.ClothVariation> pair in drawables.Variations)
                {
                    string name = drawables.Variations[pair.Key].Gxt;

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

            menu.SetData("compomentID", compomentID);

            await menu.OpenMenu(client);
            await OnCurrentItem(client, menu, 0, menu.Items[0]);
        }

        private async Task OnCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                await OpenPropsStoreMenu(client);
                return;
            }

            byte compomentID = menu.GetData("compomentID");
            int drawable = menuItem.GetData("drawable");
            int variation = menuItem.GetData("variation");
            double price = menuItem.GetData("price");

            Models.Item item = null;
            var clothdata = await Loader.ClothingLoader.FindProps(client, compomentID) ?? null;

            if (clothdata == null)
                return;

            switch (compomentID)
            {
                case 0:
                    item = new ClothItem(Models.InventoryData.ItemID.Hats, clothdata.Value.DrawablesList[drawable].Variations[(byte)variation].Gxt, "", new Models.ClothData((byte)drawable, (byte)variation, 0), 0, true, false, false, true, false, 0, classes: "cap", icon: "cap");
                    break;

                case 1:
                    item = new ClothItem(Models.InventoryData.ItemID.Glasses, clothdata.Value.DrawablesList[drawable].Variations[(byte)variation].Gxt, "", new Models.ClothData((byte)drawable, (byte)variation, 0), 0, true, false, false, true, false, 0, classes: "glasses", icon: "glasses");
                    break;

                case 2:
                    item = new ClothItem(Models.InventoryData.ItemID.Ears, clothdata.Value.DrawablesList[drawable].Variations[(byte)variation].Gxt, "", new Models.ClothData((byte)drawable, (byte)variation, 0), 0, true, false, false, true, false, 0, classes: "earring", icon: "earring");
                    break;

                case 6:
                    item = new ClothItem(Models.InventoryData.ItemID.Watch, clothdata.Value.DrawablesList[drawable].Variations[(byte)variation].Gxt, "", new Models.ClothData((byte)drawable, (byte)variation, 0), 0, true, false, false, true, false, 0, classes: "watch", icon: "watch");
                    break;

                case 7:
                    item = new ClothItem(Models.InventoryData.ItemID.Bracelet, clothdata.Value.DrawablesList[drawable].Variations[(byte)variation].Gxt, "", new Models.ClothData((byte)drawable, (byte)variation, 0), 0, true, false, false, true, false, 0, classes: "bracelet", icon: "bracelet");
                    break;
            }

            var ph = client.GetPlayerHandler();

            if (ph == null)
                return;


            if (await ph.HasBankMoney(price, $"Achat vêtement {menuItem.Text}"))
            {
                if (await ph.AddItem(item, 1))
                {
                    await client.SendNotificationSuccess($"Vous avez acheté {menuItem.Text} pour la somme de ${price}");
                    await ph.Clothing.UpdatePlayerClothing();
                    await ph.Update();
                }
                else
                    await client.SendNotificationError("Vous n'avez pas la place pour cette élément.");
            }
            else
                await client.SendNotificationError("Vous n'avez pas assez d'argent sur votre compte en banque.");
        }
        #endregion

        private async Task OnCurrentItem(IPlayer client, Menu menu, int itemIndex, IMenuItem menuItem)
        {
            try
            {
                byte compomentID = menu.GetData("compomentID");
                int drawable = menuItem.GetData("drawable");
                int variation = menuItem.GetData("variation");

                // Fix for preview not working everytime
                await Task.Delay(10);
                await client.EmitAsync("PropVariation", compomentID, drawable, variation);
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("OnCurrentItem" + ex);
            }
        }
    }
}
