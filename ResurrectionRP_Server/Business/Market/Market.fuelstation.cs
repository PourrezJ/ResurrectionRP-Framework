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
            Market fuelpump = MarketsList.Find(x => x.Station.Colshape.IsEntityInColShape(client));

            if (fuelpump == null)
            {
                client.DisplaySubtitle( "Cette pompe est hors service.", 10000);
                return;
            }

            Menu menu = new Menu("ID_GasPumpMenuMain", "Station Essence", $"Prix du litre: {fuelpump.Station.EssencePrice}", 0, 0, Menu.MenuAnchor.MiddleRight, false, true, true);
            menu.ItemSelectCallback = fuelpump.FuelMenuCallBack;
            menu.SubTitle = "Mettre le plein dans:";

            if (fuelpump.Station.VehicleInStation.Count == 0)
            {
                client.DisplaySubtitle( "Aucun véhicule près de la pompe.", 10000);
                return;
            }

            foreach (KeyValuePair<int, IVehicle> veh in fuelpump.Station.VehicleInStation)
            {
                IVehicle vehicle = veh.Value;

                if (vehicle == null)
                    continue;

                VehicleHandler vh = vehicle.GetVehicleHandler();

                if (vh.Fuel > vh.FuelMax - 2)
                {
                    client.DisplayHelp("Il se peut que certains véhicule n'aient pas besoin de plein!", 10000);
                    continue;
                }

                MenuItem item = new MenuItem(vh.VehicleManifest.DisplayName, rightLabel: vh.Plate, executeCallback: true);
                item.SetData("Vehicle", vh);
                menu.Add(item);
            }

            menu.OpenMenu(client);
        }

        private void FuelMenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            VehicleHandler vh = menuItem.GetData("Vehicle");

            if (vh == null)
                return;

            int price = CalculEssencePriceNeeded(vh, this.Station.EssencePrice);
            AcceptMenu accept = AcceptMenu.OpenMenu(client, menu.Title, $"Prix du litre: ${Station.EssencePrice} || Taxe Etat: ${GameMode.Instance?.Economy.Taxe_Essence}", $"Mettre le plein dans {vh.Plate} pour la somme de ~r~${price}~w~.", rightlabel: $"${price}");

            accept.AcceptMenuCallBack = (IPlayer _client, bool reponse) =>
            {
                if (reponse)
                {
                    if (client.GetPlayerHandler().HasBankMoney(price, $"Plein d'essence {vh.Plate}."))
                    {
                        vh.Fuel = vh.FuelMax;
                        BankAccount.AddMoney(price, $"Plein du véhicule {vh.Plate}");
                        client.DisplayHelp($"Le plein est fait.\n Vous avez payé ~r~${price}", 6000);
                        UpdateInBackground();
                    }
                    else
                        client.SendNotificationError("Vous n'avez pas assez d'argent en banque.");

                    MenuManager.CloseMenu(client);
                }
                else
                    MenuManager.CloseMenu(client);

                return Task.CompletedTask;
            };
        }
        
        private void RefuelMenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (!client.Exists)
                return;

            if(client.Vehicle.Model == 4097861161)
                client.DisplayHelp("Ce véhicule n'est plus homologué pour la livraison d'essence.", 10000);

            if (client.IsInVehicle)
            {
                if (!client.Vehicle.GetVehicleHandler().hasTrailer)
                    return;

                if (Array.IndexOf(allowedTrailers, client.Vehicle.GetVehicleHandler().Trailer.Model) == -1)
                {
                    client.DisplayHelp("La remorque n'est pas homologuée pour cette action!", 10000);
                    return;
                }

                IVehicle fueltruck = (IVehicle)client.Vehicle.GetVehicleHandler().Trailer;
                VehicleHandler hfuel = ((IVehicle)(client.Vehicle.GetVehicleHandler().Trailer)).GetVehicleHandler();
                var data = Convert.ToInt32(menuItem.InputValue);
                if (data > 0 && hfuel.OilTank.Traite >= data)
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
                                return;

                            }
                            if (EssenceTransfert >= data)
                            {
                                client.DisplaySubtitle("~g~Transfert terminé! ", 30000);
                                //API.Shared.OnProgressBar(client, false);
                                _ravitaillement = false;
                                _utilisateurRavi = null;
                                Utils.Utils.StopTimer(timer);
                                return;
                            }

                            if (_ravitaillement = false || _utilisateurRavi == null)
                            {
                                _ravitaillement = false;
                                _utilisateurRavi = null;

                                if (BankAccount.GetBankMoney(EssenceTransfert * this.Station.buyEssencePrice, "Achat essence au vendeur " + client.GetPlayerHandler()?.Identite.FirstName + " " + client.GetPlayerHandler()?.Identite.LastName, "", true))
                                    client.GetPlayerHandler()?.BankAccount.AddMoney(EssenceTransfert * this.Station.buyEssencePrice, "Vente essence à la station " + this.BusinnessName);

                                Utils.Utils.StopTimer(timer);
                                return;
                            }

                            Station.Litrage++;
                            hfuel.OilTank.Traite--;
                            EssenceTransfert++;
                            _ravitaillement = true;

                            client.DisplayHelp("Station service \n Litres en station: " + Station.Litrage + "\nLitres dans le camion: " + hfuel.OilTank.Traite, 30000);
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

        public int CalculEssencePriceNeeded(Entities.Vehicles.VehicleHandler veh, int essencePrice)
            => Convert.ToInt32((veh.FuelMax - veh.Fuel) * (GameMode.Instance.Economy.Taxe_Essence + essencePrice));
    }
}
