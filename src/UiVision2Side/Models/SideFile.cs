using System.Text.Json.Serialization;

namespace UiVision2Side.Models;

public class SideFile
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = "2.0";

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("tests")]
    public List<SideTest> Tests { get; set; } = [];

    [JsonPropertyName("suites")]
    public List<SideSuite> Suites { get; set; } = [];

    [JsonPropertyName("urls")]
    public List<string> Urls { get; set; } = [];

    [JsonPropertyName("plugins")]
    public List<object> Plugins { get; set; } = [];
}

public class SideTest
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("commands")]
    public List<SideCommand> Commands { get; set; } = [];
}

public class SideCommand
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("comment")]
    public string Comment { get; set; } = string.Empty;

    [JsonPropertyName("command")]
    public string Command { get; set; } = string.Empty;

    [JsonPropertyName("target")]
    public string Target { get; set; } = string.Empty;

    /// <summary>
    /// Alternative locators as [selector, strategyName] pairs, matching the Selenium IDE format.
    /// </summary>
    [JsonPropertyName("targets")]
    public List<List<string>> Targets { get; set; } = [];

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

public class SideSuite
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("persistSession")]
    public bool PersistSession { get; set; } = false;

    [JsonPropertyName("parallel")]
    public bool Parallel { get; set; } = false;

    [JsonPropertyName("timeout")]
    public int Timeout { get; set; } = 300;

    [JsonPropertyName("tests")]
    public List<string> Tests { get; set; } = [];
}
