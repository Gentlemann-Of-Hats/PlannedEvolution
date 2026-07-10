Planned Evolution (v2.3.3)

Planned Evolution is an open-source runtime configuration utility for Risk of Rain 2. Developed in C# using the BepInEx framework, it replaces the procedural randomness of the "Artifact of Evolution" mechanic with a predictable, user-configured progression timeline.

Key Features

Custom Progression Timeline: Define exact item builds for up to 20 stages using local configuration files.

Dynamic Configuration Parser: Parses complex configuration strings (ItemName, Count;) and maps them to in-game indices at runtime.

Tier-Based Blacklisting: Compiles a static memory cache of banned items on startup to filter random drop pools.

Case Study: Solving the State Synchronization Softlock

The Challenge

During early development, attempts to modify enemy inventories mid-tick caused severe race conditions. Mutating active lists while the Unity engine thread was actively traversing them blocked execution, causing the game to softlock permanently.

The Solution: The Clean-and-Reinject Pattern

To prevent thread freezes, the codebase was refactored to employ a complete inventory teardown followed by a chronological state rebuild:

Clear State: Calls monsterInv.CleanInventory() to safely flush dirty data and eliminate native conflicts.

Reinject Timeline: Loops chronologically from stage zero to the current stage count, re-applying configuration rules.

Resolve State: Ensures native engine checks validate a clean, reconstructed inventory.

// Purge the dirty inventory entirely to prevent native state conflicts
monsterInv.CleanInventory();

int currentStageNum = Run.instance.stageClearCount + 1;

// Sequentially rebuild the entire historical progression timeline from scratch
for (int i = 0; i < currentStageNum; i++)
{
    if (i >= StageConfigs.Length) break;
    string? configLine = StageConfigs[i]?.Value;
    if (string.IsNullOrEmpty(configLine)) continue;

    foreach (var step in ParseStageConfig(configLine))
    {
        ApplyEvolutionStep(step, monsterInv);
    }
}


Technical Stack

Language & Tools: C# (#nullable enable), Git, GitHub

Frameworks: Unity Engine, BepInEx, MonoMod hooks

Core APIs: System.Reflection, System.Linq (LINQ queries)Planned Evolution (v2.3.3)Planned Evolution is an open-source runtime configuration utility for Risk of Rain 2. Developed in C# using the BepInEx framework, it replaces the procedural randomness of the "Artifact of Evolution" mechanic with a predictable, user-configured progression timeline.Key FeaturesCustom Progression Timeline: Define exact item builds for up to 20 stages using local configuration files.Dynamic Configuration Parser: Parses complex configuration strings (ItemName, Count;) and maps them to in-game indices at runtime.Tier-Based Blacklisting: Compiles a static memory cache of banned items on startup to filter random drop pools.Case Study: Solving the State Synchronization SoftlockThe ChallengeDuring early development, attempts to modify enemy inventories mid-tick caused severe race conditions. Mutating active lists while the Unity engine thread was actively traversing them blocked execution, causing the game to softlock permanently.The Solution: The Clean-and-Reinject PatternTo prevent thread freezes, the codebase was refactored to employ a complete inventory teardown followed by a chronological state rebuild:Clear State: Calls monsterInv.CleanInventory() to safely flush dirty data and eliminate native conflicts.Reinject Timeline: Loops chronologically from stage zero to the current stage count, re-applying configuration rules.Resolve State: Ensures native engine checks validate a clean, reconstructed inventory.// Purge the dirty inventory entirely to prevent native state conflicts
monsterInv.CleanInventory();

int currentStageNum = Run.instance.stageClearCount + 1;

// Sequentially rebuild the entire historical progression timeline from scratch
for (int i = 0; i < currentStageNum; i++)
{
    if (i >= StageConfigs.Length) break;
    string? configLine = StageConfigs[i]?.Value;
    if (string.IsNullOrEmpty(configLine)) continue;

    foreach (var step in ParseStageConfig(configLine))
    {
        ApplyEvolutionStep(step, monsterInv);
    }
}
Technical StackLanguage & Tools: C# (#nullable enable), Git, GitHubFrameworks: Unity Engine, BepInEx, MonoMod hooksCore APIs: System.Reflection, System.Linq (LINQ queries)
