using YamlDotNet.Serialization;

namespace RoamingRoutes.Client.Services
{
    public class YamlWordPairData
    {
        [YamlMember(Alias = "categories")]
        public List<YamlWordPairCategory> Categories { get; set; } = new();
    }

    public class YamlWordPairCategory
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; } = "";
        
        [YamlMember(Alias = "description")]
        public string Description { get; set; } = "";
        
        [YamlMember(Alias = "pairs")]
        public List<YamlWordPair> Pairs { get; set; } = new();
    }

    public class YamlWordPair
    {
        [YamlMember(Alias = "wordA")]
        public string WordA { get; set; } = "";
        
        [YamlMember(Alias = "wordB")]
        public string WordB { get; set; } = "";
    }
}
