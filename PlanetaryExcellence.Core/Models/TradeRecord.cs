using LiteDB;

namespace PlanetaryExcellence.Core.Models
{
    public class TradeRecord
    {
        public ObjectId Id { get; set; }

        public string ProductName { get; set; } 

        public decimal Quantity { get; set; }

        public decimal PerItemPrice { get; set; }   

        public decimal TotalAmount { get; set; }

        public bool IsExpense { get; set; }
    }
}