using BetFlowSystems.Models;
using BetFlowSystems.Models.DbModels;
using BetFlowSystems.Models.DTOs.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BetFlowSystems.Services
{
    public class AdminService
    {

        private readonly UserManager<Admin> _adminManager;
        private readonly SignInManager<Admin> _signInManager;

        public AdminService(
            UserManager<Admin> adminManager,
            SignInManager<Admin> signInManager)
        {
            _adminManager = adminManager;
            _signInManager = signInManager;
        }

        public async Task<List<DisplayAdminDto>> GetAllAdmins()
        {

            var admins = await _adminManager.Users.ToListAsync();

            var adminList = new List<DisplayAdminDto>();

            foreach (var admin in admins)
            {
                var roles = await _adminManager.GetRolesAsync(admin);

                adminList.Add(new DisplayAdminDto
                {
                    AdminID = admin.Id,

                    Email = admin.Email ?? string.Empty,
                    Role = !roles.IsNullOrEmpty() ? roles[0] : string.Empty,
                    IsLocked = admin.LockoutEnd.HasValue && admin.LockoutEnd.Value > DateTimeOffset.UtcNow
                });
            }

            return adminList;

        }

        public async Task<Result> RegisterAdmin(CreateAdminDto createAdminDto)
        {

            var user = new Admin
            {
                UserName = createAdminDto.Email,
                Email = createAdminDto.Email,
                EmailConfirmed = true
            };

            var createResult = await _adminManager.CreateAsync(user, createAdminDto.Password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(" ", createResult.Errors.Select(e => e.Description.Replace("'", "")));
                Console.WriteLine(errors);
                var errorMessage = "Failed to create user. Reason(s): " + errors;
                return new Result
                {
                    Success = false,
                    ErrorMessage = errorMessage
                };

            }
            await _adminManager.AddToRoleAsync(user, createAdminDto.Role);
            return new Result { Success = true };


        }

        public async Task<UpdateAdminDto?> GetAdmin(string id)
        {

            var admin = await _adminManager.FindByIdAsync(id);

            if (admin == null)
            {
                return null;
            }

            var roles = await _adminManager.GetRolesAsync(admin);

            var model = new UpdateAdminDto
            {
                AdminId = admin.Id,
                Email = admin.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? "Clerk",
                IsLocked = admin.LockoutEnd.HasValue && admin.LockoutEnd.Value > DateTimeOffset.UtcNow
            };

            return model;

        }

        public async Task<Result> UpdateAdmin(UpdateAdminDto updateAdminDto)
        {

            var admin = await _adminManager.FindByIdAsync(updateAdminDto.AdminId);

            if (admin == null)
            {
                return new Result { Success = false, ErrorMessage = $"Admin account not found (id: {updateAdminDto.AdminId})" };
            }

            // Update email and username
            if (admin.Email != updateAdminDto.Email)
            {
                var emailResult = await _adminManager.SetEmailAsync(admin, updateAdminDto.Email);

                if (!emailResult.Succeeded)
                {
                    var errorMessage = string.Join(", ", emailResult.Errors.Select(e => e.Description));
                    return new Result { Success = false, ErrorMessage = errorMessage };

                }

                admin.EmailConfirmed = true;
            }

            var updateResult = await _adminManager.UpdateAsync(admin);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    var errorMessage = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                    return new Result { Success = false, ErrorMessage = errorMessage };
                }

            }

            // Update role
            var currentRoles = await _adminManager.GetRolesAsync(admin);

            if (currentRoles.Any())
            {
                var removeRolesResult = await _adminManager.RemoveFromRolesAsync(admin, currentRoles);

                if (!removeRolesResult.Succeeded)
                {
                    foreach (var error in removeRolesResult.Errors)
                    {
                        var errorMessage = string.Join(", ", removeRolesResult.Errors.Select(e => e.Description));
                        return new Result { Success = false, ErrorMessage = errorMessage };
                    }

                }
            }

            var addRoleResult = await _adminManager.AddToRoleAsync(admin, updateAdminDto.Role);

            if (!addRoleResult.Succeeded)
            {
                foreach (var error in addRoleResult.Errors)
                {
                    var errorMessage = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                    return new Result { Success = false, ErrorMessage = errorMessage };
                }

            }

            // Update locked/unlocked status
            if (updateAdminDto.IsLocked)
            {
                await _adminManager.SetLockoutEnabledAsync(admin, true);
                await _adminManager.SetLockoutEndDateAsync(admin, DateTimeOffset.UtcNow.AddYears(100));
            }
            else
            {
                await _adminManager.SetLockoutEndDateAsync(admin, null);
            }

            // Optional password update
            // Password only changes if one of these fields is filled in
            if (!string.IsNullOrWhiteSpace(updateAdminDto.NewPassword) &&
                !string.IsNullOrWhiteSpace(updateAdminDto.ConfirmPassword))
            {


                var resetToken = await _adminManager.GeneratePasswordResetTokenAsync(admin);

                var passwordResult = await _adminManager.ResetPasswordAsync(
                    admin,
                    resetToken,
                    updateAdminDto.NewPassword);

                if (!passwordResult.Succeeded)
                {
                    var errorMessage = string.Join(", ", passwordResult.Errors.Select(e => e.Description));
                    return new Result { Success = false, ErrorMessage = errorMessage };
                }
            }

            return new Result { Success = true };

        }

        public async Task<Result> LoginAdmin(LoginAdminDto loginAdminDto)
        {

            var result = await _signInManager.PasswordSignInAsync(
                loginAdminDto.Email,
                loginAdminDto.Password,
                loginAdminDto.RememberMe,
                lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                return new Result { Success = false, ErrorMessage = "Incorrect Email or Password" };

            }


            if (result.IsLockedOut)
            {

                return new Result { Success = false, ErrorMessage = "This account is locked. Please contact an administrator." };
            }

            return new Result { Success = true };


        }

        public async void Logout()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<Result> UpdateLockoutState(string adminID, string currentUserID)
        {


            if (adminID == currentUserID)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "You cannot lock your own account."
                };
            }


            var admin = await _adminManager.FindByIdAsync(adminID);

            if (admin == null)
            {
                return new Result { Success = false, ErrorMessage = "Cannot Find Account" };

            }

            var lockoutEnd = await _adminManager.GetLockoutEndDateAsync(admin);

            var isCurrentlyLocked = lockoutEnd.HasValue && lockoutEnd > DateTimeOffset.UtcNow;

            if (isCurrentlyLocked)
            {

                await _adminManager.SetLockoutEndDateAsync(admin, null);
            }
            else
            {

                await _adminManager.SetLockoutEnabledAsync(admin, true);
                await _adminManager.SetLockoutEndDateAsync(admin, DateTimeOffset.UtcNow.AddYears(100));
            }


            return new Result { Success = true };
        }

        public async Task<Result> DeleteAdmin(string adminID, string currentUserID)
        {


            if (adminID == currentUserID)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "You cannot delete your own account."
                };
            }


            var admin = await _adminManager.FindByIdAsync(adminID);

            if (admin == null)
            {
                return new Result { Success = false, ErrorMessage = "Cannot Find Account" };

            }


            var result = await _adminManager.DeleteAsync(admin);

            if (!result.Succeeded)
            {

                var message = string.Join(" ", result.Errors.Select(e => e.Description.Replace("'", "")));
                return new Result
                {
                    Success = false,
                    ErrorMessage = $"Cannot delete account. Reasons: {message}"
                };
            }



            return new Result { Success = true };
        }

    }
}
