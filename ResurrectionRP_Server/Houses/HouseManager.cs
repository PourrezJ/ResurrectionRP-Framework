﻿using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using MongoDB.Driver;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Utils;
using ResurrectionRP_Server.Utils.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Houses
{
    public class HouseManager
    {
        #region Public fields
        public static Dictionary<IPlayer, House> AddParkingList = new Dictionary<IPlayer, House>();

        // settings
        public static List<House> Houses = new List<House>();

        private static Dictionary<IPlayer, House> ClientHouse = new Dictionary<IPlayer, House>();
        #endregion

        #region Constructor
        public HouseManager()
        {
            AltAsync.OnClient("OutHouse", OutHouse);
            AltAsync.OnClient("ParkingHouse", ParkingHouse);

            Utils.Utils.Delay((int)TimeSpan.FromMinutes(10).TotalMilliseconds, false, async () =>
            {
                foreach (var house in Houses)
                {
                    await house.Save();
                    await Task.Delay(100);
                }
            });

        }
        #endregion

        #region Methods

        public static bool SetIntoHouse(IPlayer client, House house) => ClientHouse.TryAdd(client, house);
        public static bool IsInHouse(IPlayer client) => ClientHouse.ContainsKey(client);
        public static House GetHouseWithID(int id) => Houses.Find(h => h.ID == id) ?? null;
        public static House GetHouse(IPlayer client)
        {
            if (IsInHouse(client)) return ClientHouse.GetValueOrDefault(client);
            return null;
        }
        public static bool RemoveClientHouse(IPlayer client) => ClientHouse.Remove(client);

        public async Task RemovePlayerFromHouseList(IPlayer player)
        {
            if (IsInHouse(player))
            {
                House house = GetHouse(player);
                if (house == null) return;
                if (RemoveClientHouse(player)) await house.RemovePlayer(player, false);                
            }
        }
        #endregion

        #region Events        
        public async Task LoadAllHouses()
        {
            Alt.Server.LogInfo($"Loadings houses ...");
            
            var housesList = await Database.MongoDB.GetCollectionSafe<House>("houses").AsQueryable().ToListAsync();
            
            for (int i = 0; i < housesList.Count; i++)
            {
                try
                {
                    var house = housesList[i];
                    if (house != null)
                    {
                        await house.Load();
                        Houses.Add(house);
                        await Task.Delay(20);
                    }
                }
                catch (Exception ex)
                {
                    Alt.Server.LogError($"Erreur sur maison id:{i}: " + ex.ToString());
                }
            }

            Alt.Server.LogInfo($"Loaded {Houses.Count} houses.");
        }

        private async Task ParkingHouse(IPlayer player, object[] args)
        {
            if (!player.Exists)
                return;

            House house = GetHouseWithID((int)args[0]);
            if (house != null)
            {
                if(house.Owner == player.GetSocialClub())
                {
                    await house.Parking.OpenParkingMenu(player, "", ((player.GetPlayerHandler()?.StaffRank > Utils.Enums.AdminRank.Player) ? house.ID.ToString() : ""), true);
                }
                else
                {
                    player.SendNotificationError("Vous n'êtes pas autorisé à utiliser ce parking.");
                }
            }
        }

        private async Task OutHouse(IPlayer player, object[] args)
        {
            if (!player.Exists)
                return;

            House house = GetHouseWithID((int)args[0]);
            if (house != null)
            {
                await house.RemovePlayer(player);
            }
        }

        public static async Task OpenHouseMenu(IPlayer player, House house)
        {
            if (!player.Exists)
                return;

            Menu menu = new Menu("House_PurchaseMenu", HouseTypes.HouseTypeList[house.Type].Name, "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true);
            menu.BannerColor = new MenuColor(0, 0, 0, 0);
            menu.SetData("House", house);
            menu.ItemSelectCallback = MenuCallBack;

            menu.SubTitle = (house.Locked) ? "La porte est ~r~fermée" : "La porte est ~g~ouverte";

            // MODO ONLY
            if (player.GetPlayerHandler()?.StaffRank >= AdminRank.Moderator)
            {
                menu.SubTitle = $"Propriétaire: {((house.Owner != null) ? house.Owner : "Aucun")}";

                if (house.Parking == null)
                    menu.Add(new MenuItem("~r~[MODO] Ajouter un parking", "", "ID_AddParking", executeCallback: true));
                else
                    menu.Add(new MenuItem("~r~[MODO] Supprimer le parking", "", "ID_RemoveParking", executeCallback: true));

                menu.Add(new MenuItem("~r~[MODO] Rentrer dans la maison", "", "ID_Enter", executeCallback: true));

                MenuItem _type = new MenuItem("~r~[MODO] Changer le type de logement", "", "ID_Change", executeCallback: true);
                _type.SetInput("", 30, InputType.Text);
                menu.Add(_type);

                menu.Add(new MenuItem("~r~[MODO] Supprimer le logement", "", "ID_Delete", executeCallback: true));

                MenuItem _money = new MenuItem("~r~[MODO] Changer le prix", "", "ID_PriceChange", executeCallback: true);
                _money.SetInput("", 10, InputType.UNumber);
                menu.Add(_money);

                MenuItem _proprio = new MenuItem("~r~[MODO] Changer le proprio", "Changer le social club ou laisser vide pour mettre en vente.", "ID_ChangeProprio", executeCallback: true);
                _proprio.SetInput("", 30, InputType.Text);
                menu.Add(_proprio);
            }

            if (string.IsNullOrEmpty(house.Owner)) // Pas de proprio
            {
                menu.Add(new MenuItem("Acheter", $"Voulez-vous acheter une maison {HouseTypes.HouseTypeList[house.Type].Name} avec {HouseTypes.HouseTypeList[house.Type].InventorySize} de place.", "ID_Acheter", executeCallback: true, rightLabel: $"${house.Price}"));
            }
            else
            {
                // owned house
                if (house.Locked)
                {
                    if (house.Owner == player.GetSocialClub())
                        menu.Add(new MenuItem("Rentrer dans la maison", "", "ID_Enter", executeCallback: true));
                }
                else
                {
                    menu.Add(new MenuItem("Rentrer dans la maison", "", "ID_Enter", executeCallback: true));
                }
                menu.Add(new MenuItem("Sonner à la porte", "", "ID_Sonner", executeCallback: true));
            }
            await MenuManager.OpenMenu(player, menu);
        }

        private static async Task MenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            House house = menu.GetData("House");
            if (house == null) return;

            switch (menuItem.Id)
            {
                case "ID_Acheter":
                    PlayerHandler player = client.GetPlayerHandler();
                    if (player == null) return;
                    if (await player.HasBankMoney(house.Price, "Achat immobilier."))
                    {
                        //GameMode.Instance.Economy.CaissePublique += house.Price;
                        await house.SetOwner(client);
                        await house.SendPlayer(client);
                        await MenuManager.CloseMenu(client);
                        client.SendNotificationSuccess("Vous avez acheté ce logement.");
                    }
                    else
                    {
                        player.Client.SendNotificationError("Vous n'avez pas l'argent sur votre compte en banque.");
                    }
                    break;

                case "ID_Enter":
                    await house.SendPlayer(client);
                    break;

                case "ID_Sonner":
                    foreach (IPlayer clientInside in house.PlayersInside)
                        clientInside.SendNotification("Quelqu'un sonne à la porte.");
                    break;
                case "ID_AddParking":
                    if (HouseManager.AddParkingList.TryAdd(client, house))
                    {
                        client.SendChatMessage("Taper dans le tchat la commande /addparkinghouse pour ajouter le parking");
                    }
                    else
                    {
                        client.SendNotificationError("Un parking est déjà disponible pour cette maison.");
                    }
                    break;
                case "ID_Delete":
                    await house.Destroy(true);
                    await house.RemoveInDatabase();
                    client.SendNotificationSuccess("Vous avez supprimé le logement.");
                    break;
                case "ID_PriceChange":
                    int price = Convert.ToInt32(menuItem.InputValue);
                    house.Price = price;
                    await house.SetPrice(price);
                    client.SendNotificationSuccess("Vous avez changé le prix du logement par " + price);
                    break;
                case "ID_Change":
                    int type = Convert.ToInt32(menuItem.InputValue);
                    await house.SetType(type);
                    client.SendNotificationSuccess("Vous avez changé le type du logement par " + type);
                    break;
                case "ID_RemoveParking":
                    house.Parking = null;
                    await house.Save();
                    client.SendNotificationSuccess("Vous avez supprimé le parking");
                    break;
                case "ID_ChangeProprio":
                    await house.SetOwner(menuItem.InputValue);
                    client.SendNotificationSuccess("Proprio changé.");
                    break;
                default:
                    break;
            }
        }

        public async Task OnPlayerConnected(IPlayer player)
        {
            var social = player.GetSocialClub();
            foreach (House house in Houses.Where(h => h.Owner == social))
                house.SetOwnerHandle(player);
        }

        public async Task House_PlayerDeath(IPlayer player, IPlayer killer, uint reason)
        {
            await player.SetDimensionAsync(GameMode.GlobalDimension);
            await RemovePlayerFromHouseList(player);
        }

        public async Task House_Exit()
        {
            for(int i = 0; i < Houses.Count; i++)
            {
                await Houses[i].Save();
                await Houses[i].Destroy(true);
            }
            Houses.Clear();
        }
        #endregion
    }
}