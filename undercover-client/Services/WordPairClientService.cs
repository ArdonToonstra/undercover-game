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
                // Try to load from YAML file
                var yamlContent = await _httpClient.GetStringAsync("WordPairs.yaml");
                var deserializer = new DeserializerBuilder().Build();
                var yamlData = deserializer.Deserialize<YamlWordPairData>(yamlContent);

                return yamlData.Categories.Select(yamlCategory => new WordPairCategory
                {
                    Name = yamlCategory.Name,
                    Description = yamlCategory.Description,
                    Pairs = yamlCategory.Pairs.Select(yamlPair => new WordPair
                    {
                        Civilian = yamlPair.Civilian,
                        Undercover = yamlPair.Undercover
                    }).ToList()
                }).ToList();
            }
            catch (Exception)
            {
                // Fallback to hardcoded categories if YAML loading fails
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
