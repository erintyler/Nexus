using Nexus.Application.Common.Services;
using Nexus.Application.Features.ImageProcessing.ProcessImage;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Domain.Errors;
using Wolverine;
using Wolverine.Marten;

namespace Nexus.Application.Features.ImagePosts.ImageUploaded;

public class ImageUploadedCommandHandler
{
    public (Result, Events, OutgoingMessages) Handle(
        ImageUploadedCommand command,
        IUserContextService userContextService,
        [WriteAggregate(Required = false)] ImagePost? imagePost)
    {
        if (imagePost is null)
        {
            return (ImagePostErrors.NotFound, [], []);
        }
        
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