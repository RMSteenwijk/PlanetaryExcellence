using PlanetaryExcellence.Core.Models;
using Spectre.Console;

namespace PlanetaryExcellence.Core.Services
{
    public class CharacterService
    {
        private IServiceProvider _serviceProvider;

        public CharacterService(
            IServiceProvider provider)
        {
            _serviceProvider = provider;
        }

        public void DisplayCurrentCharacters()
        {
            using var storage = new Storage();
            var table = new Table();
            table.AddColumns($"Id", "Name", "Current Running Planets", "Total run count");
            table.Columns[3].RightAligned();

            foreach (var character in storage.ListAllCharacters())
            {
                var charHistoricalRuns = storage.ListAllHistoricalRunsForCharacterId(character.Id);
                var planets = string.Join("\n", character.CurrentRunningPlanets.Select(p => p.Name));
                var totalRunCount = charHistoricalRuns.Count() + character.CurrentRunningPlanets.Count();
                table.AddRow(new string[] { $"{character.Id}", character.Name, planets, totalRunCount.ToString() });
            }

            AnsiConsole.Write(table);
        }
        public void AddCharacter()
        {
            using var storage = new Storage();
            var characterName = AnsiConsole.Ask<string>("What is the character name?");

            if (characterName.Length < 3)
            {
                AnsiConsole.Markup("[red] Too short of a character name.[/]");
                Thread.Sleep(1000);

                MainApplication.CharactersMenu(_serviceProvider);
            }


            storage.InsertCharacter(new Character
            {
                Name = characterName,
            });
            MainApplication.CharactersMenu(_serviceProvider);
        }

        public void ModifyCharacter()
        {
            using var storage = new Storage();
            var chosenCharacter = SelectionScreen(returnAction: () => MainApplication.CharactersMenu(_serviceProvider));

            var newName = AnsiConsole.Ask<string>("What is the new character name?");

            chosenCharacter.Name = newName;
            storage.UpdateCharacter(chosenCharacter);

            MainApplication.CharactersMenu(_serviceProvider);
        }

        public void RemoveCharacter()
        {
            using var storage = new Storage();
            var chosenCharacter = SelectionScreen(returnAction: () => MainApplication.CharactersMenu(_serviceProvider));

            storage.DeleteCharacter(chosenCharacter);

            MainApplication.CharactersMenu(_serviceProvider);
        }

        public Character SelectionScreen(Action returnAction)
        {
            using var storage = new Storage();
            var allCurrentRunning = storage.ListAllCharacters();
            var SelectionNames = allCurrentRunning.Select(x => $"{x.Name}").ToList();
            SelectionNames.Add("<=== Back");

            var selection = AnsiConsole.Prompt(
              new SelectionPrompt<string>()
                  .Title("Which character do you wish to select?")
                  .PageSize(10)
                  .MoreChoicesText("[grey](Move up and down to reveal more operations)[/]")
                  .AddChoices(SelectionNames));

            if (selection == "<=== Back")
                returnAction.Invoke();

            return allCurrentRunning.First(x => x.Name == selection);
        }
    }
}
