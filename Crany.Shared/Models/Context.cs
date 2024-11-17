using System.Text.Json.Serialization;

namespace Crany.Shared.Models;

public class Context
{
    [JsonPropertyName("@vocab")]
    public string Vocab { get; set; }

    [JsonPropertyName("comment")]
    public string Comment { get; set; }
}