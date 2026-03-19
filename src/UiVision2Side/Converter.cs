using UiVision2Side.Models;

namespace UiVision2Side;

public static class Converter
{
    /// <summary>
    /// Converts a UI.Vision macro to a Selenium IDE .side file structure.
    /// </summary>
    public static SideFile Convert(UiVisionMacro macro)
    {
        var projectId = NewId();
        var testId = NewId();

        var baseUrl = ExtractBaseUrl(macro.Commands);

        var sideCommands = macro.Commands
            .Select(c => ConvertCommand(c, baseUrl))
            .ToList();

        var test = new SideTest
        {
            Id = testId,
            Name = macro.Name,
            Commands = sideCommands,
        };

        var suite = new SideSuite
        {
            Id = NewId(),
            Name = "Default Suite",
            PersistSession = false,
            Parallel = false,
            Timeout = 300,
            Tests = [testId],
        };

        return new SideFile
        {
            Id = projectId,
            Version = "2.0",
            Name = macro.Name,
            Url = baseUrl,
            Tests = [test],
            Suites = [suite],
            Urls = baseUrl.Length > 0 ? [baseUrl] : [],
            Plugins = [],
        };
    }

    private static SideCommand ConvertCommand(UiVisionCommand cmd, string baseUrl)
    {
        var target = cmd.Command.Equals("open", StringComparison.OrdinalIgnoreCase)
            ? ToRelativePath(cmd.Target, baseUrl)
            : cmd.Target;

        return new SideCommand
        {
            Id = NewId(),
            Comment = cmd.Description,
            Command = cmd.Command,
            Target = target,
            Targets = ConvertTargets(cmd.Targets),
            Value = cmd.Value,
        };
    }

    /// <summary>
    /// Converts a list of UI.Vision alternative locators to Selenium IDE target pairs.
    /// Each UI.Vision entry is a string like "xpath=//div" or "css=.btn".
    /// The Selenium IDE format is a list of [selector, strategyName] pairs.
    /// </summary>
    private static List<List<string>> ConvertTargets(List<string> targets)
    {
        return targets
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t =>
            {
                var strategy = ExtractStrategyName(t);
                return new List<string> { t, strategy };
            })
            .ToList();
    }

    /// <summary>
    /// Derives a human-readable strategy name from a locator string.
    /// e.g. "xpath=//div" → "xpath:idRelative", "css=.btn" → "css:finder"
    /// </summary>
    private static string ExtractStrategyName(string locator)
    {
        if (locator.StartsWith("xpath=", StringComparison.OrdinalIgnoreCase))
            return "xpath:idRelative";
        if (locator.StartsWith("css=", StringComparison.OrdinalIgnoreCase))
            return "css:finder";
        if (locator.StartsWith("id=", StringComparison.OrdinalIgnoreCase))
            return "id";
        if (locator.StartsWith("name=", StringComparison.OrdinalIgnoreCase))
            return "name";
        if (locator.StartsWith("linkText=", StringComparison.OrdinalIgnoreCase))
            return "linkText";

        return string.Empty;
    }

    /// <summary>
    /// Extracts the base URL (scheme + host + port) from the first open command.
    /// Returns an empty string when no open command is present.
    /// </summary>
    public static string ExtractBaseUrl(IEnumerable<UiVisionCommand> commands)
    {
        var openCmd = commands.FirstOrDefault(
            c => c.Command.Equals("open", StringComparison.OrdinalIgnoreCase));

        if (openCmd is null || string.IsNullOrWhiteSpace(openCmd.Target))
            return string.Empty;

        if (!Uri.TryCreate(openCmd.Target, UriKind.Absolute, out var uri))
            return string.Empty;

        // Keep scheme + host + port only
        return uri.GetLeftPart(UriPartial.Authority);
    }

    /// <summary>
    /// Converts an absolute URL to a path relative to the given base URL.
    /// Falls back to the original value when conversion is not possible.
    /// </summary>
    public static string ToRelativePath(string url, string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            return url;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return url;

        // Path + query + fragment
        var relative = uri.PathAndQuery;
        if (!string.IsNullOrEmpty(uri.Fragment))
            relative += uri.Fragment;

        return relative.Length > 0 ? relative : "/";
    }

    private static string NewId() => Guid.NewGuid().ToString();
}
