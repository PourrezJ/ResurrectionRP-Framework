using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;

namespace ResurrectionRP_Server.Illegal
{
    public class IllegalCommands
    {
        public IllegalCommands()
        {
            Chat.RegisterCmd("createweedlabs", CreateWeedLabs);
        }

        private Task CreateWeedLabs(IPlayer player, string[] args)
        {

            return Task.CompletedTask;
        }
    }
}
