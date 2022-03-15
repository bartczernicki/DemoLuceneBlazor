using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using LuceneBlazorWASM.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace CreateLuceneIndex
{
    class Program
    {
        static void Main(string[] args)
        {
            var startTime = DateTime.UtcNow;
            Console.WriteLine("LUCENE CREATE INDEX - Start");
            
            // Create Lucene Index Location
            var indexLocation = Path.Combine(Environment.CurrentDirectory, "LuceneIndex");
            var indexZipLocation = Path.Combine(Environment.CurrentDirectory, "LuceneIndexZip");

            if (!System.IO.Directory.Exists(indexLocation))
            {
                System.IO.Directory.CreateDirectory(indexLocation);
            }

            if (!System.IO.Directory.Exists(indexZipLocation))
            {
                System.IO.Directory.CreateDirectory(indexZipLocation);
            }

            var dataStream = GetBaseballData();

            var lines = ReadLines(() => dataStream, Encoding.UTF8);

            // Skip the first header line
            var batters = lines
                        .Skip(1)
                        .Select(v => MLBBaseballBatter.FromCsv(v))
                        .ToList();


            // LUCENE - CREATE THE INDEX
            var AppLuceneVersion = LuceneVersion.LUCENE_48;

            var dir = FSDirectory.Open(indexLocation);

            //create an analyzer to process the text
            var analyzer = new StandardAnalyzer(AppLuceneVersion);

            //create an index writer
            var indexConfig = new IndexWriterConfig(AppLuceneVersion, analyzer);
            indexConfig.OpenMode = OpenMode.CREATE;

            var writer = new IndexWriter(dir, indexConfig);

            // Get max Years Played for each batter
            var battersMaxYearsPlayed = from b in batters
                                        group b by b.ID into g
                                        select new MLBBaseballBatter { ID = g.Key, YearsPlayed = g.Max(b => b.YearsPlayed) };

            Console.WriteLine("LUCENE CREATE INDEX - Iterating Data for Index");

            foreach (var batter in batters)
            {
                var isBatterMaxYearsRecord = (from batterMax in battersMaxYearsPlayed
                                              where ((batterMax.ID == batter.ID) && (batterMax.YearsPlayed == batter.YearsPlayed))
                                              select new { ID = batterMax.ID }).Count();

                Document doc = new Document
                {
                    // Field names map to the MLBBaseballPlayer.cs class 
                    // that is used in ML.NET models, demos etc.

                    // StringField indexes but doesn't tokenize
                    new StringField("Id",
                        batter.ID,
                        Field.Store.YES),
                    new Int32Field("IsBatterMaxYearsRecord",
                        isBatterMaxYearsRecord, Field.Store.YES),
                    new TextField("FullPlayerName",
                        batter.FullPlayerName,
                        Field.Store.YES),
                    new StringField("InductedToHallOfFame",
                        batter.InductedToHallOfFame.ToString(),
                        Field.Store.YES),
                    new StringField("OnHallOfFameBallot",
                        batter.OnHallOfFameBallot.ToString(),
                        Field.Store.YES),
                    new SingleField("YearsPlayed",
                        batter.YearsPlayed, Field.Store.YES),
                    // Use StoredField to minimize index storage, since these fields won't be searched on
                    new StoredField("AB",
                        batter.AB),
                    new StoredField("R",
                        batter.R),
                    new StoredField("H",
                        batter.H),
                    new StoredField("Doubles",
                        batter.Doubles),
                    new StoredField("Triples",
                        batter.Triples),
                    new StoredField("HR",
                        batter.HR),
                    new StoredField("RBI",
                        batter.RBI),
                    new StoredField("SB",
                        batter.SB),
                    new StoredField("BattingAverage",
                        batter.BattingAverage),
                    new StoredField("SluggingPct",
                        batter.SluggingPct),
                    new StoredField("AllStarAppearances",
                        batter.AllStarAppearances),
                    //new StoredField("MVPs",
                    //    batter.MVPs),
                    //new StoredField("TripleCrowns",
                    //    batter.TripleCrowns),
                    //new StoredField("GoldGloves",
                    //    batter.GoldGloves),
                    //new StoredField("MajorLeaguePlayerOfTheYearAwards",
                    //    batter.MajorLeaguePlayerOfTheYearAwards),
                    new StoredField("TB",
                        batter.TB),
                    new StoredField("TotalPlayerAwards",
                        batter.TotalPlayerAwards),
                    new StoredField("LastYearPlayed",
                        batter.LastYearPlayed)
                };

                // Console.WriteLine("Added: " + batter.ToString());
                writer.AddDocument(doc);
            }

            writer.Flush(triggerMerge: true, applyAllDeletes: false);
            writer.Commit();

            var numberDocs = writer.NumDocs;
            Console.WriteLine("LUCENE CREATE INDEX - Number of Docs Written to Index: " + numberDocs);

            // Close the index writer
            writer.Dispose();
            Console.WriteLine("LUCENE CREATE INDEX - Index Created");

            // LUCENE - PACKAGE THE INDEX AS ZIP FILE
            var packagePath = Path.Combine(indexZipLocation, "LuceneIndex.zip");

            // Delete the Zip file before proceeding
            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }

            ZipFile.CreateFromDirectory(indexLocation, packagePath, CompressionLevel.Optimal, false);
            Console.WriteLine("LUCENE CREATE INDEX - Index Packaged (Zip)");

            // LUCENE - TEST THE INDEX
            // Load the index from Zip file (mimic it loading)
            Console.WriteLine("LUCENE CREATE INDEX - Text the Index from Packaged (Zip)");

            ZipFile.ExtractToDirectory(packagePath, Environment.CurrentDirectory, true);
            var zipDirectory = FSDirectory.Open(Environment.CurrentDirectory);

            var endTime = DateTime.UtcNow;
            TimeSpan timeDiff = endTime - startTime;
            Console.WriteLine("LUCENE CREATE INDEX - Time (seconds) taken to create index: " + Convert.ToInt32(timeDiff.TotalSeconds));

            var indexReader = DirectoryReader.Open(zipDirectory);
            var searcher = new IndexSearcher(indexReader);

            // Simple Query
            QueryParser parser = new QueryParser(AppLuceneVersion,"FullPlayerName", analyzer);
            var query = parser.Parse("Todd");
            var searchResults = searcher.Search(query, 500);// 20 /* top 20 */);
            var hits = searchResults.ScoreDocs;
            Console.WriteLine("LUCENE CREATE INDEX - Search for 'Todd': " + hits.Length);

            //foreach (var hit in hits)
            //{
            //    var foundDoc = searcher.Doc(hit.Doc);
            //    var name = foundDoc.GetField("FullPlayerName").GetStringValue();
            //    var yearsPlayed = foundDoc.GetField("YearsPlayed").GetSingleValue();
            //    var explanation = searcher.Explain(query, hit.Doc);

            //    Console.WriteLine("Found: " + name + " - " + hit.Score);
            //    Console.WriteLine("Explanation: " + explanation.ToString());

            //    var score = hit.Score;
            //}


            // Simple Query- With Filter
            var queryRanged = NumericRangeQuery.NewInt32Range("IsBatterMaxYearsRecord", 1, 1, true, true);

            BooleanQuery andQuery = new BooleanQuery();
            andQuery.Add(query, Occur.MUST);
            andQuery.Add(queryRanged, Occur.MUST);

            var searchResultsWithFilter = searcher.Search(andQuery, 500); /* top 500 */;
            var hitsWithFilter = searchResultsWithFilter.ScoreDocs;
            Console.WriteLine("LUCENE CREATE INDEX - Search for 'Todd' with Max Years Filter: " + hitsWithFilter.Length);

            //foreach (var hit in hitsWithFilter)
            //{
            //    var foundDoc = searcher.Doc(hit.Doc);
            //    var name = foundDoc.GetField("FullPlayerName").GetStringValue();
            //    var isBatterMaxYearsRecord = foundDoc.GetField("IsBatterMaxYearsRecord").GetInt32Value();
            //    var explanation = searcher.Explain(query, hit.Doc);

            //    Console.WriteLine("Found: " + name + " - " + hit.Score);
            //    Console.WriteLine("Explanation: " + explanation.ToString());

            //    var score = hit.Score;
            //}

            // Query For Id
            var termHankAaron = new Term("Id", "aaronha01");
            var termQuery = new TermQuery(termHankAaron);

            var searchResultsTermQuery = searcher.Search(termQuery, 50); /* top 50 */;
            var hitsTermQuery = searchResultsTermQuery.ScoreDocs;
            Console.WriteLine("LUCENE CREATE INDEX - Search for 'Id = aaronha01': " + hitsTermQuery.Length);

        }

        public static Stream GetBaseballData()
        {
            var assembly = typeof(CreateLuceneIndex.Program).Assembly;

            var test = assembly.GetManifestResourceNames();
            // taskkill /IM dotnet.exe /F /T 2>nul 1>nul
                                                            
            Stream resource = assembly.GetManifestResourceStream($"CreateLuceneIndex.Data.MLBBaseballBattersHistoricalPositionPlayers.csv");

            return resource;
        }

        public static IEnumerable<string> ReadLines(Func<Stream> streamProvider, Encoding encoding)
        {
            using (var stream = streamProvider())
            using (var reader = new StreamReader(stream, encoding))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }
    }
}
