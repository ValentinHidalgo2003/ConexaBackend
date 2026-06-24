using Conexa.Application.Common;
using Conexa.Application.DTOs.Movies;
using Conexa.Application.Interfaces;
using Conexa.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conexa.Api.Controllers;

/// <summary>
/// Movie management endpoints backed by local storage and SWAPI synchronization.
/// </summary>
[ApiController]
[Route("api/movies")]
[Authorize]
public class MoviesController(IMovieService movieService, ISwapiSyncService swapiSyncService) : ControllerBase
{
    /// <summary>
    /// Returns a paginated list of movies.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<MovieListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<MovieListItemDto>>> GetMovies(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await movieService.GetMoviesAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns detailed information for a specific movie. Requires User or Admin role.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{Roles.User},{Roles.Admin}")]
    [ProducesResponseType(typeof(MovieDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieDetailDto>> GetMovieById(Guid id, CancellationToken cancellationToken)
    {
        var movie = await movieService.GetMovieByIdAsync(id, cancellationToken);
        return Ok(movie);
    }

    /// <summary>
    /// Creates a new movie manually. Admin only.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(MovieDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MovieDetailDto>> CreateMovie(
        [FromBody] CreateMovieRequest request,
        CancellationToken cancellationToken)
    {
        var movie = await movieService.CreateMovieAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetMovieById), new { id = movie.Id }, movie);
    }

    /// <summary>
    /// Updates an existing movie. Admin only.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(MovieDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MovieDetailDto>> UpdateMovie(
        Guid id,
        [FromBody] UpdateMovieRequest request,
        CancellationToken cancellationToken)
    {
        var movie = await movieService.UpdateMovieAsync(id, request, cancellationToken);
        return Ok(movie);
    }

    /// <summary>
    /// Deletes a movie. Admin only.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteMovie(Guid id, CancellationToken cancellationToken)
    {
        await movieService.DeleteMovieAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Synchronizes movies from the Star Wars public API (SWAPI). Admin only.
    /// </summary>
    [HttpPost("sync")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(SyncMoviesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SyncMoviesResponse>> SyncMovies(CancellationToken cancellationToken)
    {
        var result = await swapiSyncService.SyncFilmsAsync(cancellationToken);
        return Ok(result);
    }
}
