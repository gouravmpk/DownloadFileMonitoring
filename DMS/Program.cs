using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DownloadsFolderMonitor
{
    class Program
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        
        private static string apiKey = "Enter your api key ==";
        
        static async Task Main(string[] args)
        {
            string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            Console.WriteLine($"Monitoring Downloads folder: {downloadsPath}");

            using (FileSystemWatcher watcher = new FileSystemWatcher()) 
            {
                watcher.Path = downloadsPath;
                watcher.Filter = "*.pdf";
                watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime;

                watcher.Created += OnFileCreated;

                watcher.EnableRaisingEvents = true;

                Console.WriteLine("Press 'q' to quit the application.");
                while (Console.Read() != 'q');
            }
        }

        private static async void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                string fileName = Path.GetFileName(e.FullPath);
                string fileExtension = Path.GetExtension(e.FullPath);
                DateTime eventTime = DateTime.Now;

                Console.WriteLine($"File Created: {fileName}, Extension: {fileExtension}, Time: {eventTime}");

                
                var payload = new StringBuilder();
                payload.AppendLine("{ \"index\" : { \"_index\" : \"search-index-2025\" } }");//enter name if the index
                payload.AppendLine($"{{ \"User\": \"Gouravk\", \"FileName\": \"{fileName}\", \"Extension\": \"{fileExtension}\", \"EventTime\": \"{eventTime}\", \"FullPath\": \"{e.FullPath}\" }}");

                var content = new StringContent(payload.ToString(), Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"ApiKey {apiKey}");

                var response = await _httpClient.PostAsync("https://elasticIndexUrl:port/_bulk?pretty", content); //enter your elastic search index url 
               
                Console.WriteLine("Response :"+response.StatusCode);
                
                if (response.IsSuccessStatusCode)
                {  
                    Console.WriteLine("Log successfully sent to Elasticsearch.");
                }
                else
                {
                    Console.WriteLine($"Failed to send log to Elasticsearch. Status Code: {response.StatusCode}, Error: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling file creation: {ex.Message}");
            }
        }
    }
}
