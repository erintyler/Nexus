using AutoFixture;
using Nexus.Application.Common.Models;

namespace Nexus.Application.UnitTests.Extensions;

public static class AutoFixtureExtensions
{
    extension(Fixture fixture)
    {
        public string CreateString(int length)
        {
            return string.Join(string.Empty, fixture.CreateMany<char>(length));
        }

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
    }
}