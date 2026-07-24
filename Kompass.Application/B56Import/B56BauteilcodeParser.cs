using System.Text.RegularExpressions;
using Kompass.Application.B56Import;

namespace Kompass.Persistence.B56Import;

public sealed class B56BauteilcodeParser
    : IB56BauteilcodeParser
{
    private static readonly Regex CodeMuster =
        new(
            @"(?<![A-Z0-9ÄÖÜ])" +
            @"(?<code>[A-ZÄÖÜ]{1,5}[\s\-_]?\d{0,4})" +
            @"(?![A-Z0-9ÄÖÜ])",
            RegexOptions.Compiled
            | RegexOptions.IgnoreCase
            | RegexOptions.CultureInvariant);

    private readonly IReadOnlyList<B56Bauteilzuordnung>
        _zuordnungen;

    public B56BauteilcodeParser(
        IB56BauteilzuordnungsRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        _zuordnungen =
            repository
                .Laden()
                .OrderByDescending(
                    z => z.Prioritaet)
                .ThenByDescending(
                    z => z.Code.Length)
                .ToList();
    }

    public B56Bauteilcode Parsen(
        string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new B56Bauteilcode
            {
                Originaltext = string.Empty,
                IstGueltig = false
            };
        }

        var bereinigterText =
            text.Trim();

        var code =
            ErmittleCode(bereinigterText);

        if (string.IsNullOrWhiteSpace(code))
        {
            return new B56Bauteilcode
            {
                Originaltext = bereinigterText,
                Bezeichnung = bereinigterText,
                Kategorie = "Unbekannt",
                IstGueltig = false
            };
        }

        var zuordnung =
            ErmittleZuordnung(code);

        if (zuordnung is null)
        {
            return new B56Bauteilcode
            {
                Originaltext = bereinigterText,
                Code = code,
                Bezeichnung =
                    ErmittleBezeichnung(
                        bereinigterText,
                        code),
                Kategorie = "Unbekannt",
                IstGueltig = true
            };
        }

        return new B56Bauteilcode
        {
            Originaltext = bereinigterText,
            Code = code,
            Bezeichnung =
                ErmittleBezeichnung(
                    bereinigterText,
                    code),
            Kategorie = zuordnung.Kategorie,
            IstGueltig = true
        };
    }

    private static string ErmittleCode(
        string text)
    {
        var match =
            CodeMuster.Match(
                text.ToUpperInvariant());

        if (!match.Success)
        {
            return string.Empty;
        }

        return NormalisiereCode(
            match.Groups["code"].Value);
    }

    private B56Bauteilzuordnung? ErmittleZuordnung(
        string code)
    {
        foreach (var zuordnung in _zuordnungen)
        {
            var vergleichscode =
                NormalisiereCode(
                    zuordnung.Code);

            if (code.StartsWith(
                    vergleichscode,
                    StringComparison.OrdinalIgnoreCase))
            {
                return zuordnung;
            }
        }

        return null;
    }

    private static string ErmittleBezeichnung(
        string text,
        string code)
    {
        var index =
            text.IndexOf(
                code,
                StringComparison.OrdinalIgnoreCase);

        if (index < 0)
        {
            return text;
        }

        var bezeichnung =
            text.Remove(
                    index,
                    Math.Min(
                        code.Length,
                        text.Length - index))
                .Trim(
                    ' ',
                    '-',
                    '_',
                    ':',
                    ';');

        return string.IsNullOrWhiteSpace(bezeichnung)
            ? text
            : bezeichnung;
    }

    private static string NormalisiereCode(
        string code)
    {
        return code
            .Trim()
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty)
            .Replace("_", string.Empty)
            .ToUpperInvariant();
    }
}