using FluentAssertions;
using Moq;
using MyLibrary.Application.Common.Exceptions;
using MyLibrary.Application.DTOs.Books;
using MyLibrary.Application.Interfaces;
using MyLibrary.Application.Services;
using MyLibrary.Domain.Entities;
using MyLibrary.Domain.Enums;

namespace MyLibrary.Tests.Application.Services;

public class BookServiceTests
{
    private readonly Mock<IBookRepository> _bookRepository = new();
    private readonly BookService _sut;

    public BookServiceTests()
    {
        _sut = new BookService(_bookRepository.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllBooksForUser()
    {
        var userId = Guid.NewGuid();
        var books = new List<Book>
        {
            new("Clean Code", "Robert C. Martin", userId),
            new("The Pragmatic Programmer", "Andrew Hunt", userId)
        };
        _bookRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        var result = await _sut.GetAllAsync(userId);

        result.Should().HaveCount(2);
        result.Select(b => b.Title).Should().Contain(["Clean Code", "The Pragmatic Programmer"]);
    }

    [Fact]
    public async Task GetByIdAsync_WhenBookExistsAndBelongsToUser_ReturnsBook()
    {
        var userId = Guid.NewGuid();
        var book = new Book("Clean Code", "Robert C. Martin", userId);
        _bookRepository.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var result = await _sut.GetByIdAsync(book.Id, userId);

        result.Id.Should().Be(book.Id);
        result.Title.Should().Be("Clean Code");
    }

    [Fact]
    public async Task GetByIdAsync_WhenBookDoesNotExist_ThrowsNotFoundException()
    {
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _bookRepository.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var act = () => _sut.GetByIdAsync(bookId, userId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_WhenBookBelongsToAnotherUser_ThrowsForbiddenAccessException()
    {
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var book = new Book("Clean Code", "Robert C. Martin", ownerId);
        _bookRepository.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var act = () => _sut.GetByIdAsync(book.Id, otherUserId);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task CreateAsync_AddsBookOwnedByCurrentUser()
    {
        var userId = Guid.NewGuid();
        var request = new BookCreateRequest("Clean Code", "Robert C. Martin", "Software", 2008, ReadingStatus.Read, 5, "Great");

        var result = await _sut.CreateAsync(userId, request);

        result.Title.Should().Be("Clean Code");
        _bookRepository.Verify(r => r.AddAsync(It.Is<Book>(b => b.UserId == userId && b.Title == "Clean Code"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenBookBelongsToUser_UpdatesAndReturnsBook()
    {
        var userId = Guid.NewGuid();
        var book = new Book("Clean Code", "Robert C. Martin", userId);
        _bookRepository.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        var request = new BookUpdateRequest("Clean Code (2nd Edition)", "Robert C. Martin", null, null, null, null, null);

        var result = await _sut.UpdateAsync(book.Id, userId, request);

        result.Title.Should().Be("Clean Code (2nd Edition)");
        _bookRepository.Verify(r => r.UpdateAsync(book, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenBookDoesNotExist_ThrowsNotFoundException()
    {
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _bookRepository.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);
        var request = new BookUpdateRequest("Title", "Author", null, null, null, null, null);

        var act = () => _sut.UpdateAsync(bookId, userId, request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_WhenBookBelongsToAnotherUser_ThrowsForbiddenAccessException()
    {
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var book = new Book("Clean Code", "Robert C. Martin", ownerId);
        _bookRepository.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        var request = new BookUpdateRequest("Title", "Author", null, null, null, null, null);

        var act = () => _sut.UpdateAsync(book.Id, otherUserId, request);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
        _bookRepository.Verify(r => r.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenBookBelongsToUser_DeletesBook()
    {
        var userId = Guid.NewGuid();
        var book = new Book("Clean Code", "Robert C. Martin", userId);
        _bookRepository.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        await _sut.DeleteAsync(book.Id, userId);

        _bookRepository.Verify(r => r.DeleteAsync(book, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenBookBelongsToAnotherUser_ThrowsForbiddenAccessExceptionAndDoesNotDelete()
    {
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var book = new Book("Clean Code", "Robert C. Martin", ownerId);
        _bookRepository.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var act = () => _sut.DeleteAsync(book.Id, otherUserId);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
        _bookRepository.Verify(r => r.DeleteAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenBookDoesNotExist_ThrowsNotFoundException()
    {
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _bookRepository.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var act = () => _sut.DeleteAsync(bookId, userId);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
