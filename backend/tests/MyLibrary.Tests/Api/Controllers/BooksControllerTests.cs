using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MyLibrary.Api.Controllers;
using MyLibrary.Application.Common.Exceptions;
using MyLibrary.Application.DTOs.Books;
using MyLibrary.Application.Interfaces;
using MyLibrary.Domain.Enums;

namespace MyLibrary.Tests.Api.Controllers;

/// <summary>
/// Verifies <see cref="BooksController"/> delegates to <see cref="IBookService"/> using the
/// id resolved by <see cref="ICurrentUserService"/>, and maps results to the expected HTTP
/// status codes. Ownership/validation-to-status-code mapping is covered end-to-end in
/// <see cref="BooksEndpointsTests"/>.
/// </summary>
public class BooksControllerTests
{
    private readonly Mock<IBookService> _bookService = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly BooksController _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public BooksControllerTests()
    {
        _currentUserService.Setup(s => s.UserId).Returns(_userId);
        _sut = new BooksController(_bookService.Object, _currentUserService.Object);
    }

    private static BookResponse SampleResponse(Guid? id = null) => new(
        id ?? Guid.NewGuid(), "Dune", "Frank Herbert", "Sci-Fi", 1965, ReadingStatus.Read, 5, null);

    [Fact]
    public async Task GetAll_ReturnsOkWithServiceResultForCurrentUser()
    {
        var books = new List<BookResponse> { SampleResponse() };
        _bookService.Setup(s => s.GetAllAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(books);

        var result = await _sut.GetAll(CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(books);
    }

    [Fact]
    public async Task GetById_ReturnsOkWithBook()
    {
        var book = SampleResponse();
        _bookService.Setup(s => s.GetByIdAsync(book.Id, _userId, It.IsAny<CancellationToken>())).ReturnsAsync(book);

        var result = await _sut.GetById(book.Id, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(book);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtActionWithBook()
    {
        var request = new BookCreateRequest("Dune", "Frank Herbert", "Sci-Fi", 1965, ReadingStatus.Read, 5, null);
        var response = SampleResponse();
        _bookService.Setup(s => s.CreateAsync(_userId, request, It.IsAny<CancellationToken>())).ReturnsAsync(response);

        var result = await _sut.Create(request, CancellationToken.None);

        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(BooksController.GetById));
        createdResult.RouteValues!["id"].Should().Be(response.Id);
        createdResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task Update_ReturnsOkWithUpdatedBook()
    {
        var bookId = Guid.NewGuid();
        var request = new BookUpdateRequest("Dune Messiah", "Frank Herbert", "Sci-Fi", 1969, ReadingStatus.Read, 4, null);
        var response = SampleResponse(bookId);
        _bookService.Setup(s => s.UpdateAsync(bookId, _userId, request, It.IsAny<CancellationToken>())).ReturnsAsync(response);

        var result = await _sut.Update(bookId, request, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var bookId = Guid.NewGuid();
        _bookService.Setup(s => s.DeleteAsync(bookId, _userId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _sut.Delete(bookId, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _bookService.Verify(s => s.DeleteAsync(bookId, _userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAll_WhenNoAuthenticatedUser_ThrowsAuthenticationException()
    {
        _currentUserService.Setup(s => s.UserId).Returns((Guid?)null);

        var act = () => _sut.GetAll(CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationException>();
    }
}
