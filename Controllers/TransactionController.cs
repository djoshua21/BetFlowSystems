using BetFlowSystems.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BetFlowSystems.Controllers
{
    [Authorize(Roles = "Admin,Manager")]

    [Route("/Transactions/{action=Index}/{id?}")]

    public class TransactionController : Controller
    {

        private readonly TransactionService _transactionService;

        public TransactionController(TransactionService transactionService)
        {
            _transactionService = transactionService;

        }

        // GET: TransactionController
        public async Task<ActionResult> Index(string? search, int page = 1)
        {
            ViewBag.CurrentFilter = search;

            var pageSize = 10;

            var transactions = await _transactionService.GetAllTransactions(page, pageSize, search);

            return View(transactions);
        }

        public async Task<ActionResult> BetTransactions(int id, string? search, int page = 1)
        {
            ViewBag.CurrentFilter = search;

            var pageSize = 10;

            var transactions = await _transactionService.GetBetTransactions(id, page, pageSize, search);

            return View(transactions);
        }

    }
}
