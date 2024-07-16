using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace CryptoPortfolioService_Data.Entities
{
    public class UserTransaction : TableEntity
    {
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string CurrencyName { get; set; }
        public double Amount { get; set; }
        public double Fee { get; set; }
        public DateTime TransactionDate { get; set; }

        public UserTransaction()
        {
            PartitionKey = "UserTransaction";
            RowKey = Guid.NewGuid().ToString();
        }
    }
}
