using Conexa.Application.DTOs.Auth;
using Conexa.Application.Exceptions;
using Conexa.Application.Interfaces;
using Conexa.Application.Services;
using Conexa.Domain.Constants;
using Conexa.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Conexa.Tests;

public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object,
            Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            null!, null!, null!, null!);

        _tokenServiceMock = new Mock<ITokenService>();
        _authService = new AuthService(_userManagerMock.Object, _signInManagerMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithNewEmail_ReturnsToken()
    {
        var request = new RegisterRequest("new@test.com", "Password1", "New User");
        var user = new ApplicationUser { Email = request.Email, FullName = request.FullName };
        var expected = new AuthResponse("token", request.Email, request.FullName, [Roles.User], DateTime.UtcNow.AddHours(1));

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), Roles.User))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync([Roles.User]);
        _tokenServiceMock.Setup(x => x.CreateToken(It.IsAny<ApplicationUser>(), It.IsAny<IList<string>>()))
            .Returns(expected);

        var result = await _authService.RegisterAsync(request);

        Assert.Equal(expected.Token, result.Token);
        Assert.Equal(request.Email, result.Email);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ThrowsConflict()
    {
        var request = new RegisterRequest("exists@test.com", "Password1", "Existing User");
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(new ApplicationUser { Email = request.Email });

        await Assert.ThrowsAsync<ConflictException>(() => _authService.RegisterAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ThrowsUnauthorized()
    {
        var request = new LoginRequest("missing@test.com", "Password1");
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<UnauthorizedAppException>(() => _authService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        var request = new LoginRequest("user@test.com", "Password1");
        var user = new ApplicationUser { Email = request.Email, FullName = "User" };
        var expected = new AuthResponse("token", request.Email, user.FullName, [Roles.User], DateTime.UtcNow.AddHours(1));

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync([Roles.User]);
        _tokenServiceMock.Setup(x => x.CreateToken(user, It.IsAny<IList<string>>())).Returns(expected);

        var result = await _authService.LoginAsync(request);

        Assert.Equal("token", result.Token);
    }
}
