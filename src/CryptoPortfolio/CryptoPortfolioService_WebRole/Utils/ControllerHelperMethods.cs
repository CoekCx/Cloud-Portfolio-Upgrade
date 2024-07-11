using CryptoPortfolioService_Data.Entities;
using CryptoPortfolioService_Data.Repositories;
using System.Web;

namespace CryptoPortfolioService_WebRole.Utils
{
    internal class ControllerHelperMethods
    {
        UserRepository _userRepository = new UserRepository();

        internal User GetUserFromSession()
        {            
            string userRowKey = (string)HttpContext.Current.Session["userRowKey"];
            if (userRowKey == null)
                return null;

            return _userRepository.GetUser(userRowKey);
        }
    }
}