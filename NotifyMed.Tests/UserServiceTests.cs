using NotifyMed.Application.Services;
using NotifyMed.Domain.Entities;
using NotifyMed.Domain.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NotifyMed.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockContactRepository;
        private readonly IUserService _userService;


        public UserServiceTests()
        {
            _mockContactRepository = new Mock<IUserRepository>();
            _userService = new UserService(_mockContactRepository.Object);
        }

        [Fact]
        public async Task AddUser_ShouldThrowValidationException_WhenEmailIsEmpty()
        {
            // Arrange
            var region = new User { Username = "Teste", Email = "", Cpf = "00000", Role = "user", PasswordHash = "5454", Crm = "" };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _userService.RegisterAsync(region.Username, region.Cpf, region.Crm, region.Email, region.PasswordHash, region.Role));
        }
    }
}

