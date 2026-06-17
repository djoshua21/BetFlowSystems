using BetFlowSystems.Models.DbModels;
using BetFlowSystems.Models.DTOs.Admin;
using BetFlowSystems.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;

namespace BetFlowSystems.Tests.Services
{
    public class AdminServiceTests
    {


        public class TestIdentityDbContext : IdentityDbContext<Admin>
        {
            public TestIdentityDbContext(DbContextOptions<TestIdentityDbContext> options)
                : base(options)
            {
            }
        }


        private RoleManager<IdentityRole> GetRoleManager(TestIdentityDbContext context)
        {
            var roleStore = new RoleStore<IdentityRole, TestIdentityDbContext, string>(context);

            return new RoleManager<IdentityRole>(
                roleStore,
                null,
                null,
                null,
                null
            );
        }


        private UserManager<Admin> GetUserManager(TestIdentityDbContext context)
        {
            var store = new UserStore<Admin, IdentityRole, TestIdentityDbContext, string>(context);

            return new UserManager<Admin>(
                store,
                null,
                new PasswordHasher<Admin>(),
                new List<IUserValidator<Admin>> { new UserValidator<Admin>() },
                new List<IPasswordValidator<Admin>> { new PasswordValidator<Admin>() },
                null,
                null,
                null,
                null
            );
        }




        private SignInManager<Admin> GetSignInManager(UserManager<Admin> userManager)
        {
            var contextAccessor = new HttpContextAccessor();

            var options = Options.Create(new IdentityOptions());

            var logger = new Mock<ILogger<SignInManager<Admin>>>().Object;

            var schemes = new Mock<IAuthenticationSchemeProvider>().Object;

            var confirmation = new Mock<IUserConfirmation<Admin>>().Object;

            var claimsFactory = new UserClaimsPrincipalFactory<Admin>(userManager, options);

            return new SignInManager<Admin>(
                userManager,
                contextAccessor,
                claimsFactory,
                options,
                logger,
                schemes,
                confirmation
            );
        }



        private TestIdentityDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<TestIdentityDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new TestIdentityDbContext(options);
        }


        [Fact]
        public async Task RegisterAdmin_ShouldCreateAdminSuccessfully()
        {
            // Arrange
            var context = GetDbContext();
            var userManager = GetUserManager(context);

            var roleManager = GetRoleManager(context);
            await roleManager.CreateAsync(new IdentityRole("Admin"));

            var signInManager = GetSignInManager(userManager);
            var service = new AdminService(userManager, signInManager);

            var dto = new CreateAdminDto
            {
                Email = "test@test.com",
                Password = "Password123!",
                Role = "Admin"
            };

            // Act
            var result = await service.RegisterAdmin(dto);

            // Assert
            Assert.True(result.Success);

            var user = await userManager.FindByEmailAsync(dto.Email);
            Assert.NotNull(user);
            Assert.Equal(dto.Email, user.Email);
        }

        [Fact]
        public async Task GetAdmin_ShouldReturnAdmin_WhenExists()
        {
            // Arrange
            var context = GetDbContext();
            var userManager = GetUserManager(context);
            var signInManager = GetSignInManager(userManager);
            var service = new AdminService(userManager, signInManager);

            var admin = new Admin
            {
                UserName = "admin@test.com",
                Email = "admin@test.com"
            };

            await userManager.CreateAsync(admin, "Password123!");

            // Act
            var result = await service.GetAdmin(admin.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(admin.Email, result.Email);
        }

        [Fact]
        public async Task GetAdmin_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            var context = GetDbContext();
            var userManager = GetUserManager(context);
            var signInManager = GetSignInManager(userManager);
            var service = new AdminService(userManager, signInManager);

            // Act
            var result = await service.GetAdmin("invalid-id");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAdmin_ShouldUpdateEmailSuccessfully()
        {
            // Arrange
            var context = GetDbContext();
            var userManager = GetUserManager(context);

            var roleManager = GetRoleManager(context);
            await roleManager.CreateAsync(new IdentityRole("Admin"));

            var signInManager = GetSignInManager(userManager);
            var service = new AdminService(userManager, signInManager);

            var admin = new Admin
            {
                UserName = "old@test.com",
                Email = "old@test.com"
            };

            await userManager.CreateAsync(admin, "Password123!");

            var dto = new UpdateAdminDto
            {
                AdminId = admin.Id,
                Email = "new@test.com",
                Role = "Admin",
                IsLocked = false
            };

            // Act
            var result = await service.UpdateAdmin(dto);

            // Assert
            Assert.True(result.Success);

            var updatedUser = await userManager.FindByIdAsync(admin.Id);
            Assert.Equal("new@test.com", updatedUser.Email);
        }

        [Fact]
        public async Task UpdateLockoutState_ShouldToggleLockState()
        {
            // Arrange
            var context = GetDbContext();
            var userManager = GetUserManager(context);
            var signInManager = GetSignInManager(userManager);
            var service = new AdminService(userManager, signInManager);

            var admin = new Admin
            {
                UserName = "lock@test.com",
                Email = "lock@test.com"
            };

            await userManager.CreateAsync(admin, "Password123!");

            // Act - lock user
            var result1 = await service.UpdateLockoutState(admin.Id, "different-user");

            // Assert
            Assert.True(result1.Success);

            var lockEnd = await userManager.GetLockoutEndDateAsync(admin);
            Assert.True(lockEnd.HasValue);

            // Act - unlock user
            var result2 = await service.UpdateLockoutState(admin.Id, "different-user");

            // Assert
            Assert.True(result2.Success);

            var updatedLockEnd = await userManager.GetLockoutEndDateAsync(admin);
            Assert.Null(updatedLockEnd);
        }

        [Fact]
        public async Task DeleteAdmin_ShouldDeleteUser_WhenValid()
        {
            // Arrange
            var context = GetDbContext();
            var userManager = GetUserManager(context);
            var signInManager = GetSignInManager(userManager);
            var service = new AdminService(userManager, signInManager);

            var admin = new Admin
            {
                UserName = "delete@test.com",
                Email = "delete@test.com"
            };

            await userManager.CreateAsync(admin, "Password123!");

            // Act
            var result = await service.DeleteAdmin(admin.Id, "different-user");

            // Assert
            Assert.True(result.Success);

            var deletedUser = await userManager.FindByIdAsync(admin.Id);
            Assert.Null(deletedUser);
        }

        [Fact]
        public async Task DeleteAdmin_ShouldFail_WhenDeletingSelf()
        {
            // Arrange
            var context = GetDbContext();
            var userManager = GetUserManager(context);
            var signInManager = GetSignInManager(userManager);
            var service = new AdminService(userManager, signInManager);

            var admin = new Admin
            {
                UserName = "self@test.com",
                Email = "self@test.com"
            };

            await userManager.CreateAsync(admin, "Password123!");

            // Act
            var result = await service.DeleteAdmin(admin.Id, admin.Id);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("cannot delete your own account", result.ErrorMessage.ToLower());
        }
    }
}