namespace RoamingRoutes.Shared.Models.Games
{
    public class WordPair
    {
        public string Civilian { get; set; } = string.Empty;
        public string Undercover { get; set; } = string.Empty;
    }

    public class WordPairCategory
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Language { get; set; } = "EN";
        public List<WordPair> Pairs { get; set; } = new List<WordPair>();
    }

    public class WordPairConfiguration
    {
        public List<WordPairCategory> Categories { get; set; } = new List<WordPairCategory>();
    }
}
