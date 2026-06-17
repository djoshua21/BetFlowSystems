using System;
using System.Linq;
using System.Threading.Tasks;
using BetFlowSystems.Models;
using BetFlowSystems.Models.DbModels;
using BetFlowSystems.Models.DTOs.User;
using BetFlowSystems.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BetFlowSystems.Tests.Services
{
    public class UserServiceTests
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

        private static UserService CreateService(ApplicationDbContext context)
            => new UserService(context);

        private static User CreateValidUser(
            int userId,
            string idNumber,
            string name = "John",
            string surname = "Doe",
            string address = "123 Main Road",
            bool isActive = true)
        {
            return new User
            {
                UserID = userId,
                IdNumber = idNumber,
                Name = name,
                Surname = surname,
                Address = address,
                IsActive = isActive,
                CreatedDate = new DateTime(2026, 1, 1)
            };
        }

        private static CreateUserDto CreateValidCreateUserDto(
            string idNumber = "9901015009087",
            string name = "Jane",
            string surname = "Smith",
            string address = "45 Long Street")
        {
            return new CreateUserDto
            {
                IdNumber = idNumber,
                Name = name,
                Surname = surname,
                Address = address
            };
        }

        private static UpdateUserDto CreateValidUpdateUserDto(
            int userId,
            string name = "UpdatedName",
            string surname = "UpdatedSurname",
            string address = "Updated Address",
            bool isActive = true)
        {
            return new UpdateUserDto
            {
                UserID = userId,
                Name = name,
                Surname = surname,
                Address = address,
                IsActive = isActive
            };
        }

        [Fact]
        public async Task CreateUser_Should_Create_User_When_Dto_Is_Valid()
        {
            await using var context = CreateContext();
            var service = CreateService(context);

            var dto = CreateValidCreateUserDto();

            var result = await service.CreateUser(dto);

            Assert.True(result.Success);
            Assert.Null(result.ErrorMessage);

            var userInDb = await context.AppUsers.SingleAsync();
            Assert.Equal(dto.IdNumber, userInDb.IdNumber);
            Assert.Equal(dto.Name, userInDb.Name);
            Assert.Equal(dto.Surname, userInDb.Surname);
            Assert.Equal(dto.Address, userInDb.Address);
            Assert.True(userInDb.IsActive);
        }

        [Fact]
        public async Task CreateUser_Should_Fail_When_IdNumber_Already_Exists()
        {
            await using var context = CreateContext();
            context.AppUsers.Add(CreateValidUser(
                userId: 1,
                idNumber: "9901015009087",
                name: "Existing",
                surname: "User",
                address: "Existing Address"));
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var dto = CreateValidCreateUserDto(idNumber: "9901015009087");

            var result = await service.CreateUser(dto);

            Assert.False(result.Success);
            Assert.Equal("ID already exists in DB.", result.ErrorMessage);
            Assert.Equal(1, await context.AppUsers.CountAsync());
        }

        [Fact]
        public async Task CreateUser_Should_Fail_When_Name_Is_Empty()
        {
            await using var context = CreateContext();
            var service = CreateService(context);

            var dto = CreateValidCreateUserDto(name: " ");

            var result = await service.CreateUser(dto);

            Assert.False(result.Success);
            Assert.Equal("Name cannot be empty", result.ErrorMessage);
            Assert.Empty(context.AppUsers);
        }

        [Fact]
        public async Task CreateUser_Should_Fail_When_Surname_Is_Empty()
        {
            await using var context = CreateContext();
            var service = CreateService(context);

            var dto = CreateValidCreateUserDto(surname: " ");

            var result = await service.CreateUser(dto);

            Assert.False(result.Success);
            Assert.Equal("Surname cannot be empty", result.ErrorMessage);
            Assert.Empty(context.AppUsers);
        }

        [Fact]
        public async Task CreateUser_Should_Fail_When_Address_Is_Empty()
        {
            await using var context = CreateContext();
            var service = CreateService(context);

            var dto = CreateValidCreateUserDto(address: " ");

            var result = await service.CreateUser(dto);

            Assert.False(result.Success);
            Assert.Equal("Address cannot be empty", result.ErrorMessage);
            Assert.Empty(context.AppUsers);
        }

        [Fact]
        public async Task UpdateUser_Should_Fail_When_User_Not_Found()
        {
            await using var context = CreateContext();
            var service = CreateService(context);

            var dto = CreateValidUpdateUserDto(userId: 999);

            var result = await service.UpdateUser(dto);

            Assert.False(result.Success);
            Assert.Equal("User Not Found", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateUser_Should_Fail_When_Required_Fields_Are_Empty()
        {
            await using var context = CreateContext();
            context.AppUsers.Add(CreateValidUser(
                userId: 1,
                idNumber: "9001015009087"));
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var dto = new UpdateUserDto
            {
                UserID = 1,
                Name = " ",
                Surname = "UpdatedSurname",
                Address = "Updated Address",
                IsActive = true
            };

            var result = await service.UpdateUser(dto);

            Assert.False(result.Success);
            Assert.Equal("Fields cannot be empty (Name, Surname, Address)", result.ErrorMessage);

            var userInDb = await context.AppUsers.FindAsync(1);
            Assert.NotNull(userInDb);
            Assert.Equal("John", userInDb!.Name);
        }

        [Fact]
        public async Task UpdateUser_Should_Deactivate_User_When_No_Open_Accounts_Exist()
        {
            await using var context = CreateContext();
            context.AppUsers.Add(CreateValidUser(
                userId: 1,
                idNumber: "9001015009087",
                isActive: true));
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var dto = CreateValidUpdateUserDto(
                userId: 1,
                name: "Updated",
                surname: "User",
                address: "New Address",
                isActive: false);

            var result = await service.UpdateUser(dto);

            Assert.True(result.Success);

            var userInDb = await context.AppUsers.FindAsync(1);
            Assert.NotNull(userInDb);
            Assert.False(userInDb!.IsActive);
            Assert.Equal("Updated", userInDb.Name);
            Assert.Equal("User", userInDb.Surname);
            Assert.Equal("New Address", userInDb.Address);
        }

        [Fact]
        public async Task UpdateUser_Should_Reactivate_User_When_Currently_Inactive()
        {
            await using var context = CreateContext();
            context.AppUsers.Add(CreateValidUser(
                userId: 1,
                idNumber: "9001015009087",
                isActive: false));
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var dto = CreateValidUpdateUserDto(
                userId: 1,
                name: "Reactivated",
                surname: "User",
                address: "Reactivated Address",
                isActive: true);

            var result = await service.UpdateUser(dto);

            Assert.True(result.Success);

            var userInDb = await context.AppUsers.FindAsync(1);
            Assert.NotNull(userInDb);
            Assert.True(userInDb!.IsActive);
            Assert.Equal("Reactivated", userInDb.Name);
            Assert.Equal("User", userInDb.Surname);
            Assert.Equal("Reactivated Address", userInDb.Address);
        }

        [Fact]
        public async Task GetAllUsers_Should_Return_Paged_Results()
        {
            await using var context = CreateContext();
            context.AppUsers.AddRange(
                CreateValidUser(userId: 1, idNumber: "9001015009081", surname: "Zulu"),
                CreateValidUser(userId: 2, idNumber: "9001015009082", surname: "Beta"),
                CreateValidUser(userId: 3, idNumber: "9001015009083", surname: "Alpha"));
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.GetAllUsers(pageNumber: 2, pageSize: 2, search: null);

            Assert.Equal(2, result.CurrentPage);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(3, result.TotalItems);
            Assert.Single(result.Items);
            Assert.Equal(1, result.Items.First().UserID);
        }

        [Fact]
        public async Task GetAllUsers_Should_Filter_By_Search()
        {
            await using var context = CreateContext();
            context.AppUsers.AddRange(
                CreateValidUser(userId: 1, idNumber: "9001015009081", surname: "Smith"),
                CreateValidUser(userId: 2, idNumber: "9001015009082", surname: "Jones"),
                CreateValidUser(userId: 3, idNumber: "9001015009083", surname: "Brown"));
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.GetAllUsers(pageNumber: 1, pageSize: 10, search: "Smith");

            Assert.Equal(1, result.TotalItems);
            Assert.Single(result.Items);
            Assert.Equal("Smith", result.Items.First().Surname);
        }

        [Fact]
        public async Task DeleteUser_Should_Fail_When_User_Not_Found()
        {
            await using var context = CreateContext();
            var service = CreateService(context);

            var result = await service.DeleteUser(999);

            Assert.False(result.Success);
            Assert.Equal("User Not Found", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteUser_Should_Fail_When_User_Is_Still_Active()
        {
            await using var context = CreateContext();
            context.AppUsers.Add(CreateValidUser(
                userId: 1,
                idNumber: "9001015009087",
                isActive: true));
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.DeleteUser(1);

            Assert.False(result.Success);
            Assert.Equal("User need to be deactivated first before deleting. Edit user to deactivate.", result.ErrorMessage);
            Assert.Equal(1, await context.AppUsers.CountAsync());
        }

        [Fact]
        public async Task DeleteUser_Should_Remove_User_When_User_Is_Inactive()
        {
            await using var context = CreateContext();
            context.AppUsers.Add(CreateValidUser(
                userId: 1,
                idNumber: "9001015009087",
                isActive: false));
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.DeleteUser(1);

            Assert.True(result.Success);
            Assert.Empty(context.AppUsers);
        }

        [Fact]
        public async Task DoesIdExist_Should_Return_True_When_Id_Exists()
        {
            await using var context = CreateContext();
            context.AppUsers.Add(CreateValidUser(
                userId: 1,
                idNumber: "9001015009087"));
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.DoesIdExist("9001015009087");

            Assert.True(result);
        }

        [Fact]
        public async Task DoesIdExist_Should_Return_False_When_Id_Does_Not_Exist()
        {
            await using var context = CreateContext();
            var service = CreateService(context);

            var result = await service.DoesIdExist("1111111111111");

            Assert.False(result);
        }

        [Fact]
        public async Task GetUser_Should_Return_DisplayUserDto_When_User_Exists()
        {
            await using var context = CreateContext();
            context.AppUsers.Add(CreateValidUser(
                userId: 1,
                idNumber: "9001015009087",
                name: "Test",
                surname: "User",
                address: "Cape Town",
                isActive: true));
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.GetUser(1);

            Assert.NotNull(result);
            Assert.Equal(1, result!.UserID);
            Assert.Equal("9001015009087", result.IdNumber);
            Assert.Equal("Test", result.Name);
            Assert.Equal("User", result.Surname);
            Assert.Equal("Cape Town", result.Address);
            Assert.True(result.IsActive);
        }

        [Fact]
        public async Task GetUser_Should_Return_Null_When_User_Does_Not_Exist()
        {
            await using var context = CreateContext();
            var service = CreateService(context);

            var result = await service.GetUser(999);

            Assert.Null(result);
        }
    }
}