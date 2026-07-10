Planned Evolution (v2.3.3)Planned Evolution is an open-source runtime modification utility and state manager for Risk of Rain 2. Developed in C# using the BepInEx framework, the plugin intercepts native game loops to overhaul the random procedural assignment of the "Artifact of Evolution" mechanic, replacing it with a deterministic, user-configured progression model.Technical OverviewThe plugin executes runtime hooking, custom string-token parsing, dynamic memory reflection, and inventory state rebuilds to control how the engine assigns items to enemy characters as gameplay progresses.Key Implementation DetailsReflection-Based Field Access: Because the native game manager restricts access to the static monster team inventory field (monsterTeamInventory), the plugin uses reflection with non-public static binding flags to safely reference the active Inventory instance at runtime.Custom Configuration Parser: Parses complex configuration structures formatted as sequential string coordinates (ItemName, Count; ItemName, Count) and translates them into engine-readable item indices.State Caching and Filtering: Compiles an offline blacklist cache on startup to filter potential item drop pools on the fly, preventing invalid or game-breaking items from being processed by random selection algorithms.Architectural Case Study: Resolving the State Synchronization SoftlockThe ChallengeDuring early development, attempts to perform live-mutation updates on the enemy inventory mid-tick caused severe race conditions. Modifying active item tables while the engine thread was traversing them resulted in an immediate thread block, causing the game to softlock permanently.Initial debugging attempts focused on selectively replacing or appending values directly to the active inventory list. However, because the Unity engine expects synchronous state validation across game runs, mutating active list states directly proved highly volatile.The Solution: The Clean-and-Reinject PatternThrough iterative research and troubleshooting, it became clear that the game engine requires a clean teardown to prevent thread freezes. Instead of selectively modifying active states, the codebase was refactored to employ a complete inventory teardown followed by a chronological state rebuild:State Cleansing: The plugin first invokes monsterInv.CleanInventory(), purging all active items and resetting the state machine to a pristine zero-point. This eliminates lingering pointers and engine conflicts.Sequential Historical Injection: Rather than parsing only the current stage delta, the plugin loops from stage zero up to the current stage count (Run.instance.stageClearCount + 1). It programmatically re-applies all configuration-defined items from scratch.Deterministic State Resolution: This ensures the engine's internal inventory validation checks resolve cleanly without memory or list synchronization issues.// Purge the dirty inventory entirely to prevent native state validation conflicts
monsterInv.CleanInventory();

int currentStageNum = Run.instance.stageClearCount + 1;
List<string> announcementItems = new List<string>();

// Sequentially rebuild the entire historical progression timeline from scratch
for (int i = 0; i < currentStageNum; i++)
{
    if (i >= StageConfigs.Length) break;
    string? configLine = StageConfigs[i]?.Value;
    if (string.IsNullOrEmpty(configLine)) continue;

    foreach (var step in ParseStageConfig(configLine))
    {
        string? itemName = ApplyEvolutionStep(step, monsterInv);
        if (i + 1 == currentStageNum && !string.IsNullOrEmpty(itemName))
        {
            announcementItems.Add($"{step.Count}x {itemName}");
        }
    }
}
Technical StackLanguage: C# (#nullable enable)Frameworks: BepInEx, MonoMod, Unity Engine, .NET Standard 2.0Core APIs: System.Reflection, System.Linq (LINQ query optimization)Design Practices: Deterministic State Invalidation, Hook Interception, Defensive Exception Handling
