
using BetFlowSystems.Models;
using BetFlowSystems.Models.DbModels;
using BetFlowSystems.Models.DTOs.Admin;
using BetFlowSystems.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace BetFlowSystems.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {

        private readonly AdminService _adminService;

        public AdminController(AdminService adminService)
        {
            _adminService = adminService;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var adminList = await _adminService.GetAllAdmins();

            return View(adminList);
        }



        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(CreateAdminDto model)
        {

            if (!string.IsNullOrWhiteSpace(model.Password) ||
                !string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    ModelState.AddModelError(nameof(model.Password), "New password is required when changing password.");
                    return View(model);
                }

                if (string.IsNullOrWhiteSpace(model.ConfirmPassword))
                {
                    ModelState.AddModelError(nameof(model.ConfirmPassword), "Confirm password is required when changing password.");
                    return View(model);
                }

                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError(nameof(model.ConfirmPassword), "The new password and confirm password do not match.");
                    return View(model);
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _adminService.RegisterAdmin(model);

            if (result.Success)
            {
                TempData["FeedbackMessage"] = "Account Created Successfully";
                TempData["FeedbackType"] = "success";
            }
            else
            {
                TempData["FeedbackMessage"] = result.ErrorMessage;
                TempData["FeedbackType"] = "error";
                return View(model);
            }
            return RedirectToAction(nameof(Index));

        }


        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var admin = await _adminService.GetAdmin(id);


            return View(admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateAdminDto model)
        {

            if (!string.IsNullOrWhiteSpace(model.NewPassword) ||
                !string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                if (string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    ModelState.AddModelError(nameof(model.NewPassword), "New password is required when changing password.");
                    return View(model);
                }

                if (string.IsNullOrWhiteSpace(model.ConfirmPassword))
                {
                    ModelState.AddModelError(nameof(model.ConfirmPassword), "Confirm password is required when changing password.");
                    return View(model);
                }

                if (model.NewPassword != model.ConfirmPassword)
                {
                    ModelState.AddModelError(nameof(model.ConfirmPassword), "The new password and confirm password do not match.");
                    return View(model);
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _adminService.UpdateAdmin(model);


            if (result.Success)
            {
                TempData["FeedbackMessage"] = "Staff account updated successfully.";
                TempData["FeedbackType"] = "success";
            }
            else
            {
                TempData["FeedbackMessage"] = result.ErrorMessage;
                TempData["FeedbackType"] = "error";
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }



        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginAdminDto model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _adminService.LoginAdmin(model);

            if (result.Success)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            else
            {

                TempData["FeedbackMessage"] = result.ErrorMessage;
                TempData["FeedbackType"] = "error";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateState(string id)
        {
            var currentUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _adminService.UpdateLockoutState(id, currentUserID ?? "");

            if (result.Success)
            {
                TempData["FeedbackMessage"] = "Staff account updated successfully.";
                TempData["FeedbackType"] = "success";
            }
            else
            {
                TempData["FeedbackMessage"] = result.ErrorMessage;
                TempData["FeedbackType"] = "error";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {

            var currentUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);


            var result = await _adminService.DeleteAdmin(id, currentUserID ?? "");

            if (result.Success)
            {
                TempData["FeedbackMessage"] = "Staff Member Removed";
                TempData["FeedbackType"] = "success";
            }
            else
            {
                TempData["FeedbackMessage"] = result.ErrorMessage;
                TempData["FeedbackType"] = "error";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            _adminService.Logout();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}
