using Conexa.Application.Common;
using Conexa.Application.Interfaces;
using Conexa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Conexa.Infrastructure.Persistence;

public class MovieRepository(AppDbContext dbContext) : IMovieRepository
{
    public async Task<PagedResult<Movie>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Movies.AsNoTracking().OrderBy(m => m.EpisodeId);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Movie>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Movies.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task<Movie?> GetBySwapiUidAsync(string swapiUid, CancellationToken cancellationToken = default) =>
        dbContext.Movies.FirstOrDefaultAsync(m => m.SwapiUid == swapiUid, cancellationToken);

    public async Task AddAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        dbContext.Movies.Add(movie);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        dbContext.Movies.Update(movie);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        dbContext.Movies.Remove(movie);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        dbContext.Movies.CountAsync(cancellationToken);
}
