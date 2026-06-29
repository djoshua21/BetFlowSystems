using BetFlowSystems.Models;
using BetFlowSystems.Models.DbModels;
using BetFlowSystems.Models.DTOs.Account;
using BetFlowSystems.Models.DTOs.User;
using BetFlowSystems.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BetFlowSystems.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    [Route("/Accounts/{action=Index}/{id?}")]
    public class AccountController : Controller
    {

        private readonly AccountService _accountService;

        public AccountController(AccountService accountService)
        {
            _accountService = accountService;

        }

        // GET: AccountController
        public async Task<ActionResult> Index(string? search, int page = 1)
        {
            ViewBag.CurrentFilter = search;

            var pageSize = 10;

            var accounts = await _accountService.GetAllAccounts(page, pageSize, search);

            return View(accounts);
        }

        public async Task<ActionResult> UserAccounts(int id, string? search, int page = 1)
        {
            ViewBag.CurrentFilter = search;

            var pageSize = 10;

            var accounts = await _accountService.GetUserAccounts(id, page, pageSize, search);

            //if (accounts.TotalItems == 0)
            //{
            //    TempData["FeedbackMessage"] = $"No Accounts found for User ID {id}";
            //    TempData["FeedbackType"] = "error";
            //    return RedirectToAction(nameof(Index));
            //}

            return View(accounts);
        }

        // GET: AccountController/Details/5
        public async Task<ActionResult> Details(int id)
        {

            var account = await _accountService.GetAccount(id);

            if (account == null)
            {
                TempData["FeedbackMessage"] = $"Account with ID {id} does not exist";
                TempData["FeedbackType"] = "error";
                return RedirectToAction(nameof(Index));
            }

            return View(account);
        }

        // GET: AccountController/Create
        public ActionResult Create(int? uID)
        {
            if (uID != null)
            {
                var account = new CreateAccountDto { UserID = (int)uID };
                return View(account);

            }

            return View();
        }

        // POST: AccountController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateAccountDto createAccountDto)
        {
            try
            {
                // Title
                if (string.IsNullOrWhiteSpace(createAccountDto.Title))
                    ModelState.AddModelError("Title", "Title is required");
                else if (createAccountDto.Title.Length > 50)
                    ModelState.AddModelError("Title", "Title cannot exceed 50 characters");

                // Description
                if ((createAccountDto.Description ?? "").Length > 200)
                    ModelState.AddModelError("Description", "Description cannot exceed 200 characters");

                if (!ModelState.IsValid)
                    return View(createAccountDto);

                var result = await _accountService.CreateAccount(createAccountDto);

                if (result.Success)
                {
                    TempData["FeedbackMessage"] = "Account Created Successfully!";
                    TempData["FeedbackType"] = "success";
                }
                else
                {
                    TempData["FeedbackMessage"] = result.ErrorMessage;
                    TempData["FeedbackType"] = "error";
                    return View(createAccountDto);
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AccountController/Edit/5
        public async Task<ActionResult> Edit(int id, string redirect)
        {
            var account = await _accountService.GetAccount(id);

            if (account == null)
            {
                TempData["FeedbackMessage"] = $"Account with ID {id} does not exist";
                TempData["FeedbackType"] = "error";
                return RedirectToAction(nameof(Index));
            }
            //ViewBag.Redirect = redirect;
            return View(account);
        }

        // POST: AccountController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string? redirect, DisplayAccountDto displayAccountDto)
        {
            try
            {

                // Title
                if (string.IsNullOrWhiteSpace(displayAccountDto.Title))
                    ModelState.AddModelError("Title", "Title is required");
                else if (displayAccountDto.Title.Length > 50)
                    ModelState.AddModelError("Title", "Title cannot exceed 50 characters");

                // Description
                if ((displayAccountDto.Description ?? "").Length > 200)
                    ModelState.AddModelError("Description", "Description cannot exceed 200 characters");

                if (!ModelState.IsValid)
                {
                    //ViewBag.Redirect = redirect;
                    return View(displayAccountDto);
                }


                var account = new UpdateAccountDto
                {
                    AccountId = displayAccountDto.AccountID,
                    Title = displayAccountDto.Title,
                    Description = displayAccountDto.Description,
                    AccountStatus = displayAccountDto.AccountStatus

                };

                var result = await _accountService.UpdateAccount(account);

                if (result.Success)
                {
                    TempData["FeedbackMessage"] = "Account Updated Successfully!";
                    TempData["FeedbackType"] = "success";
                }
                else
                {
                    TempData["FeedbackMessage"] = result.ErrorMessage;
                    TempData["FeedbackType"] = "error";
                    return View(displayAccountDto);
                }

                if (redirect != null && redirect.ToString().ToLower() == "useraccounts")
                {
                    return RedirectToAction(nameof(Details),
                        new { id = displayAccountDto.AccountID, redirect = "UserAccounts" });

                }
                else
                {
                    return RedirectToAction(nameof(Details), new { id = displayAccountDto.AccountID });
                }
            }
            catch
            {
                return View();
            }
        }

        //// GET: AccountController/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        // POST: AccountController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, string? search_return, int page_return = 1)
        {

            var result = await _accountService.DeleteAccount(id);


            if (result.Success)
            {
                TempData["FeedbackMessage"] = "Account Deleted";
                TempData["FeedbackType"] = "success";
            }
            else
            {
                TempData["FeedbackMessage"] = result.ErrorMessage;
                TempData["FeedbackType"] = "error";

            }


            return RedirectToAction(nameof(Index));

        }
    }
}
