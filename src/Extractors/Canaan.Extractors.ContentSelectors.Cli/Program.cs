using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Canaan.Extractors.ContentSelectors.Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var urls = File.ReadAllLines("missing-content-selectors.txt");
            HttpClient client = new HttpClient();
            for(int i = 0; i < urls.Length - 1; i++)
            {
                string html = await client.GetStringAsync(urls[i]);

            }
        }
    }
}
