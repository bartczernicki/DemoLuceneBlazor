using System;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace LuceneBlazorWASM;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        // Add Lucene Index service (load Lucene Index from zip file and expose Directory Reader)
        builder.Services.AddSingleton<LuceneIndexService>(LuceneIndexService.Instance);

        await builder.Build().RunAsync();
    }
}