using AltV.Net;
using System;
using System.Linq;

namespace ResurrectionRP_Server.Utils.TopServer
{
    public static class Vote
    {
        public static string ServerToken;
        public static double PriceReward;

        public static void InitVotePlugin()
        {
            string token = Config.GetSetting<string>("GTAVote_Token");
            int port = Config.GetSetting<int>("GTAVote_Port");
            PriceReward = Config.GetSetting<int>("GTAVote_Reward");

            if (token != "")
            {
                ServerToken = token;

                new Receptor(new Action<string, string, string, string, string>(CallbackAfterVote), port);

                return;
            }
            Alt.Server.LogError("[VotePlugin] ERREUR : Vous devez spécifier la token de votre fiche serveur (sur votre panel top-serveurs.net) dans le fichier de config (config.ini).");
        }

        public static void CallbackAfterVote(string token, string playername, string vote_ip, string date, string version)
        {
            try
            {
                if (token != ServerToken)
                {
                    Alt.Server.LogError("[VotePlugin] Un vote a été émis avec une token incorrecte (la token est accessible sur votre fiche serveur top-serveurs.net).");
                    return;
                }

                var player = Entities.Players.PlayerManager.GetAllPlayerHandlerCache().Values.FirstOrDefault(p => p.IP.ToString() == vote_ip || p.PID.ToLower() == playername.ToLower());

                if (player != null)
                {
                    bool found = false;
                    var players = Alt.GetAllPlayers();
                    lock (players)
                    {
                        foreach(var client in players)
                        {
                            if (client.GetSocialClub() == player.PID)
                            {
                                found = true;
                                client.SendNotificationSuccess($"Vous avez reçu ${PriceReward} pour votre vote sur TopServeur!");

                                var ph = client.GetPlayerHandler();
                                if (ph != null)
                                {
                                    ph.AddMoney(PriceReward);
                                    ph.UpdateInBackground();
                                }
                                return;
                            }
                        }
                    }

                    if (!found)
                    {
                        player.Rewards++;
                        player.AddMoney(PriceReward);
                        player.UpdateInBackground();
                    }

                    Alt.Server.LogColored(player.PID + " viens de voté sur top serveur!");
                }
            }
            catch(Exception ex)
            {
                Alt.Server.LogError(ex.ToString());
            }
        }
    }
}
