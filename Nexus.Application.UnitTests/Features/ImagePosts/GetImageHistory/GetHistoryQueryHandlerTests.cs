using AutoFixture;
using JasperFx.Events;
using Marten;
using Marten.Events;
using Microsoft.Extensions.Logging;
using Moq;
using Nexus.Application.Common.Models;
using Nexus.Application.Common.Pagination;
using Nexus.Application.Features.ImagePosts.GetImageHistory;
using Nexus.Domain.Common;
using Nexus.Domain.Errors;
using Nexus.Domain.Events;

namespace Nexus.Application.UnitTests.Features.ImagePosts.GetImageHistory;

public class GetHistoryQueryHandlerTests
{
    private readonly Mock<IQuerySession> _mockQuerySession = new();
    private readonly Mock<IQueryEventStore> _mockQueryEventStore = new();
    private readonly Mock<ILogger<GetHistoryQueryHandler>> _mockLogger = new();
    private readonly Fixture _fixture = new();
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public GetHistoryQueryHandlerTests()
    {
        _mockQuerySession.Setup(x => x.Events).Returns(_mockQueryEventStore.Object);
    }

    private void SetupEventStore(Guid imagePostId, DateTimeOffset? dateTo, List<IEvent> events)
    {
        _mockQueryEventStore
            .Setup(x => x.FetchStreamAsync(
                imagePostId,
                It.IsAny<long>(),
                dateTo,
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);
    }

    private async Task<Result<PagedResult<HistoryDto>>> HandleQueryAsync(GetHistoryQuery query)
    {
        return await GetHistoryQueryHandler.HandleAsync(
            query,
            _mockQuerySession.Object,
            _mockLogger.Object,
            _cancellationToken);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnPagedHistory_WhenEventsExist()
    {
        // Arrange
        var imagePostId = _fixture.Create<Guid>();
        var query = new GetHistoryQuery(imagePostId);
        var events = CreateTestEvents(5, imagePostId);

        SetupEventStore(imagePostId, query.DateTo, events);

        // Act
        var result = await HandleQueryAsync(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(5, result.Value.TotalCount);
        Assert.Equal(5, result.Value.Items.Count);
        Assert.Equal(1, result.Value.PageNumber);
        Assert.Equal(1, result.Value.TotalPages);
        
        // Verify events are ordered by timestamp descending
        var timestamps = result.Value.Items.Select(h => h.Timestamp).ToList();
        Assert.True(timestamps.SequenceEqual(timestamps.OrderByDescending(t => t)));
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNotFoundError_WhenNoEventsExist()
    {
        // Arrange
        var imagePostId = _fixture.Create<Guid>();
        var query = new GetHistoryQuery(imagePostId);

        SetupEventStore(imagePostId, query.DateTo, []);

        // Act
        var result = await HandleQueryAsync(query);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Equal(ImagePostErrors.NotFound, result.Errors[0]);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNotFoundError_WhenOnlyNonNexusEventsExist()
    {
        // Arrange
        var imagePostId = _fixture.Create<Guid>();
        var query = new GetHistoryQuery(imagePostId);
        var events = new List<IEvent> { CreateNonNexusEvent(imagePostId) };

        SetupEventStore(imagePostId, query.DateTo, events);

        // Act
        var result = await HandleQueryAsync(query);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Equal(ImagePostErrors.NotFound, result.Errors[0]);
    }

    [Fact]
    public async Task HandleAsync_ShouldFilterEventsByDateFrom_WhenDateFromIsProvided()
    {
        // Arrange
        var imagePostId = _fixture.Create<Guid>();
        var now = DateTimeOffset.UtcNow;
        var dateFrom = now.AddDays(-2);
        var query = new GetHistoryQuery(imagePostId, dateFrom);

        var allEvents = new List<IEvent>
        {
            CreateTestEvent(imagePostId, "Event 1", now.AddDays(-4), "user1"),
            CreateTestEvent(imagePostId, "Event 2", now.AddDays(-3), "user1"),
            CreateTestEvent(imagePostId, "Event 3", now.AddDays(-1), "user1"),
            CreateTestEvent(imagePostId, "Event 4", now, "user1")
        };

        SetupEventStore(imagePostId, query.DateTo, allEvents);

        // Act
        var result = await HandleQueryAsync(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCount); // Only events from last 2 days
        Assert.All(result.Value.Items, h => Assert.True(h.Timestamp >= dateFrom));
    }

    [Fact]
    public async Task HandleAsync_ShouldFilterEventsByDateTo_WhenDateToIsProvided()
    {
        // Arrange
        var imagePostId = _fixture.Create<Guid>();
        var now = DateTimeOffset.UtcNow;
        var dateTo = now.AddDays(-1);
        var query = new GetHistoryQuery(imagePostId, null, dateTo);

        var eventsUpToDateTo = new List<IEvent>
        {
            CreateTestEvent(imagePostId, "Event 1", now.AddDays(-4), "user1"),
            CreateTestEvent(imagePostId, "Event 2", now.AddDays(-3), "user1"),
            CreateTestEvent(imagePostId, "Event 3", now.AddDays(-2), "user1")
        };

        SetupEventStore(imagePostId, query.DateTo, eventsUpToDateTo);

        // Act
        var result = await HandleQueryAsync(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TotalCount);
        Assert.All(result.Value.Items, h => Assert.True(h.Timestamp <= dateTo));
    }

    [Fact]
    public async Task HandleAsync_ShouldFilterEventsByDateRange_WhenBothDatesProvided()
    {
        // Arrange
        var imagePostId = _fixture.Create<Guid>();
        var now = DateTimeOffset.UtcNow;
        var dateFrom = now.AddDays(-3);
        var dateTo = now.AddDays(-1);
        var query = new GetHistoryQuery(imagePostId, dateFrom, dateTo);

        var eventsUpToDateTo = new List<IEvent>
        {
            CreateTestEvent(imagePostId, "Event 1", now.AddDays(-4), "user1"),
            CreateTestEvent(imagePostId, "Event 2", now.AddDays(-3), "user1"),
            CreateTestEvent(imagePostId, "Event 3", now.AddDays(-2), "user1")
        };

        SetupEventStore(imagePostId, query.DateTo, eventsUpToDateTo);

        // Act
        var result = await HandleQueryAsync(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.All(result.Value.Items, h =>
        {
            Assert.True(h.Timestamp >= dateFrom);
            Assert.True(h.Timestamp <= dateTo);
        });
    }

    [Fact]
    public async Task HandleAsync_ShouldApplyPagination_WhenPageSizeIsSmaller()
    {
        // Arrange
        var imagePostId = _fixture.Create<Guid>();
        var query = new GetHistoryQuery(imagePostId)
        {
            PageNumber = 1,
            PageSize = 2
        };

        var events = CreateTestEvents(5, imagePostId);

        SetupEventStore(imagePostId, query.DateTo, events);

        // Act
        var result = await HandleQueryAsync(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.TotalCount);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(1, result.Value.PageNumber);
        Assert.Equal(2, result.Value.PageSize);
        Assert.Equal(3, result.Value.TotalPages);
        Assert.True(result.Value.HasNextPage);
        Assert.False(result.Value.HasPreviousPage);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnSecondPage_WhenPageNumberIsTwo()
    {
        // Arrange
        var imagePostId = _fixture.Create<Guid>();
        var query = new GetHistoryQuery(imagePostId)
        {
            PageNumber = 2,
            PageSize = 2
        };

        var events = CreateTestEvents(5, imagePostId);

        SetupEventStore(imagePostId, query.DateTo, events);

        // Act
        var result = await HandleQueryAsync(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.TotalCount);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(2, result.Value.PageNumber);
        Assert.True(result.Value.HasPreviousPage);
        Assert.True(result.Value.HasNextPage);
    }

    [Fact]
    public async Task HandleAsync_ShouldClampPageNumber_WhenPageNumberExceedsTotal()
    {
        // Arrange
        var imagePostId = _fixture.Create<Guid>();
        var query = new GetHistoryQuery(imagePostId)
        {
            PageNumber = 999,
            PageSize = 2
        };

        var events = CreateTestEvents(5, imagePostId);

        SetupEventStore(imagePostId, query.DateTo, events);

        // Act
        var result = await HandleQueryAsync(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.TotalCount);
        Assert.Single(result.Value.Items); // Last page has 1 item
        Assert.Equal(3, result.Value.PageNumber); // Clamped to last page
        Assert.True(result.Value.HasPreviousPage);
        Assert.False(result.Value.HasNextPage);
    }

    [Fact]
    public async Task HandleAsync_ShouldIncludeEventMetadata_InHistoryDto()
    {
        // Arrange
        var imagePostId = _fixture.Create<Guid>();
        var query = new GetHistoryQuery(imagePostId);

        var eventName = _fixture.Create<string>();
        var description = _fixture.Create<string>();
        var timestamp = _fixture.Create<DateTimeOffset>();
        var userName = _fixture.Create<string>();

        var events = new List<IEvent>
        {
            CreateTestEvent(imagePostId, eventName, timestamp, userName, description)
        };

        SetupEventStore(imagePostId, query.DateTo, events);

        // Act
        var result = await HandleQueryAsync(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        
        var historyItem = result.Value.Items[0];
        Assert.Equal(eventName, historyItem.Action);
        Assert.Equal(description, historyItem.Description);
        Assert.Equal(timestamp, historyItem.Timestamp);
        Assert.Equal(userName, historyItem.PerformedBy);
    }

    [Fact]
    public async Task HandleAsync_ShouldHandleNullUserName_InHistoryDto()
    {
        // Arrange
        var imagePostId = _fixture.Create<Guid>();
        var query = new GetHistoryQuery(imagePostId);

        var events = new List<IEvent>
        {
            CreateTestEvent(imagePostId, "Event", _fixture.Create<DateTimeOffset>(), null)
        };

        SetupEventStore(imagePostId, query.DateTo, events);

        // Act
        var result = await HandleQueryAsync(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Null(result.Value.Items[0].PerformedBy);
    }

    private List<IEvent> CreateTestEvents(int count, Guid streamId)
    {
        var events = new List<IEvent>();
        var baseTime = DateTimeOffset.UtcNow;

        for (int i = 0; i < count; i++)
        {
            events.Add(CreateTestEvent(
                streamId, 
                $"Event {i + 1}", 
                baseTime.AddMinutes(-i), 
                $"user{i % 3}"));
        }

        return events;
    }

    private IEvent CreateTestEvent(
        Guid streamId, 
        string eventName, 
        DateTimeOffset timestamp, 
        string? userName,
        string? description = null)
    {
        var mockNexusEvent = new Mock<INexusEvent>();
        mockNexusEvent.Setup(x => x.EventName).Returns(eventName);
        mockNexusEvent.Setup(x => x.Description).Returns(description ?? $"Description for {eventName}");

        var mockEvent = new Mock<IEvent<INexusEvent>>();
        mockEvent.Setup(x => x.Data).Returns(mockNexusEvent.Object);
        mockEvent.Setup(x => x.Timestamp).Returns(timestamp);
        mockEvent.Setup(x => x.UserName).Returns(userName);
        mockEvent.Setup(x => x.StreamId).Returns(streamId);

        return mockEvent.Object;
    }

    private IEvent CreateNonNexusEvent(Guid streamId)
    {
        var mockEvent = new Mock<IEvent>();
        mockEvent.Setup(x => x.StreamId).Returns(streamId);
        mockEvent.Setup(x => x.Timestamp).Returns(DateTimeOffset.UtcNow);

        return mockEvent.Object;
    }
}

