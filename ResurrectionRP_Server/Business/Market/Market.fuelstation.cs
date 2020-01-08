using AltV.Net;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Vehicles;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Business
{

    public partial class Market
    {
        private bool _ravitaillement = false;
        private IPlayer _utilisateurRavi;
        public static uint[] allowedTrailers = new uint[3] { 0xB8081009, 0xD46F4737, 0x74998082 };

        private System.Timers.Timer timer = null;

        public static void OpenGasPumpMenu(IPlayer client)
        {
            Market fuelpump = MarketsList.Find(x => x.Station.Colshape.IsEntityIn(client));

            if (fuelpump == null)
            {
                client.DisplaySubtitle( "Cette pompe est hors service.", 10000);
                return;
            }
            else if (fuelpump.Station.Litrage == 0)
            {
                client.DisplaySubtitle("La station est vide ! Impossible de faire le plein!", 10000);
                return;
            }


            Menu menu = new Menu("ID_GasPumpMenuMain", "Station Essence", $"Prix du litre: {fuelpump.Station.EssencePrice + GameMode.Instance.Economy.Taxe_Essence}", 0, 0, Menu.MenuAnchor.MiddleRight, false, true, true);
            menu.ItemSelectCallback = fuelpump.FuelMenuCallBack;

            menu.SubTitle = "Mettre le plein dans:";
            if (
                fuelpump.Station.VehicleInStation.Count == 0 &&
                (client.GetPlayerHandler().BagInventory != null &&
                    !client.GetPlayerHandler().BagInventory.HasItemID(Models.InventoryData.ItemID.Jerrycan ) ) &&
                (client.GetPlayerHandler().PocketInventory.HasItemID(Models.InventoryData.ItemID.Jerrycan) )
            )
            {
                client.DisplaySubtitle( "Rien à remplir.", 10000);
                return;
            }

            foreach (KeyValuePair<int, IVehicle> veh in fuelpump.Station.VehicleInStation)
            {
                IVehicle vehicle = veh.Value;

                if (vehicle == null)
                    continue;

                VehicleHandler vh = vehicle.GetVehicleHandler();

                if (vh.VehicleData.Fuel > (vh.VehicleData.FuelMax - 2))
                {
                    client.DisplaySubtitle("Il se peut que certains véhicule n'aient pas besoin de plein!", 10000);
                    continue;
                }

                MenuItem item = new MenuItem(vh.VehicleManifest.DisplayName, rightLabel: vh.VehicleData.Plate, executeCallback: true);
                item.SetData("Vehicle", vh);
                menu.Add(item);
            }


/*            Alt.Server.LogInfo("Status Bag Inventory : " + (client.GetPlayerHandler().BagInventory != null).ToString());
            Alt.Server.LogInfo("Has Item in Bag Inventory : " + (client.GetPlayerHandler().BagInventory != null && !client.GetPlayerHandler().BagInventory.HasItemID(Models.InventoryData.ItemID.Jerrycan)).ToString());
            Alt.Server.LogInfo("Status Pocket Inventory : " + (client.GetPlayerHandler().PocketInventory != null).ToString());
            Alt.Server.LogInfo("Has Item in Pocket Inventory : " + ( client.GetPlayerHandler().PocketInventory.HasItemID(Models.InventoryData.ItemID.Jerrycan)).ToString());*/
            if (client.GetPlayerHandler().BagInventory != null && client.GetPlayerHandler().BagInventory.HasItemID(Models.InventoryData.ItemID.Jerrycan))
            {
                MenuItem item = new MenuItem("Remplir le jerrycan", "Remplissez votre jerrycan de 15 litres (remplit le premier jerrycan dans l'inventaire)", "ID_Jerrycan", true);
                item.SetData("Jerrycan", client.GetPlayerHandler().BagInventory.GetItem(Models.InventoryData.ItemID.Jerrycan));
                menu.Add(item);
            }
            
            menu.OpenMenu(client);
        }

        private void FuelMenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            AcceptMenu accept = null;
            float price = 0;
            float maxFuel = 0;

            switch(menuItem.Id)
            {
                case "ID_Jerrycan":
                    Items.GasJerrycan item = menuItem.GetData("Jerrycan");

                    maxFuel = (Station.Litrage - 15 < 0) ? Station.Litrage : 15;

                    if(Station.Litrage - 15 < 0)
                        client.DisplaySubtitle("Il n'y a pas assez d'essence pour faire le plein, \nvous serez remplis à hauteur du possible !", 10000);

                    if(item.Fuel == 15)
                    {
                        client.DisplaySubtitle("Votre jerrycan est déjà remplis! ", 10000);
                        MenuManager.CloseMenu(client);
                        return;
                    }

                    if (item.Fuel > 0 && item.Fuel < 15)
                        maxFuel = (float)(15 - item.Fuel);

                    price = CalculEssencePriceNeeded(maxFuel, this.Station.EssencePrice);
                    accept = AcceptMenu.OpenMenu(client, menu.Title, $"Prix du litre: ${Station.EssencePrice + GameMode.Instance.Economy.Taxe_Essence} TTC ", $"Mettre le plein dans Jerrycan pour la somme de ~r~${price}~w~.", rightlabel: $"${price}");

                    accept.AcceptMenuCallBack = (IPlayer _client, bool response) =>
                    {
                        if (!response)
                            return;

                        if (Station.Litrage < maxFuel)
                            maxFuel = Station.Litrage;

                        if (maxFuel + item.Fuel > 15)
                            maxFuel = (float)(15 - item.Fuel);

                        if (client.GetPlayerHandler().HasBankMoney(price, $"Plein d'essence Jerrycan."))
                        {
                            item.Fuel += (int)maxFuel;
                            Station.Litrage -= maxFuel;
                            BankAccount.AddMoney(price, $"Plein d'un Jerrycan");
                            client.DisplaySubtitle($"Le plein est fait.\n Vous avez payé ~r~${price}", 6000);
                            UpdateInBackground();
                            client.GetPlayerHandler().UpdateFull();
                        }
                        else
                            client.SendNotificationError("Vous n'avez pas assez d'argent en banque.");

                        MenuManager.CloseMenu(client);
                    };

                    break;
                default:
                    VehicleHandler vh = menuItem.GetData("Vehicle");

                    if (vh == null)
                        return;

                    maxFuel = vh.VehicleData.FuelMax - vh.VehicleData.Fuel;

                    if ((vh.VehicleData.FuelMax - vh.VehicleData.Fuel) > Station.Litrage && Station.Litrage > 0)
                    {
                        maxFuel = Station.Litrage;
                        client.DisplaySubtitle("Il n'y a pas assez d'essence pour faire le plein, \nvous serez remplis à hauteur du possible !", 10000);
                    }

                    price = CalculEssencePriceNeeded(maxFuel, this.Station.EssencePrice);
                    accept = AcceptMenu.OpenMenu(client, menu.Title, $"Prix du litre: ${Station.EssencePrice + GameMode.Instance.Economy.Taxe_Essence} TTC ", $"Mettre le plein dans {vh.VehicleManifest.DisplayName} pour la somme de ~r~${price}~w~.", rightlabel: $"${price}");

                    accept.AcceptMenuCallBack = (IPlayer _client, bool reponse) =>
                    {
                        if (reponse && Station.Litrage >= maxFuel)
                        {
                            if (client.GetPlayerHandler().HasBankMoney(price, $"Plein d'essence {vh.VehicleData.Plate}."))
                            {
                                vh.VehicleData.Fuel += maxFuel;
                                Station.Litrage -= maxFuel;
                                BankAccount.AddMoney(price, $"Plein du véhicule {vh.VehicleData.Plate}");
                                client.DisplaySubtitle($"Le plein est fait.\n Vous avez payé ~r~${price}", 6000);
                                UpdateInBackground();
                            }
                            else
                                client.SendNotificationError("Vous n'avez pas assez d'argent en banque.");

                            MenuManager.CloseMenu(client);
                        }
                        else if (Station.Litrage < maxFuel)
                        {
                            client.DisplaySubtitle("Impossible de remplir, la station est maintenant vide ! ", 10000);
                            MenuManager.CloseMenu(client);
                        }
                        else
                            MenuManager.CloseMenu(client);
                    };
                    break;
            }
        }
        
        private void RefuelMenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (!client.Exists)
                return;

            if(client.Vehicle.Model == 4097861161)
                client.DisplayHelp("Ce véhicule n'est plus homologué pour la livraison d'essence.", 10000);

            if (client.IsInVehicle)
            {
                if (!client.Vehicle.GetVehicleHandler().HasTrailer)
                    return;

                if (Array.IndexOf(allowedTrailers, client.Vehicle.GetVehicleHandler().Trailer.Model) == -1)
                {
                    client.DisplayHelp("La remorque n'est pas homologuée pour cette action!", 10000);
                    return;
                }

                IVehicle fueltruck = (IVehicle)client.Vehicle.GetVehicleHandler().Trailer;
                VehicleHandler hfuel = ((IVehicle)(client.Vehicle.GetVehicleHandler().Trailer)).GetVehicleHandler();
                var data = Convert.ToInt32(menuItem.InputValue);
                if (data > 0 && hfuel.VehicleData.OilTank.Traite >= data)
                {
                    if (_ravitaillement || _utilisateurRavi != null)
                    {
                        client.DisplayHelp("La station service est déjà en ravitaillement!", 15000);
                        return;
                    }
                    _utilisateurRavi = client;
                    _ravitaillement = true;

                    MenuManager.CloseMenu(client);

                    var EssenceTransfert = 0;
                    //API.Shared.OnProgressBar(client, true, 0, currentmax, 750);
                    client.DisplaySubtitle("Début du transfert ...", 1000);
                    Task.Run(() =>
                    {
                        timer = Utils.Utils.SetInterval(() =>
                        {
                            //API.OnProgressBar(client, true, i, currentmax);
                            if (Station.Litrage == Station.LitrageMax)
                            {
                                client.DisplaySubtitle("~g~Le réservoir de la station est plein ! Fin du transfert!", 30000);
                                //API.Shared.OnProgressBar(client, false);
                                _ravitaillement = false;
                                _utilisateurRavi = null;
                                Utils.Utils.StopTimer(timer);
                                UpdateInBackground();
                                return;

                            }
                            if (EssenceTransfert >= data)
                            {
                                client.DisplaySubtitle("~g~Transfert terminé! ", 30000);
                                //API.Shared.OnProgressBar(client, false);
                                _ravitaillement = false;
                                _utilisateurRavi = null;
                                Utils.Utils.StopTimer(timer);
                                UpdateInBackground();
                                return;
                            }

                            if (_ravitaillement = false || _utilisateurRavi == null)
                            {
                                _ravitaillement = false;
                                _utilisateurRavi = null;


                                Utils.Utils.StopTimer(timer);
                                UpdateInBackground();
                                return;
                            }

                            int Updater = 2;

                            if (data % Updater != 0 && data - EssenceTransfert < Updater)
                                Updater = (data - EssenceTransfert);

                            Station.Litrage += Updater;
                            hfuel.VehicleData.OilTank.Traite -= Updater;
                            EssenceTransfert += Updater;
                            _ravitaillement = true;

                            client.DisplayHelp("Station service \nLitres en station: " + Station.Litrage + "\nLitres dans le camion: " + hfuel.VehicleData.OilTank.Traite, 30000);
                        }, 1250);
                    });
                }
                else
                {
                    client.DisplaySubtitle("~r~Vous n'avez pas assez en réserve ! ", 15000);
                    return;
                }
            }
        }

        public float CalculEssencePriceNeeded(float fuel, float essencePrice)
            => (float)((fuel) * (GameMode.Instance.Economy.Taxe_Essence + essencePrice));
    }
}
