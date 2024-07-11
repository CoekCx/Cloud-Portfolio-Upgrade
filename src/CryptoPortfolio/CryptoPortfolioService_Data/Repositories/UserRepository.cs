using CryptoPortfolioService_Data.Entities;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;

namespace CryptoPortfolioService_Data.Repositories
{
    public class UserRepository
    {        
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;

        public UserRepository()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            _table = tableClient.GetTableReference("UserTable"); 
            _table.CreateIfNotExists();
        }
        public IQueryable<User> RetrieveAllUsers()
        {
            var results = from g in _table.CreateQuery<User>()
                          where g.PartitionKey == "User"
                          select g;
            return results;
        }

        public void AddUsear(User newUser)
        { 
            TableOperation insertOperation = TableOperation.Insert(newUser);
            _table.Execute(insertOperation);
        }

        public bool Exists(string id)
        {
            return RetrieveAllUsers().Where(s => s.RowKey == id).FirstOrDefault() != null;
        }

        public bool IsEmailUnique(string email)
        {
            return RetrieveAllUsers().Where(s => s.Email == email).FirstOrDefault() != null;            
        }

        public void RemoveUser(string id)
        {
            User user = RetrieveAllUsers().Where(s => s.RowKey == id).FirstOrDefault();
            if (user != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(user);
                _table.Execute(deleteOperation);
            }
        }

        public User GetUser(string id)
        {
            return RetrieveAllUsers().Where(p => p.RowKey == id).FirstOrDefault();
        }
        
        public User GetUserByCredentials(string email, string password)
        {
            User user = RetrieveAllUsers().Where(p => p.Email == email && p.Password == password).FirstOrDefault();
            return user;
        }

        public void UpdateUser(User user)
        {
            TableOperation updateOperation = TableOperation.Replace(user);
            _table.Execute(updateOperation);
        }

        public bool IsPasswordValid(string email, string password)
        {
            return RetrieveAllUsers().Where(s => s.Email == email && s.Password == password).FirstOrDefault() != null;
        }
    }
}
