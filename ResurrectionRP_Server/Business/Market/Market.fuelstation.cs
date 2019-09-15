using AltV.Net;
using AltV.Net.Elements.Entities;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Business
{

    public partial class Market
    {
        private bool _ravitaillement = false;
        private IPlayer _utilisateurRavi;

        public static async Task OpenGasPumpMenu(IPlayer client)
        {
            var fuelpump = Market.MarketsList.Find(x => x.Station.Colshape.IsEntityInColShape(client));
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
                if (vh.GetFuel() > vh.FuelMax - 2)
                {
                    client.DisplayHelp("Il se peut que certains véhicule n'aient pas besoin de plein!", 10000);
                    continue;
                }

                MenuItem item = new MenuItem(vh.VehicleManifest.DisplayName, rightLabel: vh.Plate, executeCallback: true);
                item.SetData("Vehicle", vh);
                menu.Add(item);
            }

            await menu.OpenMenu(client);
        }

        private async Task FuelMenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            VehicleHandler vh = menuItem.GetData("Vehicle");
            if (vh == null) return;
            int price = CalculEssencePriceNeeded(vh, this.Station.EssencePrice);
            AcceptMenu accept = await AcceptMenu.OpenMenu(client, menu.Title, $"Prix du litre: ${Station.EssencePrice} || Taxe Etat: ${GameMode.Instance?.Economy.Taxe_Essence}", $"Mettre le plein dans {vh.Plate} pour la somme de ~r~${price}~w~.", rightlabel: $"${price}");

            accept.AcceptMenuCallBack += (async (IPlayer _client, bool reponse) => {
                if (reponse)
                {
                    if (await client.GetPlayerHandler().HasBankMoney(price, $"Plein d'essence {vh.Plate}."))
                    {
                        vh.SetFuel(vh.FuelMax);
                        await BankAccount.AddMoney(price, $"Plein du véhicule {vh.Plate}");
                        client.DisplayHelp($"Le plein est fait.\n Vous avez payé ~r~${price}", 6000);
                        await Update();
                    }
                    else
                    {
                        client.SendNotificationError("Vous n'avez pas assez d'argent en banque.");
                    }
                    await MenuManager.CloseMenu(client);
                }
                else
                {
                    await MenuManager.CloseMenu(client);
                }
            });
        }
        
        private async Task RefuelMenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (!client.Exists)
                return;

            if (client.IsInVehicle &&  client.Vehicle.Model== 4097861161)
            {
                IVehicle fueltruck = client.Vehicle;
                VehicleHandler hfuel = client.Vehicle.GetVehicleHandler();
                var data = hfuel.OilTank.Traite;
                if (data > 0)
                {
                    if (_ravitaillement || _utilisateurRavi != null)
                    {
                        client.DisplayHelp("La station service est déjà en ravitaillement!", 15000);
                        return;
                    }
                    _utilisateurRavi = client;
                    _ravitaillement = true;

                    await MenuManager.CloseMenu(client);


                    var EssenceTransfert = 0;
                    //API.Shared.OnProgressBar(client, true, 0, currentmax, 750);
                    client.DisplaySubtitle("Début du transfert ...", 30000);
                    while (_ravitaillement)
                    {
                        await Task.Delay(1000); // prod : 750
                        client.DisplayHelp("Station service \n Litres en station: " + Station.Litrage + "\nLitres dans le camion: " + hfuel.OilTank.Traite, 1000);
                        //API.OnProgressBar(client, true, i, currentmax);
                        if(Station.Litrage == Station.LitrageMax)
                        {
                            client.DisplaySubtitle("~g~Le réservoir de la station est plein ! Fin du transfert!", 30000);
                            //API.Shared.OnProgressBar(client, false);
                            _ravitaillement = false;
                            _utilisateurRavi = null;
                            client.GetPlayerHandler()?.HasBankMoney(EssenceTransfert * this.Station.buyEssencePrice, "Vente essence à la station " + this.BusinnessName);
                            await BankAccount.AddMoney(EssenceTransfert * this.Station.buyEssencePrice, "Achat essence au vendeur " + client.GetPlayerHandler()?.Identite.FirstName + " " + client.GetPlayerHandler()?.Identite.LastName, true);

                            return;

                        }
                        if(hfuel.OilTank.Traite == 0)
                        {
                            client.DisplaySubtitle("~g~Votre citerne est vide, fin du transfert! ", 30000);
                            //API.Shared.OnProgressBar(client, false);
                            _ravitaillement = false;
                            _utilisateurRavi = null;
                            client.GetPlayerHandler()?.HasBankMoney(EssenceTransfert * this.Station.buyEssencePrice, "Vente essence à la station " + this.BusinnessName);
                            await BankAccount.AddMoney(EssenceTransfert * this.Station.buyEssencePrice, "Achat essence au vendeur " + client.GetPlayerHandler()?.Identite.FirstName + " " + client.GetPlayerHandler()?.Identite.LastName, true);
                            return;
                        }

                        if(_ravitaillement = false && _utilisateurRavi == null)
                        {
                            _ravitaillement = false;
                            _utilisateurRavi = null;
                            client.GetPlayerHandler()?.HasBankMoney(EssenceTransfert * this.Station.buyEssencePrice, "Vente essence à la station " + this.BusinnessName);
                            await BankAccount.AddMoney(EssenceTransfert * this.Station.buyEssencePrice, "Achat essence au vendeur " + client.GetPlayerHandler()?.Identite.FirstName + " " + client.GetPlayerHandler()?.Identite.LastName, true);
                            return;
                        }

                        Station.Litrage++;
                        hfuel.OilTank.Traite--;
                        EssenceTransfert++;
                        _ravitaillement = true;
                        _utilisateurRavi = client;
                    }
                }
                else
                {
                    client.DisplaySubtitle("~r~Votre citerne est vide!", 15000);
                    return;
                }
            }
        }

        public int CalculEssencePriceNeeded(Entities.Vehicles.VehicleHandler veh, int essencePrice)
            => Convert.ToInt32((veh.FuelMax - veh.Fuel) * (GameMode.Instance.Economy.Taxe_Essence + essencePrice));
    }
}
