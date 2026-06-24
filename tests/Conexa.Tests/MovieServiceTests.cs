using Conexa.Application.Common;
using Conexa.Application.DTOs.Movies;
using Conexa.Application.Exceptions;
using Conexa.Application.Interfaces;
using Conexa.Application.Services;
using Conexa.Domain.Entities;
using Conexa.Domain.Enums;
using Moq;

namespace Conexa.Tests;

public class MovieServiceTests
{
    private readonly Mock<IMovieRepository> _repositoryMock = new();
    private readonly MovieService _movieService;

    public MovieServiceTests()
    {
        _movieService = new MovieService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetMovieByIdAsync_WhenNotFound_ThrowsNotFound()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Movie?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _movieService.GetMovieByIdAsync(id));
    }

    [Fact]
    public async Task CreateMovieAsync_PersistsManualMovie()
    {
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Movie>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new CreateMovieRequest(
            "Rogue One",
            3,
            "Gareth Edwards",
            "Kathleen Kennedy",
            new DateOnly(2016, 12, 16),
            "It is a time of conflict...");

        var result = await _movieService.CreateMovieAsync(request);

        Assert.Equal(request.Title, result.Title);
        Assert.Equal(MovieSource.Manual, result.Source);
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Movie>(m => m.Source == MovieSource.Manual), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMoviesAsync_NormalizesPagination()
    {
        _repositoryMock.Setup(r => r.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Movie>
            {
                Items = [],
                Page = 1,
                PageSize = 10,
                TotalCount = 0
            });

        await _movieService.GetMoviesAsync(0, 500);

        _repositoryMock.Verify(r => r.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteMovieAsync_WhenNotFound_ThrowsNotFound()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Movie?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _movieService.DeleteMovieAsync(id));
    }
}
