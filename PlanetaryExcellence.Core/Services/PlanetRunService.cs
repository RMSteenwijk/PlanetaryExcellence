using LiteDB;
using PlanetaryExcellence.Core.Models;
using Spectre.Console;

namespace PlanetaryExcellence.Core.Services
{
    public class PlanetRunService
    {
        private CharacterService _characterService;
        private IServiceProvider _serviceProvider;

        public PlanetRunService(
            CharacterService characterService, 
            IServiceProvider provider)
        {
            _characterService = characterService;
            _serviceProvider = provider;
        }


        public void DisplayCurrentRuns()
        {
            using var storage = new Storage();
            var table = new Table();
            table.AddColumns($"Id", "PlanetName", "CharacterName", "Profit", "Time left");
            table.Columns[3].RightAligned();

            foreach (var character in storage.ListAllCharacters())
            {
                var charName = character.Name;
                foreach(var p in character.CurrentRunningPlanets)
                {
                    p.Trades = storage.ListTradeRecordsForRun(p.Id);
                    var timeTillComplete = p.StartTime.AddHours(p.RunDurationInHours) - DateTime.Now;
                    var totalTradesAmount = p.Trades.Sum(x => x.TotalAmount);
                    var tradeColorMarkup = totalTradesAmount < 0 ? "red" : "green";
                    table.AddRow(new Markup[] { new Markup($"{p.Id}"), new Markup(p.Name), new Markup(charName), new Markup($"[{tradeColorMarkup}]{totalTradesAmount.ToString("N")}[/]"), new Markup($"{timeTillComplete.Days} days {timeTillComplete.Hours} hrs {timeTillComplete.Minutes} mins") });
                }
            }

            AnsiConsole.Write(table);
        }

        public void AddPlanetRun()
        {
            using var storage = new Storage();
            var chosenCharacter = _characterService.SelectionScreen(() => MainApplication.PIRunsMenu(_serviceProvider));

            var planetName = AnsiConsole.Ask<string>("What planet is doing the run?");
            var runTimeInHours = AnsiConsole.Ask<int>("What is the run time to completion in hours?");

            AnsiConsole.WriteLine("");
            if (AnsiConsole.Confirm("Are you happy with these inputs?"))
            {
                storage.InsertPlanetRun(chosenCharacter.Id, new PlanetRun
                {
                    Name = planetName,
                    RunDurationInHours = runTimeInHours,
                    StartTime = DateTime.Now,
                });
            }

            MainApplication.PIRunsMenu(_serviceProvider);
        }

        public void ModifyPlanetRun()
        {
            using var storage = new Storage();
            var chosenCharacter = _characterService.SelectionScreen(() => MainApplication.PIRunsMenu(_serviceProvider));
            var chosenPlanetRun = SelectionScreen(chosenCharacter.Id, () => MainApplication.PIRunsMenu(_serviceProvider));

            if (AnsiConsole.Confirm("Modify Name?"))
            {
                chosenPlanetRun.Name = AnsiConsole.Ask<string>("What is the new name?");
            }
            if (AnsiConsole.Confirm("Modify Hours?"))
            {
                chosenPlanetRun.RunDurationInHours = AnsiConsole.Ask<int>("What is the new amount of hours?");
            }

            AnsiConsole.WriteLine("");
            if (AnsiConsole.Confirm("Are you happy with these inputs?"))
            {
                storage.UpdatePlanetRun(chosenPlanetRun);
            }

            MainApplication.PIRunsMenu(_serviceProvider);
        }

        public void RemovePlanetRun()
        {
            using var storage = new Storage();
            var chosenCharacter = _characterService.SelectionScreen(() => MainApplication.PIRunsMenu(_serviceProvider));
            var chosenPlanetRun = SelectionScreen(chosenCharacter.Id, () => MainApplication.PIRunsMenu(_serviceProvider));

            AnsiConsole.WriteLine("");
            if (AnsiConsole.Confirm($"Are you sure you wish to delete {chosenCharacter.Name} his planet run on [red]{chosenPlanetRun.Name}[/]?"))
            {
                storage.DeletePlanetRun(chosenPlanetRun);
            }

            MainApplication.PIRunsMenu(_serviceProvider);
        }

        public void ArchiveRun()
        {
            using var storage = new Storage();
            var chosenCharacter = _characterService.SelectionScreen(() => MainApplication.PIRunsMenu(_serviceProvider));
            var chosenPlanetRun = SelectionScreen(chosenCharacter.Id, () => MainApplication.PIRunsMenu(_serviceProvider));

            AnsiConsole.WriteLine("");
            if (AnsiConsole.Confirm($"Are you sure you wish to archive {chosenCharacter.Name} his planet run on [red]{chosenPlanetRun.Name}[/]?"))
            {
                storage.ArchiveRun(chosenCharacter.Id, chosenPlanetRun);
            }

            MainApplication.PIRunsMenu(_serviceProvider);
        }

        public void AddTradeRecord()
        {
            using var storage = new Storage();
            var chosenCharacter = _characterService.SelectionScreen(() => MainApplication.PIRunsMenu(_serviceProvider));
            var chosenPlanetRun = SelectionScreen(chosenCharacter.Id, () => MainApplication.PIRunsMenu(_serviceProvider));

            var isExpenses = AnsiConsole.Confirm("Are these expenditures?");

            var table = new Table();
            table.AddColumns($"Product Name", "Quantity", "perItemPrice", "TotalAmount");
            table.Columns[1].RightAligned();
            table.Columns[2].RightAligned();
            table.Columns[3].RightAligned();
            AnsiConsole.MarkupLine($"Current Planet: [green]{chosenCharacter.Name}:{chosenPlanetRun.Name}[/]");
            AnsiConsole.Confirm($"Have you copied the expenses into your clipboard?");

            var trades = new ClipboardParser().ParseTrades(isExpenses);
            if(trades == null)
            {
                MainApplication.PIRunsMenu(_serviceProvider);
            }

            foreach (var expense in trades)
            {
                var markupColor = expense.TotalAmount < 0 ? "red" : "green";
                table.AddRow(new Markup[] { new Markup(expense.ProductName), new Markup(expense.Quantity.ToString("N")), new Markup($"[{markupColor}]{expense.PerItemPrice.ToString("N")}[/]"), new Markup($"[{markupColor}]{expense.TotalAmount.ToString("N")}[/]") });
            }
            AnsiConsole.Write(table);

            if (AnsiConsole.Confirm("[green]Is this correct?[/]"))
            {
                storage.InsertTradeRecordOnRun(chosenPlanetRun, trades);
            }
            MainApplication.PIRunsMenu(_serviceProvider);
        }

        public void RemoveTradeRecord()
        {
            using var storage = new Storage();
            var chosenCharacter = _characterService.SelectionScreen(() => MainApplication.PIRunsMenu(_serviceProvider));
            var chosenPlanetRun = SelectionScreen(chosenCharacter.Id, () => MainApplication.PIRunsMenu(_serviceProvider));
            var chosenTradeRecord = TradeRecordSelectionScreen(chosenPlanetRun.Id, () => MainApplication.PIRunsMenu(_serviceProvider));

            var table = new Table();
            table.AddColumns($"Product Name", "Quantity", "perItemPrice", "TotalAmount");
            table.Columns[1].RightAligned();
            table.Columns[2].RightAligned();
            table.Columns[3].RightAligned();

            var markupColor = chosenTradeRecord.TotalAmount < 0 ? "red" : "green";
            table.AddRow(new Markup[] { new Markup(chosenTradeRecord.ProductName), new Markup(chosenTradeRecord.Quantity.ToString("N")), new Markup($"[{markupColor}]{chosenTradeRecord.PerItemPrice.ToString("N")}[/]"), new Markup($"[{markupColor}]{chosenTradeRecord.TotalAmount.ToString("N")}[/]") });
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine("");

            if (AnsiConsole.Confirm("Do you wish to [red]delete[/] this record?"))
            {
                storage.DeleteTradeRecord(chosenTradeRecord);
            }
            MainApplication.PIRunsMenu(_serviceProvider);
        }

        public TradeRecord TradeRecordSelectionScreen(ObjectId planetRunId, Action returnAction)
        {
            using var storage = new Storage();
            var allCurrentRunning = storage.ListTradeRecordsForRun(planetRunId);
            var SelectionNames = allCurrentRunning.Select(x => $"{x.ProductName} x{x.Quantity} - {x.TotalAmount.ToString("N")}").ToList();
            SelectionNames.Add("<=== Back");

            var selection = AnsiConsole.Prompt(
              new SelectionPrompt<string>()
                  .Title("Which planetRun do you wish to select?")
                  .PageSize(10)
                  .MoreChoicesText("[grey](Move up and down to reveal more)[/]")
                  .AddChoices(SelectionNames));

            if (selection == "<=== Back")
                returnAction.Invoke();

            return storage.GetTradeRecordById(allCurrentRunning.First(x => $"{x.ProductName} x{x.Quantity} - {x.TotalAmount.ToString("N")}" == selection).Id);
        }

        public PlanetRun SelectionScreen(ObjectId characterId, Action returnAction)
        {
            using var storage = new Storage();
            var allCurrentRunning = storage.ListAllRunsOfCharacter(characterId);
            var SelectionNames = allCurrentRunning.Select(x => $"{x.Name}").ToList();
            SelectionNames.Add("<=== Back");

            var selection = AnsiConsole.Prompt(
              new SelectionPrompt<string>()
                  .Title("Which planetRun do you wish to select?")
                  .PageSize(10)
                  .MoreChoicesText("[grey](Move up and down to reveal more)[/]")
                  .AddChoices(SelectionNames));

            if (selection == "<=== Back")
                returnAction.Invoke();

            return storage.GetPlanetRunById(allCurrentRunning.First(x => x.Name == selection).Id);
        }
    }
}
