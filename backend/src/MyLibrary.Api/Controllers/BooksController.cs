using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyLibrary.Application.Common.Exceptions;
using MyLibrary.Application.DTOs.Books;
using MyLibrary.Application.Interfaces;

namespace MyLibrary.Api.Controllers;

/// <summary>
/// CRUD endpoints for the current authenticated user's personal book collection.
/// Every endpoint only ever operates on the caller's own books; attempting to read,
/// update or delete another user's book returns 403 Forbidden (Specification.md §4).
/// </summary>
[ApiController]
[Authorize]
[Route("api/books")]
[Produces("application/json")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;
    private readonly ICurrentUserService _currentUserService;

    public BooksController(IBookService bookService, ICurrentUserService currentUserService)
    {
        _bookService = bookService;
        _currentUserService = currentUserService;
    }

    private Guid CurrentUserId => _currentUserService.UserId
        ?? throw new AuthenticationException("The request is not authenticated.");

    /// <summary>Lists every book in the current user's library.</summary>
    /// <response code="200">The list of books (empty if the user has none yet).</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BookResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<BookResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var books = await _bookService.GetAllAsync(CurrentUserId, cancellationToken);
        return Ok(books);
    }

    /// <summary>Gets a single book owned by the current user.</summary>
    /// <param name="id">The book's id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The requested book.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">The book exists but belongs to another user.</response>
    /// <response code="404">No book with that id exists.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var book = await _bookService.GetByIdAsync(id, CurrentUserId, cancellationToken);
        return Ok(book);
    }

    /// <summary>Adds a new book to the current user's library.</summary>
    /// <param name="request">The book's details. Only Title and Author are required.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">The book was created.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    [HttpPost]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BookResponse>> Create(BookCreateRequest request, CancellationToken cancellationToken)
    {
        var book = await _bookService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }

    /// <summary>Updates a book owned by the current user.</summary>
    /// <param name="id">The book's id.</param>
    /// <param name="request">The book's new details. Only Title and Author are required.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The updated book.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">The book exists but belongs to another user.</response>
    /// <response code="404">No book with that id exists.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookResponse>> Update(Guid id, BookUpdateRequest request, CancellationToken cancellationToken)
    {
        var book = await _bookService.UpdateAsync(id, CurrentUserId, request, cancellationToken);
        return Ok(book);
    }

    /// <summary>Deletes a book owned by the current user.</summary>
    /// <param name="id">The book's id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">The book was deleted.</response>
    /// <response code="401">Missing or invalid authentication token.</response>
    /// <response code="403">The book exists but belongs to another user.</response>
    /// <response code="404">No book with that id exists.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _bookService.DeleteAsync(id, CurrentUserId, cancellationToken);
        return NoContent();
    }
}
