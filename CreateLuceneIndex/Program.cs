using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
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
            var indexLocation = @"D:\Data\TestLucene";

            Console.WriteLine("LUCENE DEMO - Start Creating Index");

            var dataStream = GetBaseballData();

            var lines = ReadLines(() => dataStream, Encoding.UTF8);

            // Skip the first header line
            var batters = lines
                        .Skip(1)
                        .Select(v => MLBBaseballBatter.FromCsv(v));

            // LUCENE - CREATE THE INDEX
            var AppLuceneVersion = LuceneVersion.LUCENE_48;

            var dir = FSDirectory.Open(indexLocation);

            var test = Environment.CurrentDirectory;

            //create an analyzer to process the text
            var analyzer = new StandardAnalyzer(AppLuceneVersion);

            //create an index writer
            var indexConfig = new IndexWriterConfig(AppLuceneVersion, analyzer);
            indexConfig.OpenMode = OpenMode.CREATE;

            var writer = new IndexWriter(dir, indexConfig);

            foreach(var batter in batters)
            {
                Document doc = new Document
                {
                    // StringField indexes but doesn't tokenize
                    new StringField("Id",
                        batter.ID,
                        Field.Store.YES),
                    new TextField("FullPlayerName",
                        batter.FullPlayerName,
                        Field.Store.YES),
                    new StringField("InductedToHallOfFame",
                        batter.InductedToHallOfFame.ToString(),
                        Field.Store.YES),
                    new StringField("OnHallOfFameBallot",
                        batter.OnHallOfFameBallot.ToString(),
                        Field.Store.YES),
                    new StoredField("YearsPlayed",
                        batter.YearsPlayed),
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
                    new StoredField("MVPs",
                        batter.MVPs),
                    new StoredField("TripleCrowns",
                        batter.TripleCrowns),
                    new StoredField("GoldGloves",
                        batter.GoldGloves),
                    new StoredField("MajorLeaguePlayerOfTheYearAwards",
                        batter.MajorLeaguePlayerOfTheYearAwards),
                    new StoredField("TB",
                        batter.TB),
                    new StoredField("TotalPlayerAwards",
                        batter.TotalPlayerAwards),
                    new SingleField("LastYearPlayed",
                        batter.LastYearPlayed, Field.Store.YES)
                };

                // Console.WriteLine("Added: " + batter.ToString());
                writer.AddDocument(doc);
            }

            writer.Flush(triggerMerge: true, applyAllDeletes: false);
            writer.Commit();

            var numberDocs = writer.NumDocs;
            Console.WriteLine("Number of Docs Written: " + numberDocs);

            // Close the index writer
            writer.Dispose();
            Console.WriteLine("LUCENE DEMO - Index Created");

            // LUCENE - PACKAGE THE INDEX AS ZIP FILE
            var packagePath = Path.Combine(@"D:\Data\LuceneIndex\", "LuceneIndex.zip");
            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }

            ZipFile.CreateFromDirectory(indexLocation, packagePath, CompressionLevel.Optimal, false);
            Console.WriteLine("LUCENE DEMO - Index Packaged (Zip)");

            // LUCENE - TEST THE INDEX
            // Load the index from Zip file (mimic it loading)
            Console.WriteLine("LUCENE DEMO - Text the Index from Packaged (Zip)");

            ZipFile.ExtractToDirectory(packagePath, Environment.CurrentDirectory, true);
            var zipDirectory = FSDirectory.Open(Environment.CurrentDirectory);

            var indexReader = DirectoryReader.Open(zipDirectory);
            var searcher = new IndexSearcher(indexReader);

            QueryParser parser = new QueryParser(AppLuceneVersion,"FullPlayerName", analyzer);
            var query = parser.Parse("Trout");

            var searchResults = searcher.Search(query, 10000);// 20 /* top 20 */);
            var hits = searchResults.ScoreDocs;
            foreach (var hit in hits)
            {
                var foundDoc = searcher.Doc(hit.Doc);
                var name = foundDoc.GetField("FullPlayerName").GetStringValue();
                var yearsPlayed = foundDoc.GetField("YearsPlayed").GetSingleValue();
                var explanation = searcher.Explain(query, hit.Doc);

                Console.WriteLine("Found: " + name + " - " + hit.Score);
                Console.WriteLine("Explanation: " + explanation.ToString());

                var score = hit.Score;
            }



            searcher = new IndexSearcher(indexReader);

            var queryRanged = NumericRangeQuery.NewSingleRange("LastYearPlayed", 1990, 2000, true, true);



            var searchResultsRanged = searcher.Search(queryRanged, 100000);// 20 /* top 20 */);
            var hitsRanged = searchResultsRanged.ScoreDocs;
            foreach (var hit in hitsRanged)
            {
                var foundDoc = searcher.Doc(hit.Doc);

                var score = hit.Score;
            }
        }

        public static Stream GetBaseballData()
        {
            var assembly = typeof(CreateLuceneIndex.Program).Assembly;

            var test = assembly.GetManifestResourceNames();
            // taskkill /IM dotnet.exe /F /T 2>nul 1>nul
                                                            
            Stream resource = assembly.GetManifestResourceStream($"CreateLuceneIndex.Data.MLBBaseballBattersHistorical.csv");

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
