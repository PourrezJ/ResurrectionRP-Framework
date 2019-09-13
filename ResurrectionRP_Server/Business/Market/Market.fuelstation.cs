﻿using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Utils;
using System;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Businesses
{
    public partial class Market
    {
        private bool _ravitaillement = false;
        private IPlayer _utilisateurRavi;

        public static async Task OpenGasPompMenu(IPlayer client, int id)
        {
            var fuelpump = Market.MarketsList.Find(x => x.ID == id);

            if (fuelpump == null)
            {
                client.SendNotificationError("Cette pompe est hors service.");
                return;
            }

            Menu menu = new Menu("ID_GasPumpMenuMain", "Station Essence", $"Prix du litre: {fuelpump.EssencePrice}", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true);
            menu.ItemSelectCallback = fuelpump.FuelMenuCallBack;

            menu.SubTitle = "Mettre le plein dans:";

            var VehicleList = fuelpump.StationPos.GetPlayersInRange( fuelpump.Range);
            if (VehicleList.Count <= 0)
            {
                client.SendNotificationError("Aucun véhicule près de la pompe.");
                return;
            }

            foreach (IVehicle vehicle in VehicleList)
            {
                Entities.Vehicles.VehicleHandler vh = vehicle.GetVehicleHandler() ;

                MenuItem item = new MenuItem(vh.VehicleManifest.DisplayName, rightLabel: vh.Plate, executeCallback: true);
                item.SetData("Vehicle", vh);
                if (vh != null) menu.Add(item);
            }

            await MenuManager.OpenMenu(client, menu);
        }

        private async Task FuelMenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            Entities.Vehicles.VehicleHandler vh = menuItem.GetData("Vehicle");
            if (vh == null) return;
            int price = CalculEssencePriceNeeded(vh, this.EssencePrice);
            AcceptMenu accept = await AcceptMenu.OpenMenu(client, menu.Title, $"Prix du litre: ${EssencePrice} || Taxe Etat: ${GameMode.Instance?.Economy.Taxe_Essence}", $"Mettre le plein dans {vh.Plate} pour la somme de ~r~${price}~w~.", rightlabel: $"${price}");

            accept.AcceptMenuCallBack += (async (IPlayer _client, bool reponse) => {
                if (reponse)
                {
                    if (await client.GetPlayerHandler().HasBankMoney(price, $"Plein d'essence."))
                    {
                        vh.SetFuel(vh.FuelMax);
                        await BankAccount.AddMoney(price, $"Plein du véhicule {vh.Plate}", false);
                        client.SendNotificationSuccess($"Vous avez fait le plein dans le véhicule pour la somme de ${price}");
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

            if ( client.IsInVehicle == true &&  ( client.Vehicle).Model == 4097861161)
            {
                IVehicle fueltruck = client.Vehicle;
                if (fueltruck.GetData("RefuelRaffine", out object dataa))
                {
                    if (_ravitaillement || _utilisateurRavi != null)
                    {
                        client.DisplayHelp("Il y a déjà un ravitaillement en cours !", 15000);
                        return;
                    }
                    _utilisateurRavi = client;
                    _ravitaillement = true;
                    int currentmax = Convert.ToInt32(dataa);

                    await MenuManager.CloseMenu(client);


                    if (currentmax + Litrage > LitrageMax)
                        currentmax = LitrageMax;

                    //API.Shared.OnProgressBar(client, true, 0, currentmax, 750);
                    client.DisplayHelp("Le ravitaillement vient de démarrer.\n Patientez", 15000);
                    while (_ravitaillement)
                    {
                        await Task.Delay(750);
                        //API.OnProgressBar(client, true, i, currentmax);
                        fueltruck.SetData("RefuelRaffine", currentmax - 1);
                        if (Litrage >= LitrageMax)
                        {
                            client.DisplayHelp("~r~[Abandon] Le réservoir de la station est plein!", 15000);
                            //API.Shared.OnProgressBar(client, false);
                            _ravitaillement = false;
                            _utilisateurRavi = null;
                            return;
                        }

                        if (currentmax <= 0)
                        {
                            client.DisplayHelp("Ravitaillement terminé, merci !", 15000);
                            //API.Shared.OnProgressBar(client, false);
                            _ravitaillement = false;
                            _utilisateurRavi = null;
                            await Update();
                            return;
                        }
                        Litrage++;
                    }
                }
                else
                {
                    client.DisplayHelp("Impossible, votre citerne est vide !", 15000);
                    return;
                }
            }
        }

        public int CalculEssencePriceNeeded(Entities.Vehicles.VehicleHandler veh, int essencePrice)
            => Convert.ToInt32((veh.FuelMax - veh.Fuel) * (GameMode.Instance.Economy.Taxe_Essence + essencePrice));
    }
}
