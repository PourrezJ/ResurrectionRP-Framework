using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server;
using ResurrectionRP_Server.Entities.Players;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaltyServer
{
    public static class Voice
    {
        #region Properties
        public static string ServerUniqueIdentifier { get; private set; }
        public static string RequiredUpdateBranch { get; private set; }
        public static string MinimumPluginVersion { get; private set; }
        public static string SoundPack { get; private set; }
        public static string IngameChannel { get; private set; }
        public static string IngameChannelPassword { get; private set; }
        
        private static ConcurrentDictionary<string, List<IPlayer>> RadioChannels = new ConcurrentDictionary<string, List<IPlayer>>();
        private static ConcurrentDictionary<string, List<IPlayer>> PlayersTalkingOnRadioChannels = new ConcurrentDictionary<string, List<IPlayer>>();
        private static List<string> TsNames = new List<string>();
        #endregion

        #region Events

        public static void Init()
        {
            ServerUniqueIdentifier = Config.GetSetting<string>("ServerUniqueIdentifier");
            RequiredUpdateBranch = Config.GetSetting<string>("RequiredUpdateBranch");
            MinimumPluginVersion = Config.GetSetting<string>("MinimumPluginVersion");
            SoundPack = Config.GetSetting<string>("SoundPack");
            IngameChannel = Config.GetSetting<string>("IngameChannel");
            IngameChannelPassword = Config.GetSetting<string>("IngameChannelPassword");

            Alt.OnPlayerDisconnect += OnPlayerDisconnect;
            Alt.OnClient<IPlayer, string, string>(SaltyShared.Event.Voice_RejectedVersion, OnRejectedVersion);
            Alt.OnClient<IPlayer, string, bool>(SaltyShared.Event.Voice_TalkingOnRadio, OnPlayerTalkingOnRadio);
            Alt.OnClient<IPlayer, bool>(SaltyShared.Event.Voice_IsTalking, OnPlayerTalking);
        }

        public static void OnPlayerDisconnect(IPlayer client, string reason)
        {
            RemovePlayerRadioChannel(client);

            if (!client.GetSyncedMetaData(SaltyShared.SharedData.Voice_TeamSpeakName, out object tsName))
                return;

            foreach (IPlayer cl in Alt.GetAllPlayers().ToArray())
            {
                if (!cl.Exists)
                    continue;

                cl.EmitLocked(SaltyShared.Event.Player_Disconnected, tsName);
            }
        }

        private static void OnRejectedVersion(IPlayer client, string updateBranch, string version)
        {
            if (!client.Exists)
                return;

            if (string.IsNullOrWhiteSpace(RequiredUpdateBranch) && string.IsNullOrWhiteSpace(MinimumPluginVersion))
                return;

            if (!string.IsNullOrWhiteSpace(RequiredUpdateBranch) && updateBranch != RequiredUpdateBranch)
                client.SendNotification($"[Salty Chat] Required update branch: {RequiredUpdateBranch} | Your update branch: {updateBranch}");
            else
                client.SendNotification($"[Salty Chat] Required version: {MinimumPluginVersion} | Your version: {version}");

            client.Kick("SaltyChat version");
        }

        public static void OnPlayerTalking(IPlayer client, bool isTalking)
        {
            if (!client.Exists)
                return;

            if (!client.GetSyncedMetaData(SaltyShared.SharedData.Voice_TeamSpeakName, out object tsName))
                return;

            foreach (PlayerHandler cl in PlayerManager.GetPlayersList())
            {
                if (!cl.Client.Exists)
                    continue;
                cl.Client.Emit(SaltyShared.Event.Voice_IsTalking, tsName, isTalking);
            }
        }

        public static void OnPlayerTalkingOnRadio(IPlayer client, string radioChannel, bool isSending)
        {
            if (!client.Exists)
                return;

            PlayerTalkingOnRadioChannel(client, radioChannel, isSending);
        }

        public static void SetRadioChannel(IPlayer client, string radioChannel)
        {
            if (string.IsNullOrEmpty(radioChannel))
                return;

            RemovePlayerRadioChannel(client);
            AddPlayerRadioChannel(client, radioChannel);
            client.EmitLocked(SaltyShared.Event.Voice_SetRadioChannel, radioChannel);
        }
        #endregion

        #region Methods
        internal static string CreateTeamSpeakName()
        {
            string name;
            {
                name = Guid.NewGuid().ToString().Replace("-", "");

                if (name.Length > 30)
                    name = name.Remove(29, name.Length - 30);
            }
            while (TsNames.Any(p => p == name));

            return name;
        }

        /// <summary>
        /// Returns all radio channels the client is currently in
        /// </summary>
        internal static List<string> GetRadioChannels(IPlayer client)
        {
            List<string> radioChannels = new List<string>();

            foreach (KeyValuePair<string, List<IPlayer>> radioChannel in RadioChannels)
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
            if (!RadioChannels.ContainsKey(radioChannel))
                RadioChannels.TryAdd(radioChannel, new List<IPlayer> { client });
            else if (!RadioChannels[radioChannel].Contains(client))
                RadioChannels[radioChannel].Add(client);

            PlayersTalkingOnRadioChannels.TryAdd(radioChannel, new List<IPlayer>());
        }

        /// <summary>
        /// Removes player from all radio channels
        /// </summary>
        /// <param name="client"></param>
        public static void RemovePlayerRadioChannel(IPlayer client)
        {
            foreach (string radioChannel in GetRadioChannels(client))
                RemovePlayerRadioChannel(client, radioChannel);
        }

        /// <summary>
        /// Removes player from a specific radio channel
        /// </summary>
        /// <param name="client"></param>
        /// <param name="radioChannel"></param>
        public static void RemovePlayerRadioChannel(IPlayer client, string radioChannel)
        {
            if (PlayersTalkingOnRadioChannels.ContainsKey(radioChannel) && PlayersTalkingOnRadioChannels[radioChannel].Contains(client))
            {
                PlayersTalkingOnRadioChannels[radioChannel].Remove(client);

                if (PlayersTalkingOnRadioChannels[radioChannel].Count == 0)
                    PlayersTalkingOnRadioChannels.Remove(radioChannel, out _);

                if (client.GetSyncedMetaData(SaltyShared.SharedData.Voice_TeamSpeakName, out object tsName))
                {
                    foreach (IPlayer radioClient in RadioChannels[radioChannel])
                        radioClient.EmitLocked(SaltyShared.Event.Voice_TalkingOnRadio, tsName, false);
                }
            }

            if (RadioChannels.ContainsKey(radioChannel) && RadioChannels[radioChannel].Contains(client))
            {
                RadioChannels[radioChannel].Remove(client);

                if (RadioChannels[radioChannel].Count == 0)
                {
                    RadioChannels.Remove(radioChannel, out List<IPlayer> value);
                    PlayersTalkingOnRadioChannels.Remove(radioChannel, out List<IPlayer> tsName);
                }
            }
        }

        public static void PlayerTalkingOnRadioChannel(IPlayer client, string radioChannel, bool isTalking)
        {
            if (!RadioChannels.ContainsKey(radioChannel) || !RadioChannels[radioChannel].Contains(client) || !client.GetSyncedMetaData(SaltyShared.SharedData.Voice_TeamSpeakName, out object tsName))
                return;

            try
            {
                if (isTalking && !PlayersTalkingOnRadioChannels[radioChannel].Contains(client))
                {
                    PlayersTalkingOnRadioChannels[radioChannel].Add(client);

                    foreach (IPlayer radioClient in RadioChannels[radioChannel])
                        radioClient.EmitLocked(SaltyShared.Event.Voice_TalkingOnRadio, tsName, true, radioChannel);
                }
                else if (!isTalking && PlayersTalkingOnRadioChannels[radioChannel].Contains(client))
                {
                    PlayersTalkingOnRadioChannels[radioChannel].Remove(client);

                    foreach (IPlayer radioClient in RadioChannels[radioChannel])
                        radioClient.EmitLocked(SaltyShared.Event.Voice_TalkingOnRadio, tsName, false, radioChannel);
                }
            }
            catch(Exception ex)
            {
                Alt.Server.LogError($"SetPlayerSendingOnRadioChannel - radioChannel: {radioChannel}, RadioChannels: {RadioChannels.ContainsKey(radioChannel)}, PlayersTalkingOnRadioChannels: {PlayersTalkingOnRadioChannels .ContainsKey(radioChannel)} - {ex}");
            }
        }
        #endregion
    }
}