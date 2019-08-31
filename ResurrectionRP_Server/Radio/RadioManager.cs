using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using RadioModes = ResurrectionRP_Server.Radio.Data.RadioModes;

namespace ResurrectionRP_Server.Radio
{

    public class FrequenceRadio
    {
        private List<IPlayer> playersInFrequence = new List<IPlayer>();
        private List<IPlayer> speakers = new List<IPlayer>();

        public FrequenceRadio(IPlayer player)
        {
            playersInFrequence.Add(player);
        }

        public void RemovePlayer(IPlayer player)
        {
            if (playersInFrequence.Contains(player))
                playersInFrequence.Remove(player);

            RemoveSpeaker(player);
        }

        public void RemoveSpeaker(IPlayer player)
        {
            if (speakers.Contains(player))
                speakers.Remove(player);
        }

        public void AddSpeaker(IPlayer player)
        {
            if (!speakers.Contains(player)) speakers.Add(player);
        }

        public void AddPlayerInFrequence(IPlayer player)
        {
            if (!playersInFrequence.Contains(player)) playersInFrequence.Add(player);
        }

        public List<IPlayer> GetAllPlayersInFrequence() => playersInFrequence;
        public List<IPlayer> GetAllSpeakersInFrequence() => speakers;
    }
    public class RadioManager
    {
        #region Private static properties
        private static ConcurrentDictionary<IPlayer, Radio> _clientMenus = new ConcurrentDictionary<IPlayer, Radio>();
        #endregion

        public RadioManager()
        {
            Alt.OnClient("RadioManager", EventTrigered);
            Alt.OnClient("RadioManager", RadioChange);
        }

        private async void RadioChange(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            var player = client;

            var ph = player.GetPlayerHandler();
            if (ph == null)
                return;

            switch (args[0])
            {
                case "Open":
                    await OpenRadio(player, ph.RadioSelected);
                    break;

                case "Use":
                    await ph.RadioSelected?.UseRadio(player);
                    break;
                case "DoesntUse":
                    await ph.RadioSelected?.DontUse(player);
                    break;
                default:
                    Alt.Server.LogError("RadioManager RadioChange Hm args[0] is not valid... problem in client side ? args 0 mmust be the event name");
                    break;
            }
        }

        public static async Task Close(IPlayer client)
        {
            Radio radio = null;
            if (_clientMenus.TryGetValue(client, out radio))
                await radio.HideRadio(client);
        }

        public static async Task<bool> OpenRadio(IPlayer client, Radio radio)
        {
            Radio oldRadio = null;
            _clientMenus.TryRemove(client, out oldRadio);

            if (oldRadio != null)
            {
                // oldRadio.ShutdownRadio();     
            }

            if (_clientMenus.TryAdd(client, radio))
            {
                await radio.OpenRadio(client);
               // await client.EmitAsync("ShowCursor", true);
                return true;
            }
            return false;
        }

        private async void EventTrigered(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            var player = client;

            var ph = player.GetPlayerHandler();
            if (ph == null)
                return;

            Radio radio = null;
            _clientMenus.TryGetValue(player, out radio);

            if (radio == null)
                return;

            switch (args[0])
            {
                case "OnOff":
                    radio.Statut = (RadioModes)args[2];
                    if (radio.Statut != RadioModes.OFF)
                    {
                        //await GameMode.Instance.VoiceController.OnSetRadioChannel(player, radio.Frequence.ToString());
                        await SaltyServer.Voice.SetPlayerSendingOnRadioChannel(player, radio.GetCurrentFrequence().ToString(), false);
                    }
                    else
                    {
                        //radio.Statut = RadioModes.OFF;
                        await SaltyServer.Voice.RemovePlayerRadioChannel(player);
                    }
                    break;

                case "Close":     
                    if(_clientMenus.TryRemove(player, out radio))
                        await radio.HideRadio(player);
                    break;

                case "SaveFrequence":
                    try
                    {
                        radio.SaveFrequeceRadio(Convert.ToInt32(args[2].ToString()), Convert.ToDouble(args[3]));
                        await player.GetPlayerHandler()?.Update();
                    }
                    catch
                    {
                        await player.SendNotificationError("Erreur dans la saisie");
                    }
                    break;

                case "ChangeFrequence":
                    radio.CurrentFrequence = (byte)args[2];
                    //await SaltyServer.Voice.SetPlayerSendingOnRadioChannel(player, radio.Frequence.ToString(), false);
                    break;
                default:
                    Alt.Server.LogError("RadioManager RadioChange Hm args[0] is not valid... problem in client side ? args 0 mmust be the event name");
                    break;
            }
            return;
        }

        public int FindRadioInItemList(Radio radio, List<Items.RadioItem> itemList)
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                if (itemList[i].Radio == radio) return i;
            }
            return -1;
        }
    }
}
