using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Spectre.Console;

namespace OpenTOC;

public class Program
{
    public static void Main(string[] args)
    {
        var fileName = args is {Length: > 0} ? args[0] : "CoreTOC.dat";
        AnsiConsole.Write(new Rule("CoreTOC.dat Parser").Centered().RuleStyle("steelblue1"));
        AnsiConsole.Write(new Rule("https://forum.ragezone.com/f1034/release-coretoc-dat-parser-1211014/#post9160797").Centered().RuleStyle("italic steelblue1"));
        AnsiConsole.Write(new Rule("Credits:").RightJustified().RuleStyle("red"));
        AnsiConsole.Write(new Rule("Droppy - https://github.com/iamdroppy").RightJustified().RuleStyle("red"));
        AnsiConsole.Write(new Rule("advocaite - https://github.com/advocaite").RightJustified().RuleStyle("red"));
        if (!File.Exists(fileName))
        {
            AnsiConsole.MarkupLine(
                "[underline red on white] Fatal: [/] File not found. Please drag and drop the file onto the executable.");
            return;
        }

        var name = Path.GetFileName(fileName);
        AnsiConsole.Status().Start("Parsing [italic]" + name + "[/]...", ctx =>
        {
            // We open the file, and create a BinaryReader.
            using var stream = File.OpenRead(fileName);
            using var reader = new BinaryReader(stream);
            // We read the TOC, then we sort it by SnoGroup and SnoId, then we group it by SnoGroup.
            // We then convert it to a dictionary, where the key is the SnoGroup, and the value is an array of all the entries in that group.
            var body = TableOfContent.Read(reader)
                .EntryData
                .OrderBy(s => s.SnoGroup)
                .ThenBy(s => s.SnoId)
                .GroupBy(x => x.SnoGroup, x => x)
                .ToDictionary(s => (SnoGroup)s.Key, s => s.ToArray());
            ctx.Status = "Extracting to [italic]exports.json[/]...";
            File.WriteAllText("exports.json",
                JsonSerializer.Serialize(new { Data = body },
                    new JsonSerializerOptions() { WriteIndented = true, PropertyNameCaseInsensitive = false }));
        });

        AnsiConsole.MarkupLine("[green]Finished![/] Shutting down...");
        Process.Start("explorer.exe", ".");
        AnsiConsole.Progress().Start(ctx =>
        {
            var task = ctx.AddTask("Shutting down");
            for (int i = 0; i < 100; i++)
            {
                task.Increment(1);
                Thread.Sleep(25);
            }
        });
    }
}