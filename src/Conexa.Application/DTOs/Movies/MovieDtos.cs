using Conexa.Domain.Enums;

namespace Conexa.Application.DTOs.Movies;

public record MovieListItemDto(
    Guid Id,
    string Title,
    int EpisodeId,
    string Director,
    DateOnly ReleaseDate,
    MovieSource Source);

public record MovieDetailDto(
    Guid Id,
    string? SwapiUid,
    string Title,
    int EpisodeId,
    string Director,
    string Producer,
    DateOnly ReleaseDate,
    string OpeningCrawl,
    MovieSource Source,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CreateMovieRequest(
    string Title,
    int EpisodeId,
    string Director,
    string Producer,
    DateOnly ReleaseDate,
    string OpeningCrawl);

public record UpdateMovieRequest(
    string Title,
    int EpisodeId,
    string Director,
    string Producer,
    DateOnly ReleaseDate,
    string OpeningCrawl);

public record SyncMoviesResponse(int Created, int Updated, int Total);
