using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Utils;

namespace LootNormalizer;

[Injectable(TypePriority = OnLoadOrder.PostLoad + 3)]
public class LootNormalizer(LocationTable locationTable, 
    ModHelper modHelper,
    JsonUtil jsonUtil,
    ISptLogger<LootNormalizer> logger) : IOnLoad
{
    public readonly string ConfigPath = Path.Join(modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()), "config.json");
    public ModConfig ModConfig = null!;
    
    public async Task OnLoadAsync(CancellationToken cancellationToken)
    {
        ModConfig = await jsonUtil.DeserializeFromFileAsync<ModConfig>(ConfigPath, cancellationToken) ?? new ModConfig();

        if (ModConfig.LogBase <= 1)
        {
            logger.Warning("[LootNormalizer] Invalid Log Base used. Use a value above 1.");
            ModConfig.LogBase = 1.15;
        }
        
        await File.WriteAllTextAsync(ConfigPath, jsonUtil.Serialize(ModConfig, true), cancellationToken);

        Dictionary<string, Location> locationMap = locationTable.GetDictionary();
        
        foreach ((string locationId, Location location) in locationMap)
        {
            if (ModConfig.LocationBlacklist.Contains(locationId))
                continue;
            
            location.LooseLoot?.AddTransformer(lootTransformer =>
            {
                AdjustLooseLoot(lootTransformer);

                return lootTransformer;
            });
        }
    }

    private void AdjustLooseLoot(LooseLoot? looseLoot)
    {
        if (looseLoot == null || looseLoot.Spawnpoints == null)
            return;
        
        foreach (Spawnpoint lootSpawn in looseLoot.Spawnpoints)
        {
            if (lootSpawn.ItemDistribution == null)
                continue;
            
            foreach (LooseLootItemDistribution itemDistribution in lootSpawn.ItemDistribution)
            {
                if (itemDistribution.RelativeProbability < ModConfig.MinProbability || 
                    itemDistribution.RelativeProbability == null)
                    continue;

                itemDistribution.RelativeProbability =
                    Math.Max(Math.Round(Math.Log((itemDistribution.RelativeProbability ?? 0) + 1, ModConfig.LogBase)), 1);
            }
        }
        
        logger.Success("[LootNormalizer] Successfully normalized item relative probabilities.");
    }
}

public record ModConfig
{
    [JsonPropertyName("logBase")] public double LogBase { get; set; } = 1.5;
    [JsonPropertyName("minProbability")] public int MinProbability { get; set; } = 0;
    [JsonPropertyName("locationBlacklist")] public List<string> LocationBlacklist { get; set; } = [];
}