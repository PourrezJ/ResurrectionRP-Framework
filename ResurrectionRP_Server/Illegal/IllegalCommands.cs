using System.Threading.Tasks;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;

namespace ResurrectionRP_Server.Illegal
{
    public class IllegalCommands
    {
        public IllegalCommands()
        {
            Chat.RegisterCmd("createweedlabs", CreateWeedLabs);
        }

        private void CreateWeedLabs(IPlayer player, string[] args)
        {
            if (IllegalManager.WeedBusiness != null)
                IllegalManager.WeedBusiness.MakeDoor(new Models.Location(player.Position, player.Rotation));
        }
    }
}
