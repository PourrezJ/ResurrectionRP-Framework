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
        private static ConcurrentDictionary<IPlayer, Radio> _clientRadios = new ConcurrentDictionary<IPlayer, Radio>();
        #endregion

        public static void Init()
        {
            Alt.OnClient("RadioManager", EventTrigered);
        }
        public static void Close(IPlayer client)
        {
            Radio radio;

            if (_clientRadios.TryGetValue(client, out radio))
                radio.CloseRadio(client);
        }

        public static bool OpenRadio(IPlayer client, Radio radio)
        {
            _clientRadios.TryRemove(client, out _);

            if (_clientRadios.TryAdd(client, radio))
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
            _clientRadios.TryGetValue(player, out radio);

            if (radio == null)
                return;

            switch (args[0])
            {
                case "Open":
                    OpenRadio(player, ph.RadioSelected);
                    break;

                case "OnOff":
                    radio.Statut = ((bool)args[1]) ? RadioModes.LISTENING : RadioModes.OFF;

                    if (radio.Statut == RadioModes.OFF)
                        Voice.RemovePlayerRadioChannel(player);
                    else
                        Voice.SetRadioChannel(player, radio.GetCurrentFrequence().ToString());
                    break;

                case "Close":     
                    if(_clientRadios.TryRemove(player, out radio))
                        radio.CloseRadio(player);
                    break;

                case "SaveFrequence":
                    try
                    {
                        if (args[2] == null)
                            return;

                        if (double.TryParse(args[2].ToString(),  NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double frequence))
                        {
                            radio.SaveFrequeceRadio(Convert.ToInt32(args[1]), frequence);
                            Voice.SetRadioChannel(player, radio.GetCurrentFrequence().ToString());
                            player.GetPlayerHandler().UpdateFull();
                        }
                    }
                    catch(Exception ex)
                    {
                        Alt.Server.LogError(ex.ToString());
                        player.SendNotificationError("Erreur dans la saisie");
                    }
                    break;

                case "ChangeChannel":
                    radio.CurrentChannel = int.Parse(args[1].ToString());
                    Voice.SetRadioChannel(player, radio.GetCurrentFrequence().ToString());
                    break;

                case "ChangeVolume":
                    radio.Volume = int.Parse(args[1].ToString());
                    break;

                default:
                    break;
            }
        }
    }
}
