using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Utils.Enums;

namespace ResurrectionRP_Server.Entities.Players
{
    public class PlayerCommands
    {
        public PlayerCommands()
        {
            Chat.RegisterCmd("tp", TpCoord);
            Chat.RegisterCmd("cls", Cls);
            Chat.RegisterCmd("cloth", Cloth);
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
        public void Cls(IPlayer player, string[] args)
        {
            player.Emit("EmptyChat");
        }

        private void Cloth(IPlayer player, object[] args)
        {
            Task.Run(async () =>
            {
                await player.SetClothAsync((Models.ClothSlot)Convert.ToInt32(args[0]), (int)args[1], (int)args[2], (int)args[3]);
            });
        }
    }
}
