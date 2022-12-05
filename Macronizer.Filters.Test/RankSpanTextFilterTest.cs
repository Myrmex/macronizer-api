using System.Text;
using Xunit;

namespace Macronizer.Filters.Test
{
    public sealed class RankSpanTextFilterTest
    {
        private static RankSpanTextFilterOptions GetDefaultOptions() =>
            new()
            {
                UnmarkedEscapeOpen = "[N]",
                UnmarkedEscapeClose = "[/N]",
                AmbiguousEscapeOpen = "[A]",
                AmbiguousEscapeClose = "[/A]",
                UnknownEscapeOpen = "[U]",
                UnknownEscapeClose = "[/U]",
            };

        [Theory]
        [InlineData("")]
        [InlineData("Hello world")]
        [InlineData("<span>Hello</span> <span>world</span>")]
        [InlineData("<span class=\"ambig\">Hello</span> world")]
        [InlineData("<span class=\"ambig\">Hello</span> " +
            "<span class=\"ambig\">world</span>")]
        [InlineData("<span class=\"unknown\">Hello</span> world")]
        [InlineData("<span class=\"unknown\">Hello</span> " +
            "<span class=\"unknown\">world</span>")]
        [InlineData("<span>Hello</span> " +
            "<span class=\"ambig\">my</span> " +
            "<span class=\"unknown\">Gorzorg</span> " +
            "<span>world</span>")]
        public void Apply_AllPreserved(string text)
        {
            RankSpanTextFilter filter = new();
            StringBuilder sb = new(text);
            filter.Apply(sb, new RankSpanTextFilterOptions());
            Assert.Equal(text, sb.ToString());
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("Hello world", "Hello world")]
        [InlineData("<span>Hello</span> <span>world</span>",
            "[N]Hello[/N] [N]world[/N]")]
        [InlineData("<span class=\"ambig\">Hello</span> world",
            "[A]Hello[/A] world")]
        [InlineData("<span class=\"ambig\">Hello</span> " +
            "<span class=\"ambig\">world</span>",
            "[A]Hello[/A] [A]world[/A]")]
        [InlineData("<span class=\"unknown\">Hello</span> world",
            "[U]Hello[/U] world")]
        [InlineData("<span class=\"unknown\">Hello</span> " +
            "<span class=\"unknown\">world</span>",
            "[U]Hello[/U] [U]world[/U]")]
        [InlineData("<span>Hello</span> " +
            "<span class=\"ambig\">my</span> " +
            "<span class=\"unknown\">Gorzorg</span> " +
            "<span>world</span>",
            "[N]Hello[/N] [A]my[/A] [U]Gorzorg[/U] [N]world[/N]")]
        public void Apply_AllReplaced(string text, string expected)
        {
            RankSpanTextFilter filter = new();
            StringBuilder sb = new(text);
            filter.Apply(sb, GetDefaultOptions());
            Assert.Equal(expected, sb.ToString());
        }

        [Theory]
        [InlineData("", "")]
        [InlineData(
            "<span>Hello</span> <span class=\"ambig\">my</span> <span>world</span>",
            "Hello ¿my world")]
        public void Apply_OpenReplaced(string text, string expected)
        {
            RankSpanTextFilter filter = new();
            StringBuilder sb = new(text);
            filter.Apply(sb, new RankSpanTextFilterOptions
            {
                AmbiguousEscapeOpen = "¿",
                AmbiguousEscapeClose = null,
            });
            Assert.Equal(expected, sb.ToString());
        }

        [Theory]
        [InlineData("", "")]
        [InlineData(
            "<span>Hello</span> <span class=\"ambig\">my</span> <span>world</span>",
            "Hello my¿ world")]
        public void Apply_CloseReplaced(string text, string expected)
        {
            RankSpanTextFilter filter = new();
            StringBuilder sb = new(text);
            filter.Apply(sb, new RankSpanTextFilterOptions
            {
                AmbiguousEscapeOpen = null,
                AmbiguousEscapeClose = "¿",
            });
            Assert.Equal(expected, sb.ToString());
        }
    }
}
