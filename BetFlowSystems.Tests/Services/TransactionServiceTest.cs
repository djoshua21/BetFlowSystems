using System;
using System.Linq;
using System.Threading.Tasks;
using BetFlowSystems.Models;
using BetFlowSystems.Models.DbModels;
using BetFlowSystems.Models.DTOs.Transaction;
using BetFlowSystems.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BetFlowSystems.Tests.Services
{
    public class TransactionServiceTests
    {
        private static ApplicationDbContext CreateContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: databaseName)
                .Options;

            return new ApplicationDbContext(options);
        }

        private static async Task SeedAccountAsync(ApplicationDbContext context, int accountId = 1)
        {
            var account = new Account
            {
                AccountID = accountId,
                AccountStatus = (AccState)0,
                Title = $"Test Account {accountId}",
                Balance = 1000m,
                Description = "Seeded account for testing",
                CreatedDate = DateTime.UtcNow,
                UserID = 1
            };

            await context.Accounts.AddAsync(account);
            await context.SaveChangesAsync();
        }

        private static async Task SeedBetTypeAsync(ApplicationDbContext context, int betTypeId = 1)
        {
            var betType = new BetType
            {
                BetTypeID = betTypeId,
                Sport = "Football",
                EventName = $"Event {betTypeId}",
                Description = "Seeded bet type for testing"
            };

            await context.Set<BetType>().AddAsync(betType);
            await context.SaveChangesAsync();
        }

        private static async Task SeedBetAsync(
            ApplicationDbContext context,
            int betId = 1,
            int accountId = 1,
            int betTypeId = 1)
        {
            var bet = new Bet
            {
                BetID = betId,
                AccountID = accountId,
                BetTypeID = betTypeId,
                BetAmount = 100m,
                PossibleWinAmount = 250m,
                Result = (BetStatus)0,
                BetDate = DateTime.UtcNow.AddDays(-1),
                LastUpdatedDate = DateTime.UtcNow
            };

            await context.Bets.AddAsync(bet);
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task CreateTransaction_ShouldReturnFailure_WhenBetDoesNotExist()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();

            await using var context = CreateContext(dbName);
            await SeedAccountAsync(context, accountId: 1);

            var service = new TransactionService(context);

            var dto = new CreateTransactionDto
            {
                AccountID = 1,
                BetID = 999,
                Amount = 150m,
                TransactionType = (DebitOrCredit)0
            };

            // Act
            var result = await service.CreateTransaction(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Bet does not exist", result.ErrorMessage);
            Assert.Empty(context.Transactions);
        }

        [Fact]
        public async Task CreateTransaction_ShouldReturnFailure_WhenAccountDoesNotExist()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();

            await using var context = CreateContext(dbName);
            await SeedBetTypeAsync(context, betTypeId: 1);
            await SeedBetAsync(context, betId: 1, accountId: 1, betTypeId: 1);

            var service = new TransactionService(context);

            var dto = new CreateTransactionDto
            {
                AccountID = 999,
                BetID = 1,
                Amount = 200m,
                TransactionType = (DebitOrCredit)0
            };

            // Act
            var result = await service.CreateTransaction(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Account does not exist", result.ErrorMessage);
            Assert.Empty(context.Transactions);
        }

        [Fact]
        public async Task CreateTransaction_ShouldCreateAndSaveTransaction_WhenInputIsValid()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();

            await using var context = CreateContext(dbName);
            await SeedAccountAsync(context, accountId: 1);
            await SeedBetTypeAsync(context, betTypeId: 1);
            await SeedBetAsync(context, betId: 1, accountId: 1, betTypeId: 1);

            var service = new TransactionService(context);

            var dto = new CreateTransactionDto
            {
                AccountID = 1,
                BetID = 1,
                Amount = 300m,
                TransactionType = (DebitOrCredit)0
            };

            // Act
            var result = await service.CreateTransaction(dto);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);

            var savedTransaction = await context.Transactions.FirstOrDefaultAsync();

            Assert.NotNull(savedTransaction);
            Assert.Equal(1, savedTransaction.AccountID);
            Assert.Equal(1, savedTransaction.BetID);
            Assert.Equal(300m, savedTransaction.Amount);
            Assert.Equal((DebitOrCredit)0, savedTransaction.TransactionType);
            Assert.True(savedTransaction.TransactionDate <= DateTime.UtcNow);
        }

        [Fact]
        public async Task GetTransaction_ShouldReturnDto_WhenTransactionExists()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();

            await using var context = CreateContext(dbName);

            var transaction = new Transaction
            {
                TransactionID = 1,
                AccountID = 1,
                BetID = 10,
                Amount = 450m,
                TransactionType = (DebitOrCredit)0,
                TransactionDate = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc)
            };

            await context.Transactions.AddAsync(transaction);
            await context.SaveChangesAsync();

            var service = new TransactionService(context);

            // Act
            var result = await service.GetTransaction(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TransactionID);
            Assert.Equal(1, result.AccountID);
            Assert.Equal(10, result.BetID);
            Assert.Equal(450m, result.Amount);
            Assert.Equal((DebitOrCredit)0, result.TransactionType);
            Assert.Equal(transaction.TransactionDate, result.TransactionDate);
        }

        [Fact]
        public async Task GetTransaction_ShouldReturnNull_WhenTransactionDoesNotExist()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();

            await using var context = CreateContext(dbName);
            var service = new TransactionService(context);

            // Act
            var result = await service.GetTransaction(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllTransactions_ShouldReturnPagedAndFilteredResults_InDescendingDateOrder()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();

            await using var context = CreateContext(dbName);

            await context.Transactions.AddRangeAsync(
                new Transaction
                {
                    TransactionID = 101,
                    AccountID = 1,
                    BetID = 10,
                    Amount = 100m,
                    TransactionType = (DebitOrCredit)0,
                    TransactionDate = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc)
                },
                new Transaction
                {
                    TransactionID = 102,
                    AccountID = 1,
                    BetID = 20,
                    Amount = 200m,
                    TransactionType = (DebitOrCredit)0,
                    TransactionDate = new DateTime(2026, 1, 2, 8, 0, 0, DateTimeKind.Utc)
                },
                new Transaction
                {
                    TransactionID = 201,
                    AccountID = 2,
                    BetID = 30,
                    Amount = 300m,
                    TransactionType = (DebitOrCredit)0,
                    TransactionDate = new DateTime(2026, 1, 3, 8, 0, 0, DateTimeKind.Utc)
                }
            );

            await context.SaveChangesAsync();

            var service = new TransactionService(context);

            // Search "10" should match:
            // - TransactionID 101
            // - TransactionID 102
            // - BetID 10
            var search = "10";

            // Act
            var result = await service.GetAllTransactions(pageNumber: 1, pageSize: 10, search: search);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalItems);
            Assert.Equal(1, result.CurrentPage);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(2, result.Items.Count);

            Assert.Equal(102, result.Items[0].TransactionID);
            Assert.Equal(101, result.Items[1].TransactionID); // oldest date
        }

        [Fact]
        public async Task GetBetTransactions_ShouldReturnOnlyTransactionsForGivenBet_AndMatchSearch()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();

            await using var context = CreateContext(dbName);

            await context.Transactions.AddRangeAsync(
                new Transaction
                {
                    TransactionID = 111,
                    AccountID = 1,
                    BetID = 5,
                    Amount = 100m,
                    TransactionType = (DebitOrCredit)0,
                    TransactionDate = new DateTime(2026, 2, 1, 8, 0, 0, DateTimeKind.Utc)
                },
                new Transaction
                {
                    TransactionID = 112,
                    AccountID = 1,
                    BetID = 5,
                    Amount = 200m,
                    TransactionType = (DebitOrCredit)0,
                    TransactionDate = new DateTime(2026, 2, 2, 8, 0, 0, DateTimeKind.Utc)
                },
                new Transaction
                {
                    TransactionID = 211,
                    AccountID = 2,
                    BetID = 8,
                    Amount = 300m,
                    TransactionType = (DebitOrCredit)0,
                    TransactionDate = new DateTime(2026, 2, 3, 8, 0, 0, DateTimeKind.Utc)
                }
            );

            await context.SaveChangesAsync();

            var service = new TransactionService(context);

            // Search "11" matches TransactionID 111 and 112,
            // but method must also restrict to BetID == 5
            var search = "11";

            // Act
            var result = await service.GetBetTransactions(id: 5, pageNumber: 1, pageSize: 10, search: search);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalItems);
            Assert.Equal(2, result.Items.Count);

            Assert.All(result.Items, item => Assert.Equal(5, item.BetID));
            Assert.Equal(112, result.Items[0].TransactionID); // newest first
            Assert.Equal(111, result.Items[1].TransactionID);
        }

        [Fact]
        public async Task GetAllTransactions_ShouldApplyPagingCorrectly()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();

            await using var context = CreateContext(dbName);

            await context.Transactions.AddRangeAsync(
                new Transaction
                {
                    TransactionID = 1,
                    AccountID = 1,
                    BetID = 1,
                    Amount = 10m,
                    TransactionType = (DebitOrCredit)0,
                    TransactionDate = new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc)
                },
                new Transaction
                {
                    TransactionID = 2,
                    AccountID = 1,
                    BetID = 2,
                    Amount = 20m,
                    TransactionType = (DebitOrCredit)0,
                    TransactionDate = new DateTime(2026, 3, 2, 8, 0, 0, DateTimeKind.Utc)
                },
                new Transaction
                {
                    TransactionID = 3,
                    AccountID = 1,
                    BetID = 3,
                    Amount = 30m,
                    TransactionType = (DebitOrCredit)0,
                    TransactionDate = new DateTime(2026, 3, 3, 8, 0, 0, DateTimeKind.Utc)
                }
            );

            await context.SaveChangesAsync();

            var service = new TransactionService(context);

            // Act
            var result = await service.GetAllTransactions(pageNumber: 2, pageSize: 1, search: null);

            // Assert
            Assert.Equal(3, result.TotalItems);
            Assert.Single(result.Items);
            Assert.Equal(2, result.CurrentPage);
            Assert.Equal(1, result.PageSize);

            // Ordered by TransactionDate descending: 3, 2, 1
            // Page 2 with page size 1 should return TransactionID 2
            Assert.Equal(2, result.Items[0].TransactionID);
        }
    }
}