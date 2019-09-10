using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaltyServer
{
    public class Voice
    {
        #region Properties
        public static string ServerUniqueIdentifier { get; private set; }
        public static string RequiredUpdateBranch { get; private set; }
        public static string MinimumPluginVersion { get; private set; }
        public static string SoundPack { get; private set; }
        public static string IngameChannel { get; private set; }
        public static string IngameChannelPassword { get; private set; }

        private static Dictionary<string, List<IPlayer>> RadioChannels = new Dictionary<string, List<IPlayer>>();
        private static Dictionary<string, List<IPlayer>> PlayersTalkingOnRadioChannels = new Dictionary<string, List<IPlayer>>();
        #endregion

        #region Events

        public void OnResourceStart()
        {
            Voice.ServerUniqueIdentifier = Config.GetSetting<string>("ServerUniqueIdentifier");
            Voice.RequiredUpdateBranch = Config.GetSetting<string>("RequiredUpdateBranch");
            Voice.MinimumPluginVersion = Config.GetSetting<string>("MinimumPluginVersion");
            Voice.SoundPack = Config.GetSetting<string>("SoundPack");
            Voice.IngameChannel = Config.GetSetting<string>("IngameChannel");
            Voice.IngameChannelPassword = Config.GetSetting<string>("IngameChannelPassword");

            AltAsync.OnClient(SaltyShared.Event.Voice_RejectedVersion, OnRejectedVersion);
            AltAsync.OnClient(SaltyShared.Event.Voice_IsTalking, OnPlayerTalking);
            AltAsync.OnClient(SaltyShared.Event.Voice_TalkingOnRadio, OnPlayerTalkingOnRadio);
        }

        public async Task OnPlayerConnected(IPlayer client)
        {
            await client.SetSyncedMetaDataAsync(SaltyShared.SharedData.Voice_TeamSpeakName, Voice.GetTeamSpeakName());
            await client.SetSyncedMetaDataAsync(SaltyShared.SharedData.Voice_VoiceRange, "Parler");

            await client.EmitAsync(SaltyShared.Event.Voice_Initialize, Voice.ServerUniqueIdentifier, Voice.RequiredUpdateBranch, Voice.MinimumPluginVersion, Voice.SoundPack, Voice.IngameChannel, Voice.IngameChannelPassword);
        }

        public void OnPlayerDisconnected(IPlayer client)
        {
            Voice.RemovePlayerRadioChannel(client);

            AltAsync.Do(() =>
            {
                if (!client.GetSyncedMetaData(SaltyShared.SharedData.Voice_TeamSpeakName, out object tsName))
                    return;

                foreach (IPlayer cl in Alt.GetAllPlayers())
                {
                    if (!cl.Exists)
                        continue;
                    cl.Emit(SaltyShared.Event.Player_Disconnected, tsName);
                }
            });
        }

        private async Task OnRejectedVersion(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            string updateBranch = args[0].ToString();
            string version = args[1].ToString();

            if (String.IsNullOrWhiteSpace(Voice.RequiredUpdateBranch) && String.IsNullOrWhiteSpace(Voice.MinimumPluginVersion))
                return;

            if (!String.IsNullOrWhiteSpace(Voice.RequiredUpdateBranch) && updateBranch != Voice.RequiredUpdateBranch)
                client.SendNotification($"[Salty Chat] Required update branch: {Voice.RequiredUpdateBranch} | Your update branch: {updateBranch}");
            else
                client.SendNotification($"[Salty Chat] Required version: {Voice.MinimumPluginVersion} | Your version: {version}");

            await client.KickAsync("SaltyChat version");
        }

        public async Task OnPlayerTalking(IPlayer client, object[] args)
        {
            // #warning There seems to be an issue where the "client"-object is not correctly referenced on the client, remove workaround if the issue is resolved

            await AltAsync.Do(() =>
            {
                if (!client.Exists)
                    return;

                if (!client.GetSyncedMetaData(SaltyShared.SharedData.Voice_TeamSpeakName, out object tsName))
                    return;

                foreach (IPlayer cl in Alt.GetAllPlayers())
                {
                    if (!cl.Exists)
                        continue;
                    cl.Emit(SaltyShared.Event.Voice_IsTalking, tsName, (bool)args[0]);
                }
            });
        }

        public async Task OnPlayerTalkingOnRadio(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            string radioChannel = args[0].ToString();
            bool isSending = (bool)args[1];
            await Voice.SetPlayerSendingOnRadioChannel(client, radioChannel, isSending);
        }

        public async Task OnSetRadioChannel(IPlayer client, string radioChannel)
        {
            if (String.IsNullOrWhiteSpace(radioChannel))
                return;

            await Voice.RemovePlayerRadioChannel(client);
            Voice.AddPlayerRadioChannel(client, radioChannel);

            await client.EmitAsync(SaltyShared.Event.Voice_SetRadioChannel, radioChannel);
        }

        public void OnLeaveRadioChannel(IPlayer client)
        {
            Voice.RemovePlayerRadioChannel(client);

            client.Emit(SaltyShared.Event.Voice_SetRadioChannel, String.Empty);
        }

        #endregion

        #region Methods
        internal static string GetTeamSpeakName()
        {
            string name;
            var playerList = Alt.GetAllPlayers();

            do
            {
                name = Guid.NewGuid().ToString().Replace("-", "");

                if (name.Length > 30)
                {
                    name = name.Remove(29, name.Length - 30);
                }
            }
            while (playerList.Any(p => p.GetSyncedMetaData(SaltyShared.SharedData.Voice_TeamSpeakName, out object tsName) && tsName.ToString() == name));

            return name;
        }

        /// <summary>
        /// Returns all radio channels the client is currently in
        /// </summary>
        internal static List<string> GetRadioChannels(IPlayer client)
        {
            List<string> radioChannels = new List<string>();

            foreach (KeyValuePair<string, List<IPlayer>> radioChannel in Voice.RadioChannels)
            {
                if (radioChannel.Value.Contains(client))
                    radioChannels.Add(radioChannel.Key);
            }

            return radioChannels;
        }

        /// <summary>
        /// Adds player to a radio channel
        /// </summary>
        /// <param name="client"></param>
        /// <param name="radioChannel"></param>
        public static void AddPlayerRadioChannel(IPlayer client, string radioChannel)
        {
            if (Voice.RadioChannels.ContainsKey(radioChannel) && Voice.RadioChannels[radioChannel].Contains(client))
            {
                return;
            }
            else if (Voice.RadioChannels.ContainsKey(radioChannel))
            {
                Voice.RadioChannels[radioChannel].Add(client);
            }
            else
            {
                Voice.RadioChannels.Add(radioChannel, new List<IPlayer> { client });
                Voice.PlayersTalkingOnRadioChannels.Add(radioChannel, new List<IPlayer>());
            }
        }

        /// <summary>
        /// Removes player from all radio channels
        /// </summary>
        /// <param name="client"></param>
        public static async Task RemovePlayerRadioChannel(IPlayer client)
        {
            foreach (string radioChannel in Voice.GetRadioChannels(client))
            {
                await Voice.RemovePlayerRadioChannel(client, radioChannel);
            }
        }

        /// <summary>
        /// Removes player from a specific radio channel
        /// </summary>
        /// <param name="client"></param>
        /// <param name="radioChannel"></param>
        public static async Task RemovePlayerRadioChannel(IPlayer client, string radioChannel)
        {
            await AltAsync.Do(() =>
            {
                if (Voice.PlayersTalkingOnRadioChannels.ContainsKey(radioChannel) && Voice.PlayersTalkingOnRadioChannels[radioChannel].Contains(client))
                {
                    Voice.PlayersTalkingOnRadioChannels[radioChannel].Remove(client);

                    if (Voice.PlayersTalkingOnRadioChannels[radioChannel].Count == 0)
                        Voice.PlayersTalkingOnRadioChannels.Remove(radioChannel);

                    if (client.GetSyncedMetaData(SaltyShared.SharedData.Voice_TeamSpeakName, out object tsName))
                    {
                        foreach (IPlayer radioClient in Voice.RadioChannels[radioChannel])
                            radioClient.Emit(SaltyShared.Event.Voice_TalkingOnRadio, tsName, false);
                    }
                }

                if (Voice.RadioChannels.ContainsKey(radioChannel) && Voice.RadioChannels[radioChannel].Contains(client))
                {
                    Voice.RadioChannels[radioChannel].Remove(client);

                    if (Voice.RadioChannels[radioChannel].Count == 0)
                    {
                        Voice.RadioChannels.Remove(radioChannel);
                        Voice.PlayersTalkingOnRadioChannels.Remove(radioChannel);
                    }
                }
            });
        }

        public static async Task SetPlayerSendingOnRadioChannel(IPlayer client, string radioChannel, bool isSending)
        {
            await AltAsync.Do(() =>
            {
                if (!Voice.RadioChannels.ContainsKey(radioChannel) || !Voice.RadioChannels[radioChannel].Contains(client) || !client.GetSyncedMetaData(SaltyShared.SharedData.Voice_TeamSpeakName, out object tsName))
                    return;

                if (isSending && !Voice.PlayersTalkingOnRadioChannels[radioChannel].Contains(client))
                {
                    Voice.PlayersTalkingOnRadioChannels[radioChannel].Add(client);

                    foreach (IPlayer radioClient in Voice.RadioChannels[radioChannel])
                    {
                        radioClient.Emit(SaltyShared.Event.Voice_TalkingOnRadio, tsName, true, radioChannel);
                    }
                }
                else if (!isSending && Voice.PlayersTalkingOnRadioChannels[radioChannel].Contains(client))
                {
                    Voice.PlayersTalkingOnRadioChannels[radioChannel].Remove(client);

                    foreach (IPlayer radioClient in Voice.RadioChannels[radioChannel])
                    {
                        radioClient.Emit(SaltyShared.Event.Voice_TalkingOnRadio, tsName, false, radioChannel);
                    }
                }
            });
        }
        #endregion
    }
}