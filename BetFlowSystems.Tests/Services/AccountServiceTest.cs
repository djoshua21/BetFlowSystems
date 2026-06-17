using System;
using System.Linq;
using System.Threading.Tasks;
using BetFlowSystems.Models;
using BetFlowSystems.Models.DbModels;
using BetFlowSystems.Models.DTOs.Account;
using BetFlowSystems.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BetFlowSystems.Tests.Services
{
    public class AccountServiceTests
    {
        private ApplicationDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        private async Task<User> SeedUserAsync(
            ApplicationDbContext context,
            int userId,
            bool isActive = true)
        {

            var user = new User
            {
                UserID = userId,
                Name = "Test",
                Surname = "User",
                IdNumber = $"9001010000{userId}",
                Address = "Test Address",
                IsActive = isActive
            };


            context.AppUsers.Add(user);
            await context.SaveChangesAsync();

            return user;
        }

        private async Task<Account> SeedAccountAsync(
            ApplicationDbContext context,
            int accountId,
            int userId,
            AccState status = AccState.Open,
            decimal balance = 0,
            string title = "Main Account",
            string? description = "Test account")
        {
            var account = new Account
            {
                AccountID = accountId,
                AccountStatus = status,
                Title = title,
                Balance = balance,
                Description = description,
                CreatedDate = new DateTime(2026, 1, 1),
                UserID = userId
            };

            context.Accounts.Add(account);
            await context.SaveChangesAsync();

            return account;
        }

        private async Task<Bet> SeedBetAsync(
            ApplicationDbContext context,
            int accountId,
            BetStatus result)
        {
            var bet = new Bet
            {
                AccountID = accountId,
                Result = result
            };

            context.Bets.Add(bet);
            await context.SaveChangesAsync();

            return bet;
        }

        [Fact]
        public async Task GetAccount_ReturnsAccount_WhenAccountExists()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            await SeedUserAsync(context, 1, true);
            await SeedAccountAsync(context, 1, 1, AccState.Open, 100, "Savings", "Primary account");

            var service = new AccountService(context);

            var result = await service.GetAccount(1);

            Assert.NotNull(result);
            Assert.Equal(1, result!.AccountID);
            Assert.Equal("Savings", result.Title);
            Assert.Equal(100, result.Balance);
            Assert.Equal(1, result.UserID);
            Assert.Equal(AccState.Open, result.AccountStatus);
        }

        [Fact]
        public async Task GetAccount_ReturnsNull_WhenAccountDoesNotExist()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            var service = new AccountService(context);

            var result = await service.GetAccount(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAccounts_ReturnsPagedAccounts_WhenSearchIsNull()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            await SeedUserAsync(context, 1, true);
            await SeedUserAsync(context, 2, true);

            await SeedAccountAsync(context, 1, 1, AccState.Open, 10, "Account 1");
            await SeedAccountAsync(context, 2, 1, AccState.Open, 20, "Account 2");
            await SeedAccountAsync(context, 3, 2, AccState.Open, 30, "Account 3");

            var service = new AccountService(context);

            var result = await service.GetAllAccounts(1, 2, null);

            Assert.NotNull(result);
            Assert.Equal(1, result.CurrentPage);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(3, result.TotalItems);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(1, result.Items[0].AccountID);
            Assert.Equal(2, result.Items[1].AccountID);
        }

        [Fact]
        public async Task GetAllAccounts_FiltersBySearch_OnAccountIdOrUserId()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            await SeedUserAsync(context, 12, true);
            await SeedUserAsync(context, 99, true);

            await SeedAccountAsync(context, 1, 12, AccState.Open, 10, "A1");
            await SeedAccountAsync(context, 25, 99, AccState.Open, 10, "A25");
            await SeedAccountAsync(context, 3, 99, AccState.Open, 10, "A3");

            var service = new AccountService(context);

            var result = await service.GetAllAccounts(1, 10, "25");

            Assert.Equal(1, result.TotalItems);
            Assert.Single(result.Items);
            Assert.Equal(25, result.Items[0].AccountID);
        }

        [Fact]
        public async Task GetUserAccounts_ReturnsOnlyAccountsForSpecifiedUser()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            await SeedUserAsync(context, 1, true);
            await SeedUserAsync(context, 2, true);

            await SeedAccountAsync(context, 1, 1, AccState.Open, 10, "User1-Acc1");
            await SeedAccountAsync(context, 2, 1, AccState.Open, 20, "User1-Acc2");
            await SeedAccountAsync(context, 3, 2, AccState.Open, 30, "User2-Acc1");

            var service = new AccountService(context);

            var result = await service.GetUserAccounts(1, 1, 10, null);

            Assert.Equal(2, result.TotalItems);
            Assert.Equal(2, result.Items.Count);
            Assert.All(result.Items, x => Assert.Equal(1, x.UserID));
        }

        [Fact]
        public async Task CreateAccount_ReturnsFailure_WhenTitleIsEmpty()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            var service = new AccountService(context);

            var dto = new CreateAccountDto
            {
                Title = "",
                Description = "New account",
                UserID = 1
            };

            var result = await service.CreateAccount(dto);

            Assert.False(result.Success);
            Assert.Equal("Title cannot be empty", result.ErrorMessage);
            Assert.Empty(context.Accounts);
        }

        [Fact]
        public async Task CreateAccount_ReturnsFailure_WhenUserIdIsInvalid()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            var service = new AccountService(context);

            var dto = new CreateAccountDto
            {
                Title = "Account",
                Description = "New account",
                UserID = 0
            };

            var result = await service.CreateAccount(dto);

            Assert.False(result.Success);
            Assert.Equal("Invalid User ID", result.ErrorMessage);
            Assert.Empty(context.Accounts);
        }

        [Fact]
        public async Task CreateAccount_ReturnsFailure_WhenUserDoesNotExist()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            var service = new AccountService(context);

            var dto = new CreateAccountDto
            {
                Title = "Account",
                Description = "New account",
                UserID = 5
            };

            var result = await service.CreateAccount(dto);

            Assert.False(result.Success);
            Assert.Equal("User not found", result.ErrorMessage);
            Assert.Empty(context.Accounts);
        }

        [Fact]
        public async Task CreateAccount_ReturnsFailure_WhenUserIsDeactivated()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            await SeedUserAsync(context, 1, false);

            var service = new AccountService(context);

            var dto = new CreateAccountDto
            {
                Title = "Account",
                Description = "New account",
                UserID = 1
            };

            var result = await service.CreateAccount(dto);

            Assert.False(result.Success);
            Assert.Equal("User is Deactivated. Cannot create accounts on deactivated users", result.ErrorMessage);
            Assert.Empty(context.Accounts);
        }

        [Fact]
        public async Task CreateAccount_CreatesAccount_WhenInputIsValid()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            await SeedUserAsync(context, 1, true);

            var service = new AccountService(context);

            var dto = new CreateAccountDto
            {
                Title = "New Account",
                Description = "Created in test",
                UserID = 1
            };

            var result = await service.CreateAccount(dto);

            Assert.True(result.Success);

            var savedAccount = await context.Accounts.SingleAsync();
            Assert.Equal("New Account", savedAccount.Title);
            Assert.Equal("Created in test", savedAccount.Description);
            Assert.Equal(1, savedAccount.UserID);
            Assert.Equal(0, savedAccount.Balance);
            Assert.Equal(AccState.Open, savedAccount.AccountStatus);
        }

        [Fact]
        public async Task UpdateAccount_ReturnsFailure_WhenAccountDoesNotExist()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            var service = new AccountService(context);

            var dto = new UpdateAccountDto
            {
                AccountId = 1,
                Title = "Updated",
                Description = "Updated description",
                AccountStatus = AccState.Open
            };

            var result = await service.UpdateAccount(dto);

            Assert.False(result.Success);
            Assert.Equal("Account Not Found", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateAccount_ReturnsFailure_WhenUserIsDeactivated()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            await SeedUserAsync(context, 1, false);
            await SeedAccountAsync(context, 1, 1, AccState.Open, 0, "Original", "Original description");

            var service = new AccountService(context);

            var dto = new UpdateAccountDto
            {
                AccountId = 1,
                Title = "Updated",
                Description = "Updated description",
                AccountStatus = AccState.Open
            };

            var result = await service.UpdateAccount(dto);

            Assert.False(result.Success);
            Assert.Equal("User is Deactivated. Cannot update accounts on deactivated users", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateAccount_ReturnsFailure_WhenAccountIsAlreadyClosedAndStaysClosed()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            await SeedUserAsync(context, 1, true);
            await SeedAccountAsync(context, 1, 1, AccState.Closed, 0, "Closed Account", "Desc");

            var service = new AccountService(context);

            var dto = new UpdateAccountDto
            {
                AccountId = 1,
                Title = "Updated",
                Description = "Updated description",
                AccountStatus = AccState.Closed
            };

            var result = await service.UpdateAccount(dto);

            Assert.False(result.Success);
            Assert.Equal("Cannot update account while it is closed", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateAccount_ReturnsFailure_WhenClosingAccountWithPendingBets()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            await SeedUserAsync(context, 1, true);
            await SeedAccountAsync(context, 1, 1, AccState.Open, 0, "Open Account", "Desc");
            await SeedBetAsync(context, 1, BetStatus.Pending);

            var service = new AccountService(context);

            var dto = new UpdateAccountDto
            {
                AccountId = 1,
                Title = "Updated",
                Description = "Updated description",
                AccountStatus = AccState.Closed
            };

            var result = await service.UpdateAccount(dto);

            Assert.False(result.Success);
            Assert.Equal("Cannot close account while Bets Results are Pending", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateAccount_ReturnsFailure_WhenClosingAccountWithNonZeroBalance()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            await SeedUserAsync(context, 1, true);
            await SeedAccountAsync(context, 1, 1, AccState.Open, 50, "Open Account", "Desc");

            var service = new AccountService(context);

            var dto = new UpdateAccountDto
            {
                AccountId = 1,
                Title = "Updated",
                Description = "Updated description",
                AccountStatus = AccState.Closed
            };

            var result = await service.UpdateAccount(dto);

            Assert.False(result.Success);
            Assert.Equal("Balance must be 0 to close account.", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateAccount_UpdatesAccount_WhenValid()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            await SeedUserAsync(context, 1, true);
            await SeedAccountAsync(context, 1, 1, AccState.Open, 0, "Original Title", "Original description");

            var service = new AccountService(context);

            var dto = new UpdateAccountDto
            {
                AccountId = 1,
                Title = "Updated Title",
                Description = "Updated description",
                AccountStatus = AccState.Closed
            };

            var result = await service.UpdateAccount(dto);

            Assert.True(result.Success);

            var updated = await context.Accounts.FindAsync(1);
            Assert.NotNull(updated);
            Assert.Equal("Updated Title", updated!.Title);
            Assert.Equal("Updated description", updated.Description);
            Assert.Equal(AccState.Closed, updated.AccountStatus);
        }

        [Fact]
        public async Task UpdateBalance_ReturnsFailure_WhenAccountDoesNotExist()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            var service = new AccountService(context);

            var result = await service.UpdateBalance(999, 50);

            Assert.False(result.Success);
            Assert.Equal("Account not found", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateBalance_ReturnsFailure_WhenAccountIsClosed()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            await SeedUserAsync(context, 1, true);
            await SeedAccountAsync(context, 1, 1, AccState.Closed, 100, "Closed", "Desc");

            var service = new AccountService(context);

            var result = await service.UpdateBalance(1, 50);

            Assert.False(result.Success);
            Assert.Equal("Account is closed", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateBalance_ReturnsFailure_WhenWithdrawalWouldMakeBalanceNegative()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            await SeedUserAsync(context, 1, true);
            await SeedAccountAsync(context, 1, 1, AccState.Open, 25, "Open", "Desc");

            var service = new AccountService(context);

            var result = await service.UpdateBalance(1, -30);

            Assert.False(result.Success);
            Assert.Equal("Insufficient Funds. Please check balance.", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateBalance_UpdatesBalance_WhenValid()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            await SeedUserAsync(context, 1, true);
            await SeedAccountAsync(context, 1, 1, AccState.Open, 25, "Open", "Desc");

            var service = new AccountService(context);

            var result = await service.UpdateBalance(1, 15);

            Assert.True(result.Success);

            var account = await context.Accounts.FindAsync(1);
            Assert.NotNull(account);
            Assert.Equal(40, account!.Balance);
        }

        [Fact]
        public async Task DeactivateAccount_ReturnsFailure_WhenAccountDoesNotExist()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            var service = new AccountService(context);

            var result = await service.DeactivateAccount(999);

            Assert.False(result.Success);
            Assert.Equal("Account not found", result.ErrorMessage);
        }

        [Fact]
        public async Task DeactivateAccount_ReturnsFailure_WhenBalanceIsNotZero()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            await SeedUserAsync(context, 1, true);
            await SeedAccountAsync(context, 1, 1, AccState.Open, 10, "Open", "Desc");

            var service = new AccountService(context);

            var result = await service.DeactivateAccount(1);

            Assert.False(result.Success);
            Assert.Equal("Balance must be 0 to deactivate account.", result.ErrorMessage);
        }

        [Fact]
        public async Task DeactivateAccount_ClosesAccount_WhenBalanceIsZero()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            await SeedUserAsync(context, 1, true);
            await SeedAccountAsync(context, 1, 1, AccState.Open, 0, "Open", "Desc");

            var service = new AccountService(context);

            var result = await service.DeactivateAccount(1);

            Assert.True(result.Success);

            var account = await context.Accounts.FindAsync(1);
            Assert.NotNull(account);
            Assert.Equal(AccState.Closed, account!.AccountStatus);
        }

        [Fact]
        public async Task DeleteAccount_ReturnsFailure_WhenAccountDoesNotExist()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            var service = new AccountService(context);

            var result = await service.DeleteAccount(999);

            Assert.False(result.Success);
            Assert.Equal("Account not found", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteAccount_ReturnsFailure_WhenAccountIsStillOpen()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            await SeedUserAsync(context, 1, true);
            await SeedAccountAsync(context, 1, 1, AccState.Open, 0, "Open", "Desc");

            var service = new AccountService(context);

            var result = await service.DeleteAccount(1);

            Assert.False(result.Success);
            Assert.Equal("Account must be closed before it is deleted", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteAccount_ReturnsFailure_WhenClosedAccountHasBalance()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            await SeedUserAsync(context, 1, true);
            await SeedAccountAsync(context, 1, 1, AccState.Closed, 25, "Closed", "Desc");

            var service = new AccountService(context);

            var result = await service.DeleteAccount(1);

            Assert.False(result.Success);
            Assert.Equal("Balance must be 0 to deactivate account.", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteAccount_RemovesAccount_WhenClosedAndBalanceIsZero()
        {
            var dbName = Guid.NewGuid().ToString();

            using var context = CreateContext(dbName);
            await SeedUserAsync(context, 1, true);
            await SeedAccountAsync(context, 1, 1, AccState.Closed, 0, "Closed", "Desc");

            var service = new AccountService(context);

            var result = await service.DeleteAccount(1);

            Assert.True(result.Success);
            Assert.Empty(context.Accounts);
        }
    }
}