using Macronizer.Filters;
using System.Text;
using Xunit;

namespace Macronizer.Filter.Test;

public sealed class WhitespaceLineFilterTest
{
    [Theory]
    [InlineData("", "")]
    [InlineData("Hello world", "Hello world")]
    [InlineData("Hello\tworld", "Hello world")]
    [InlineData("Hello   world", "Hello world")]
    [InlineData("  left trim", "left trim")]
    [InlineData("right trim  ", "right trim")]
    [InlineData("  both  trim\t", "both trim")]
    [InlineData("Hello\r\nmy\nworld!", "Hello\nmy\nworld!")]
    public void Apply(string text, string expected)
    {
        WhitespaceTextFilter filter = new();
        StringBuilder sb = new(text);
        filter.Apply(sb);
        Assert.Equal(expected, sb.ToString());
    }
}
