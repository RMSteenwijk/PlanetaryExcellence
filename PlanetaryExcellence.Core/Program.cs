using Microsoft.Extensions.DependencyInjection;
using PlanetaryExcellence.Core;
using PlanetaryExcellence.Core.Services;
using Spectre.Console;
using System;
using System.Diagnostics;

try
{
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddTransient<CharacterService>();
    serviceCollection.AddTransient<PlanetRunService>();
    serviceCollection.AddTransient<ArchiveService>();
    serviceCollection.AddTransient<MainApplication>();

    var provider = serviceCollection.BuildServiceProvider();

    MainApplication.MainMenu(provider);
}
catch(Exception e)
{
    AnsiConsole.WriteException(e);
    Console.ReadKey();
}


public class MainApplication
{
    
    public static void MainMenu(IServiceProvider provider)
    {
        AnsiConsole.Clear();

        AnsiConsole.Write(new FigletText("PI Excellence").Centered().Color(Color.Blue));

        var selection = AnsiConsole.Prompt(
              new SelectionPrompt<string>()
                  .Title("What is your next [green]action[/]?")
                  .PageSize(10)
                  .MoreChoicesText("[grey](Move up and down to reveal more actions)[/]")
                  .AddChoices(new[] {
                        "Modify PI runs", "Modify Characters", "View Archive", "Exit"
                  }));

        switch (selection)
        {
            case "Modify PI runs":
                PIRunsMenu(provider);
                break;
            case "Modify Characters":
                CharactersMenu(provider);
                break;
            case "View Archive":
                provider.GetRequiredService<ArchiveService>().Menu();
                break;
            case "Exit":
                Environment.Exit(0);
                break;
        }

    }

    public static void CharactersMenu(IServiceProvider provider)
    {
        AnsiConsole.Clear();

        AnsiConsole.Write(new FigletText("PI Excellence").Centered().Color(Color.Blue));

        provider.GetRequiredService<CharacterService>().DisplayCurrentCharacters();

        var selection = AnsiConsole.Prompt(
              new SelectionPrompt<string>()
                  .Title("What is your next [green]action[/]?")
                  .PageSize(10)
                  .MoreChoicesText("[grey](Move up and down to reveal more actions)[/]")
                  .AddChoices(new[] {
                        "Add Character", "Edit existing Character", "Remove Character", "<=== Back"
                  }));

        switch (selection)
        {
            case "Add Character":
                provider.GetRequiredService<CharacterService>().AddCharacter();
                break;
            case "Edit existing Character":
                provider.GetRequiredService<CharacterService>().ModifyCharacter();
                break;
            case "Remove Character":
                provider.GetRequiredService<CharacterService>().RemoveCharacter();
                break;
            case "<=== Back":
                MainMenu(provider);
                break;
        }
    }

    public static void PIRunsMenu(IServiceProvider provider)
    {
        AnsiConsole.Clear();

        AnsiConsole.Write(new FigletText("PI Excellence").Centered().Color(Color.Blue));

        provider.GetRequiredService<PlanetRunService>().DisplayCurrentRuns();

        var selection = AnsiConsole.Prompt(
              new SelectionPrompt<string>()
                  .Title("What is your next [green]action[/]?")
                  .PageSize(10)
                  .MoreChoicesText("[grey](Move up and down to reveal more actions)[/]")
                  .AddChoices(new[] {
                        "Add PI run", "Edit existing PI Run", "Remove PI run", "Add Trade Records", "Remove Trade Record", "Open Janice", "Archive PI run", "<=== Back"
                  }));

        switch (selection)
        {
            case "Add PI run":
                provider.GetRequiredService<PlanetRunService>().AddPlanetRun();
                break;
            case "Edit existing PI Run":
                provider.GetRequiredService<PlanetRunService>().ModifyPlanetRun();
                break;
            case "Remove PI run":
                provider.GetRequiredService<PlanetRunService>().RemovePlanetRun();
                break;
            case "Add Trade Records":
                provider.GetRequiredService<PlanetRunService>().AddTradeRecord();
                break;
            case "Remove Trade Record":
                provider.GetRequiredService<PlanetRunService>().RemoveTradeRecord();
                break;
            case "Archive PI run":
                provider.GetRequiredService<PlanetRunService>().ArchiveRun();
                break;
            case "Open Janice":
                Process.Start(new ProcessStartInfo("cmd", $"/c start https://janice.e-351.com/") { CreateNoWindow = true });
                PIRunsMenu(provider);
                break;
            case "<=== Back":
                MainMenu(provider);
                break;
        }
    }
}



