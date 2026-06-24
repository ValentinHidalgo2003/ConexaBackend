using Conexa.Application.DTOs.Auth;
using Conexa.Application.DTOs.Movies;
using Conexa.Application.Exceptions;
using Conexa.Application.Interfaces;
using Conexa.Domain.Constants;
using Conexa.Domain.Entities;
using Conexa.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Conexa.Application.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ITokenService tokenService) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            throw new ConflictException("A user with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new ValidationAppException(errors);
        }

        await userManager.AddToRoleAsync(user, Roles.User);
        var roles = await userManager.GetRolesAsync(user);
        return tokenService.CreateToken(user, roles);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        var roles = await userManager.GetRolesAsync(user);
        return tokenService.CreateToken(user, roles);
    }
}

public class SwapiSyncService(
    ISwapiClient swapiClient,
    IMovieRepository movieRepository) : ISwapiSyncService
{
    public async Task<SyncMoviesResponse> SyncFilmsAsync(CancellationToken cancellationToken = default)
    {
        var films = await swapiClient.GetFilmsAsync(cancellationToken);
        var created = 0;
        var updated = 0;

        foreach (var film in films)
        {
            var existing = await movieRepository.GetBySwapiUidAsync(film.Uid, cancellationToken);
            if (existing is null)
            {
                var now = DateTime.UtcNow;
                await movieRepository.AddAsync(new Movie
                {
                    Id = Guid.NewGuid(),
                    SwapiUid = film.Uid,
                    Title = film.Title,
                    EpisodeId = film.EpisodeId,
                    Director = film.Director,
                    Producer = film.Producer,
                    ReleaseDate = film.ReleaseDate,
                    OpeningCrawl = film.OpeningCrawl,
                    Source = MovieSource.Swapi,
                    CreatedAt = now,
                    UpdatedAt = now
                }, cancellationToken);
                created++;
            }
            else
            {
                existing.Title = film.Title;
                existing.EpisodeId = film.EpisodeId;
                existing.Director = film.Director;
                existing.Producer = film.Producer;
                existing.ReleaseDate = film.ReleaseDate;
                existing.OpeningCrawl = film.OpeningCrawl;
                existing.Source = MovieSource.Swapi;
                existing.UpdatedAt = DateTime.UtcNow;
                await movieRepository.UpdateAsync(existing, cancellationToken);
                updated++;
            }
        }

        return new SyncMoviesResponse(created, updated, films.Count);
    }
}
