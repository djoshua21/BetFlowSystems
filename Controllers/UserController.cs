using BetFlowSystems.Models.DTOs.User;
using BetFlowSystems.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BetFlowSystems.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    [Route("/Users/{action=Index}/{id?}")]
    public class UserController : Controller
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        // GET: UserController
        public async Task<ActionResult> Index(string? search, int page = 1)
        {
            ViewBag.CurrentFilter = search;

            var pageSize = 10;

            var users = await _userService.GetAllUsers(page, pageSize, search);

            return View(users);
        }

        // GET: UserController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var user = await _userService.GetUser(id);

            if (user == null)
            {
                TempData["FeedbackMessage"] = $"User ID {id} does not exist";
                TempData["FeedbackType"] = "error";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // GET: UserController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UserController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateUserDto createUserDto)
        {
            try
            {
                // IdNumber
                if (string.IsNullOrWhiteSpace(createUserDto.IdNumber))
                    ModelState.AddModelError("IdNumber", "ID Number is required");
                else if (createUserDto.IdNumber.Length != 13)
                    ModelState.AddModelError("IdNumber", "ID Number must be exactly 13 digits");
                else if (!createUserDto.IdNumber.All(char.IsDigit))
                    ModelState.AddModelError("IdNumber", "ID Number must contain only numbers");

                // Name
                if (string.IsNullOrWhiteSpace(createUserDto.Name))
                    ModelState.AddModelError("Name", "Name is required");
                else if (createUserDto.Name.Length > 50)
                    ModelState.AddModelError("Name", "Name cannot exceed 50 characters");

                // Surname
                if (string.IsNullOrWhiteSpace(createUserDto.Surname))
                    ModelState.AddModelError("Surname", "Surname is required");
                else if (createUserDto.Surname.Length > 50)
                    ModelState.AddModelError("Surname", "Surname cannot exceed 50 characters");

                // Address
                if (
                    !string.IsNullOrEmpty(createUserDto.Address)
                    && createUserDto.Address.Length > 200
                )
                    ModelState.AddModelError("Address", "Address cannot exceed 200 characters");

                if (await _userService.DoesIdExist(createUserDto.IdNumber))
                {
                    ModelState.AddModelError("IdNumber", "ID Number already in use.");
                }

                if (!ModelState.IsValid)
                    return View(createUserDto);

                var result = await _userService.CreateUser(createUserDto);

                //var result = new Result { Success = true, ErrorMessage= "Defs Broken" };

                if (result.Success)
                {
                    TempData["FeedbackMessage"] = "User Created Successfully!";
                    TempData["FeedbackType"] = "success";
                }
                else
                {
                    TempData["FeedbackMessage"] = result.ErrorMessage;
                    TempData["FeedbackType"] = "error";
                    return View(createUserDto);
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: User/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var user = await _userService.GetUser(id);

            if (user == null)
            {
                TempData["FeedbackMessage"] = $"User ID {id} does not exist";
                TempData["FeedbackType"] = "error";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // POST: UserController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(DisplayUserDto displayUserDto)
        {
            try
            {
                // Name
                if (string.IsNullOrWhiteSpace(displayUserDto.Name))
                    ModelState.AddModelError("Name", "Name is required");
                else if (displayUserDto.Name.Length > 50)
                    ModelState.AddModelError("Name", "Name cannot exceed 50 characters");

                // Surname
                if (string.IsNullOrWhiteSpace(displayUserDto.Surname))
                    ModelState.AddModelError("Surname", "Surname is required");
                else if (displayUserDto.Surname.Length > 50)
                    ModelState.AddModelError("Surname", "Surname cannot exceed 50 characters");

                // Address
                if (
                    !string.IsNullOrEmpty(displayUserDto.Address)
                    && displayUserDto.Address.Length > 200
                )
                    ModelState.AddModelError("Address", "Address cannot exceed 200 characters");

                if (!ModelState.IsValid)
                    return View(displayUserDto);

                var user = new UpdateUserDto
                {
                    UserID = displayUserDto.UserID,
                    IsActive = displayUserDto.IsActive,
                    Name = displayUserDto.Name,
                    Surname = displayUserDto.Surname,
                    Address = displayUserDto.Address,
                };

                var result = await _userService.UpdateUser(user);

                if (result.Success)
                {
                    TempData["FeedbackMessage"] = "User Updated Successfully!";
                    TempData["FeedbackType"] = "success";
                }
                else
                {
                    TempData["FeedbackMessage"] = result.ErrorMessage;
                    TempData["FeedbackType"] = "error";
                    return View(displayUserDto);
                }

                return RedirectToAction(nameof(Details), new { id = displayUserDto.UserID });
            }
            catch
            {
                return View(displayUserDto);
            }
        }

        // POST: User/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string? search_return, int page_return = 1)
        {
            var result = await _userService.DeleteUser(id);

            if (result.Success)
            {
                TempData["FeedbackMessage"] = "User Deleted Successfully!";
                TempData["FeedbackType"] = "success";
            }
            else
            {
                TempData["FeedbackMessage"] = result.ErrorMessage;
                TempData["FeedbackType"] = "error";
                page_return = 1;
            }

            return RedirectToAction(
                nameof(Index),
                new { search = search_return, page = page_return }
            );
        }
    }
}
