using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net;
using Lucene;
using System.IO;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System.IO.Compression;

namespace LuceneBlazorWASM
{
    public sealed class LuceneIndexService
    {
        private static readonly LuceneIndexService instance = new LuceneIndexService();

        private LuceneIndexService()
        {
            var assembly = typeof(LuceneBlazorWASM.LuceneIndexService).Assembly;

            Stream resource = assembly.GetManifestResourceStream($"LuceneBlazorWASM.LuceneIndex.LuceneIndex.zip");
            Console.WriteLine("LuceneIndexService - Retrieved Stream");

            var indexPath = Path.Combine(Environment.CurrentDirectory, "LuceneIndex.zip");
            Console.WriteLine("LuceneIndexService - Retrieved Stream");

            var fileStream = File.Create(indexPath);
            Console.WriteLine("LuceneIndexService - Created file stream");

            resource.CopyTo(fileStream);
            Console.WriteLine("LuceneIndexService - Copied To Stream");

            ZipFile.ExtractToDirectory(indexPath, Environment.CurrentDirectory, true);
            Console.WriteLine("LuceneIndexService - Extracted index to dir");

            var zipDirectory = FSDirectory.Open(Environment.CurrentDirectory);
            Console.WriteLine("LuceneIndexService - Opened FSI Lucene Index Dir");

            this.IndexReader = DirectoryReader.Open(zipDirectory);
        }

        static LuceneIndexService()
        {
        }

        public static LuceneIndexService Instance
        {
            get
            {
                return instance;
            }
        }

        public DirectoryReader IndexReader
        {
            get;
            set;
        }

    }
}
