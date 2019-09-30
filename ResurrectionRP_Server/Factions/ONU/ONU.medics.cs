using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Players.Data;

namespace ResurrectionRP_Server.Factions
{
    public partial class ONU
    {
        private void ONU_CallUrgenceMedic(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;


            var players = GetEmployeeOnline();

            if (players.Count > 0)
            {
                foreach (IPlayer player in players)
                {
                    if (player.Exists && player != client)
                        player.Emit("ONU_BlesseCalled", client, "INCONNU", JsonConvert.SerializeObject(client.Position.ConvertToEntityPosition())); ;
                }
            }
            client.Emit("ONU_Callback", ServicePlayerList.Count);
        }

        private void ONU_IAccept(IPlayer client, object[] args)
        {
            IPlayer victim = GameMode.Instance.PlayerList.Find(p => p.Id == ushort.Parse(args[0].ToString()));

            if (victim == null)
                return;

            if (!victim.Exists)
                return;


            DeadPlayer result = PlayerManager.DeadPlayers.FindLast(b => (b.Victime == victim));
            if (result != null)
            {
                result.TakeCall();
                client.Emit("ONU_BlesseCallTaken", victim);
            }

            PlayerManager.DeadPlayers.FindLast(b => (b.Victime == victim))?.TakeCall();
            client.Emit("ONU_IAccept", victim);

            var players = GetEmployeeOnline();

            if (players.Count > 0)
            {
                foreach (IPlayer medic in players)
                {
                    if (medic.Exists && medic != client)
                        medic.Emit("ONU_BlesseCalled_Accepted", victim, client.GetPlayerHandler()?.Identite.Name);
                }
            }
            victim.Emit("ONU_CallbackAccept");
        }

        private void ONU_BlesseRemoveBlip(IPlayer client, object[] args)
        {
            var blesse = args[0] as IPlayer;
            var players = GetEmployeeOnline();
            DeadPlayer result = PlayerManager.DeadPlayers.FindLast(b => (b.Victime == blesse));

            if (result != null)
            {
                result.Remove();
            }
            if (players.Count > 0)
            {
                foreach (IPlayer medic in players)
                {
                    if (medic.Exists)
                        medic.Emit("ONU_BlesseEnd", blesse);
                }
            }
        }

    }
}
