using AlternateLife.RageMP.Net.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResurrectionRP.Server
{
    class IdentityCard : Item
    {
        // A MEDITER POUR FAIRE DES FAUX PAPIERS ...

        public IdentityCard(ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "identitycard", string icon = "unknown-item", string classes = "basic") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice,type, icon, classes)
        {
        }

        public override async Task Use(IPlayer client, string inventoryType, int slot)
        {
            Identite identite = PlayerManager.GetPlayerByClient(client).Identite;

            List<PlayerHandler> players = await PlayerManager.GetNearestPlayers(client, 5f);
            foreach (PlayerHandler player in players)
            {
                string paper = $"~r~Nom: ~w~{identite.LastName} \n" +
                    $"~r~Prénom: ~w~{identite.FirstName} \n" +
                    $"~r~Âge: ~w~{identite.Age} \n" +
                    $"~r~Nationalité: ~w~{identite.Nationalite}";

                await player.Client.SendNotificationPicture(paper, CharPicture.CHAR_DEFAULT, false, 0, "Carte d'identité", "");
            }
                
        }
    }
}
