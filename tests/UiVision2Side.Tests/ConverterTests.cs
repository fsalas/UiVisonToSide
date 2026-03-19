using System.Text.Json;
using UiVision2Side;
using UiVision2Side.Models;

namespace UiVision2Side.Tests;

public class ConverterTests
{
    private static UiVisionMacro LoadFixture(string name)
    {
        var path = Path.Combine(
            AppContext.BaseDirectory, "Fixtures", name);

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<UiVisionMacro>(json)
               ?? throw new InvalidOperationException("Fixture deserialization returned null.");
    }

    [Fact]
    public void Convert_LoginFixture_ProducesValidSideFile()
    {
        var macro = LoadFixture("Login.json");

        var side = Converter.Convert(macro);

        Assert.NotNull(side);
        Assert.False(string.IsNullOrWhiteSpace(side.Id));
        Assert.Equal("2.0", side.Version);
        Assert.Equal("Login", side.Name);
    }

    [Fact]
    public void Convert_LoginFixture_ExtractsBaseUrl()
    {
        var macro = LoadFixture("Login.json");

        var side = Converter.Convert(macro);

        Assert.Equal("https://example.com", side.Url);
        Assert.Contains("https://example.com", side.Urls);
    }

    [Fact]
    public void Convert_LoginFixture_ProducesOneTestAndOneSuite()
    {
        var macro = LoadFixture("Login.json");

        var side = Converter.Convert(macro);

        Assert.Single(side.Tests);
        Assert.Single(side.Suites);
        Assert.Equal("Login", side.Tests[0].Name);
        Assert.Equal("Default Suite", side.Suites[0].Name);
    }

    [Fact]
    public void Convert_LoginFixture_SuiteReferencesTest()
    {
        var macro = LoadFixture("Login.json");

        var side = Converter.Convert(macro);

        Assert.Contains(side.Tests[0].Id, side.Suites[0].Tests);
    }

    [Fact]
    public void Convert_LoginFixture_MapsAllFourCommands()
    {
        var macro = LoadFixture("Login.json");

        var side = Converter.Convert(macro);
        var cmds = side.Tests[0].Commands;

        Assert.Equal(4, cmds.Count);
        Assert.Equal("open", cmds[0].Command);
        Assert.Equal("click", cmds[1].Command);
        Assert.Equal("type", cmds[2].Command);
        Assert.Equal("clickAndWait", cmds[3].Command);
    }

    [Fact]
    public void Convert_LoginFixture_OpenCommandUsesRelativePath()
    {
        var macro = LoadFixture("Login.json");

        var side = Converter.Convert(macro);
        var openCmd = side.Tests[0].Commands[0];

        Assert.Equal("/login", openCmd.Target);
    }

    [Fact]
    public void Convert_LoginFixture_ClickCommandPreservesXpathTarget()
    {
        var macro = LoadFixture("Login.json");

        var side = Converter.Convert(macro);
        var clickCmd = side.Tests[0].Commands[1];

        Assert.Equal("xpath=//input[@id='username']", clickCmd.Target);
    }

    [Fact]
    public void Convert_LoginFixture_TypeCommandHasValue()
    {
        var macro = LoadFixture("Login.json");

        var side = Converter.Convert(macro);
        var typeCmd = side.Tests[0].Commands[2];

        Assert.Equal("type", typeCmd.Command);
        Assert.Equal("admin", typeCmd.Value);
    }

    [Fact]
    public void Convert_LoginFixture_AlternativeTargetsConverted()
    {
        var macro = LoadFixture("Login.json");

        var side = Converter.Convert(macro);

        // click command has 2 alternative targets
        var clickCmd = side.Tests[0].Commands[1];
        Assert.Equal(2, clickCmd.Targets.Count);

        // Each entry is a [selector, strategyName] pair
        Assert.Equal("xpath=//input[@id='username']", clickCmd.Targets[0][0]);
        Assert.Equal("xpath:idRelative", clickCmd.Targets[0][1]);

        Assert.Equal("css=#username", clickCmd.Targets[1][0]);
        Assert.Equal("css:finder", clickCmd.Targets[1][1]);
    }

    [Fact]
    public void Convert_LoginFixture_ClickAndWaitPreservesCssTarget()
    {
        var macro = LoadFixture("Login.json");

        var side = Converter.Convert(macro);
        var submitCmd = side.Tests[0].Commands[3];

        Assert.Equal("css=button[type='submit']", submitCmd.Target);
    }

    [Fact]
    public void Convert_LoginFixture_DescriptionMappedToComment()
    {
        var macro = LoadFixture("Login.json");

        var side = Converter.Convert(macro);

        Assert.Equal("Navigate to login page", side.Tests[0].Commands[0].Comment);
        Assert.Equal("Click username field", side.Tests[0].Commands[1].Comment);
    }

    [Fact]
    public void Convert_LoginFixture_AllCommandIdsAreUniqueGuids()
    {
        var macro = LoadFixture("Login.json");

        var side = Converter.Convert(macro);
        var ids = side.Tests[0].Commands.Select(c => c.Id).ToList();

        Assert.All(ids, id => Assert.True(Guid.TryParse(id, out _)));
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void Convert_MacroWithNoOpenCommand_BaseUrlIsEmpty()
    {
        var macro = new UiVisionMacro
        {
            Name = "NoOpen",
            Commands =
            [
                new UiVisionCommand { Command = "click", Target = "css=#btn" },
            ],
        };

        var side = Converter.Convert(macro);

        Assert.Equal(string.Empty, side.Url);
        Assert.Empty(side.Urls);
    }

    [Fact]
    public void ExtractBaseUrl_ReturnsSchemeAndHost()
    {
        var commands = new List<UiVisionCommand>
        {
            new() { Command = "open", Target = "https://example.com/path?q=1" },
        };

        var baseUrl = Converter.ExtractBaseUrl(commands);

        Assert.Equal("https://example.com", baseUrl);
    }

    [Fact]
    public void ToRelativePath_ReturnsPathAndQuery()
    {
        var relative = Converter.ToRelativePath(
            "https://example.com/path?q=1", "https://example.com");

        Assert.Equal("/path?q=1", relative);
    }

    [Fact]
    public void ToRelativePath_RootUrl_ReturnsSlash()
    {
        var relative = Converter.ToRelativePath(
            "https://example.com", "https://example.com");

        Assert.Equal("/", relative);
    }

    [Fact]
    public void Convert_LoginFixture_SerializesToValidJson()
    {
        var macro = LoadFixture("Login.json");
        var side = Converter.Convert(macro);

        var json = JsonSerializer.Serialize(side, new JsonSerializerOptions { WriteIndented = true });

        Assert.NotEmpty(json);
        // Round-trip: must deserialize back without error
        var reparsed = JsonSerializer.Deserialize<SideFile>(json);
        Assert.NotNull(reparsed);
        Assert.Equal(side.Name, reparsed!.Name);
        Assert.Single(reparsed.Tests);
    }
}
