using FluentAssertions;
using ModLoader.Core;

namespace ModLoader.Tests;

public class WorkshopIdParserTests
{
    [Theory]
    [InlineData("441616640", 441616640UL)]
    [InlineData("  12345  ", 12345UL)]
    [InlineData("https://steamcommunity.com/sharedfiles/filedetails/?id=441616640", 441616640UL)]
    [InlineData("https://steamcommunity.com/sharedfiles/filedetails/?l=russian&id=441616640", 441616640UL)]
    [InlineData("steamcommunity.com/sharedfiles/filedetails/?id=999", 999UL)]
    public void Parse_ВалидныйВвод_ВозвращаетId(string input, ulong expected)
    {
        WorkshopIdParser.TryParse(input, out var id).Should().BeTrue();
        id.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("https://steamcommunity.com/sharedfiles/filedetails/")]
    public void Parse_НевалидныйВвод_False(string input)
    {
        WorkshopIdParser.TryParse(input, out _).Should().BeFalse();
    }
}
