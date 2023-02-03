using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using SixLabors.ImageSharp.Processing;
using Spectre.Console;

namespace OpenTOC;

public class Program
{
    public static void Main(string[] args)
    {
        var fileName = args is {Length: > 0} ? string.Join(" ", args) : "CoreTOC.dat";
        AnsiConsole.Write(new Rule("CoreTOC.dat Parser").Centered().RuleStyle("steelblue1"));
        AnsiConsole.Write(new Rule("https://[underline]forum.ragezone.com[/]/f1034/release-coretoc-dat-parser-1211014/").Centered().RuleStyle("italic steelblue1"));
        AnsiConsole.Write(new Rule("Credits:").RightJustified().RuleStyle("red"));
        AnsiConsole.Write(new Rule("Droppy - [underline]https://github.com/iamdroppy[/]").RightJustified().RuleStyle("red"));
        AnsiConsole.Write(new Rule("advocaite - [underline]https://github.com/advocaite[/]").RightJustified().RuleStyle("red"));
        if (!File.Exists(fileName))
        {
            AnsiConsole.MarkupLine(
                "[underline red on white] Fatal: [/] The [italic]CoreTOC.dat[/] file could not be located.\n[underline]Please provide the CoreTOC.dat file by dragging and dropping it onto the executable, or by passing it as an argument.[/]");
            AnsiConsole.Progress().Start(ctx =>
            {
                var task = ctx.AddTask("Shutdown");
                for (int i = 0; i < 100; i++)
                {
                    task.Increment(1);
                    Thread.Sleep(60);
                }
            });
            Environment.Exit(-1);
        }

        var table = new Table().Border(TableBorder.AsciiDoubleHead);
        table.AddColumns("[underline steelblue1]SNO Group[/]", "[underline steelblue1]Count[/]");
        
        var name = Path.GetFileName(fileName);
        var dict = new Dictionary<SnoGroup, ITableOfContentEntry[]>();
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
            dict = body;
            ctx.Status = "Extracting to [italic]exports.json[/]...";
            File.WriteAllText("exports.json",
                JsonSerializer.Serialize(new { Data = body },
                    new JsonSerializerOptions() { WriteIndented = true, PropertyNameCaseInsensitive = false }));

            ctx.Status = "Rendering table...";
            foreach (var v in body)
            {
                table.AddRow(v.Key.ToString(), v.Value.Length.ToString());
            }
        });

        AnsiConsole.Write(table.Centered());
#if !DEBUG
        Process.Start("explorer.exe", ".");
#endif        
        AnsiConsole.Write(new Rule("[green]Finished![/]").RuleStyle(new Style(foreground: Color.Gold1)));
        var originalTop = Console.CursorTop;
        AnsiConsole.Write(new Markup("[deepskyblue4_1]RaGEZONE Forums[/]").Centered());
        AnsiConsole.Write(new Markup("[deepskyblue4_1 underline]https://forum.ragezone.com/f1034/[/]").Centered());
        AnsiConsole.Write(new Markup(
            "[white] > Create your own [yellow]MMO[/] and [yellow]MMORPG[/] game server or find free [yellow]MMORPG[/] servers [underline]for free[/].[/] <").Centered());
        AnsiConsole.WriteLine("\n");
        AnsiConsole.Write(new Markup("[deepskyblue4_1]Blizzless GitHub[/]").Centered());
        AnsiConsole.Write(new Markup("[deepskyblue4_1 underline]https://github.com/blizzless/blizzless-diiis/tree/community/[/]").Centered());
        AnsiConsole.Write(new Markup(
            "[white] > Stay tuned for the latest server updates, [yellow]open-source[/]![/] <").Centered());
        AnsiConsole.WriteLine("\n");
        AnsiConsole.Write(new Markup("[italic underline mediumpurple]To stay informed on the progress of this project, please follow us for updates.[/]").Centered());
        AnsiConsole.Write(new Rule("[green]Lookup for SNO[/]").RuleStyle(new Style(foreground: Color.Gold1)));
        while (true)
        {
            var lookup = AnsiConsole.Prompt(new TextPrompt<string>("What are you [green]searching[/] for?").PromptStyle("lightseagreen"));
            bool found = false;
            foreach (var d in dict.SelectMany(s=>s.Value))
            {
                if (d.Name.Contains(lookup, StringComparison.InvariantCulture) || d.SnoId.ToString().Contains(lookup, StringComparison.InvariantCulture))
                {
                    // TODO: change to regex replace
                    AnsiConsole.MarkupLine($"[red][[{((SnoGroup)d.SnoGroup).ToString(), 15}]][/] [darkmagenta]Name[/]: {d.Name.Replace(lookup, $"[italic blue3_1]" + lookup + "[/]")} - " +
                                      $"[darkmagenta]SNO[/]: {d.SnoId.ToString().Replace(lookup, $"[italic blue3_1]" + lookup + "[/]")}");
                    found = true;
                }
            }

            if (!found)
            {
                AnsiConsole.MarkupLine($"[red]No SNO Names or Ids matching [underline]{lookup}[/][/]");
            }
            AnsiConsole.Write(new Rule("[green]Lookup for SNO[/]").RuleStyle(new Style(foreground: Color.Gold1)));
        }
    }
}