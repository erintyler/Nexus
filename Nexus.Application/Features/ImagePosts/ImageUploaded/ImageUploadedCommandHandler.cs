using Nexus.Application.Common.Services;
using Nexus.Application.Features.ImageProcessing.ProcessImage;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Wolverine;
using Wolverine.Marten;

namespace Nexus.Application.Features.ImagePosts.ImageUploaded;

public class ImageUploadedCommandHandler
{
    public (Result, Events, OutgoingMessages) Handle(
        ImageUploadedCommand command,
        IUserContextService userContextService,
        [WriteAggregate] ImagePost imagePost)
    {
        var userId = userContextService.GetUserId();
        var markAsProcessingResult = imagePost.MarkAsProcessing(userId);

        if (markAsProcessingResult.IsFailure)
        {
            return (Result.Failure(markAsProcessingResult.Errors), [], []);
        }

        var processImageCommand = new ProcessImageCommand(command.Id);
        return (Result.Success(), [markAsProcessingResult.Value], [processImageCommand]);
    }
}