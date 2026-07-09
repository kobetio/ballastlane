using FluentAssertions;
using MyLibrary.Domain.Entities;
using MyLibrary.Domain.Enums;
using MyLibrary.Domain.Exceptions;

namespace MyLibrary.Tests.Domain;

public class BookTests
{
    private static readonly Guid ValidUserId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithRequiredDataOnly_CreatesBook()
    {
        var book = new Book("Clean Code", "Robert C. Martin", ValidUserId);

        book.Id.Should().NotBe(Guid.Empty);
        book.Title.Should().Be("Clean Code");
        book.Author.Should().Be("Robert C. Martin");
        book.UserId.Should().Be(ValidUserId);
        book.Genre.Should().BeNull();
        book.PublicationYear.Should().BeNull();
        book.ReadingStatus.Should().BeNull();
        book.Rating.Should().BeNull();
        book.Notes.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithAllData_CreatesBook()
    {
        var book = new Book(
            title: "Clean Code",
            author: "Robert C. Martin",
            userId: ValidUserId,
            genre: "Software Engineering",
            publicationYear: 2008,
            readingStatus: ReadingStatus.Read,
            rating: 5,
            notes: "A must-read.");

        book.Genre.Should().Be("Software Engineering");
        book.PublicationYear.Should().Be(2008);
        book.ReadingStatus.Should().Be(ReadingStatus.Read);
        book.Rating.Should().Be(5);
        book.Notes.Should().Be("A must-read.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidTitle_ThrowsDomainException(string? invalidTitle)
    {
        var act = () => new Book(invalidTitle!, "Robert C. Martin", ValidUserId);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidAuthor_ThrowsDomainException(string? invalidAuthor)
    {
        var act = () => new Book("Clean Code", invalidAuthor!, ValidUserId);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ThrowsDomainException()
    {
        var act = () => new Book("Clean Code", "Robert C. Martin", Guid.Empty);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void BelongsTo_WithMatchingUserId_ReturnsTrue()
    {
        var book = new Book("Clean Code", "Robert C. Martin", ValidUserId);

        book.BelongsTo(ValidUserId).Should().BeTrue();
    }

    [Fact]
    public void BelongsTo_WithDifferentUserId_ReturnsFalse()
    {
        var book = new Book("Clean Code", "Robert C. Martin", ValidUserId);

        book.BelongsTo(Guid.NewGuid()).Should().BeFalse();
    }

    [Fact]
    public void Update_ReplacesAllMutableFields()
    {
        var book = new Book("Clean Code", "Robert C. Martin", ValidUserId);

        book.Update(
            title: "The Pragmatic Programmer",
            author: "Andrew Hunt",
            genre: "Software Engineering",
            publicationYear: 1999,
            readingStatus: ReadingStatus.Reading,
            rating: 4,
            notes: "Great tips.");

        book.Title.Should().Be("The Pragmatic Programmer");
        book.Author.Should().Be("Andrew Hunt");
        book.Genre.Should().Be("Software Engineering");
        book.PublicationYear.Should().Be(1999);
        book.ReadingStatus.Should().Be(ReadingStatus.Reading);
        book.Rating.Should().Be(4);
        book.Notes.Should().Be("Great tips.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Update_WithInvalidTitle_ThrowsDomainException(string? invalidTitle)
    {
        var book = new Book("Clean Code", "Robert C. Martin", ValidUserId);

        var act = () => book.Update(invalidTitle!, "Robert C. Martin", null, null, null, null, null);

        act.Should().Throw<DomainException>();
    }
}
