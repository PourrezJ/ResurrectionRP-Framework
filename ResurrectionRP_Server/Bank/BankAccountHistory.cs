using System;
using System.Collections.Generic;
using System.Text;

namespace ResurrectionRP_Server.Bank
{
    public class BankAccountHistory
    {
        #region Variables
        public DateTime Date { get; set; }
        public string Text { get; set; }
        public double Amount { get; set; }
        public double Balance { get; set; }
        public string Details { get; set; }
        #endregion
    }
}
