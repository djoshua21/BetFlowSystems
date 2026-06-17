using BetFlowSystems.Models;
using BetFlowSystems.Models.DbModels;
using BetFlowSystems.Models.DTOs.Account;
using BetFlowSystems.Models.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BetFlowSystems.Services
{
    public class AccountService
    {

        private readonly ApplicationDbContext _context;

        public AccountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DisplayAccountDto?> GetAccount(int id)
        {

            var acc = await _context.Accounts.FindAsync(id);


            if (acc == null)
            {
                return null;

            }

            var account = new DisplayAccountDto
            {
                AccountID = acc.AccountID,
                AccountStatus = acc.AccountStatus,
                Title = acc.Title,
                Balance = acc.Balance,
                Description = acc.Description,
                CreatedDate = acc.CreatedDate,
                UserID = acc.UserID,
            };

            return account;

        }

        public async Task<PagedResult<DisplayAccountDto>> GetAllAccounts(int pageNumber, int pageSize, string? search)
        {
            var query = _context.Accounts
                .AsNoTracking()
                .Where(a => a.UserID.ToString().Contains(search ?? "") || a.AccountID.ToString().Contains(search ?? ""))
                .OrderBy(x => x.AccountID);

            var totalItems = await query.CountAsync();

            var accounts = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new DisplayAccountDto
                {
                    AccountID = a.AccountID,
                    AccountStatus = a.AccountStatus,
                    Title = a.Title,
                    Balance = a.Balance,
                    Description = a.Description,
                    CreatedDate = a.CreatedDate,
                    UserID = a.UserID

                }).ToListAsync();

            var pagedUsers = new PagedResult<DisplayAccountDto>
            {
                Items = accounts,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };


            return pagedUsers;
        }


        public async Task<PagedResult<DisplayAccountDto>> GetUserAccounts(int id, int pageNumber, int pageSize, string? search)
        {
            var query = _context.Accounts
                .AsNoTracking()
                .Where(a => a.UserID == id && a.AccountID.ToString().Contains(search ?? ""))
                .OrderBy(x => x.AccountID);

            var totalItems = await query.CountAsync();

            var accounts = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new DisplayAccountDto
                {
                    AccountID = a.AccountID,
                    AccountStatus = a.AccountStatus,
                    Title = a.Title,
                    Balance = a.Balance,
                    Description = a.Description,
                    CreatedDate = a.CreatedDate,
                    UserID = a.UserID

                }).ToListAsync();

            var pagedUsers = new PagedResult<DisplayAccountDto>
            {
                Items = accounts,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };


            return pagedUsers;
        }

        public async Task<Result> CreateAccount(CreateAccountDto createAccountDto)
        {

            if (string.IsNullOrWhiteSpace(createAccountDto.Title))
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Title cannot be empty"
                };
            }

            if (createAccountDto.UserID <= 0)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Invalid User ID"
                };
            }

            var user = await _context.AppUsers
                .FindAsync(createAccountDto.UserID);

            if (user == null)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "User not found"
                };
            }

            if (!user.IsActive)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "User is Deactivated. Cannot create accounts on deactivated users"
                };
            }



            var account = new Account
            {
                AccountStatus = AccState.Open,
                Balance = 0,
                Title = createAccountDto.Title,
                Description = createAccountDto.Description,
                UserID = createAccountDto.UserID,
            };

            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            return new Result { Success = true };
        }

        public async Task<Result> UpdateAccount(UpdateAccountDto updateAccountDto)
        {

            var account = await _context.Accounts.FindAsync(updateAccountDto.AccountId);


            if (account == null)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Account Not Found"
                };
            }

            var user = await _context.AppUsers.FindAsync(account.UserID);

            if (user == null)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "User not found"
                };
            }

            if (!user.IsActive)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "User is Deactivated. Cannot update accounts on deactivated users"
                };
            }



            if (account.AccountStatus == AccState.Closed
                && updateAccountDto.AccountStatus == AccState.Closed)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Cannot update account while it is closed",
                };
            }

            var betsPending = await _context.Bets.
                    AnyAsync(b => b.AccountID == account.AccountID
                              && b.Result == BetStatus.Pending);

            if (betsPending
                && account.AccountStatus == AccState.Open
                && updateAccountDto.AccountStatus == AccState.Closed)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Cannot close account while Bets Results are Pending",
                };
            }


            if (account.AccountStatus == AccState.Open
                && updateAccountDto.AccountStatus == AccState.Closed
                && account.Balance != 0
                )
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Balance must be 0 to close account.",
                };

            }


            account.Title = updateAccountDto.Title;
            account.Description = updateAccountDto.Description;
            account.AccountStatus = updateAccountDto.AccountStatus;

            await _context.SaveChangesAsync();

            return new Result { Success = true };

        }

        public async Task<Result> UpdateBalance(int id, decimal amount)
        {
            var acc = await _context.Accounts.FindAsync(id);

            if (acc == null)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Account not found"
                };

            }

            if (acc.AccountStatus == AccState.Closed)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Account is closed"
                };
            }

            var new_balance = acc.Balance + amount;


            if (new_balance < 0)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Insufficient Funds. Please check balance."
                };
            }

            acc.Balance = new_balance;

            await _context.SaveChangesAsync();

            return new Result { Success = true };
        }

        public async Task<Result> DeactivateAccount(int accountID)
        {
            var acc = _context.Accounts.Find(accountID);

            if (acc == null)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Account not found"
                };
            }

            if (acc.Balance != 0)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Balance must be 0 to deactivate account."
                };
            }

            acc.AccountStatus = AccState.Closed;

            await _context.SaveChangesAsync();


            return new Result { Success = true };

        }

        public async Task<Result> DeleteAccount(int accountID)
        {
            var acc = _context.Accounts.Find(accountID);

            if (acc == null)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Account not found"
                };
            }
            if (acc.AccountStatus == AccState.Open)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Account must be closed before it is deleted"
                };
            }

            if (acc.Balance != 0)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Balance must be 0 to deactivate account."
                };
            }
            _context.Remove(acc);

            await _context.SaveChangesAsync();

            return new Result
            {
                Success = true,
                ErrorMessage = "Balance must be 0 to deactivate account."
            };
        }
    }
}