using Nexus.Domain.Enums;

namespace Nexus.Domain.Events.ImagePosts;

public record StatusChangedDomainEvent(Guid ImageId, UploadStatus UploadStatus, Guid? UserId = null) : INexusEvent
{
    public string EventName => "Image post status changed";
    public string Description => $"New Status: {UploadStatus}";
}