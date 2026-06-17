using BetFlowSystems.Models;
using BetFlowSystems.Models.DTOs.Bet;
using Microsoft.EntityFrameworkCore;

namespace BetFlowSystems.Services
{
    public class UserSimulationService
    {
        private readonly ApplicationDbContext _context;
        private readonly BetTransactionService _betTransactionService;

        //private readonly TransactionService _transactionService;
        //private readonly BetService _betService;
        //private readonly AccountService _accountService;
        //private readonly UserService _userService;

        public UserSimulationService(
            ApplicationDbContext context,
            BetTransactionService betTransactionService
        //TransactionService transactionService,
        //BetService betService,
        //AccountService accountService,
        //UserService userService
        )
        {
            _context = context;
            _betTransactionService = betTransactionService;
            //_transactionService = transactionService;
            //_betService = betService;
            //_accountService = accountService;
            //_userService = userService;
        }

        public async Task<Result> Deposit(int accountID, decimal amount)
        {
            if (amount < 0)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Invalid amount, it should be > 0",
                };
            }

            var acc = await _context.Accounts.FindAsync(accountID);
            if (acc == null)
            {
                return new Result { Success = false, ErrorMessage = "Account does not exist" };
            }

            if (acc.AccountStatus == AccState.Closed)
            {
                return new Result { Success = false, ErrorMessage = "Account is closed" };
            }

            acc.Balance += amount;
            _context.SaveChanges();

            return new Result { Success = true };
        }

        public async Task<Result> Withdrawal(int accountID, decimal amount)
        {
            if (amount < 0)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Invalid amount, it should be > 0",
                };
            }

            var acc = await _context.Accounts.FindAsync(accountID);

            if (acc == null)
            {
                return new Result { Success = false, ErrorMessage = "Account does not exist" };
            }

            if (acc.AccountStatus == AccState.Closed)
            {
                return new Result { Success = false, ErrorMessage = "Account is closed" };
            }

            if (amount > acc.Balance)
            {
                return new Result { Success = false, ErrorMessage = "Insufficient Funds." };
            }

            acc.Balance -= amount;
            _context.SaveChanges();

            return new Result { Success = true };
        }

        public async Task<Result> ZeroAccount(int accountID)
        {
            var acc = await _context.Accounts.FindAsync(accountID);

            if (acc == null)
            {
                return new Result { Success = false, ErrorMessage = "Account does not exist" };
            }

            if (acc.AccountStatus == AccState.Closed)
            {
                return new Result { Success = false, ErrorMessage = "Account is closed" };
            }

            acc.Balance = 0;
            _context.SaveChanges();

            return new Result { Success = true };
        }

        public async Task<Result> GenerateRandomBet(int accountID)
        {
            var acc = await _context.Accounts.FindAsync(accountID);

            if (acc == null)
            {
                return new Result { Success = false, ErrorMessage = "Account does not exist" };
            }

            Random randomNumber = new Random();
            var amount = randomNumber.Next(1, (int)acc.Balance + 1);
            var winAmount = randomNumber.Next(amount * 10);

            var betTypes = await _context
                .BetTypes.AsNoTracking()
                .Select(bt => bt.BetTypeID)
                .ToArrayAsync();

            var betTypeID = betTypes[randomNumber.Next(betTypes.Length)];

            var createBetDto = new CreateBetDto
            {
                AccountID = accountID,
                BetTypeID = betTypeID,
                BetDate = DateTime.UtcNow,
                BetAmount = amount,
                PossibleWinAmount = amount + winAmount,
            };

            var result = await _betTransactionService.CreateBetAndTransaction(createBetDto);

            if (result.Success)
            {
                return new Result { Success = true };
            }
            else
            {
                return result;
            }
        }
    }
}
