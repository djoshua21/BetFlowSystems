using BetFlowSystems.Models;
using BetFlowSystems.Models.DbModels;
using BetFlowSystems.Models.DTOs.User;
using BetFlowSystems.Models.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BetFlowSystems.Services
{
    public class UserService
    {

        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }



        public async Task<Result> CreateUser(CreateUserDto createUserDto)
        {

            //validation
            var userExists = await _context.AppUsers.AnyAsync(u => u.IdNumber == createUserDto.IdNumber);

            if (userExists)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "ID already exists in DB."
                };
            }

            if (string.IsNullOrWhiteSpace(createUserDto.Name))
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Name cannot be empty"
                };
            }

            if (string.IsNullOrWhiteSpace(createUserDto.Surname))
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Surname cannot be empty"
                };
            }

            if (string.IsNullOrWhiteSpace(createUserDto.Address))
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Address cannot be empty"
                };
            }


            var user = new User
            {
                IdNumber = createUserDto.IdNumber,
                Name = createUserDto.Name,
                Surname = createUserDto.Surname,
                Address = createUserDto.Address,

            };

            await _context.AppUsers.AddAsync(user);
            await _context.SaveChangesAsync();

            return new Result { Success = true };

        }

        public async Task<Result> UpdateUser(UpdateUserDto updateUserDto)
        {
            var user = await _context.AppUsers.FindAsync(updateUserDto.UserID);
            if (user == null)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "User Not Found"
                };
            }

            if (
                string.IsNullOrWhiteSpace(updateUserDto.Name) ||
                string.IsNullOrWhiteSpace(updateUserDto.Surname) ||
                string.IsNullOrWhiteSpace(updateUserDto.Address)
                )
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "Fields cannot be empty (Name, Surname, Address)"
                };
            }

            if (user.IsActive == true && updateUserDto.IsActive == false)
            {
                var accounts = await _context.Accounts.AnyAsync(a => a.UserID == user.UserID && a.AccountStatus == AccState.Open);

                if (accounts)
                {
                    return new Result
                    {
                        Success = false,
                        ErrorMessage = $"There are still open accounts for User ID ({user.UserID}). Close all accounts to Deactivate User"
                    };
                }

                user.IsActive = false;
            }
            else if (user.IsActive == false && updateUserDto.IsActive == true)
            {
                user.IsActive = true;
            }


            user.Name = updateUserDto.Name;
            user.Surname = updateUserDto.Surname;
            user.Address = updateUserDto.Address;


            await _context.SaveChangesAsync();


            return new Result { Success = true };
        }

        public async Task<PagedResult<DisplayUserDto>> GetAllUsers(int pageNumber, int pageSize, string? search)
        {

            var query = _context.AppUsers
                .AsNoTracking()
                .Where(u => u.UserID.ToString().Contains(search ?? "") || u.Surname.Contains(search ?? "") || u.IdNumber.Contains(search ?? ""))
                .OrderBy(x => x.UserID);


            var totalItems = await query.CountAsync();


            var users = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new DisplayUserDto
                {
                    UserID = u.UserID,
                    IdNumber = u.IdNumber,
                    IsActive = u.IsActive,
                    Name = u.Name,
                    Surname = u.Surname,
                    Address = u.Address,
                    CreatedDate = u.CreatedDate
                }).ToListAsync();

            var pagedUsers = new PagedResult<DisplayUserDto>
            {
                Items = users,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };


            return pagedUsers;
        }


        public async Task<Result> DeleteUser(int userID)
        {

            var user = await _context.AppUsers.FindAsync(userID);


            if (user == null)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "User Not Found"
                };
            }

            if (user.IsActive)
            {
                return new Result
                {
                    Success = false,
                    ErrorMessage = "User need to be deactivated first before deleting. Edit user to deactivate."
                };

            }

            _context.AppUsers.Remove(user);
            await _context.SaveChangesAsync();



            return new Result { Success = true };

        }

        public async Task<bool> DoesIdExist(string idNumber)
        {
            return await _context.AppUsers.AnyAsync(u => u.IdNumber == idNumber);
        }
        public async Task<DisplayUserDto?> GetUser(int userID)
        {
            var u = await _context.AppUsers.FindAsync(userID);

            if (u == null)
            {
                return null;

            }

            var user = new DisplayUserDto
            {
                UserID = u.UserID,
                IdNumber = u.IdNumber,
                IsActive = u.IsActive,
                Name = u.Name,
                Surname = u.Surname,
                Address = u.Address,
                CreatedDate = u.CreatedDate
            };

            return user;
        }








    }
}
