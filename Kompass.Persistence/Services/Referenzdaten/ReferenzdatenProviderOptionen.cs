namespace Kompass.Persistence.Services.Referenzdaten;

public sealed class ReferenzdatenProviderOptionen
{
    public const string SectionName = "ReferenzdatenProvider";

    public string JsonDateiPfad { get; set; } = string.Empty;

    public string CsvDateiPfad { get; set; } = string.Empty;

    public string XmlDateiPfad { get; set; } = string.Empty;

    public string ExcelDateiPfad { get; set; } = string.Empty;
}
