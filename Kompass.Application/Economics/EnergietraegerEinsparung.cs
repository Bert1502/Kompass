using Kompass.Domain.Economics;

namespace Kompass.Application.Economics;

/// <summary>
/// Jährliche Energiekosteneinsparung für einen einzelnen Energieträger.
/// </summary>
public sealed record EnergietraegerEinsparung(
    Energietraeger Energietraeger,
    decimal JaehrlicheEinsparungKwh);
