using AltV.Net;
using System;

namespace ResurrectionRP_Server.Utils.TopServer
{
    public static class Vote
    {
        public static string ServerToken;

        public static void InitVotePlugin()
        {
            string token = Config.GetSetting<string>("GTAVote_Token");
            int port = Config.GetSetting<int>("GTAVote_Port");
            if (token != "")
            {
                ServerToken = token;

                new Receptor(new Action<string, string, string, string, string>(CallbackAfterVote), port);

                Alt.Emit("onVotePluginInit", new object[]
                {
                    true
                });
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
            
            Alt.Emit("onPlayerVote", new object[]
            {
                playername,
                vote_ip,
                date
            });
            /*
            if (version != this.currentVersion && !this.newVersionNotified)
            {
                this.newVersionNotified = true;
                Debug.WriteLine("[VotePlugin] Attention : Une nouvelle version du plugin de vote est disponible !");
                Debug.WriteLine("[VotePlugin] Pensez à la télécharger pour profiter des dernières fonctionnalités et des éventuelles corrections de bugs.");
                Debug.WriteLine("[VotePlugin] Lien de teléchargement de la nouvelle version : https://top-serveurs.net/plugin/fivem/latest");
            }*/
        }
    }
}
