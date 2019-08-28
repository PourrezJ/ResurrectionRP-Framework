﻿using System;
using System.Collections.Generic;
using System.Text;
using XMenuItem = ResurrectionRP_Server.XMenuManager.XMenuItem;
using XMenu = ResurrectionRP_Server.XMenuManager.XMenu;
using System.Threading.Tasks;
using XMenuItemIcons = ResurrectionRP_Server.XMenuManager.XMenuItemIcons;
using InputType = ResurrectionRP_Server.Utils.Enums.InputType;
using AltV.Net;
using AltV.Net.Elements.Entities;

namespace ResurrectionRP_Server.Entities.Players
{
    public partial class PlayerHandler
    {
        public async Task OpenXtremAdmin()
        {
            var menu = new XMenu("ID_Admin");
            menu.Callback += XtreamCallBack;

            if (StaffRank >= Utils.Enums.AdminRank.Moderator)
            {
                var kick = new XMenuItem("Kick", "", "ID_Kick", XMenuItemIcons.REPORT);
                kick.SetInput("", 99, InputType.Text);
                menu.Add(kick);

                var ban = new XMenuItem("Ban", "", "ID_Ban", XMenuItemIcons.GAVEL_SOLID);
                ban.SetInput("", 99, InputType.Text);
                menu.Add(ban);

                var give = new XMenuItem("Give", "", "ID_Give", XMenuItemIcons.PAYMENT);
                give.SetInput("", 99, InputType.Text);
                menu.Add(give);

                menu.Add(new XMenuItem("Kill", "", "ID_Kill", XMenuItemIcons.CANCEL));
                menu.Add(new XMenuItem("Revive", "", "ID_Revive", XMenuItemIcons.HEART_SOLID));
                menu.Add(new XMenuItem("Heal", "", "ID_Heal", XMenuItemIcons.BRIEFCASE_MEDICAL_SOLID));
                menu.Add(new XMenuItem("Rassasier", "", "ID_Food", XMenuItemIcons.FASTFOOD));

            }

            await menu.OpenXMenu(Client);
        }

        private async Task XtreamCallBack(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            switch (menuItem.Id)
            {
                case "ID_Kick":
                    await client.SendNotificationSuccess($"Vous venez de kick {TargetHandler.Identite.Name}.");
                    await TargetClient.NotifyAsync($"Kick raison: {menuItem.InputValue}");
                    await Task.Delay(100);
                    TargetClient.Kick(menuItem.InputValue);
                    break;

                case "ID_Ban":
                    await client.SendNotificationSuccess($"Vous venez de ban {TargetHandler.Identite.Name}.");
                    await TargetClient.NotifyAsync($"Ban raison: {menuItem.InputValue}");
                    await Task.Delay(100);
                    await Models.BanManager.BanPlayer(TargetClient, menuItem.InputValue, new DateTime(2031, 1, 1));
                    break;

                case "ID_Give":
                    double money = Convert.ToDouble(menuItem.InputValue);
                    await TargetHandler.AddMoney(money);
                    await client.SendNotificationSuccess($"Vous venez de donner à {TargetHandler.Identite.Name} ${money}.");
                    break;

                case "ID_Kill":
                    await client.SendNotificationSuccess($"Vous venez de tuer {TargetHandler.Identite.Name}.");
                    TargetClient.Health = 0;
                    break;

                case "ID_Revive":
                    await client.SendNotificationSuccess($"Vous venez de revive {TargetHandler.Identite.Name}.");
                    await PlayerManager.Revive(TargetClient);
                    break;

                case "ID_Heal":
                    await client.SendNotificationSuccess($"Vous venez de soigner {TargetHandler.Identite.Name}.");
                    TargetClient.Health = (200);
                    break;
                case "ID_Food":
                    await client.SendNotificationSuccess($"Vous venez de rassasier {TargetHandler.Identite.Name}.");
                    TargetClient.GetPlayerHandler()?.UpdateHungerThirst(100, 100);
                    break;
            }
        }
    }
}