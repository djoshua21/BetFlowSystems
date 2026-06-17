using BetFlowSystems.Models;
using BetFlowSystems.Models.DbModels;
using BetFlowSystems.Models.DTOs.Account;
using BetFlowSystems.Models.DTOs.Bet;
using BetFlowSystems.Models.DTOs.User;
using BetFlowSystems.Models.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BetFlowSystems.Services
{
    public class BetService
    {
        private readonly ApplicationDbContext _context;

        public BetService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> CreateBet(CreateBetDto createBetDto)
        {

            var account = await _context.Accounts.FindAsync(createBetDto.AccountID);

            if (account == null)
            {

                return new Result
                {
                    Success = false,
                    ErrorMessage = "Account does not exist"
                };
            }

            if (account.AccountStatus == AccState.Closed)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Selected Account is closed, cannot create bet."
                };
            }



            var betTypeExists = await _context.BetTypes.AnyAsync(b => b.BetTypeID == createBetDto.BetTypeID);

            if (!betTypeExists)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Bet Type does not exist"
                };
            }


            // Calculation required in conjuction with transaction

            if (createBetDto.BetAmount <= 0)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Bet Amount must be > 0"
                };
            }

            if (createBetDto.PossibleWinAmount <= 0)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Win Amount must be > 0"
                };
            }


            if (createBetDto.BetDate.Date > DateTime.UtcNow.Date)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Bet Date cannot be in the future"
                };

            }

            var bet = new Bet
            {
                AccountID = createBetDto.AccountID,
                BetTypeID = createBetDto.BetTypeID,
                BetAmount = createBetDto.BetAmount,
                PossibleWinAmount = createBetDto.PossibleWinAmount,
                Result = BetStatus.Pending,
                BetDate = createBetDto.BetDate,
                LastUpdatedDate = DateTime.UtcNow,
            }
;


            await _context.Bets.AddAsync(bet);
            await _context.SaveChangesAsync();

            return new Result { Success = true, Data = bet };

        }

        public async Task<Result> UpdateBet(UpdateBetDto updateBetDto)
        {

            var bet = await _context.Bets.FindAsync(updateBetDto.BetID);

            if (bet == null)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Bet Not Found"
                };
            }

            if (bet.Result != BetStatus.Pending)
            {

                return new Result
                {
                    Success = false,
                    ErrorMessage = $"Bet Cannot be updated once in this state ({bet.Result})."
                };

            }



            bet.Result = updateBetDto.Result;
            bet.LastUpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new Result { Success = true, Data = bet };

        }

        public async Task<DisplayBetDto?> GetBet(int id)
        {
            var b = await _context.Bets.FindAsync(id);


            if (b == null)
            {
                return null;

            }

            var bet = new DisplayBetDto
            {
                BetID = b.BetID,
                AccountID = b.AccountID,
                BetTypeID = b.BetTypeID,
                BetAmount = b.BetAmount,
                PossibleWinAmount = b.PossibleWinAmount,
                Result = b.Result,
                BetDate = b.BetDate,
                LastUpdatedDate = b.LastUpdatedDate,
            };

            return bet;
        }

        public async Task<PagedResult<DisplayBetDto>> GetAllBets(int pageNumber, int pageSize, string? search)
        {

            var query = _context.Bets
                    .AsNoTracking()
                    .Where(b => b.BetID.ToString().Contains(search ?? "") || (b.AccountID.ToString()!).Contains(search ?? ""))
                    .OrderByDescending(b => b.LastUpdatedDate);


            var totalItems = await query.CountAsync();


            var bets = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new DisplayBetDto
                {
                    BetID = b.BetID,
                    AccountID = b.AccountID,
                    BetTypeID = b.BetTypeID,
                    BetAmount = b.BetAmount,
                    PossibleWinAmount = b.PossibleWinAmount,
                    Result = b.Result,
                    BetDate = b.BetDate,
                    LastUpdatedDate = b.LastUpdatedDate,

                }).ToListAsync();

            var pagedBets = new PagedResult<DisplayBetDto>
            {
                Items = bets,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };


            return pagedBets;
        }

        public async Task<PagedResult<DisplayBetDto>> GetAccountBets(int id, int pageNumber, int pageSize, string? search)
        {

            var query = _context.Bets
                    .AsNoTracking()
                    .Where(b => 
                    b.AccountID == id
                    && b.BetID.ToString()!.Contains(search ?? "")
                    )
                    .OrderByDescending(b => b.LastUpdatedDate);


            var totalItems = await query.CountAsync();


            var bets = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new DisplayBetDto
                {
                    BetID = b.BetID,
                    AccountID = b.AccountID,
                    BetTypeID = b.BetTypeID,
                    BetAmount = b.BetAmount,
                    PossibleWinAmount = b.PossibleWinAmount,
                    Result = b.Result,
                    BetDate = b.BetDate,
                    LastUpdatedDate = b.LastUpdatedDate,

                }).ToListAsync();

            var pagedBets = new PagedResult<DisplayBetDto>
            {
                Items = bets,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };


            return pagedBets;
        }

        public async Task<bool> AcccountBetsIncomplete(int accountID)
        {


            return await _context.Bets.AnyAsync(b => b.AccountID == accountID && b.Result != BetStatus.Pending); ;
        }


    }
}
