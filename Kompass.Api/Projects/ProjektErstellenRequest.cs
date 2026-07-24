using System.ComponentModel.DataAnnotations;

namespace Kompass.Api.Projects;

public sealed class ProjektErstellenRequest
{
    [Required(
        ErrorMessage = "Der Projektname ist erforderlich.")]
    [StringLength(
        200,
        MinimumLength = 1,
        ErrorMessage =
            "Der Projektname muss zwischen 1 und 200 Zeichen enthalten.")]
    public string Name { get; init; } = string.Empty;
}