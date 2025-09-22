using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using PropertyManagement.Application.DTOs.Auth;
using PropertyManagement.Application.Interfaces;
using PropertyManagement.Application.Mappings;
using PropertyManagement.Application.Services;
using PropertyManagement.Domain.Entities;
using PropertyManagement.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Tests.Unit;

[TestFixture]
public class AuthServiceTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
    private Mock<IJwtTokenService> _mockJwtTokenService;
    private Mock<ILogger<AuthService>> _mockLogger;
    private IMapper _mapper;
    private AuthService _authService;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _mockLogger = new Mock<ILogger<AuthService>>();

        _mockUnitOfWork.Setup(x => x.Users).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(x => x.RefreshTokens).Returns(_mockRefreshTokenRepository.Object);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _authService = new AuthService(
            _mockUnitOfWork.Object,
            _mockJwtTokenService.Object,
            _mapper,
            _mockLogger.Object);
    }

    [Test]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccessResponse()
    {
        // Arrange
        var loginDto = new LoginDto { Username = "testuser", Password = "Test123!" };
        var user = CreateSampleUser();
        var accessToken = "access-token";
        var refreshToken = "refresh-token";

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(loginDto.Username)).ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(user);
        _mockJwtTokenService.Setup(x => x.GenerateAccessToken(user)).Returns(accessToken);
        _mockJwtTokenService.Setup(x => x.GenerateRefreshToken()).Returns(refreshToken);
        _mockRefreshTokenRepository.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>())).ReturnsAsync(new RefreshToken());
        _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Username, Is.EqualTo(user.Username));
        Assert.That(result.Data.AccessToken, Is.EqualTo(accessToken));
        Assert.That(result.Data.RefreshToken, Is.EqualTo(refreshToken));
        Assert.That(result.Message, Is.EqualTo("Login successful"));

        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        _mockRefreshTokenRepository.Verify(x => x.CreateAsync(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Test]
    public async Task LoginAsync_InvalidPassword_ReturnsErrorResponse()
    {
        // Arrange
        var loginDto = new LoginDto { Username = "testuser", Password = "WrongPassword!" };
        var user = CreateSampleUser();

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(loginDto.Username)).ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Invalid username or password"));
        Assert.That(result.Data, Is.Null);

        _mockJwtTokenService.Verify(x => x.GenerateAccessToken(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public async Task RegisterAsync_ValidData_ReturnsSuccessResponse()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Username = "newuser",
            Email = "newuser@test.com",
            Password = "NewUser123!",
            ConfirmPassword = "NewUser123!",
            FirstName = "New",
            LastName = "User"
        };

        var accessToken = "access-token";
        var refreshToken = "refresh-token";

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(registerDto.Username)).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.GetByEmailAsync(registerDto.Email)).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => { user.IdUser = 1; return user; });
        _mockJwtTokenService.Setup(x => x.GenerateAccessToken(It.IsAny<User>())).Returns(accessToken);
        _mockJwtTokenService.Setup(x => x.GenerateRefreshToken()).Returns(refreshToken);
        _mockRefreshTokenRepository.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>())).ReturnsAsync(new RefreshToken());
        _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Username, Is.EqualTo(registerDto.Username));
        Assert.That(result.Message, Is.EqualTo("Registration successful"));

        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    [Test]
    public async Task RefreshTokenAsync_ValidToken_ReturnsSuccessResponse()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto { RefreshToken = "valid-refresh-token" };
        var user = CreateSampleUser();
        var refreshToken = new RefreshToken
        {
            Token = refreshTokenDto.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false,
            IdUser = user.IdUser
        };
        var newAccessToken = "new-access-token";
        var newRefreshToken = "new-refresh-token";

        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(refreshTokenDto.RefreshToken)).ReturnsAsync(refreshToken);
        _mockUserRepository.Setup(x => x.GetByIdAsync(refreshToken.IdUser)).ReturnsAsync(user);
        _mockRefreshTokenRepository.Setup(x => x.UpdateAsync(It.IsAny<RefreshToken>())).ReturnsAsync(refreshToken);
        _mockJwtTokenService.Setup(x => x.GenerateAccessToken(user)).Returns(newAccessToken);
        _mockJwtTokenService.Setup(x => x.GenerateRefreshToken()).Returns(newRefreshToken);
        _mockRefreshTokenRepository.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>())).ReturnsAsync(new RefreshToken());
        _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

        // Act
        var result = await _authService.RefreshTokenAsync(refreshTokenDto);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.AccessToken, Is.EqualTo(newAccessToken));
        Assert.That(result.Data.RefreshToken, Is.EqualTo(newRefreshToken));
        Assert.That(result.Message, Is.EqualTo("Token refreshed successfully"));
    }

    private User CreateSampleUser()
    {
        return new User
        {
            IdUser = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            FirstName = "Test",
            LastName = "User",
            Role = "User",
            IsActive = true,
            RefreshTokens = new List<RefreshToken>()
        };
    }
}