using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Players;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Bank
{
    #region Public enums
    public enum AtmType
    {
        ATM,
        Bank,
        Faction,
        Business
    }
    #endregion

    public class BankMenu
    {
        #region Private fields
        private PlayerHandler _player;
        private BankAccount _bankAccount;
        private Menu _bankMenu;
        private AtmType _atmType;
        private Menu.MenuCallback _businessMenuCallback;
        #endregion

        #region Constructors
        public BankMenu(PlayerHandler player, BankAccount bankAccount, AtmType atmType)
        {
            _player = player;
            _bankAccount = bankAccount;
            _atmType = atmType;
            _bankMenu = null;
            _businessMenuCallback = null;
        }

        public BankMenu(PlayerHandler player, BankAccount bankAccount, AtmType atmType, Menu businessMenu, Menu.MenuCallback businessMenuCallback)
        {
            _player = player;
            _bankAccount = bankAccount;
            _atmType = atmType;
            _bankMenu = businessMenu;
            _businessMenuCallback = businessMenuCallback;
        }
        #endregion

        #region Menu
        public static async Task<Menu> OpenBankMenu(PlayerHandler player, BankAccount bankAccount, AtmType atmType = AtmType.ATM)
        {
            BankMenu BM = new BankMenu(player, bankAccount, atmType);
            return await BM.OpenBankMenu(player.Client);
        }

        public static async Task<Menu> OpenBankMenu(IPlayer client, BankAccount bankAccount, AtmType atmType, Menu businessMenu, Menu.MenuCallback businessMenuCallback)
        {
            #region Vérification
            PlayerHandler player = client.GetPlayerHandler();

            if (player == null)
                return null;
            #endregion
            
            BankMenu BM = new BankMenu(player, bankAccount, atmType, businessMenu, businessMenuCallback);
            return await BM.OpenBankMenu(client);
        }

        private async Task<Menu> OpenBankMenu(IPlayer client)
        {
            #region Creation
            if (_atmType == AtmType.ATM || _atmType == AtmType.Bank)
            {
                _bankMenu = new Menu("ATM_Menu", (_atmType == AtmType.ATM ? "ATM" : "Banque"), $"Compte: {_bankAccount.AccountNumber} | Solde: ${_bankAccount.Balance}", backCloseMenu: true);
                _bankMenu.BannerColor = new MenuColor(0, 0, 0, 0);
            }
            else
            {
                _bankMenu.SubTitle = $"Compte: {_bankAccount.AccountNumber} | Solde: ${_bankAccount.Balance}";
                _bankMenu.BackCloseMenu = false;
                _bankMenu.ItemSelectCallback = BankMenuCallback;
                _bankMenu.Reset();
            }
            #endregion

            #region Account History
            var historyItem = new MenuItem("Historique", executeCallback: true);
            historyItem.OnMenuItemCallback += OnHistoryAccount;
            _bankMenu.Add(historyItem);
            #endregion

            #region Withdraw
            var withdrawItem = new MenuItem("Retirer", null, "ID_Withdraw", executeCallback: true);
            withdrawItem.SetInput("", (_atmType == AtmType.ATM ? (byte)4 : (byte)9), InputType.UNumber, true);
            withdrawItem.OnMenuItemCallback += OnWithdrawItem;
            _bankMenu.Add(withdrawItem);
            #endregion

            #region Deposit
            var depositItem = new MenuItem("Déposer", null, "ID_Deposit", executeCallback: true);
            depositItem.SetInput("", (_atmType == AtmType.ATM ? (byte)4 : (byte)9), InputType.UNumber, true);
            depositItem.OnMenuItemCallback += OnDepositItem;
            _bankMenu.Add(depositItem);
            #endregion

            #region Transfert
            var transfertItem = new MenuItem("Transférer", executeCallback: true);
            transfertItem.OnMenuItemCallback += OnTransfertItem;
            _bankMenu.Add(transfertItem);
            #endregion

            #region End
            await _bankMenu.OpenMenu(client);
            #endregion

            return _bankMenu;
        }
        #endregion

        #region Callbacks
        private async Task BankMenuCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem != null)
                return;

            await _businessMenuCallback(client, menu, menuItem, itemIndex);
        }

        private async Task HistoryAccountCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            await OpenBankMenu(client);
        }

        private async Task OnHistoryAccount(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            _bankMenu.SubTitle = "Historique:";
            _bankMenu.BackCloseMenu = false;
            _bankMenu.ItemSelectCallback = HistoryAccountCallback;
            _bankMenu.Reset();
            List<BankAccountHistory> history = new List<BankAccountHistory>(_bankAccount.History);
            history = history.OrderByDescending(h => h.Date).ToList();

            int maxHistory = 0;

            if (history.Count > 50)
                maxHistory = 50;
            else
                maxHistory = history.Count;

            for (int i = 0; i < maxHistory; i++)
            {
                var item = new MenuItem(history[i].Text);
                item.Description = $"Date: {history[i].Date}\nMontant: ${history[i].Amount}";
                _bankMenu.Add(item);
            }

            await _bankMenu.OpenMenu(client);
        }

        private async Task OnWithdrawItem(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (uint.TryParse(menuItem.InputValue, out uint money) && money > 0)
            {
                bool save = !(_bankAccount == _player.BankAccount);

                if (! await _bankAccount.GetBankMoney(money, $"Retrait {((_atmType == AtmType.ATM) ? "ATM" : "Banque")}", save: save))
                {
                    await client.SendNotificationError("Vous n'avez pas assez d'argent sur ce compte en banque.");
                    await client.PlaySoundFrontEndFix(-1,"ERROR", "HUD_FRONTEND_DEFAULT_SOUNDSET");
                }
                else
                {
                    await _player.AddMoney(money);
                    await client.SendNotificationSuccess($"Vous avez retiré ${menuItem.InputValue} sur le compte {_bankAccount.AccountNumber}");
                }
            }
            else
            {
                await client.SendNotificationError("Problème de saisie.");
                await client.PlaySoundFrontEndFix(-1,"ERROR", "HUD_FRONTEND_DEFAULT_SOUNDSET");
            }

            if (_atmType == AtmType.ATM || _atmType == AtmType.Bank)
                await menu.CloseMenu(client);
        }

        private async Task OnDepositItem(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (uint.TryParse(menuItem.InputValue, out uint somme) && somme > 0)
            {
                if (!await _player.HasMoney(somme))
                {
                    await client.SendNotificationError("Vous n'avez pas assez d'argent sur vous.");
                }
                else
                {
                    _bankAccount.AddMoney(somme, $"Dépôt {((_atmType == AtmType.ATM) ? "ATM" : "Banque")}", true);
                    await client.SendNotificationSuccess($"Vous avez déposé ${menuItem.InputValue} sur le compte {_bankAccount.AccountNumber}");
                }
            }
            else
            {
                await client.SendNotificationError("Problème de saisie.");
                await client.PlaySoundFrontEndFix(-1, "ERROR", "HUD_FRONTEND_DEFAULT_SOUNDSET");
            }

            if (_atmType == AtmType.ATM || _atmType == AtmType.Bank)
                await menu.CloseMenu(client);
        }

        private async Task OnTransfertItem(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            await client.SendNotificationError("Pas encore implanté, S00N!");
        }
        #endregion
    }
}
