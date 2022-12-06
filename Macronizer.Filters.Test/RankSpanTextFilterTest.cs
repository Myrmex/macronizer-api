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
        [InlineData("", "")]
        [InlineData("Hello world", "Hello world")]
        [InlineData("<span class=\"ambig\">t<span>ō</span>t<span>ā</span></span> " +
            "<span class=\"ambig\">G<span>a</span>ll<span>i</span><span>ā</span>" +
            "</span> <span class=\"ambig\">d<span>ī</span>v<span>ī</span>s" +
            "<span>a</span></span> <span class=\"ambig\"><span>e</span>st</span> " +
            "<span class=\"auto\"><span>i</span>n</span> " +
            "<span class=\"auto\">p<span>a</span>rt<span>ē</span>s</span> " +
            "<span class=\"auto\">tr<span>ē</span>s</span>.",
            "t[A]ō[/A]t[A]ā[/A] G[A]a[/A]ll[A]i[/A][A]ā[/A] " +
            "d[A]ī[/A]v[A]ī[/A]s[A]a[/A] [A]e[/A]st [N]i[/N]n " +
            "p[N]a[/N]rt[N]ē[/N]s tr[N]ē[/N]s.")]
        public void Apply_AllReplaced(string text, string expected)
        {
            RankSpanTextFilter filter = new();
            StringBuilder sb = new(text);
            filter.Apply(sb, GetDefaultOptions());
            Assert.Equal(expected, sb.ToString());
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("Hello world", "Hello world")]
        [InlineData("<span class=\"ambig\">t<span>ō</span>t<span>ā</span></span> " +
            "<span class=\"ambig\">G<span>a</span>ll<span>i</span><span>ā</span>" +
            "</span> <span class=\"ambig\">d<span>ī</span>v<span>ī</span>s" +
            "<span>a</span></span> <span class=\"ambig\"><span>e</span>st</span> " +
            "<span class=\"auto\"><span>i</span>n</span> " +
            "<span class=\"auto\">p<span>a</span>rt<span>ē</span>s</span> " +
            "<span class=\"auto\">tr<span>ē</span>s</span>.",
            "t[A]ō[/A]t[A]ā[/A] G[A]a[/A]ll[A]i[/A][A]ā[/A] " +
            "d[A]ī[/A]v[A]ī[/A]s[A]a[/A] [A]e[/A]st in " +
            "partēs trēs.")]
        public void Apply_AmbigReplaced(string text, string expected)
        {
            RankSpanTextFilter filter = new();
            StringBuilder sb = new(text);
            filter.Apply(sb, new RankSpanTextFilterOptions()
            {
                AmbiguousEscapeOpen = "[A]",
                AmbiguousEscapeClose = "[/A]"
            });
            Assert.Equal(expected, sb.ToString());
        }
    }
}
