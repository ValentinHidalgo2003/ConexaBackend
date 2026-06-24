using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Conexa.Application.DTOs.Auth;
using Conexa.Application.DTOs.Movies;

namespace Conexa.Tests;

public class MoviesControllerAuthorizationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task GetMovies_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/movies");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateMovie_AsRegularUser_ReturnsForbidden()
    {
        var email = $"regular-{Guid.NewGuid():N}@test.com";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password1", "Regular User"));
        var userLogin = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Password1"));
        var userAuth = await userLogin.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userAuth!.Token);

        var response = await _client.PostAsJsonAsync("/api/movies", new CreateMovieRequest(
            "Test Movie",
            99,
            "Director",
            "Producer",
            new DateOnly(2020, 1, 1),
            "Opening crawl"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateMovie_AsAdmin_ReturnsCreated()
    {
        var token = await LoginAsAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/movies", new CreateMovieRequest(
            "Admin Movie",
            100,
            "Admin Director",
            "Admin Producer",
            new DateOnly(2021, 5, 4),
            "A long time ago..."));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetMovieById_AsRegularUser_ReturnsOk()
    {
        var adminToken = await LoginAsAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var createResponse = await _client.PostAsJsonAsync("/api/movies", new CreateMovieRequest(
            "Detail Movie",
            101,
            "Director",
            "Producer",
            new DateOnly(2019, 12, 20),
            "Opening crawl text"));

        var created = await createResponse.Content.ReadFromJsonAsync<MovieDetailDto>(JsonOptions);

        var email = $"detail-user-{Guid.NewGuid():N}@test.com";
        _client.DefaultRequestHeaders.Authorization = null;
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password1", "Detail User"));
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Password1"));
        var userAuth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userAuth!.Token);
        var response = await _client.GetAsync($"/api/movies/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<string> LoginAsAdminAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin@conexa.com", "Admin123!"));
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        return auth!.Token;
    }
}
