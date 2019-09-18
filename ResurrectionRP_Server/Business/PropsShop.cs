
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Business
{
    public class PropsStore : Business
    {
        #region Variables
        public Vector3 ClothingPos;

        [BsonIgnore]
        private IColShape _clothingColshape;

        private Banner BannerStyle;

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
            _clothingColshape = Alt.CreateColShapeCylinder(ClothingPos, 4f, 3f);
            _clothingColshape.SetData("ClothingID", this._id);
            Marker.CreateMarker(MarkerType.VerticalCylinder, ClothingPos - new Vector3(0, 0, 4f), new Vector3(0, 0, 3f), Color.FromArgb(80, 255, 255, 255));
            //MP.Markers.New(MarkerType.VerticalCylinder, ClothingPos - new Vector3(0, 0, 4f), new Vector3(0, 0, 180), new Vector3(), 3f, Color.FromArgb(80, 255, 255, 255), true, MP.GlobalDimension);

            Entities.Blips.BlipsManager.SetColor(Blip, 25);

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

        public void OnPlayerEnterColShape(IColShape colShape, IPlayer client)
        {
            if (_clothingColshape == null)
                return;
        }

        public virtual async Task OnPlayerLeaveColShape(IColShape colShape, IPlayer client)
        {
            PlayerHandler player = client.GetPlayerHandler();

            if (player == null)
                return;

            if (player.HasOpenMenu())
                await MenuManager.CloseMenu(client);
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

        #region MainMenu
        public async Task OpenPropsStoreMenu(IPlayer client)
        {
            Menu menu = new Menu("ClothingMenu", "", "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true, BannerStyle);
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
                client.SendNotificationError("Vous n'êtes pas autorisé à utiliser la boutique de vêtements avec ce skin.");
            }


            await menu.OpenMenu(client);
        }

        private async Task MenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            switch (menuItem.Id)
            {
                case "ID_Hats":
                    await OpenComponentMenuWithoutCat(client, menu, 0);
                    break;

                case "ID_Glasses":
                    await OpenComponentMenuWithoutCat(client, menu, 1);
                    break;

                case "ID_Ears":
                    await OpenComponentMenuWithoutCat(client, menu, 2);
                    break;

                case "ID_Watches":
                    await OpenComponentMenuWithoutCat(client, menu, 6);
                    break;

                case "ID_Bracelets":
                    await OpenComponentMenuWithoutCat(client, menu, 7);
                    break;
            }
        }
        #endregion

        #region WithoutCategorie
        private async Task OpenComponentMenuWithoutCat(IPlayer client, Menu menu, byte compomentID)
        {
            var data = Loader.ClothingLoader.FindProps(client, compomentID) ?? null;

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

                foreach (KeyValuePair<int, Loader.ClothVariation> pair in drawables.Variations)
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
            var clothdata = Loader.ClothingLoader.FindProps(client, compomentID) ?? null;

            if (clothdata == null)
                return;

            switch (compomentID)
            {
                case 0:
                    item = new ClothItem(Models.InventoryData.ItemID.Hats, clothdata.Value.DrawablesList[drawable].Variations[variation].Gxt, "", new Models.ClothData(drawable, variation, 0), 0, true, false, false, true, false, 0, classes: "cap", icon: "cap");
                    break;

                case 1:
                    item = new ClothItem(Models.InventoryData.ItemID.Glasses, clothdata.Value.DrawablesList[drawable].Variations[variation].Gxt, "", new Models.ClothData(drawable, variation, 0), 0, true, false, false, true, false, 0, classes: "glasses", icon: "glasses");
                    break;

                case 2:
                    item = new ClothItem(Models.InventoryData.ItemID.Ears, clothdata.Value.DrawablesList[drawable].Variations[variation].Gxt, "", new Models.ClothData(drawable, variation, 0), 0, true, false, false, true, false, 0, classes: "earring", icon: "earring");
                    break;

                case 6:
                    item = new ClothItem(Models.InventoryData.ItemID.Watch, clothdata.Value.DrawablesList[drawable].Variations[variation].Gxt, "", new Models.ClothData(drawable, variation, 0), 0, true, false, false, true, false, 0, classes: "watch", icon: "watch");
                    break;

                case 7:
                    item = new ClothItem(Models.InventoryData.ItemID.Bracelet, clothdata.Value.DrawablesList[drawable].Variations[variation].Gxt, "", new Models.ClothData(drawable, variation, 0), 0, true, false, false, true, false, 0, classes: "bracelet", icon: "bracelet");
                    break;
            }

            var ph = client.GetPlayerHandler();

            if (ph == null)
                return;


            if (await ph.HasBankMoney(price, $"Achat vêtement {menuItem.Text}"))
            {
                if (await ph.AddItem(item, 1))
                {
                    client.SendNotificationSuccess($"Vous avez acheté {menuItem.Text} pour la somme de ${price}");
                    ph.Update();
                }
                else
                    client.SendNotificationError("Vous n'avez pas la place pour cette élément.");
            }
            else
                client.SendNotificationError("Vous n'avez pas assez d'argent sur votre compte en banque.");
        }
        #endregion

        private Task OnCurrentItem(IPlayer client, Menu menu, int itemIndex, IMenuItem menuItem)
        {
            try
            {
                byte compomentID = menu.GetData("compomentID");
                int drawable = menuItem.GetData("drawable");
                int variation = menuItem.GetData("variation");
                client.SetProp((PropSlot)compomentID, new PropData(drawable, variation));
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("OnCurrentItem" + ex);
            }
            return Task.CompletedTask;
        }
    }
}
