using FluentAssertions;
using Moq;
using teladoc.domain.Contracts.Repositories;
using teladoc.domain.Contracts.Services;
using teladoc.domain.DTOs;
using teladoc.domain.Entities;
using teladoc.domain.Enum;
using teladoc.domain.Exceptions;
using teladoc.domain.Services;
using teladoc.unit.test;
using Xunit;

public class UserServiceTests
{
    private static UserService CreateSut(
        Mock<IUserRepository> repo,
        Mock<IUnitOfWork> uow,
        TimeProvider timeProvider,
        Mock<IUserReadService> readService)
    {
        return new UserService(repo.Object, uow.Object, timeProvider, readService.Object);
    }

    [Fact]
    public async Task GetUserByIdAsync_DelegatesToReadService()
    {
        // Arrange
        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        var uow = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var read = new Mock<IUserReadService>(MockBehavior.Strict);
        var tp = new TestTimeProvider(new DateTimeOffset(2025, 12, 15, 0, 0, 0, TimeSpan.Zero));

        var expected = new UserResponse { Id = 10, FirstName = "A", LastName = "B", Email = "a@b.com", Age = 30 };

        read.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var sut = CreateSut(repo, uow, tp, read);

        // Act
        var result = await sut.GetUserByIdAsync(10);

        // Assert
        result.Should().BeEquivalentTo(expected);

        read.Verify(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyNoOtherCalls();
        uow.VerifyNoOtherCalls();
        read.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateUserAsync_WhenUnder18_ThrowsUserValidationException()
    {
        // Arrange
        var repo = new Mock<IUserRepository>();
        var uow = new Mock<IUnitOfWork>();
        var read = new Mock<IUserReadService>();
        var tp = new TestTimeProvider(new DateTimeOffset(2025, 12, 15, 0, 0, 0, TimeSpan.Zero));
        var sut = CreateSut(repo, uow, tp, read);

        var req = new CreateUserRequest
        {
            FirstName = "Kid",
            LastName = "User",
            Email = "kid@test.com",
            DateOfBirth = new DateTime(2010, 1, 1)
        };

        // Act
        Func<Task> act = () => sut.CreateUserAsync(req);

        // Assert
        var ex = await act.Should().ThrowAsync<UserValidationException>();
        ex.Which.Errors.Should().ContainKey("DateOfBirth");

        repo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateUserAsync_WhenEmailExists_ThrowsUserAlreadyExistsException()
    {
        // Arrange
        var repo = new Mock<IUserRepository>();
        var uow = new Mock<IUnitOfWork>();
        var read = new Mock<IUserReadService>();
        var tp = new TestTimeProvider(new DateTimeOffset(2025, 12, 15, 0, 0, 0, TimeSpan.Zero));
        var sut = CreateSut(repo, uow, tp, read);
     
        read.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserResponse { Id = 1, Email = "seba.zunini", FirstName = "S", LastName = "Z", DateOfBirth = new DateTime(2000, 1, 1) });
    }





        [Fact]
    public async Task CreateUserAsync_WhenValid_AddsUser_AndSaves_AndReturnsResponse()
    {
       
        var repo = new Mock<IUserRepository>();
        var uow = new Mock<IUnitOfWork>();
        var read = new Mock<IUserReadService>();
        var tp = new TestTimeProvider(new DateTimeOffset(2025, 12, 15, 0, 0, 0, TimeSpan.Zero));
        var sut = CreateSut(repo, uow, tp, read);

        User? added = null;

        repo.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) =>
            {
                u.Id = 123; // simula identity
                added = u;
            })
            .Returns(Task.CompletedTask);

        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var req = new CreateUserRequest
        {
            FirstName = "Sebas",
            LastName = "Z",
            Email = "sebas@test.com",
            DateOfBirth = new DateTime(2000, 12, 16), // al 2025-12-15 tiene 24
            FriendCount = 7
        };

        // Act
        var resp = await sut.CreateUserAsync(req);

        // Assert
        resp.Id.Should().Be(123);
        resp.Email.Should().Be("sebas@test.com");
        resp.Age.Should().Be(24);

        added.Should().NotBeNull();
        added!.EmailNormalized.Should().Be(req.Email.Trim().ToUpperInvariant());

        repo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReplaceUserAsync_WhenNotFound_ReturnsNotFound_AndDoesNotSave()
    {
        // Arrange
        var repo = new Mock<IUserRepository>();
        var uow = new Mock<IUnitOfWork>();
        var read = new Mock<IUserReadService>();
        var tp = new TestTimeProvider(new DateTimeOffset(2025, 12, 15, 0, 0, 0, TimeSpan.Zero));
        var sut = CreateSut(repo, uow, tp, read);

        repo.Setup(r => r.GetByIdForUpdateAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var req = new UpdateUserRequest
        {
            FirstName = "A",
            LastName = "B",
            Email = "a@b.com",
            DateOfBirth = new DateTime(1990, 1, 1),
            FriendCount = 1
        };

        // Act
        var result = await sut.ReplaceUserAsync(5, req);

        // Assert
        result.Type.Should().Be(PatchUserResultEnum.NotFound);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReplaceUserAsync_WhenUnder18_ReturnsValidationError_AndDoesNotSave()
    {
        // Arrange
        var repo = new Mock<IUserRepository>();
        var uow = new Mock<IUnitOfWork>();
        var read = new Mock<IUserReadService>();
        var tp = new TestTimeProvider(new DateTimeOffset(2025, 12, 15, 0, 0, 0, TimeSpan.Zero));
        var sut = CreateSut(repo, uow, tp, read);

        repo.Setup(r => r.GetByIdForUpdateAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = 5, FirstName = "X", LastName = "Y", DateOfBirth = new DateTime(1990, 1, 1) });

        var req = new UpdateUserRequest
        {
            FirstName = "A",
            LastName = "B",
            Email = "a@b.com",
            DateOfBirth = new DateTime(2010, 1, 1)
        };

        // Act
        var result = await sut.ReplaceUserAsync(5, req);

        // Assert
        result.Type.Should().Be(PatchUserResultEnum.ValidationError);
        result.Errors.Should().ContainKey("DateOfBirth");
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteUserAsync_WhenNotFound_ReturnsNotFound_AndDoesNotSave()
    {
        var repo = new Mock<IUserRepository>();
        var uow = new Mock<IUnitOfWork>();
        var read = new Mock<IUserReadService>();
        var tp = new TestTimeProvider(new DateTimeOffset(2025, 12, 15, 0, 0, 0, TimeSpan.Zero));
        var sut = CreateSut(repo, uow, tp, read);

        repo.Setup(r => r.GetByIdForUpdateAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await sut.DeleteUserAsync(99);

        result.Should().Be(DeleteUserResultEnum.NotFound);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
