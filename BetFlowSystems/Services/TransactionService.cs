using BetFlowSystems.Models;
using BetFlowSystems.Models.DbModels;
using BetFlowSystems.Models.DTOs.Bet;
using BetFlowSystems.Models.DTOs.Transaction;
using BetFlowSystems.Models.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BetFlowSystems.Services
{
    public class TransactionService
    {

        private readonly ApplicationDbContext _context;

        public TransactionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> CreateTransaction(CreateTransactionDto createTransactionDto)
        {

            var betExists = await _context.Bets
                .AnyAsync(b => b.BetID == createTransactionDto.BetID);


            if (!betExists)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Bet does not exist"
                };
            }

            var accountExists = await _context.Accounts.AnyAsync(a => a.AccountID == createTransactionDto.AccountID);

            if (!accountExists)
            {

                return new Result
                {
                    Success = false,
                    ErrorMessage = "Account does not exist"
                };
            }


            var transaction = new Transaction
            {
                AccountID = createTransactionDto.AccountID,
                BetID = createTransactionDto.BetID,
                Amount = createTransactionDto.Amount,
                TransactionType = createTransactionDto.TransactionType,
                TransactionDate = DateTime.UtcNow
            };

            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();


            return new Result
            {
                Success = true,
                Data = transaction
            };

        }
        public async Task<DisplayTransactionDto?> GetTransaction(int id)
        {
            var t = await _context.Transactions.FindAsync(id);

            if (t == null)
            {
                return null;

            }

            var transaction = new DisplayTransactionDto
            {
                TransactionID = t.TransactionID,
                AccountID = t.AccountID,
                BetID = t.BetID,
                Amount = t.Amount,
                TransactionType = t.TransactionType,
                TransactionDate = t.TransactionDate,
            };

            return transaction;

        }

        public async Task<PagedResult<DisplayTransactionDto>> GetAllTransactions(int pageNumber, int pageSize, string? search)
        {

            var query = _context.Transactions
               .AsNoTracking()
               .Where(t => t.TransactionID.ToString().Contains(search ?? "")
               || t.BetID.ToString().Contains(search ?? ""))
               .OrderByDescending(t => t.TransactionDate);


            var totalItems = await query.CountAsync();


            var bets = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new DisplayTransactionDto
                {
                    TransactionID = t.TransactionID,
                    AccountID = t.AccountID,
                    BetID = t.BetID,
                    Amount = t.Amount,
                    TransactionType = t.TransactionType,
                    TransactionDate = t.TransactionDate,

                }).ToListAsync();

            var pagedTransactions = new PagedResult<DisplayTransactionDto>
            {
                Items = bets,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };


            return pagedTransactions;
        }


        public async Task<PagedResult<DisplayTransactionDto>> GetBetTransactions(int id, int pageNumber, int pageSize, string? search)
        {

            var query = _context.Transactions
               .AsNoTracking()
               .Where(t => 
               t.TransactionID.ToString().Contains(search ?? "")
               && t.BetID == id)
               .OrderByDescending(t => t.TransactionDate);


            var totalItems = await query.CountAsync();


            var bets = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new DisplayTransactionDto
                {
                    TransactionID = t.TransactionID,
                    AccountID = t.AccountID,
                    BetID = t.BetID,
                    Amount = t.Amount,
                    TransactionType = t.TransactionType,
                    TransactionDate = t.TransactionDate,

                }).ToListAsync();

            var pagedTransactions = new PagedResult<DisplayTransactionDto>
            {
                Items = bets,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };


            return pagedTransactions;
        }

    }
}
