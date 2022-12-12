using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Fusi.Text.Unicode;
using Proteus.Core;

namespace Macronizer.Filters;

/// <summary>
/// Unicode Mn-category characters composer. This is used to compose all
/// the Mn characters in the specified text, wherever and to the extent it
/// is possible.
/// This operation is useful in some specific scenarios, like in
/// <see cref="SequenceMatcherFilter"/>, which removes all the non relevant
/// diacritics. These diacritics usually are precombined, and they are
/// defined as such in the filter configuration, e.g. an <c>o</c> plus a
/// macron in a unique Unicode code; so, should an input text contain
/// the decomposed version of the same text <c>o</c> + macron, it would
/// drop the diacritic altogether, as it does not look for "macron" alone
/// (a Mn character), but only for <c>o</c> + macron. Pre-composing these
/// sequences where possible allows to avoid this issue, while where this
/// is not possible, it is safe to assume that the decomposed version
/// would be present in the configuration too. Apart from that, this
/// composer can be used wherever you want to get a more compact text,
/// usually easier to be processed and better displayed.
/// </summary>
public sealed class MnComposer
{
    private readonly MappingMatcher _matcher;
    private readonly Regex _mnRegex;

    /// <summary>
    /// Initializes a new instance of the <see cref="MnComposer"/> class.
    /// </summary>
    /// <param name="ud">The Unicode data helper to use.</param>
    /// <exception cref="ArgumentNullException">null ud</exception>
    public MnComposer(UniData ud)
    {
        if (ud == null) throw new ArgumentNullException(nameof(ud));

        // source
        PartitionTable sourceTable = new(ud);
        using (TextReader sourceReader =
            new StreamReader(GetResourceStream("Unicode.txt"), Encoding.UTF8))
        {
            sourceTable.Read(sourceReader, 1);
        }
        // target
        PartitionTable targetTable = new(ud);
        using (TextReader targetReader =
            new StreamReader(GetResourceStream("Unicode.txt"), Encoding.UTF8))
        {
            targetTable.Read(targetReader, 2);
        }

        _matcher = new MappingMatcher
        {
            PreferredModifiers = new[]
            {
                0x0304, // macron
                0x0306  // breve
            }
        };
        _matcher.SetSourceMappings(sourceTable.Mappings);
        _matcher.SetTargetMappings(targetTable.Mappings);

        _mnRegex = new Regex(@"\P{Mn}\p{Mn}+");
    }

    private static Stream GetResourceStream(string name)
    {
        return Assembly.GetExecutingAssembly()
            .GetManifestResourceStream($"Macronizer.Filters.Assets.{name}")!;
    }

    /// <summary>
    /// Wherever and to the extent it is possible, composes all the Mn
    /// characters in the specified text.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="padding">The optional padding character to be used.
    /// Pass 0 to avoid padding; any other character will be used to pad
    /// each sequence of non-Mn character + Mn character(s) so that its
    /// length is the same of the original sequence.</param>
    /// <returns>composed text</returns>
    public string Compose(string text, char padding)
    {
        if (string.IsNullOrEmpty(text)) return text;

        if (text.All(c => char.GetUnicodeCategory(c)
            != UnicodeCategory.NonSpacingMark))
        {
            return text;
        }

        List<int> codes = (from c in text select (int)c).ToList();

        return _mnRegex.Replace(text, m =>
        {
            SegmentMapping? mapping = _matcher.MapToken(
                codes, Tuple.Create(m.Index, m.Length));
            if (mapping == null) return text;
            MatchResult? result = _matcher.MatchMapping(mapping);
            if (result == null) return text;
            string converted = new((from part in result.Parts
                                    select (char)part.Mapping.Id).ToArray());

            if (padding != '\0')
                converted = converted.PadRight(m.Length, padding);
            return converted;
        });
    }
}
