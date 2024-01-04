using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetaryExcellence.Core.Models
{
    public class Character
    {
        public ObjectId Id { get; set; } 

        public string Name { get; set; }

        [BsonRef("PlanetRuns")]
        public List<PlanetRun> CurrentRunningPlanets { get; set; } = new();
    }
}
