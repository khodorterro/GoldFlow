using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer;

namespace BussinnessLayer
{
    public class Transactions
    {
        private Transactionsdata dal = new Transactionsdata();

        public bool CreateTransaction(Transaction transaction)
        {
            // Basic validation can be done here
            if (transaction.GoldPrice <= 0 || transaction.Liras < 0)
                throw new ArgumentException("Invalid gold price or amount");

            // Business logic: Calculate total price if not provided
            transaction.TotalPrice = transaction.GoldPrice * transaction.Ounces;

            // Set the date if not set
            if (transaction.Date == default)
                transaction.Date = DateTime.Now;

            return dal.AddTransaction(transaction);
        }

        public List<Transaction> GetTransactions()
        {
            return dal.GetAllTransactions();
        }

        public Transaction GetTransactionById(int transactionID)
        {
            return dal.GetTransactionById(transactionID);
        }


        public List<Transaction> GetTransactionByTraderID(string traderID)
        {
            if (string.IsNullOrWhiteSpace(traderID))
                throw new ArgumentException("Trader ID cannot be empty");

            return dal.GetTransactionByTraderID(traderID);
        }

    }
}
