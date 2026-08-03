namespace Kompass.Application.B56Import;

public static class B56SnapshotVersionen
{
    public const int AeltesteUnterstuetzteSchemaVersion = 1;

    public const int AktuelleSchemaVersion = 2;

    public const string AktuelleParserVersion = "1.3";

    public const string LegacyParserVersion = "legacy";

    public static bool WirdUnterstuetzt(int schemaVersion)
    {
        return schemaVersion >= AeltesteUnterstuetzteSchemaVersion &&
               schemaVersion <= AktuelleSchemaVersion;
    }
}
