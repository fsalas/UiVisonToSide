using System.Text.Json;
using UiVision2Side;
using UiVision2Side.Models;

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: uivision2side <input.json> <output.side>");
    return 1;
}

var inputPath = args[0];
var outputPath = args[1];

if (!File.Exists(inputPath))
{
    Console.Error.WriteLine($"Error: input file not found: {inputPath}");
    return 2;
}

UiVisionMacro macro;
try
{
    var json = await File.ReadAllTextAsync(inputPath);
    macro = JsonSerializer.Deserialize<UiVisionMacro>(json)
            ?? throw new InvalidOperationException("Deserialization returned null.");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error reading input file: {ex.Message}");
    return 3;
}

var sideFile = Converter.Convert(macro);

var outputJson = JsonSerializer.Serialize(sideFile, new JsonSerializerOptions
{
    WriteIndented = true,
});

try
{
    await File.WriteAllTextAsync(outputPath, outputJson);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error writing output file: {ex.Message}");
    return 4;
}

Console.WriteLine($"Converted '{macro.Name}' \u2192 {outputPath}");
return 0;
