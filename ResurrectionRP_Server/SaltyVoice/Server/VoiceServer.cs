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
            Voice.ServerUniqueIdentifier = Config.GetSetting<string>("ServerUniqueIdentifier");
            Voice.RequiredUpdateBranch = Config.GetSetting<string>("RequiredUpdateBranch");
            Voice.MinimumPluginVersion = Config.GetSetting<string>("MinimumPluginVersion");
            Voice.SoundPack = Config.GetSetting<string>("SoundPack");
            Voice.IngameChannel = Config.GetSetting<string>("IngameChannel");
            Voice.IngameChannelPassword = Config.GetSetting<string>("IngameChannelPassword");

            Alt.OnClient(SaltyShared.Event.Voice_RejectedVersion, OnRejectedVersion);
            Alt.OnClient(SaltyShared.Event.Voice_TalkingOnRadio, OnPlayerTalkingOnRadio);
            Alt.OnClient(SaltyShared.Event.Voice_IsTalking, OnPlayerTalking);
        }

        public static void OnPlayerDisconnected(IPlayer client)
        {
            Voice.RemovePlayerRadioChannel(client);

            if (!client.GetSyncedMetaData(SaltyShared.SharedData.Voice_TeamSpeakName, out object tsName))
                return;

            foreach (IPlayer cl in Alt.GetAllPlayers())
            {
                if (!cl.Exists)
                    continue;
                cl.EmitLocked(SaltyShared.Event.Player_Disconnected, tsName);
            }
        }

        private static void OnRejectedVersion(IPlayer client, object[] args)
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

            client.Kick("SaltyChat version");
        }

        public static void OnPlayerTalking(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            if (!client.GetSyncedMetaData(SaltyShared.SharedData.Voice_TeamSpeakName, out object tsName))
                return;

            foreach (PlayerHandler cl in PlayerManager.GetPlayersList())
            {
                if (!cl.Client.Exists)
                    continue;
                cl.Client.Emit(SaltyShared.Event.Voice_IsTalking, tsName, (bool)args[0]);
            }
        }

        public static void OnPlayerTalkingOnRadio(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            string radioChannel = args[0].ToString();
            bool isSending = (bool)args[1];
            Voice.SetPlayerSendingOnRadioChannel(client, radioChannel, isSending);
        }

        public static void OnSetRadioChannel(IPlayer client, string radioChannel)
        {
            if (String.IsNullOrWhiteSpace(radioChannel))
                return;

            Voice.RemovePlayerRadioChannel(client);
            Voice.AddPlayerRadioChannel(client, radioChannel);

            client.EmitLocked(SaltyShared.Event.Voice_SetRadioChannel, radioChannel);
        }

        public static void OnLeaveRadioChannel(IPlayer client)
        {
            Voice.RemovePlayerRadioChannel(client);

            client.EmitLocked(SaltyShared.Event.Voice_SetRadioChannel, String.Empty);
        }

        #endregion

        #region Methods
        internal static string CreateTeamSpeakName()
        {
            string name;
            {
                name = Guid.NewGuid().ToString().Replace("-", "");

                if (name.Length > 30)
                {
                    name = name.Remove(29, name.Length - 30);
                }
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
                Voice.RadioChannels.TryAdd(radioChannel, new List<IPlayer> { client });
                Voice.PlayersTalkingOnRadioChannels.TryAdd(radioChannel, new List<IPlayer>());
            }
        }

        /// <summary>
        /// Removes player from all radio channels
        /// </summary>
        /// <param name="client"></param>
        public static void RemovePlayerRadioChannel(IPlayer client)
        {
            foreach (string radioChannel in Voice.GetRadioChannels(client))
            {
                Voice.RemovePlayerRadioChannel(client, radioChannel);
            }
        }

        /// <summary>
        /// Removes player from a specific radio channel
        /// </summary>
        /// <param name="client"></param>
        /// <param name="radioChannel"></param>
        public static void RemovePlayerRadioChannel(IPlayer client, string radioChannel)
        {
            if (Voice.PlayersTalkingOnRadioChannels.ContainsKey(radioChannel) && Voice.PlayersTalkingOnRadioChannels[radioChannel].Contains(client))
            {
                Voice.PlayersTalkingOnRadioChannels[radioChannel].Remove(client);

                if (Voice.PlayersTalkingOnRadioChannels[radioChannel].Count == 0)
                    Voice.PlayersTalkingOnRadioChannels.Remove(radioChannel, out List<IPlayer> value);

                if (client.GetSyncedMetaData(SaltyShared.SharedData.Voice_TeamSpeakName, out object tsName))
                {
                    foreach (IPlayer radioClient in Voice.RadioChannels[radioChannel])
                        radioClient.EmitLocked(SaltyShared.Event.Voice_TalkingOnRadio, tsName, false);
                }
            }

            if (Voice.RadioChannels.ContainsKey(radioChannel) && Voice.RadioChannels[radioChannel].Contains(client))
            {
                Voice.RadioChannels[radioChannel].Remove(client);

                if (Voice.RadioChannels[radioChannel].Count == 0)
                {
                    Voice.RadioChannels.Remove(radioChannel, out List<IPlayer> value);
                    Voice.PlayersTalkingOnRadioChannels.Remove(radioChannel, out List<IPlayer> tsName);
                }
            }
        }

        public static void SetPlayerSendingOnRadioChannel(IPlayer client, string radioChannel, bool isSending)
        {
            if (!Voice.RadioChannels.ContainsKey(radioChannel) || !Voice.RadioChannels[radioChannel].Contains(client) || !client.GetSyncedMetaData(SaltyShared.SharedData.Voice_TeamSpeakName, out object tsName))
                return;

            try
            {
                if (isSending && !Voice.PlayersTalkingOnRadioChannels[radioChannel].Contains(client))
                {
                    Voice.PlayersTalkingOnRadioChannels[radioChannel].Add(client);

                    foreach (IPlayer radioClient in Voice.RadioChannels[radioChannel])
                        radioClient.EmitLocked(SaltyShared.Event.Voice_TalkingOnRadio, tsName, true, radioChannel);
                }
                else if (!isSending && Voice.PlayersTalkingOnRadioChannels[radioChannel].Contains(client))
                {
                    Voice.PlayersTalkingOnRadioChannels[radioChannel].Remove(client);

                    foreach (IPlayer radioClient in Voice.RadioChannels[radioChannel])
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