using BetFlowSystems.Models;
using BetFlowSystems.Models.DbModels;
using BetFlowSystems.Models.DTOs.Bet;
using BetFlowSystems.Models.DTOs.Transaction;

namespace BetFlowSystems.Services
{

    public class BetTransactionService
    {
        private readonly ApplicationDbContext _context;
        private readonly TransactionService _transactionService;
        private readonly BetService _betService;
        private readonly AccountService _accountService;

        public BetTransactionService(ApplicationDbContext context,
            TransactionService transactionService,
            BetService betService,
            AccountService accountService)
        {
            _context = context;
            _transactionService = transactionService;
            _betService = betService;
            _accountService = accountService;
        }


        public async Task<Result> CreateBetAndTransaction(CreateBetDto createBetDto)
        {

            await using var dbTransaction =
                        await _context.Database.BeginTransactionAsync();

            var message = "Could not create bet. Unhandled reason";
            try
            {
                var amount = -createBetDto.BetAmount;


                var acc_result = await _accountService.UpdateBalance(createBetDto.AccountID, amount);

                if (!acc_result.Success)
                {
                    message = acc_result.ErrorMessage;
                    throw new Exception(message);
                }



                var bet_result = await _betService.CreateBet(createBetDto);

                if (!bet_result.Success)
                {
                    message = bet_result.ErrorMessage;
                    throw new Exception(message);
                }

                var bet = (Bet)bet_result.Data!;

                var createTransactionDto = new CreateTransactionDto
                {
                    BetID = bet.BetID,
                    AccountID = bet.AccountID,
                    Amount = bet.BetAmount,
                    TransactionType = DebitOrCredit.Debit
                };

                var t_result = await _transactionService.CreateTransaction(createTransactionDto);

                if (!t_result.Success)
                {
                    message = t_result.ErrorMessage;
                    throw new Exception(message);

                }

                await dbTransaction.CommitAsync();

            }
            catch
            {
                await dbTransaction.RollbackAsync();
                return new Result
                {
                    Success = false,
                    ErrorMessage = message
                };
            }

            return new Result { Success = true };

        }

        public async Task<Result> UpdateBetAndTransaction(UpdateBetDto updateBetDto)
        {


            await using var dbTransaction =
            await _context.Database.BeginTransactionAsync();

            var message = "Could not update bet";
            try
            {

                var bet_result = await _betService.UpdateBet(updateBetDto);

                if (!bet_result.Success)
                {
                    message = bet_result.ErrorMessage;
                    throw new Exception(message);
                }

                var bet = (Bet)bet_result.Data!;

                decimal? amount = null;
                switch (bet.Result)
                {
                    case BetStatus.Win:
                        amount = bet.PossibleWinAmount;
                        break;

                    case BetStatus.Cancelled:
                        amount = bet.BetAmount;
                        break;

                    default:
                        break;
                }

                if (bet.AccountID == null)
                {
                    message = bet_result.ErrorMessage;
                    throw new Exception(message);

                }

                if (amount != null)
                {
                    var acc_result = await _accountService.UpdateBalance((int)bet.AccountID, (decimal)amount);

                    if (!acc_result.Success)
                    {
                        message = acc_result.ErrorMessage;
                        throw new Exception(message);
                    }



                    var createTransactionDto = new CreateTransactionDto
                    {
                        BetID = bet.BetID,
                        AccountID = bet.AccountID,
                        Amount = (decimal)(amount < 0 ? -amount : amount),
                        TransactionType = DebitOrCredit.Credit
                    };

                    var t_result = await _transactionService.CreateTransaction(createTransactionDto);

                    if (!t_result.Success)
                    {
                        message = t_result.ErrorMessage;
                        throw new Exception(message);

                    }
                }



                await dbTransaction.CommitAsync();

            }
            catch
            {
                await dbTransaction.RollbackAsync();
                return new Result
                {
                    Success = false,
                    ErrorMessage = message
                };
            }

            return new Result { Success = true };

        }



    }
}
