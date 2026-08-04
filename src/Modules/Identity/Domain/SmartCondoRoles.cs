namespace Modules.Identity.Domain;

/// <summary>
/// Papéis RBAC do SmartCondo conforme FN-ID-02.
/// </summary>
public static class SmartCondoRoles
{
    public const string Administradora = "Administradora";
    public const string Sindico = "Sindico";
    public const string Zelador = "Zelador";
    public const string Portaria = "Portaria";
    public const string Condomino = "Condomino";

    public static readonly IReadOnlyList<string> All =
    [
        Administradora,
        Sindico,
        Zelador,
        Portaria,
        Condomino
    ];

    /// <summary>
    /// Mapeia rótulos da UI Stitch para roles canônicas do domínio.
    /// </summary>
    public static string FromStitchLabel(string stitchLabel) => stitchLabel switch
    {
        "Porteiro" => Portaria,
        "Morador" => Condomino,
        _ when All.Contains(stitchLabel) => stitchLabel,
        _ => throw new ArgumentException($"Perfil Stitch desconhecido: {stitchLabel}", nameof(stitchLabel))
    };
}
