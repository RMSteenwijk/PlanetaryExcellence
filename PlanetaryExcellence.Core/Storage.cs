using LiteDB;
using PlanetaryExcellence.Core.Models;

namespace PlanetaryExcellence.Core
{
    public class Storage : IDisposable
    {

        private string _dbName = "FileName=planetaryexcellence.db; Connection=Shared;";

        private LiteDatabase _instance;

        public LiteDatabase Instance
        {
            get
            {
                if (_instance == null)
                    _instance = CreateInstanceStorageAccess();
                return _instance;
            }
        }

        public ILiteCollection<Character> Characters { get { return Instance.GetCollection<Character>("Characters"); } }
        public ILiteCollection<PlanetRun> PlanetRuns { get { return Instance.GetCollection<PlanetRun>("PlanetRuns"); } }
        public ILiteCollection<TradeRecord> TradeRecords { get { return Instance.GetCollection<TradeRecord>("TradeRecords"); } }
        public ILiteCollection<PlanetaryHistoricalRun> HistoricalRuns { get { return Instance.GetCollection<PlanetaryHistoricalRun>("HistoricalRuns"); } }

        public LiteDatabase CreateInstanceStorageAccess()
        {
            var mapper = BsonMapper.Global;

            mapper.Entity<Character>()
                .DbRef(x => x.CurrentRunningPlanets, "PlanetRuns");

            mapper.Entity<PlanetRun>()
                .DbRef(x => x.Trades, "TradeRecords");

            var db = new LiteDatabase(connectionString: _dbName);

            db.GetCollection<Character>("Characters");
            db.GetCollection<PlanetRun>("PlanetRuns");
            db.GetCollection<TradeRecord>("TradeRecords");
            db.GetCollection<PlanetaryHistoricalRun>("HistoricalRuns");

            db.GetCollection<PlanetaryHistoricalRun>("HistoricalRuns").EnsureIndex(x => x.CharacterName);

            return db;
        }

        public void InsertPlanetRun(ObjectId characterId, PlanetRun p)
        {
            p.Id = PlanetRuns.Insert(p);
            var character = Characters.FindById(characterId);
            character.CurrentRunningPlanets.Add(p);
            Characters.Update(character);
        }

        public void UpdatePlanetRun(PlanetRun planetRun)
        {
            PlanetRuns.Update(planetRun);
        }

        public void DeletePlanetRun(PlanetRun planetRun)
        {
            PlanetRuns.Delete(planetRun.Id);
        }

        public void InsertTradeRecordOnRun(PlanetRun planet, List<TradeRecord> expenses)
        {
            foreach (var tradeRecord in expenses)
            {
                tradeRecord.Id = TradeRecords.Insert(tradeRecord);
            }
            var run = PlanetRuns.FindById(planet.Id);
            run.Trades.AddRange(expenses);

            PlanetRuns.Update(run);
        }
        public void InsertCharacter(Character c)
        {
            Characters.Insert(c);
        }

        public void UpdateCharacter(Character character)
        {
            Characters.Update(character);
        }

        public void DeleteCharacter(Character character)
        {
            Characters.Delete(character.Id);
        }

        public List<PlanetRun> ListAllRunsOfCharacter(ObjectId characterId)
        {
            return Instance.GetCollection<Character>("Characters")
                .Include(c => c.CurrentRunningPlanets)
                .FindById(characterId)
                .CurrentRunningPlanets;
        }

        public PlanetRun GetPlanetRunById(ObjectId planetRunId)
        {
            return PlanetRuns
                .Include(c => c.Trades)
                .FindById(planetRunId);
        }

        public List<PlanetRun> ListAllRuns()
        {
            return Characters
                .Include(c => c.CurrentRunningPlanets)
                .FindAll()
                .ToList()
                .SelectMany(c => c.CurrentRunningPlanets)
                .ToList();
        }

        public List<Character> ListAllCharacters()
        {
            return Characters
                .Include(x => x.CurrentRunningPlanets)
                .FindAll()
                .ToList();
        }

        public List<TradeRecord> ListTradeRecordsForRun(ObjectId planetRunId)
        {
            return PlanetRuns
                .Include(p => p.Trades)
                .FindById(planetRunId)
                .Trades;
        }

        public List<PlanetaryHistoricalRun> ListAllHistoricalRuns()
        {
            return HistoricalRuns
                .FindAll()
                .ToList();
        }

        public List<PlanetaryHistoricalRun> ListAllHistoricalRunsForCharacterId(ObjectId characterId)
        {
            var character = Characters.FindById(characterId);
            if (character == null)
                return new List<PlanetaryHistoricalRun>();

            return HistoricalRuns
                .Find(ph => ph.CharacterName == character.Name)
                .ToList();
        }

        public void ArchiveRun(ObjectId characterId, PlanetRun planetRun)
        {
            var character = Characters.FindById(characterId);

            HistoricalRuns.Insert(new PlanetaryHistoricalRun
            {
                InitialInvestment = planetRun.Trades.Where(t => t.IsExpense)?.Sum(t => t.TotalAmount) ?? 0,
                SellPrice = planetRun.Trades.Where(t => !t.IsExpense)?.Sum(t => t.TotalAmount) ?? 0,
                PlanetName = planetRun.Name,
                RunStart = planetRun.StartTime,
                RunEnd = planetRun.StartTime.AddHours(planetRun.RunDurationInHours),
                CharacterName = character.Name
            });
            
            foreach(var t in planetRun.Trades)
            {
                TradeRecords.Delete(t.Id);
            }
            PlanetRuns.Delete(planetRun.Id);
        }

        public void Dispose()
        {
            if (_instance != null)
                _instance.Dispose();
        }
    }
}
