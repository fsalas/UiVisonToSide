using System.Text.Json.Serialization;

namespace UiVision2Side.Models;

public class UiVisionMacro
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("CreationDate")]
    public string CreationDate { get; set; } = string.Empty;

    [JsonPropertyName("Commands")]
    public List<UiVisionCommand> Commands { get; set; } = [];
}

public class UiVisionCommand
{
    [JsonPropertyName("Command")]
    public string Command { get; set; } = string.Empty;

    [JsonPropertyName("Target")]
    public string Target { get; set; } = string.Empty;

    [JsonPropertyName("Value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("Targets")]
    public List<string> Targets { get; set; } = [];

    [JsonPropertyName("Description")]
    public string Description { get; set; } = string.Empty;
}
