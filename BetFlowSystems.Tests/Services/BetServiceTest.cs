using System;
using System.Linq;
using System.Threading.Tasks;
using BetFlowSystems.Models;
using BetFlowSystems.Models.DbModels;
using BetFlowSystems.Models.DTOs.Bet;
using BetFlowSystems.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BetFlowSystems.Tests.Services
{
    public class BetServiceTests
    {
        private static ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            return context;
        }

        private static AccState GetOpenAccountState()
        {
            var state = Enum.GetValues<AccState>().FirstOrDefault(x => x != AccState.Closed);

            if (state.Equals(default(AccState)) && state == AccState.Closed)
            {
                throw new InvalidOperationException("No non-closed AccState value was found for testing.");
            }

            return state;
        }

        private static BetStatus GetNonPendingBetStatus()
        {
            var values = Enum.GetValues<BetStatus>();
            var nonPending = values.FirstOrDefault(x => x != BetStatus.Pending);

            if (nonPending.Equals(default(BetStatus)) && nonPending == BetStatus.Pending)
            {
                throw new InvalidOperationException("No non-pending BetStatus value was found for testing.");
            }

            return nonPending;
        }

        private static Account CreateValidAccount(int accountId = 1, AccState? status = null)
        {
            return new Account
            {
                AccountID = accountId,
                AccountStatus = status ?? GetOpenAccountState(),
                Title = $"Test Account {accountId}",
                Balance = 1000m,
                Description = "Seeded test account",
                CreatedDate = DateTime.UtcNow.AddDays(-5),
                UserID = 1
            };
        }

        private static BetType CreateValidBetType(int betTypeId = 1)
        {
            return new BetType
            {
                BetTypeID = betTypeId,
                Sport = "Football",
                EventName = $"Match {betTypeId}",
                Description = "Seeded test bet type"
            };
        }

        private static Bet CreateValidBet(
            int betId = 1,
            int? accountId = 1,
            int betTypeId = 1,
            BetStatus? result = null,
            DateTime? lastUpdatedDate = null)
        {
            return new Bet
            {
                BetID = betId,
                AccountID = accountId,
                BetTypeID = betTypeId,
                BetAmount = 100m,
                PossibleWinAmount = 250m,
                Result = result ?? BetStatus.Pending,
                BetDate = DateTime.UtcNow.AddDays(-1),
                LastUpdatedDate = lastUpdatedDate ?? DateTime.UtcNow.AddMinutes(-10)
            };
        }

        [Fact]
        public async Task CreateBet_ReturnsFailure_WhenAccountDoesNotExist()
        {
            using var context = CreateContext();
            context.BetTypes.Add(CreateValidBetType(1));
            await context.SaveChangesAsync();

            var service = new BetService(context);

            var dto = new CreateBetDto
            {
                AccountID = 999,
                BetTypeID = 1,
                BetAmount = 100m,
                PossibleWinAmount = 200m,
                BetDate = DateTime.UtcNow.AddDays(-1)
            };

            var result = await service.CreateBet(dto);

            Assert.False(result.Success);
            Assert.Equal("Account does not exist", result.ErrorMessage);
            Assert.Empty(context.Bets);
        }

        [Fact]
        public async Task CreateBet_ReturnsFailure_WhenAccountIsClosed()
        {
            using var context = CreateContext();
            context.Accounts.Add(CreateValidAccount(1, AccState.Closed));
            context.BetTypes.Add(CreateValidBetType(1));
            await context.SaveChangesAsync();

            var service = new BetService(context);

            var dto = new CreateBetDto
            {
                AccountID = 1,
                BetTypeID = 1,
                BetAmount = 100m,
                PossibleWinAmount = 200m,
                BetDate = DateTime.UtcNow.AddDays(-1)
            };

            var result = await service.CreateBet(dto);

            Assert.False(result.Success);
            Assert.Equal("Selected Account is closed, cannot create bet.", result.ErrorMessage);
            Assert.Empty(context.Bets);
        }

        [Fact]
        public async Task CreateBet_ReturnsFailure_WhenBetTypeDoesNotExist()
        {
            using var context = CreateContext();
            context.Accounts.Add(CreateValidAccount(1));
            await context.SaveChangesAsync();

            var service = new BetService(context);

            var dto = new CreateBetDto
            {
                AccountID = 1,
                BetTypeID = 999,
                BetAmount = 100m,
                PossibleWinAmount = 200m,
                BetDate = DateTime.UtcNow.AddDays(-1)
            };

            var result = await service.CreateBet(dto);

            Assert.False(result.Success);
            Assert.Equal("Bet Type does not exist", result.ErrorMessage);
            Assert.Empty(context.Bets);
        }

        [Fact]
        public async Task CreateBet_ReturnsFailure_WhenBetAmountIsZeroOrLess()
        {
            using var context = CreateContext();
            context.Accounts.Add(CreateValidAccount(1));
            context.BetTypes.Add(CreateValidBetType(1));
            await context.SaveChangesAsync();

            var service = new BetService(context);

            var dto = new CreateBetDto
            {
                AccountID = 1,
                BetTypeID = 1,
                BetAmount = 0m,
                PossibleWinAmount = 200m,
                BetDate = DateTime.UtcNow.AddDays(-1)
            };

            var result = await service.CreateBet(dto);

            Assert.False(result.Success);
            Assert.Equal("Bet Amount must be > 0", result.ErrorMessage);
            Assert.Empty(context.Bets);
        }

        [Fact]
        public async Task CreateBet_ReturnsFailure_WhenPossibleWinAmountIsZeroOrLess()
        {
            using var context = CreateContext();
            context.Accounts.Add(CreateValidAccount(1));
            context.BetTypes.Add(CreateValidBetType(1));
            await context.SaveChangesAsync();

            var service = new BetService(context);

            var dto = new CreateBetDto
            {
                AccountID = 1,
                BetTypeID = 1,
                BetAmount = 100m,
                PossibleWinAmount = 0m,
                BetDate = DateTime.UtcNow.AddDays(-1)
            };

            var result = await service.CreateBet(dto);

            Assert.False(result.Success);
            Assert.Equal("Win Amount must be > 0", result.ErrorMessage);
            Assert.Empty(context.Bets);
        }

        [Fact]
        public async Task CreateBet_ReturnsFailure_WhenBetDateIsInFuture()
        {
            using var context = CreateContext();
            context.Accounts.Add(CreateValidAccount(1));
            context.BetTypes.Add(CreateValidBetType(1));
            await context.SaveChangesAsync();

            var service = new BetService(context);

            var dto = new CreateBetDto
            {
                AccountID = 1,
                BetTypeID = 1,
                BetAmount = 100m,
                PossibleWinAmount = 200m,
                BetDate = DateTime.UtcNow.AddDays(1)
            };

            var result = await service.CreateBet(dto);

            Assert.False(result.Success);
            Assert.Equal("Bet Date cannot be in the future", result.ErrorMessage);
            Assert.Empty(context.Bets);
        }

        [Fact]
        public async Task CreateBet_CreatesBet_WhenInputIsValid()
        {
            using var context = CreateContext();
            context.Accounts.Add(CreateValidAccount(1));
            context.BetTypes.Add(CreateValidBetType(1));
            await context.SaveChangesAsync();

            var service = new BetService(context);

            var dto = new CreateBetDto
            {
                AccountID = 1,
                BetTypeID = 1,
                BetAmount = 150m,
                PossibleWinAmount = 300m,
                BetDate = DateTime.UtcNow.AddDays(-1)
            };

            var result = await service.CreateBet(dto);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);

            var bet = Assert.IsType<Bet>(result.Data);
            Assert.Equal(1, bet.AccountID);
            Assert.Equal(1, bet.BetTypeID);
            Assert.Equal(150m, bet.BetAmount);
            Assert.Equal(300m, bet.PossibleWinAmount);
            Assert.Equal(BetStatus.Pending, bet.Result);

            Assert.Single(context.Bets);
        }

        [Fact]
        public async Task UpdateBet_ReturnsFailure_WhenBetDoesNotExist()
        {
            using var context = CreateContext();
            var service = new BetService(context);

            var dto = new UpdateBetDto
            {
                BetID = 999,
                Result = GetNonPendingBetStatus()
            };

            var result = await service.UpdateBet(dto);

            Assert.False(result.Success);
            Assert.Equal("Bet Not Found", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateBet_ReturnsFailure_WhenBetIsNotPending()
        {
            using var context = CreateContext();
            var resolvedStatus = GetNonPendingBetStatus();

            context.Bets.Add(CreateValidBet(
                betId: 1,
                result: resolvedStatus,
                lastUpdatedDate: DateTime.UtcNow.AddHours(-2)));

            await context.SaveChangesAsync();

            var service = new BetService(context);

            var dto = new UpdateBetDto
            {
                BetID = 1,
                Result = BetStatus.Pending
            };

            var result = await service.UpdateBet(dto);

            Assert.False(result.Success);
            Assert.Equal($"Bet Cannot be updated once in this state ({resolvedStatus}).", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateBet_UpdatesBet_WhenBetIsPending()
        {
            using var context = CreateContext();
            context.Bets.Add(CreateValidBet(
                betId: 1,
                result: BetStatus.Pending,
                lastUpdatedDate: DateTime.UtcNow.AddHours(-2)));

            await context.SaveChangesAsync();

            var service = new BetService(context);
            var newStatus = GetNonPendingBetStatus();

            var dto = new UpdateBetDto
            {
                BetID = 1,
                Result = newStatus
            };

            var beforeUpdate = context.Bets.Single(b => b.BetID == 1).LastUpdatedDate;

            var result = await service.UpdateBet(dto);

            Assert.True(result.Success);

            var updatedBet = await context.Bets.FindAsync(1);
            Assert.NotNull(updatedBet);
            Assert.Equal(newStatus, updatedBet!.Result);
            Assert.True(updatedBet.LastUpdatedDate >= beforeUpdate);
        }

        [Fact]
        public async Task GetBet_ReturnsNull_WhenBetDoesNotExist()
        {
            using var context = CreateContext();
            var service = new BetService(context);

            var result = await service.GetBet(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetBet_ReturnsDisplayBetDto_WhenBetExists()
        {
            using var context = CreateContext();
            context.Bets.Add(CreateValidBet(betId: 1));
            await context.SaveChangesAsync();

            var service = new BetService(context);

            var result = await service.GetBet(1);

            Assert.NotNull(result);
            Assert.Equal(1, result!.BetID);
            Assert.Equal(1, result.AccountID);
            Assert.Equal(1, result.BetTypeID);
            Assert.Equal(100m, result.BetAmount);
            Assert.Equal(250m, result.PossibleWinAmount);
            Assert.Equal(BetStatus.Pending, result.Result);
        }

        [Fact]
        public async Task GetAllBets_ReturnsPagedResults_OrderedByLastUpdatedDateDescending()
        {
            using var context = CreateContext();

            context.Bets.AddRange(
                CreateValidBet(betId: 1, accountId: 1, lastUpdatedDate: DateTime.UtcNow.AddHours(-3)),
                CreateValidBet(betId: 2, accountId: 2, lastUpdatedDate: DateTime.UtcNow.AddHours(-1)),
                CreateValidBet(betId: 3, accountId: 3, lastUpdatedDate: DateTime.UtcNow.AddHours(-2))
            );

            await context.SaveChangesAsync();

            var service = new BetService(context);

            var result = await service.GetAllBets(pageNumber: 1, pageSize: 2, search: null);

            Assert.Equal(3, result.TotalItems);
            Assert.Equal(1, result.CurrentPage);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(2, result.Items.Count);

            Assert.Equal(2, result.Items[0].BetID);
            Assert.Equal(3, result.Items[1].BetID);
        }

        [Fact]
        public async Task GetAllBets_AppliesSearchAgainstBetIdOrAccountId()
        {
            using var context = CreateContext();

            context.Bets.AddRange(
                CreateValidBet(betId: 10, accountId: 101),
                CreateValidBet(betId: 20, accountId: 202),
                CreateValidBet(betId: 30, accountId: 999)
            );

            await context.SaveChangesAsync();

            var service = new BetService(context);

            var byBetId = await service.GetAllBets(pageNumber: 1, pageSize: 10, search: "20");
            var byAccountId = await service.GetAllBets(pageNumber: 1, pageSize: 10, search: "999");

            Assert.Single(byBetId.Items);
            Assert.Equal(20, byBetId.Items[0].BetID);

            Assert.Single(byAccountId.Items);
            Assert.Equal(30, byAccountId.Items[0].BetID);
        }

        [Fact]
        public async Task GetAccountBets_ReturnsOnlyBetsForSpecifiedAccount_AndAppliesSearch()
        {
            using var context = CreateContext();

            context.Bets.AddRange(
                CreateValidBet(betId: 11, accountId: 1, lastUpdatedDate: DateTime.UtcNow.AddHours(-1)),
                CreateValidBet(betId: 12, accountId: 1, lastUpdatedDate: DateTime.UtcNow.AddHours(-2)),
                CreateValidBet(betId: 21, accountId: 2, lastUpdatedDate: DateTime.UtcNow.AddHours(-3))
            );

            await context.SaveChangesAsync();

            var service = new BetService(context);

            var allForAccount1 = await service.GetAccountBets(id: 1, pageNumber: 1, pageSize: 10, search: null);
            var searchedForAccount1 = await service.GetAccountBets(id: 1, pageNumber: 1, pageSize: 10, search: "12");

            Assert.Equal(2, allForAccount1.TotalItems);
            Assert.Equal(2, allForAccount1.Items.Count);
            Assert.All(allForAccount1.Items, x => Assert.Equal(1, x.AccountID));

            Assert.Single(searchedForAccount1.Items);
            Assert.Equal(12, searchedForAccount1.Items[0].BetID);
        }

        [Fact]
        public async Task AcccountBetsIncomplete_ReturnsFalse_WhenAllBetsArePending()
        {
            using var context = CreateContext();

            context.Bets.AddRange(
                CreateValidBet(betId: 1, accountId: 1, result: BetStatus.Pending),
                CreateValidBet(betId: 2, accountId: 1, result: BetStatus.Pending)
            );

            await context.SaveChangesAsync();

            var service = new BetService(context);

            var result = await service.AcccountBetsIncomplete(1);

            Assert.False(result);
        }

        [Fact]
        public async Task AcccountBetsIncomplete_ReturnsTrue_WhenAnyBetIsNotPending()
        {
            using var context = CreateContext();
            var resolvedStatus = GetNonPendingBetStatus();

            context.Bets.AddRange(
                CreateValidBet(betId: 1, accountId: 1, result: BetStatus.Pending),
                CreateValidBet(betId: 2, accountId: 1, result: resolvedStatus)
            );

            await context.SaveChangesAsync();

            var service = new BetService(context);

            var result = await service.AcccountBetsIncomplete(1);

            Assert.True(result);
        }
    }
}
