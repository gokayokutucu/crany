using System.Text.Json.Serialization;

namespace Crany.Shared.Models;

public class ServiceIndex
{
    [JsonPropertyName("version")]
    public string Version { get; set; }

    [JsonPropertyName("resources")]
    public Resource[] Resources { get; set; }

    [JsonPropertyName("@context")]
    public Context Context { get; set; }
}