using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetaryExcellence.Core.Models
{
    public class PlanetRun
    {
        public ObjectId Id { get; set; }

        public string Name { get; set; }

        public DateTime StartTime { get; set; } 

        public int RunDurationInHours { get; set; }

        public List<TradeRecord> Trades { get; set; } = new();
    }
}
