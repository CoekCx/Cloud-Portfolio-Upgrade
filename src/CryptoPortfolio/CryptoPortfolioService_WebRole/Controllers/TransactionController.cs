using CryptoPortfolioService_Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CryptoPortfolioService_WebRole.Utils;
using CryptoPortfolioService_Data.Entities.Enums;
using CryptoPortfolioService_Data.Entities;

namespace CryptoPortfolioService_WebRole.Controllers
{
    public class TransactionController : Controller
    {
        UserTransactionRepository _userTransactionRepository = new UserTransactionRepository();
        CryptoCurrencyRepository _cryptoCurrencyRepository = new CryptoCurrencyRepository();
        UserRepository _userRepository = new UserRepository();
        ControllerHelperMethods _helper= new ControllerHelperMethods();
        public ActionResult Transactions()
        {
            if (!_helper.LoggedInUserIsType(UserType.ADMIN))
            {
                return RedirectToAction("Login", "Authentication");
            }
            var transactions = _userTransactionRepository.RetrieveAllTransactions();
            ViewBag.Transactions = transactions;
            return View();
        }

        public ActionResult MyTransactions()
        {
            if (!_helper.LoggedInUserIsType(UserType.VISITOR))
            {
                return RedirectToAction("Login", "Authentication");
            }
            var user = _helper.GetUserFromSession();
            var transactions = _userTransactionRepository.GetTransactionByUser(user.Email);
            ViewBag.Transactions = transactions;
            return View();
        }

        public ActionResult NewTransaction()
        {
            if (!_helper.LoggedInUserIsType(UserType.VISITOR))
            {
                return RedirectToAction("Login", "Authentication");
            }
            var user = _helper.GetUserFromSession();
            var currencies = _cryptoCurrencyRepository.RetrieveAllCurrenciesForUser(user.RowKey);
            var currencyNames = currencies.Select(x => x.CurrencyName).ToList();
            ViewBag.CurrencyNames = currencyNames;
            return View();
        }

        [HttpPost]
        public ActionResult CreateTransaction(string email, string currency, int amount)
        {
            if (!_helper.LoggedInUserIsType(UserType.VISITOR))
            {
                return RedirectToAction("Login", "Authentication");
            }

            var user = _helper.GetUserFromSession();
            var recipient = _userRepository.GetUserByEmail(email);

            if (recipient == null || recipient.Type != UserType.VISITOR.ToString() || user.RowKey == recipient.RowKey)
            {
                ViewBag.ErrorMessage = "Invalid recipient email.";
                var currencies = _cryptoCurrencyRepository.RetrieveAllCurrenciesForUser(user.RowKey);
                var currencyNames = currencies.Select(x => x.CurrencyName).ToList();
                ViewBag.CurrencyNames = currencyNames;
                return View("NewTransaction");
            }

            var userCurrency = _cryptoCurrencyRepository.RetrieveCurrencyForUser(currency, user.RowKey);
            if (userCurrency == null || userCurrency.Quantity < amount)
            {
                ViewBag.ErrorMessage = "Insufficient funds.";
                var currencies = _cryptoCurrencyRepository.RetrieveAllCurrenciesForUser(user.RowKey);
                var currencyNames = currencies.Select(x => x.CurrencyName).ToList();
                ViewBag.CurrencyNames = currencyNames;
                return View("NewTransaction");
            }

            var profitPercentage = (double)amount / userCurrency.Quantity;
            var profit = userCurrency.Profit * profitPercentage;

            var transaction = new UserTransaction
            {
                SenderEmail = user.Email,
                ReceiverEmail = recipient.Email,
                CurrencyName = currency,
                Amount = amount,
                Fee = profit*CalculateTransactionFee(amount),
                TransactionDate = DateTime.UtcNow
            };

            _userTransactionRepository.AddTransaction(transaction);

            userCurrency.Quantity -= amount;
            userCurrency.Profit -= profit;

            if (userCurrency.Quantity == 0)
            {
                _cryptoCurrencyRepository.RemoveCryptoCurrency(userCurrency.RowKey);
            }
            else
            {
                _cryptoCurrencyRepository.UpdateCryptoCurrency(userCurrency);
            }
                

            var recipientCurrency = _cryptoCurrencyRepository.RetrieveCurrencyForUser(currency, recipient.RowKey);
            if (recipientCurrency != null)
            {
                recipientCurrency.Quantity += amount;
                _cryptoCurrencyRepository.UpdateCryptoCurrency(recipientCurrency);
            }
            else
            {
                recipientCurrency = new CryptoCurrency();
                recipientCurrency.UserId = recipient.RowKey;
                recipientCurrency.CurrencyName = currency;
                recipientCurrency.Profit = profit;
                recipientCurrency.Quantity = amount;

                _cryptoCurrencyRepository.AddCryptoCurrency(recipientCurrency);
            }

            return RedirectToAction("MyTransactions");
        }

        private double CalculateTransactionFee(double amount)
        {
            // Example fee calculation
            return amount * 0.01;
        }
    }
}