using SPTarkov.Server.Core.Models.Spt.Mod;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace LootNormalizer;

public static class Metadata
{
    public record ModMetadata : IModMetadata
    {
        public string ModGuid { get; init; } = "com.minesettimi.lootnormalizer";
        public string Name { get; init; } = "Loot Normalizer";
        public string Author { get; init; } = "minesettimi";
        public List<string>? Contributors { get; init; }

        public Version Version { get; init; } = new(1, 0, 0);
        public Range SptVersion { get; init; } = new("~4.1.5");

        public bool HasPrepatcher { get; init; } = false;
        
        public List<string>? Incompatibilities { get; init; }
        public Dictionary<string, Range>? ModDependencies { get; init; } = new();

        public string? Url { get; init; } = "https://github.com/minesettimi/LootNormalizer";
        public string License { get; init; } = "MIT";
    }
}