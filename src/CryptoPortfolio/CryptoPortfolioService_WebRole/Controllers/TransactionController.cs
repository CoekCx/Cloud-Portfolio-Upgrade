using CryptoPortfolioService_Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CryptoPortfolioService_WebRole.Utils;
using CryptoPortfolioService_Data.Entities.Enums;

namespace CryptoPortfolioService_WebRole.Controllers
{
    public class TransactionController : Controller
    {
        UserTransactionRepository _userTransactionRepository = new UserTransactionRepository();
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
            var transactions = _userTransactionRepository.GetTransactionByUser(user.RowKey);
            ViewBag.Transactions = transactions;
            return View();
        }

        public ActionResult NewTransaction()
        {
            return View();
        }
    }
}