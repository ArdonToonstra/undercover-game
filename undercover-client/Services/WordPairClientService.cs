using RoamingRoutes.Shared.Models.Games;
using YamlDotNet.Serialization;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace RoamingRoutes.Client.Services
{
    public interface IWordPairClientService
    {
        Task<List<WordPairCategory>> GetCategoriesAsync();
        Task<WordPair> GetRandomPairFromCategoryAsync(string categoryName);
    }

    public class WordPairClientService : IWordPairClientService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public WordPairClientService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task<List<WordPairCategory>> GetCategoriesAsync()
        {
            try
            {
                var categories = await _httpClient.GetFromJsonAsync<List<WordPairCategory>>("/api/wordpairs/categories");
                return categories ?? await GetDefaultCategoriesAsync();
            }
            catch (Exception)
            {
                // Return default categories if API call fails
                return await GetDefaultCategoriesAsync();
            }
        }

        public async Task<WordPair> GetRandomPairFromCategoryAsync(string categoryName)
        {
            try
            {
                var pair = await _httpClient.GetFromJsonAsync<WordPair>($"/api/wordpairs/categories/{Uri.EscapeDataString(categoryName)}/random");
                return pair ?? GetDefaultPair();
            }
            catch (Exception)
            {
                // Return default pair if API call fails
                return GetDefaultPair();
            }
        }

        private async Task<List<WordPairCategory>> GetDefaultCategoriesAsync()
        {
            try
            {
                // Try several likely paths for the static YAML file. When Blazor is hosted
                // the wwwroot contents are served from the app base address, so prefer
                // building URIs off HttpClient.BaseAddress when available.
                Console.WriteLine("Attempting to load WordPairs.yaml...");

                var candidatePaths = new List<string>
                {
                    "WordPairs.yaml",
                    "/WordPairs.yaml",
                    "./WordPairs.yaml",
                    "wwwroot/WordPairs.yaml"
                };

                string? yamlContent = null;
                Exception? lastException = null;

                foreach (var p in candidatePaths)
                {
                    try
                    {
                        Uri requestUri;
                        if (_httpClient.BaseAddress != null)
                        {
                            requestUri = new Uri(_httpClient.BaseAddress, p);
                        }
                        else
                        {
                            requestUri = new Uri(p, UriKind.RelativeOrAbsolute);
                        }

                        Console.WriteLine($"Trying YAML path: {requestUri}");
                        yamlContent = await _httpClient.GetStringAsync(requestUri.ToString());
                        if (!string.IsNullOrWhiteSpace(yamlContent))
                        {
                            Console.WriteLine($"YAML content loaded from {requestUri}, length: {yamlContent.Length}");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        Console.WriteLine($"Failed to load YAML from path '{p}': {ex.Message}");
                    }
                }

                if (string.IsNullOrWhiteSpace(yamlContent))
                {
                    // As a last-resort fallback (useful for `dotnet run` dev), try to read
                    // the file from the local file system under the project wwwroot folder.
                    try
                    {
                        var localPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "WordPairs.yaml");
                        Console.WriteLine($"HTTP attempts failed — trying local file path: {localPath}");
                        if (File.Exists(localPath))
                        {
                            yamlContent = await File.ReadAllTextAsync(localPath);
                            Console.WriteLine($"Loaded YAML from local file, length: {yamlContent.Length}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Local file fallback failed: {ex.Message}");
                    }

                    if (string.IsNullOrWhiteSpace(yamlContent))
                    {
                        throw lastException ?? new InvalidOperationException("Could not load WordPairs.yaml from any candidate path.");
                    }
                }

                var deserializer = new DeserializerBuilder().Build();
                var yamlData = deserializer.Deserialize<YamlWordPairData>(yamlContent);

                if (yamlData == null || yamlData.Categories == null)
                {
                    throw new InvalidOperationException("YAML file parsed but returned no categories.");
                }

                Console.WriteLine($"YAML parsed successfully, found {yamlData.Categories.Count} categories");

                // Debug: Log each category name
                foreach (var category in yamlData.Categories)
                {
                    Console.WriteLine($"Category: '{category.Name}' with {category.Pairs.Count} pairs");
                }

                var rng = new Random();
                return yamlData.Categories.Select(yamlCategory => new WordPairCategory
                {
                    Name = yamlCategory.Name,
                    Description = yamlCategory.Description,
                    Pairs = yamlCategory.Pairs.Select(yamlPair => 
                    {
                        // Randomly assign wordA and wordB to civilian and undercover roles
                        var assignWordATocivilian = rng.Next(2) == 0;
                        
                        return new WordPair
                        {
                            Civilian = assignWordATocivilian ? yamlPair.WordA : yamlPair.WordB,
                            Undercover = assignWordATocivilian ? yamlPair.WordB : yamlPair.WordA
                        };
                    }).ToList()
                }).ToList();
            }
            catch (Exception ex)
            {
                // Fallback to hardcoded categories if YAML loading fails
                Console.WriteLine($"Failed to load YAML: {ex.Message}");
                return new List<WordPairCategory>
                {
                    new WordPairCategory
                    {
                        Name = "Everyday Words",
                        Description = "Common everyday objects and concepts",
                        Pairs = new List<WordPair>
                        {
                            new WordPair { Civilian = "Coffee", Undercover = "Tea" },
                            new WordPair { Civilian = "Dog", Undercover = "Cat" },
                            new WordPair { Civilian = "Car", Undercover = "Bus" }
                        }
                    }
                };
            }
        }

        private WordPair GetDefaultPair()
        {
            return new WordPair { Civilian = "Coffee", Undercover = "Tea" };
        }
    }
}
