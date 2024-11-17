using System.Text.Json.Serialization;

namespace Crany.Shared.Models;

public class Resource
{
    [JsonPropertyName("@id")]
    public string Id { get; set; }

    [JsonPropertyName("@type")]
    public string Type { get; set; }

    [JsonPropertyName("comment")]
    public string Comment { get; set; }
}