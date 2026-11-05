using System.Text.Json.Serialization;

namespace Edt.Infrastructure.HttpClients.EdtReader;

public sealed record EdtReaderCourse(
    [property: JsonPropertyName("jour")] string Day,
    [property: JsonPropertyName("mois")] string Month,
    [property: JsonPropertyName("annee")] string Year,
    [property: JsonPropertyName("cours")] string Title,
    [property: JsonPropertyName("typeCours")] string Type,
    [property: JsonPropertyName("groupe")] string Group,
    [property: JsonPropertyName("salle")] string Room,
    [property: JsonPropertyName("enseignant")] string Teacher,
    [property: JsonPropertyName("hDebut")] string Start,
    [property: JsonPropertyName("hFin")] string End);
