using Kompass.Domain.Referenzdaten;

namespace Kompass.Application.Referenzdaten;

public sealed record ReferenzwertAufloesung(
    Referenzdatensatz Datensatz,
    ReferenzdatenPrioritaet Prioritaet);
