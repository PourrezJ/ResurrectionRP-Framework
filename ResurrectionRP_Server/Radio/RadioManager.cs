using AltV.Net;
using AltV.Net.Elements.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using RadioModes = ResurrectionRP_Server.Radio.Data.RadioModes;
using System.Globalization;
using SaltyServer;

namespace ResurrectionRP_Server.Radio
{
    public static class RadioManager
    {
        #region Private static properties
        private static ConcurrentDictionary<IPlayer, Radio> _clientMenus = new ConcurrentDictionary<IPlayer, Radio>();
        #endregion

        public static void Init()
        {
            Alt.OnClient("RadioManager", EventTrigered);
        }
        public static void Close(IPlayer client)
        {
            Radio radio;
            if (_clientMenus.TryGetValue(client, out radio))
                radio.CloseRadio(client);
        }

        public static bool OpenRadio(IPlayer client, Radio radio)
        {
            Radio oldRadio;
            _clientMenus.TryRemove(client, out oldRadio);

            if (_clientMenus.TryAdd(client, radio))
            {
                radio.OpenRadio(client);
                return true;
            }
            return false;
        }

        private static void EventTrigered(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            var player = client;

            var ph = player.GetPlayerHandler();
            if (ph == null)
                return;

            Radio radio;
            _clientMenus.TryGetValue(player, out radio);

            if (radio == null)
                return;

            switch (args[0])
            {
                case "Open":
                    OpenRadio(player, ph.RadioSelected);
                    break;

                case "OnOff":
                    try
                    {
                        radio.Statut = ((bool)args[1]) ? RadioModes.LISTENING : RadioModes.OFF;
                        if (radio.Statut == RadioModes.OFF)
                        {
                            Voice.RemovePlayerRadioChannel(player);
                        }
                        else
                        {
                            Voice.OnSetRadioChannel(player, radio.GetCurrentFrequence().ToString());
                            Voice.SetPlayerSendingOnRadioChannel(player, radio.GetCurrentFrequence().ToString(), false);
                        }
                    }
                    catch (Exception ex)
                    {

                        Alt.Server.LogError(ex.ToString());
                    }
                    break;


                case "Close":     
                    if(_clientMenus.TryRemove(player, out radio))
                        radio.CloseRadio(player);
                    break;

                case "SaveFrequence":
                    try
                    {
                        if (args[2] == null)
                            return;

                        if (double.TryParse(args[2].ToString(),  NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double frequence)){

                            radio.SaveFrequeceRadio(Convert.ToInt32(args[1]), frequence);
                            player.GetPlayerHandler()?.UpdateFull();
                            Voice.SetPlayerSendingOnRadioChannel(player, radio.GetCurrentFrequence().ToString(), false);
                        }
                    }
                    catch(Exception ex)
                    {
                        Alt.Server.LogError(ex.ToString());
                        player.SendNotificationError("Erreur dans la saisie");
                    }
                    break;

                case "ChangeFrequence":
                    radio.CurrentFrequence = int.Parse(args[1].ToString());
                    Voice.SetPlayerSendingOnRadioChannel(player, radio.GetCurrentFrequence().ToString(), false);
                    break;
                case "ChangeVolume":
                    radio.Volume = int.Parse(args[1].ToString());
                    break;
                default:
                    Alt.Server.LogError("RadioManager RadioChange Hm args[0] is not valid... problem in client side ? args 0 mmust be the event name");
                    break;
            }
        }
    }
}
