using Conexa.Application.Common;
using Conexa.Application.DTOs.Auth;
using Conexa.Application.DTOs.Movies;
using Conexa.Domain.Entities;

namespace Conexa.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}

public interface ITokenService
{
    AuthResponse CreateToken(ApplicationUser user, IList<string> roles);
}

public interface IMovieRepository
{
    Task<PagedResult<Movie>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Movie?> GetBySwapiUidAsync(string swapiUid, CancellationToken cancellationToken = default);
    Task AddAsync(Movie movie, CancellationToken cancellationToken = default);
    Task UpdateAsync(Movie movie, CancellationToken cancellationToken = default);
    Task DeleteAsync(Movie movie, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}

public interface IMovieService
{
    Task<PagedResult<MovieListItemDto>> GetMoviesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<MovieDetailDto> GetMovieByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MovieDetailDto> CreateMovieAsync(CreateMovieRequest request, CancellationToken cancellationToken = default);
    Task<MovieDetailDto> UpdateMovieAsync(Guid id, UpdateMovieRequest request, CancellationToken cancellationToken = default);
    Task DeleteMovieAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface ISwapiClient
{
    Task<IReadOnlyList<SwapiFilmData>> GetFilmsAsync(CancellationToken cancellationToken = default);
}

public record SwapiFilmData(
    string Uid,
    string Title,
    int EpisodeId,
    string Director,
    string Producer,
    DateOnly ReleaseDate,
    string OpeningCrawl);

public interface ISwapiSyncService
{
    Task<SyncMoviesResponse> SyncFilmsAsync(CancellationToken cancellationToken = default);
}
