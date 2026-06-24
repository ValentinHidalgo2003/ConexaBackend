using Conexa.Application.Interfaces;
using Conexa.Application.Services;
using Conexa.Domain.Entities;
using Conexa.Domain.Enums;
using Moq;

namespace Conexa.Tests;

public class SwapiSyncServiceTests
{
    [Fact]
    public async Task SyncFilmsAsync_CreatesAndUpdatesMovies()
    {
        var swapiClientMock = new Mock<ISwapiClient>();
        var repositoryMock = new Mock<IMovieRepository>();

        swapiClientMock.Setup(c => c.GetFilmsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new SwapiFilmData("1", "A New Hope", 4, "George Lucas", "Gary Kurtz", new DateOnly(1977, 5, 25), "Crawl"),
                new SwapiFilmData("2", "Empire Strikes Back", 5, "Irvin Kershner", "Gary Kurtz", new DateOnly(1980, 5, 21), "Crawl 2")
            ]);

        repositoryMock.Setup(r => r.GetBySwapiUidAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Movie { Id = Guid.NewGuid(), SwapiUid = "1", Title = "Old Title", Source = MovieSource.Swapi });
        repositoryMock.Setup(r => r.GetBySwapiUidAsync("2", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Movie?)null);

        var service = new SwapiSyncService(swapiClientMock.Object, repositoryMock.Object);
        var result = await service.SyncFilmsAsync();

        Assert.Equal(1, result.Created);
        Assert.Equal(1, result.Updated);
        Assert.Equal(2, result.Total);
        repositoryMock.Verify(r => r.AddAsync(It.IsAny<Movie>(), It.IsAny<CancellationToken>()), Times.Once);
        repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Movie>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
