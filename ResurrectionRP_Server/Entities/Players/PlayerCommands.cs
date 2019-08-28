using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Utils.Enums;

namespace ResurrectionRP_Server.Entities.Players
{
    public class PlayerCommands
    {
        public PlayerCommands()
        {
            Chat.RegisterCmd("tp", TpCoord);
        }

        private void TpCoord(IPlayer player, string[] args)
        {
            try
            {
                if (player.GetPlayerHandler().StaffRank <= AdminRank.Player)
                    return;

                player.Position = (new Vector3(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2])));
            }
            catch
            {
                player.SendChatMessage("Erreur dans la saisie des coordonnées");
            }
        }
    }
}
