using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetaryExcellence.Core.Services
{
    public class ArchiveService
    {
        private IServiceProvider _serviceProvider;

        public ArchiveService(
            IServiceProvider provider) 
        {
            _serviceProvider = provider;  
        }

        public void Menu()
        {
            using var storage = new Storage();

            var allHistoricalRuns = storage.ListAllHistoricalRuns();
            var table = new Table()
            {
                ShowRowSeparators = true,
            };
            decimal totalRevenue = 0;
            table.AddColumns(
                new TableColumn($"CharacterName"),
                new TableColumn("Planet"),
                new TableColumn("Start run date") {  NoWrap = true },
                new TableColumn("End run date") { NoWrap = true },
                new TableColumn("Expenses"),
                new TableColumn("Total SellPrice"),
                new TableColumn("Revenue"));
            table.Columns[2].RightAligned();
            table.Columns[3].RightAligned();
            table.Columns[4].RightAligned();
            table.Columns[5].RightAligned();
            table.Columns[6].RightAligned();

            foreach(var run in allHistoricalRuns)
            {
                var revenue = (run.SellPrice - (run.InitialInvestment * -1));
                totalRevenue += revenue;
                table.AddRow(new Markup[]
                {
                    new Markup(run.CharacterName),
                    new Markup(run.PlanetName),
                    new Markup($"{run.RunStart.ToString("yyyy-MM-dd hh:mm:ss")}"),
                    new Markup($"{run.RunEnd.ToString("yyyy-MM-dd hh:mm:ss")}"),
                    new Markup($"{run.InitialInvestment.ToString("N")}"),
                    new Markup($"{run.SellPrice.ToString("N")}"),
                    new Markup($"{revenue.ToString("N")}"),
                });
            }

            var color = totalRevenue < 0 ? "red" : "green";
            table.Columns.Last().Footer = new Markup($"[{color}]{totalRevenue.ToString("N")}[/]");

            AnsiConsole.Write(table);
            AnsiConsole.Confirm("[green]Done?[/]");
            MainApplication.MainMenu(_serviceProvider);
        }
    }
}
