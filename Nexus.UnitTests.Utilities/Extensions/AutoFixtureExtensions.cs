using AutoFixture;
using Nexus.Application.Common.Models;
using Nexus.Domain.Entities;
using Nexus.Domain.Enums;
using Nexus.Domain.Events.ImagePosts;
using Nexus.Domain.Primitives;

namespace Nexus.UnitTests.Utilities.Extensions;

public static class AutoFixtureExtensions
{
    extension(Fixture fixture)
    {
        public string CreateString(int length)
        {
            return string.Join(string.Empty, fixture.CreateMany<char>(length));
        }

        // Application layer extensions
        public TagDto CreateTagDto()
        {
            return fixture.Build<TagDto>()
                .With(t => t.Value, fixture.CreateString(30))
                .Create();
        }
        
        public IReadOnlyList<TagDto> CreateTagDtoList(int? count = null)
        {
            var builder = fixture.Build<TagDto>()
                .With(t => t.Value, fixture.CreateString(30));
            
            return count.HasValue ? builder.CreateMany(count.Value).ToList() : builder.CreateMany().ToList();
        }

        // Domain layer extensions
        public TagData CreateTagData()
        {
            return new TagData(
                fixture.Create<TagType>(),
                fixture.CreateString(15));
        }
        
        public IReadOnlyList<TagData> CreateTagDataList(int? count = null)
        {
            var tags = new List<TagData>();
            var tagCount = count ?? fixture.Create<int>() % 5 + 1;
            
            for (var i = 0; i < tagCount; i++)
            {
                tags.Add(fixture.CreateTagData());
            }
            
            return tags;
        }
        
        public ImagePost CreateImagePost(Guid? userId = null, string? title = null, IReadOnlyList<TagData>? tags = null)
        {
            var imagePost = new ImagePost();
            var createdEvent = new ImagePostCreatedDomainEvent(
                userId ?? fixture.Create<Guid>(),
                title ?? fixture.CreateString(50),
                tags ?? fixture.CreateTagDataList(3));
            
            imagePost.Apply(createdEvent);
            return imagePost;
        }
        
        public ImagePost CreateImagePostWithStatus(UploadStatus status, Guid? userId = null)
        {
            var imagePost = fixture.CreateImagePost(userId);
            
            if (status != UploadStatus.Pending)
            {
                var statusEvent = new StatusChangedDomainEvent(
                    imagePost.Id, 
                    status, 
                    userId);
                imagePost.Apply(statusEvent);
            }
            
            return imagePost;
        }
    }
}

