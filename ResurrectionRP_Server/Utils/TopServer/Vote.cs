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
            if (token != ServerToken)
            {
                Alt.Server.LogError("[VotePlugin] Un vote a été émis avec une token incorrecte (la token est accessible sur votre fiche serveur top-serveurs.net).");
                return;
            }

            var player = Entities.Players.PlayerManager.GetAllPlayerHandlerCache().Values.FirstOrDefault(p => p.IP.ToString() == vote_ip || p.PID.ToLower() == playername.ToLower());

            if (player != null)
            {
                if (player.BankAccount.Owner == null)
                    player.BankAccount.Owner = player;
                player.BankAccount.AddMoney(PriceReward, true);

                Alt.Server.LogColored(player.PID + " viens de voté sur top serveur!");

                if (player.Client != null && player.Client.Exists)
                    player.Client.SendNotificationSuccess($"Vous avez reçu ${PriceReward} pour votre vote sur TopServeur!");

            }
        }
    }
}
