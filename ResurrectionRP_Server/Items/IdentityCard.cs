using AltV.Net.Elements.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Items
{
    class IdentityCard : Models.Item
    {
        // A MEDITER POUR FAIRE DES FAUX PAPIERS ...

        public IdentityCard(Models.InventoryData.ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "identitycard", string icon = "unknown-item", string classes = "basic") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
        }

        public override Task Use(IPlayer client, string inventoryType, int slot)
        {
            Models.Identite identite = client.GetPlayerHandler().Identite;

            List<Entities.Players.PlayerHandler> players = client.GetPlayersHandlerInRange(5f);
            foreach (Entities.Players.PlayerHandler player in players)
            {
                string paper = $"~r~Nom: ~w~{identite.LastName} \n" +
                    $"~r~Prénom: ~w~{identite.FirstName} \n" +
                    $"~r~Âge: ~w~{identite.Age} \n" +
                    $"~r~Nationalité: ~w~{identite.Nationalite}";

                player.Client.SendNotification("Carte d'identité\n" + paper);
            }
            return Task.CompletedTask;
        }
    }
}
