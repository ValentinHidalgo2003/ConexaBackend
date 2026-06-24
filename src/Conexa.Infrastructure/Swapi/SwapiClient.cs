using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Conexa.Application.Interfaces;
using Conexa.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Conexa.Infrastructure.Swapi;

public class SwapiClient(
    HttpClient httpClient,
    IOptions<SwapiSettings> swapiOptions,
    ILogger<SwapiClient> logger) : ISwapiClient
{
    private readonly SwapiSettings _settings = swapiOptions.Value;

    public async Task<IReadOnlyList<SwapiFilmData>> GetFilmsAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<SwapiFilmsResponse>(
            $"{_settings.BaseUrl.TrimEnd('/')}/films",
            cancellationToken);

        if (response?.Result is null || response.Result.Count == 0)
        {
            logger.LogWarning("SWAPI returned no films.");
            return [];
        }

        return response.Result
            .Where(item => item.Properties is not null && !string.IsNullOrWhiteSpace(item.Uid))
            .Select(item => new SwapiFilmData(
                item.Uid!,
                item.Properties!.Title ?? string.Empty,
                item.Properties.EpisodeId,
                item.Properties.Director ?? string.Empty,
                item.Properties.Producer ?? string.Empty,
                DateOnly.TryParse(item.Properties.ReleaseDate, out var releaseDate)
                    ? releaseDate
                    : DateOnly.FromDateTime(DateTime.UtcNow),
                item.Properties.OpeningCrawl ?? string.Empty))
            .ToList();
    }
}

internal sealed class SwapiFilmsResponse
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("result")]
    public List<SwapiFilmResult>? Result { get; set; }
}

internal sealed class SwapiFilmResult
{
    [JsonPropertyName("uid")]
    public string? Uid { get; set; }

    [JsonPropertyName("properties")]
    public SwapiFilmProperties? Properties { get; set; }
}

internal sealed class SwapiFilmProperties
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("episode_id")]
    public int EpisodeId { get; set; }

    [JsonPropertyName("director")]
    public string? Director { get; set; }

    [JsonPropertyName("producer")]
    public string? Producer { get; set; }

    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; set; }

    [JsonPropertyName("opening_crawl")]
    public string? OpeningCrawl { get; set; }
}
