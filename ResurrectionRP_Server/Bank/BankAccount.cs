using AltV.Net.Elements.Entities;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Bank
{
    public enum AccountType
    {
        Personal,
        Society,
        Faction,
        Business
    }

    public class BankAccount
    {
        #region Static Variable
        public static List<BankAccount> BankAccountsList = new List<BankAccount>();
        #endregion

        #region Variables
        public AccountType AccountType { get; set; }
        public string AccountNumber { get; set; }
        public double Balance { get; private set; }

        [BsonIgnore]
        public object Owner { get; set; }

        public delegate Task OnDepositDelagate(IPlayer client);
        public delegate Task OnWithdrawDelegate(IPlayer client);
        [JsonIgnore]
        public OnDepositDelagate OnDeposit { get; set; }
        public OnWithdrawDelegate OnWithdraw { get; set; }

        public List<BankAccountHistory> History { get; private set; }
        #endregion

        #region Constructor
        public BankAccount(AccountType type, string accountNumber, double balance = 0)
        {
            AccountType = type;
            AccountNumber = accountNumber;
            Balance = balance;
            History = new List<BankAccountHistory>();
        }
        #endregion

        #region Method
        public void AddMoney(double money, string reason, bool save = true)
        {
            Balance += money;
            History.Add(new BankAccountHistory()
            {
                Amount = money,
                Balance = this.Balance,
                Date = DateTime.Now,
                Text = reason
            });

            if (save)
                Save();
        }

        public void AddMoney(double money, bool save = true)
        {
            Balance += money;

            if (save)
                Save();
        }

        public async Task<bool> GetBankMoney(double money, string reason, string details = null, bool save = true)
        {
            if (money == 0)
                return true;

            if (Balance - money >= 0)
            {
                Balance -= money;
                History.Add(new BankAccountHistory()
                {
                    Amount = money,
                    Balance = this.Balance,
                    Date = DateTime.Now,
                    Text = reason,
                    Details = details
                });

                if (save)
                    await Save();

                return true;
            }

            return false;
        }

        public static async Task<string> GenerateNewAccountNumber()
        {
            string _accountNumber = GenerateString();

            while (BankAccountsList.Exists(b => b.AccountNumber == _accountNumber))
            {
                _accountNumber = GenerateString();
                await Task.Delay(25);
            }

            return _accountNumber;
        }

        public void Clear() => Balance = 0;

        private static string GenerateString() =>
            $"{new Random().Next(1000000, 9999999)}";

        private async Task Save()
        {
            await Database.MongoDB.UpdateBankAccount(this);
        }
        #endregion
    }
}
