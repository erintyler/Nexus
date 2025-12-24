using AutoFixture;
using Nexus.Domain.Enums;
using Nexus.Domain.Primitives;

namespace Nexus.Domain.UnitTests.Extensions;

public static class AutoFixtureExtensions
{
    extension(Fixture fixture)
    {
        public string CreateString(int length)
        {
            return string.Join(string.Empty, fixture.CreateMany<char>(length));
        }

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
    }
}

