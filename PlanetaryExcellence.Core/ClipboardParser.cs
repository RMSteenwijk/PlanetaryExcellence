using PlanetaryExcellence.Core.Models;
using SoftCircuits.Parsing.Helper;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetaryExcellence.Core
{
    public class ClipboardParser
    {
        public List<TradeRecord> ParseTrades(bool isExpenses)
        {
            var clipboardText = TextCopy.ClipboardService.GetText();
            var parser = new ParsingHelper(clipboardText);
            var expenses = new List<TradeRecord>();

            //Eve wallet transactions have the following order
            //Date | quantity | productName | itemPrice | totalAmount | buyer/seller | location

            //Janice transactions have the following order
            //productName | quantity | unknown | BuyOrderPerItem | SellOrderPerItem

            var columns = parser.ParseTokens('\t', '\n').ToList();

            if (columns.Count == 0)
            {
                AnsiConsole.WriteException(new NullReferenceException("Clipboard empty or invalid."));
                Thread.Sleep(1000);
                return null;
            }

            var singleRowColumnCount = clipboardText.Substring(0, clipboardText.IndexOf('\n')).Count(c => c == '\t') + 1;
            var isEveWallet = singleRowColumnCount == 7;

            var rowAmount = columns.Count / singleRowColumnCount;

            var Chunks = columns.Select((x, i) => new { index = i, value = x })
                .GroupBy(x => x.index / singleRowColumnCount)
                .Select(x => x.Select(v => v.value).ToList())
                .ToList();

            foreach (var row in Chunks)
            {
                var expense = new TradeRecord();
                if (isEveWallet)
                {
                    var quantity = decimal.Parse(row[1]);
                    var productName = row[2];
                    var perItemPrice = decimal.Parse(row[3].Substring(0, row[3].IndexOf("ISK")).Trim());
                    var totalAmount = decimal.Parse(row[4].Substring(0, row[4].IndexOf("ISK")).Trim());

                    expenses.Add(new TradeRecord
                    {
                        Quantity = quantity,
                        PerItemPrice = perItemPrice,
                        TotalAmount = totalAmount,
                        ProductName = productName,
                        IsExpense = (totalAmount < 0)
                    });
                }
                else
                {
                    var productName = row[0];
                    var quantity = decimal.Parse(row[1]);
                    var perItemPrice = decimal.Parse(row[4].Replace('.', ','));
                    var totalAmount = perItemPrice * decimal.Parse(row[1]);

                    expenses.Add(new TradeRecord
                    {
                        Quantity = quantity,
                        PerItemPrice = (isExpenses) ? perItemPrice * -1 : perItemPrice,
                        TotalAmount = (isExpenses) ? totalAmount * -1 : totalAmount,
                        ProductName = productName,
                        IsExpense = isExpenses
                    });
                }
            }
            return expenses;
        }
    }
}
