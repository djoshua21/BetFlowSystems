using BetFlowSystems.Models;
using BetFlowSystems.Models.DbModels;
using BetFlowSystems.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.NetworkInformation;
using System.Security.Principal;
using System.Threading.Tasks;
using Xunit;

namespace BetFlowSystems.Tests.Services
{
    public class UserSimulationServiceTests
    {
        private ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private UserSimulationService CreateService(ApplicationDbContext context)
        {
            // These tests do not use GenerateRandomBet,
            // so BetTransactionService is not needed here.
            return new UserSimulationService(context, null!);
        }

        private async Task SeedAccount(
            ApplicationDbContext context,
            int accountId,
            decimal balance,
            AccState status)
        {
            var account = new Account
            {
                AccountID = accountId,
                Title = "Test Account",
                Balance = balance,
                AccountStatus = status,
                UserID = accountId,
                
            };

            context.Accounts.Add(account);
            await context.SaveChangesAsync();
        }

        // -------------------------
        // Deposit Tests
        // -------------------------

        [Fact]
        public async Task Deposit_Should_Return_Failure_When_Amount_Is_Negative()
        {
            using var context = CreateContext();
            var service = CreateService(context);

            var result = await service.Deposit(1, -50);

            Assert.False(result.Success);
            Assert.Equal("Invalid amount, it should be > 0", result.ErrorMessage);
        }

        [Fact]
        public async Task Deposit_Should_Return_Failure_When_Account_Does_Not_Exist()
        {
            using var context = CreateContext();
            var service = CreateService(context);

            var result = await service.Deposit(999, 100);

            Assert.False(result.Success);
            Assert.Equal("Account does not exist", result.ErrorMessage);
        }

        [Fact]
        public async Task Deposit_Should_Return_Failure_When_Account_Is_Closed()
        {
            using var context = CreateContext();
            await SeedAccount(context, 1, 100, AccState.Closed);

            var service = CreateService(context);

            var result = await service.Deposit(1, 50);

            Assert.False(result.Success);
            Assert.Equal("Account is closed", result.ErrorMessage);
        }

        [Fact]
        public async Task Deposit_Should_Increase_Balance_When_Request_Is_Valid()
        {
            using var context = CreateContext();
            await SeedAccount(context, 1, 100, AccState.Open); // change if needed

            var service = CreateService(context);

            var result = await service.Deposit(1, 50);
            var updatedAccount = await context.Accounts.FindAsync(1);

            Assert.True(result.Success);
            Assert.NotNull(updatedAccount);
            Assert.Equal(150, updatedAccount!.Balance);
        }

        // -------------------------
        // Withdrawal Tests
        // -------------------------

        [Fact]
        public async Task Withdrawal_Should_Return_Failure_When_Amount_Is_Negative()
        {
            using var context = CreateContext();
            var service = CreateService(context);

            var result = await service.Withdrawal(1, -10);

            Assert.False(result.Success);
            Assert.Equal("Invalid amount, it should be > 0", result.ErrorMessage);
        }

        [Fact]
        public async Task Withdrawal_Should_Return_Failure_When_Account_Does_Not_Exist()
        {
            using var context = CreateContext();
            var service = CreateService(context);

            var result = await service.Withdrawal(999, 50);

            Assert.False(result.Success);
            Assert.Equal("Account does not exist", result.ErrorMessage);
        }

        [Fact]
        public async Task Withdrawal_Should_Return_Failure_When_Account_Is_Closed()
        {
            using var context = CreateContext();
            await SeedAccount(context, 1, 100, AccState.Closed);

            var service = CreateService(context);

            var result = await service.Withdrawal(1, 50);

            Assert.False(result.Success);
            Assert.Equal("Account is closed", result.ErrorMessage);
        }

        [Fact]
        public async Task Withdrawal_Should_Return_Failure_When_Funds_Are_Insufficient()
        {
            using var context = CreateContext();
            await SeedAccount(context, 1, 100, AccState.Open); // change if needed

            var service = CreateService(context);

            var result = await service.Withdrawal(1, 150);

            Assert.False(result.Success);
            Assert.Equal("Insufficient Funds.", result.ErrorMessage);
        }

        [Fact]
        public async Task Withdrawal_Should_Decrease_Balance_When_Request_Is_Valid()
        {
            using var context = CreateContext();
            await SeedAccount(context, 1, 100, AccState.Open); // change if needed

            var service = CreateService(context);

            var result = await service.Withdrawal(1, 40);
            var updatedAccount = await context.Accounts.FindAsync(1);

            Assert.True(result.Success);
            Assert.NotNull(updatedAccount);
            Assert.Equal(60, updatedAccount!.Balance);
        }

        // -------------------------
        // ZeroAccount Tests
        // -------------------------

        [Fact]
        public async Task ZeroAccount_Should_Return_Failure_When_Account_Does_Not_Exist()
        {
            using var context = CreateContext();
            var service = CreateService(context);

            var result = await service.ZeroAccount(999);

            Assert.False(result.Success);
            Assert.Equal("Account does not exist", result.ErrorMessage);
        }

        [Fact]
        public async Task ZeroAccount_Should_Return_Failure_When_Account_Is_Closed()
        {
            using var context = CreateContext();
            await SeedAccount(context, 1, 100, AccState.Closed);

            var service = CreateService(context);

            var result = await service.ZeroAccount(1);

            Assert.False(result.Success);
            Assert.Equal("Account is closed", result.ErrorMessage);
        }

        [Fact]
        public async Task ZeroAccount_Should_Set_Balance_To_Zero_When_Request_Is_Valid()
        {
            using var context = CreateContext();
            await SeedAccount(context, 1, 250, AccState.Open); // change if needed

            var service = CreateService(context);

            var result = await service.ZeroAccount(1);
            var updatedAccount = await context.Accounts.FindAsync(1);

            Assert.True(result.Success);
            Assert.NotNull(updatedAccount);
            Assert.Equal(0, updatedAccount!.Balance);
        }
    }
}