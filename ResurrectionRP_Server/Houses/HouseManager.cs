using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using MongoDB.Driver;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Utils;
using ResurrectionRP_Server.Utils.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Houses
{
    public static class HouseManager
    {
        #region Public fields
        public static Dictionary<IPlayer, House> AddParkingList = new Dictionary<IPlayer, House>();

        // settings
        public static List<House> Houses = new List<House>();

        private static ConcurrentDictionary<IPlayer, House> ClientHouse = new ConcurrentDictionary<IPlayer, House>();
        #endregion

        #region Init
        public static void Init()
        {
            Alt.OnPlayerDisconnect += OnPlayerDisconnect;
            Alt.OnPlayerDead += OnPlayerDead;

            for (int i = 0; i < HouseTypes.HouseTypeList.Count; i++)
                Marker.CreateMarker(MarkerType.VerticalCylinder, HouseTypes.HouseTypeList[i].Position.Pos - new Vector3(0.0f, 0.0f, 1.0f), new Vector3(1,1,1));
            /*
            Utils.Util.SetInterval(async () =>
            {
                foreach (var house in Houses)
                {
                    house.UpdateInBackground();
                    await Task.Delay(100);
                }
            }, (int)TimeSpan.FromMinutes(10).TotalMilliseconds);*/
        }
        #endregion

        #region Event handlers
        public static void OnPlayerConnected(IPlayer player)
        {
            string social = player.GetSocialClub();

            foreach (House house in Houses.Where(h => h.Owner == social))
                house.SetOwnerHandle(player);
        }

        public static void OnPlayerDisconnect(IPlayer player, string reason)
        {
            RemovePlayerFromHouseList(player);
        }

        private static void OnPlayerDead(IPlayer player, IEntity killer, uint weapon)
        {
            player.Dimension = GameMode.GlobalDimension;
            RemovePlayerFromHouseList(player);
        }
        #endregion

        #region Methods
        public static bool SetIntoHouse(IPlayer client, House house)
        {
            Alt.Server.LogInfo($"Set client {client.Id} into house {house.ID}");
            return ClientHouse.TryAdd(client, house);
        }

        public static bool IsInHouse(IPlayer client) => ClientHouse.ContainsKey(client);

        public static House GetHouseWithID(int id) => Houses.Find(h => h.ID == id) ?? null;

        public static House GetHouse(IPlayer client)
        {
            if (IsInHouse(client))
                return ClientHouse.GetValueOrDefault(client);

            return null;
        }

        public static IEnumerable<IPlayer> GetHousePlayers(House house)
        {
            return ClientHouse.Where(ch => ch.Value == house).Select(ch => ch.Key);
        }

        public static bool RemoveClientHouse(IPlayer client)
        {
            bool result = ClientHouse.TryRemove(client, out House house);
            Alt.Server.LogInfo($"Remove client {client.Id} from house {house.ID}");
            return result;
        }

        public static void RemovePlayerFromHouseList(IPlayer player)
        {
            if (IsInHouse(player))
            {
                House house = GetHouse(player);

                if (house == null)
                    return;

                house.RemovePlayer(player, false);                
            }
        }
        #endregion

        #region Events        
        public static void LoadAllHouses()
        {
            Alt.Server.LogInfo($"Loading houses ...");

            var housesList = Database.MongoDB.GetCollectionSafe<House>("houses").AsQueryable();

            foreach(var house in housesList)
            {
                try
                {
                    house.Init();
                    Houses.Add(house);
                }
                catch (Exception ex)
                {
                    Alt.Server.LogError($"Erreur sur maison id:{house.ID}: " + ex.ToString());
                }
            }

            Alt.Server.LogInfo($"Loaded {Houses.Count} houses.");
        }

        public static void OpenHouseMenu(IPlayer player, House house)
        {
            if (!player.Exists)
                return;

            Menu menu = new Menu("House_PurchaseMenu", HouseTypes.HouseTypeList[house.Type].Name, "Choisissez une option :", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true);
            menu.BannerColor = new MenuColor(0, 0, 0, 0);
            menu.SetData("House", house);
            menu.ItemSelectCallback = MenuCallBack;

            menu.SubTitle = (house.Locked) ? "La porte est ~r~fermée" : "La porte est ~g~ouverte";

            // MODO ONLY
            if (player.GetPlayerHandler()?.StaffRank >= StaffRank.Moderator)
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
            MenuManager.OpenMenu(player, menu);
        }

        private static void MenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            House house = menu.GetData("House");

            if (house == null)
                return;

            switch (menuItem.Id)
            {
                case "ID_Acheter":
                    PlayerHandler player = client.GetPlayerHandler();

                    if (player == null)
                        return;

                    if (player.HasBankMoney(house.Price, "Achat immobilier."))
                    {
                        GameMode.Instance.Economy.CaissePublique += house.Price;
                        house.SetOwner(client.GetPlayerHandler().PID);
                        house.SendPlayer(client);
                        MenuManager.CloseMenu(client);
                        client.SendNotificationSuccess("Vous avez acheté ce logement.");
                    }
                    else
                        player.Client.SendNotificationError("Vous n'avez pas l'argent sur votre compte en banque.");
                    break;

                case "ID_Enter":
                    house.SendPlayer(client);
                    menu.CloseMenu(client);
                    break;

                case "ID_Sonner":
                    foreach (IPlayer clientInside in GetHousePlayers(house))
                        clientInside.SendNotification("Quelqu'un sonne à la porte.");
                    break;
                case "ID_AddParking":
                    if (AddParkingList.TryAdd(client, house))
                        client.SendChatMessage("Taper dans le tchat la commande /addparkinghouse pour ajouter le parking");
                    else
                        client.SendNotificationError("Un parking est déjà disponible pour cette maison.");
                    break;
                case "ID_Delete":
                    house.Destroy();
                    Task.Run(async ()=> await house.RemoveInDatabase());
                    client.SendNotificationSuccess("Vous avez supprimé le logement.");
                    break;
                case "ID_PriceChange":
                    int price = Convert.ToInt32(menuItem.InputValue);
                    house.Price = price;
                    house.SetPrice(price);
                    client.SendNotificationSuccess("Vous avez changé le prix du logement par " + price);
                    break;
                case "ID_Change":
                    int type = Convert.ToInt32(menuItem.InputValue);
                    house.SetType(type);
                    client.SendNotificationSuccess("Vous avez changé le type du logement par " + type);
                    break;
                case "ID_RemoveParking":
                    house.Parking.Destroy();
                    house.Parking = null;
                    house.UpdateInBackground();
                    client.SendNotificationSuccess("Vous avez supprimé le parking");
                    break;
                case "ID_ChangeProprio":
                    house.SetOwner(menuItem.InputValue);
                    client.SendNotificationSuccess("Proprio changé.");
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
