using BetFlowSystems.Models;
using BetFlowSystems.Models.DTOs.Bet;
using BetFlowSystems.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BetFlowSystems.Controllers
{
    [Authorize]
    [Route("/Bets/{action=Index}/{id?}")]
    public class BetController : Controller
    {

        private readonly BetService _betService;
        private readonly BetTransactionService _betTransactionService;

        public BetController(BetService betService, BetTransactionService betTransactionService)
        {
            _betService = betService;
            _betTransactionService = betTransactionService;

        }

        // GET: BetController
        public async Task<ActionResult> Index(string? search, int page = 1)
        {
            ViewBag.CurrentFilter = search;

            var pageSize = 10;

            var bets = await _betService.GetAllBets(page, pageSize, search);

            return View(bets);
        }

        public async Task<ActionResult> AccountBets(int id, string? search, int page = 1)
        {
            ViewBag.CurrentFilter = search;

            var pageSize = 10;

            var bets = await _betService.GetAccountBets(id, page, pageSize, search);

            return View(bets);
        }

        // GET: BetController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var bet = await _betService.GetBet(id);

            if (bet == null)
            {
                TempData["FeedbackMessage"] = $"Bet with ID {id} does not exist";
                TempData["FeedbackType"] = "error";
                return RedirectToAction(nameof(Index));
            }

            return View(bet);

        }

        // GET: BetController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: BetController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateBetDto createBetDto)
        {
            try
            {

                // AccountID (optional, but if supplied it must be valid)
                if (createBetDto.AccountID <= 0)
                    ModelState.AddModelError("AccountID", "Account ID must be greater than 0");

                // BetTypeID
                if (createBetDto.BetTypeID <= 0)
                    ModelState.AddModelError("BetTypeID", "Bet type is required");

                // BetAmount
                if (createBetDto.BetAmount <= 0)
                    ModelState.AddModelError("BetAmount", "Bet amount must be greater than 0");

                // PossibleWinAmount
                if (createBetDto.PossibleWinAmount <= 0)
                    ModelState.AddModelError("PossibleWinAmount", "Possible win amount must be greater than 0");

                // BetDate
                if (createBetDto.BetDate == default)
                    ModelState.AddModelError("BetDate", "Bet date is required");
                else if (createBetDto.BetDate.Date > DateTime.Today)
                    ModelState.AddModelError("BetDate", "Bet date cannot be in the future");

                if (!ModelState.IsValid)
                    return View(createBetDto);

                var result = await _betTransactionService.CreateBetAndTransaction(createBetDto);

                if (result.Success)
                {
                    TempData["FeedbackMessage"] = "Bet Created Successfully!";
                    TempData["FeedbackType"] = "success";
                }
                else
                {
                    TempData["FeedbackMessage"] = result.ErrorMessage;
                    TempData["FeedbackType"] = "error";
                    return View(createBetDto);
                }

                return RedirectToAction(nameof(Index));
                ;
            }
            catch
            {
                return View();
            }
        }

        // GET: BetController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {


            var bet = await _betService.GetBet(id);

            if (bet == null)
            {
                TempData["FeedbackMessage"] = $"Bet with ID {id} does not exist";
                TempData["FeedbackType"] = "error";
                return RedirectToAction(nameof(Index));
            }

            return View(bet);
        }

        // POST: BetController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(DisplayBetDto displayBetDto, string? redirect)
        {
            try
            {
                // Title

                if (!Enum.IsDefined<BetStatus>(displayBetDto.Result))
                {
                    ModelState.AddModelError("Result", "Invalid status");
                }



                var updateBetDto = new UpdateBetDto
                {
                    BetID = displayBetDto.BetID,
                    Result = displayBetDto.Result

                };

                var result = await _betTransactionService.UpdateBetAndTransaction(updateBetDto);

                Console.WriteLine(result.Success.ToString());
                Console.WriteLine(result.ErrorMessage);
                if (result.Success)
                {
                    TempData["FeedbackMessage"] = "Bet Updated Successfully!";
                    TempData["FeedbackType"] = "success";
                }
                else
                {
                    TempData["FeedbackMessage"] = result.ErrorMessage;
                    TempData["FeedbackType"] = "error";
                    return View(displayBetDto);
                }

                return RedirectToAction(nameof(Details), new { id = displayBetDto.BetID });

            }
            catch
            {
                return View();
            }
        }

    }
}
