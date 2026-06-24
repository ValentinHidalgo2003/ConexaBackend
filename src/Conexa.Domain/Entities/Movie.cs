using Conexa.Domain.Enums;

namespace Conexa.Domain.Entities;

public class Movie
{
    public Guid Id { get; set; }
    public string? SwapiUid { get; set; }
    public string Title { get; set; } = string.Empty;
    public int EpisodeId { get; set; }
    public string Director { get; set; } = string.Empty;
    public string Producer { get; set; } = string.Empty;
    public DateOnly ReleaseDate { get; set; }
    public string OpeningCrawl { get; set; } = string.Empty;
    public MovieSource Source { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
