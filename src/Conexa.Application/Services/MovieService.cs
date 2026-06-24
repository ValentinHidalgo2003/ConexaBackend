using Conexa.Application.Common;
using Conexa.Application.DTOs.Movies;
using Conexa.Application.Exceptions;
using Conexa.Application.Interfaces;
using Conexa.Domain.Entities;
using Conexa.Domain.Enums;

namespace Conexa.Application.Services;

public class MovieService(IMovieRepository movieRepository) : IMovieService
{
    public async Task<PagedResult<MovieListItemDto>> GetMoviesAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

        var result = await movieRepository.GetPagedAsync(normalizedPage, normalizedPageSize, cancellationToken);

        return new PagedResult<MovieListItemDto>
        {
            Items = result.Items.Select(MapToListItem).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<MovieDetailDto> GetMovieByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var movie = await movieRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Movie with id '{id}' was not found.");

        return MapToDetail(movie);
    }

    public async Task<MovieDetailDto> CreateMovieAsync(CreateMovieRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            EpisodeId = request.EpisodeId,
            Director = request.Director,
            Producer = request.Producer,
            ReleaseDate = request.ReleaseDate,
            OpeningCrawl = request.OpeningCrawl,
            Source = MovieSource.Manual,
            CreatedAt = now,
            UpdatedAt = now
        };

        await movieRepository.AddAsync(movie, cancellationToken);
        return MapToDetail(movie);
    }

    public async Task<MovieDetailDto> UpdateMovieAsync(Guid id, UpdateMovieRequest request, CancellationToken cancellationToken = default)
    {
        var movie = await movieRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Movie with id '{id}' was not found.");

        movie.Title = request.Title;
        movie.EpisodeId = request.EpisodeId;
        movie.Director = request.Director;
        movie.Producer = request.Producer;
        movie.ReleaseDate = request.ReleaseDate;
        movie.OpeningCrawl = request.OpeningCrawl;
        movie.UpdatedAt = DateTime.UtcNow;

        await movieRepository.UpdateAsync(movie, cancellationToken);
        return MapToDetail(movie);
    }

    public async Task DeleteMovieAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var movie = await movieRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Movie with id '{id}' was not found.");

        await movieRepository.DeleteAsync(movie, cancellationToken);
    }

    private static MovieListItemDto MapToListItem(Movie movie) =>
        new(movie.Id, movie.Title, movie.EpisodeId, movie.Director, movie.ReleaseDate, movie.Source);

    private static MovieDetailDto MapToDetail(Movie movie) =>
        new(
            movie.Id,
            movie.SwapiUid,
            movie.Title,
            movie.EpisodeId,
            movie.Director,
            movie.Producer,
            movie.ReleaseDate,
            movie.OpeningCrawl,
            movie.Source,
            movie.CreatedAt,
            movie.UpdatedAt);
}
