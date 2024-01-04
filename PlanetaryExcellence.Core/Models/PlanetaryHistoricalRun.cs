using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetaryExcellence.Core.Models
{
    public class PlanetaryHistoricalRun
    {
        public ObjectId Id { get; set; }

        public string PlanetName { get; set; }

        public decimal InitialInvestment { get; set; }

        public decimal SellPrice { get; set; }

        public string CharacterName { get; set; }

        public DateTime RunStart { get; set; }

        public DateTime RunEnd { get; set; }
    }
}
