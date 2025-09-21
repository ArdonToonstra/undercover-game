using RoamingRoutes.Shared.Models.Games;
using YamlDotNet.Serialization;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace RoamingRoutes.Client.Services
{
    public interface IWordPairClientService
    {
        Task<List<WordPairCategory>> GetCategoriesAsync(string? languageCode = null);
        Task<List<string>> GetAvailableLanguagesAsync();
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

        public async Task<List<WordPairCategory>> GetCategoriesAsync(string? languageCode = null)
        {
            // Skip API call since we're using local YAML files
            return await GetDefaultCategoriesAsync();
        }

        public async Task<List<string>> GetAvailableLanguagesAsync()
        {
            try
            {
                // Get all categories to extract unique languages
                var allCategories = await GetDefaultCategoriesAsync(); // Load from YAML without filtering
                var languages = allCategories
                    .Where(c => !string.IsNullOrWhiteSpace(c.Language))
                    .Select(c => c.Language)
                    .Distinct()
                    .OrderBy(lang => lang)
                    .ToList();
                
                return languages.Any() ? languages : new List<string> { "EN" };
            }
            catch (Exception)
            {
                return new List<string> { "EN" };
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

                        yamlContent = await _httpClient.GetStringAsync(requestUri.ToString());
                        if (!string.IsNullOrWhiteSpace(yamlContent))
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                    }
                }

                if (string.IsNullOrWhiteSpace(yamlContent))
                {
                    try
                    {
                        var localPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "WordPairs.yaml");
                        if (File.Exists(localPath))
                        {
                            yamlContent = await File.ReadAllTextAsync(localPath);
                        }
                    }
                    catch (Exception)
                    {
                        // Continue to fallback
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

                var rng = new Random();
                var allCategories = yamlData.Categories.Select(yamlCategory => new WordPairCategory
                {
                    Name = yamlCategory.Name,
                    Description = yamlCategory.Description,
                    Language = yamlCategory.Language,
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

                return allCategories;
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
                        Language = "EN",
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
