#nullable enable
using BepInEx;
using BepInEx.Configuration;
using RoR2;
using RoR2.Artifacts;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;

namespace PlannedEvolution
{
    [BepInPlugin("com.FortressForce.PlannedEvolution", "Planned Evolution", "2.3.3")]
    public class PlannedEvolutionPlugin : BaseUnityPlugin
    {
        private ConfigEntry<bool>? ModEnabled;
        private ConfigEntry<string>[] StageConfigs = new ConfigEntry<string>[20];
        private static ConfigEntry<string>? BlacklistWhite;
        private static ConfigEntry<string>? BlacklistGreen;
        private static ConfigEntry<string>? BlacklistRed;

        private List<ItemIndex> BannedItemIndices = new List<ItemIndex>();
        private bool isCacheBuilt = false;

        // Reflection helper to get the private 'monsterTeamInventory' field
        private static Inventory? MonsterInventory
        {
            get
            {
                FieldInfo? field = typeof(MonsterTeamGainsItemsArtifactManager).GetField("monsterTeamInventory", BindingFlags.Static | BindingFlags.NonPublic);
                return field?.GetValue(null) as Inventory;
            }
        }

        private class EvolutionStep
        {
            public string ItemIdentifier = string.Empty;
            public int Count;
        }

        public void Awake()
        {
            ModEnabled = Config.Bind("General", "Enabled", true, "Set to false to disable the mod.");

            for (int i = 0; i < 20; i++)
            {
                int stageNum = i + 1;
                string defaultVal = (stageNum <= 5) ? "AnyWhite, 1" : (stageNum <= 10) ? "AnyGreen, 1" : "AnyRed, 1";
                StageConfigs[i] = Config.Bind("Evolution Schedule", $"Stage {stageNum}", defaultVal, "ItemName, Count; ItemName, Count");
            }

            BlacklistWhite = Config.Bind("Blacklists", "Banned Whites", "RollOfPennies", "Banned White items.");
            BlacklistGreen = Config.Bind("Blacklists", "Banned Greens", "SquidPolyp", "Banned Green items.");
            BlacklistRed = Config.Bind("Blacklists", "Banned Reds", "ShockNearby", "Banned Red items.");

            if (ModEnabled?.Value ?? true)
            {
                On.RoR2.Artifacts.MonsterTeamGainsItemsArtifactManager.GrantMonsterTeamItem += OverrideEvolution;
                Logger.LogInfo("PlannedEvolution 2.3.1: Private field access fixed via Reflection.");
            }
        }

        private void OverrideEvolution(On.RoR2.Artifacts.MonsterTeamGainsItemsArtifactManager.orig_GrantMonsterTeamItem orig)
        {
            // Call orig() first to let vanilla initialize properly
            orig();

            try
            {
                if (!isCacheBuilt) BuildBlacklistCache();
                Inventory? monsterInv = MonsterInventory;
                if (monsterInv != null)
                {
                    // Clean the inventory to remove vanilla items
                    monsterInv.CleanInventory();

                    int currentStageNum = Run.instance.stageClearCount + 1;
                    List<string> announcementItems = new List<string>();

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
                    if (RunArtifactManager.instance.IsArtifactEnabled(ArtifactCatalog.FindArtifactDef("Evolution")))
                    {
                        if (announcementItems.Count > 0)
                        {
                            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                            {
                                baseToken = $"[Planned Evolution] Stage {currentStageNum}: " + string.Join(", ", announcementItems)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error in OverrideEvolution: {ex}");
            }
        }


        private List<EvolutionStep> ParseStageConfig(string rawText)
        {
            List<EvolutionStep> steps = new List<EvolutionStep>();
            foreach (string entry in rawText.Split(';'))
            {
                string[] parts = entry.Trim().Split(',');
                if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out int count))
                {
                    steps.Add(new EvolutionStep { ItemIdentifier = parts[0].Trim(), Count = count });
                }
            }
            return steps;
        }

        private string? ApplyEvolutionStep(EvolutionStep step, Inventory inventory)
        {
            ItemIndex idx = step.ItemIdentifier.StartsWith("Any")
                ? PickRandomItem(step.ItemIdentifier)
                : FindItemIndexBetter(step.ItemIdentifier);

            if (idx != ItemIndex.None)
            {
                ItemDef def = ItemCatalog.GetItemDef(idx);
                if (step.Count > 0) inventory.GiveItemPermanent(idx, step.Count);
                else if (step.Count < 0) inventory.RemoveItemPermanent(idx, Math.Abs(step.Count));

                return Language.GetString(def.nameToken);
            }
            return null;
        }

        private void BuildBlacklistCache()
        {
            if (isCacheBuilt || !ItemCatalog.availability.available) return;
            BannedItemIndices.Clear();
            string combined = $"{(BlacklistWhite?.Value ?? "")},{(BlacklistGreen?.Value ?? "")},{(BlacklistRed?.Value ?? "")}";
            foreach (string name in combined.Split(','))
            {
                ItemIndex idx = FindItemIndexBetter(name.Trim());
                if (idx != ItemIndex.None) BannedItemIndices.Add(idx);
            }
            isCacheBuilt = true;
        }

        private ItemIndex FindItemIndexBetter(string identifier)
        {
            ItemIndex idx = ItemCatalog.FindItemIndex(identifier);
            if (idx != ItemIndex.None) return idx;

            foreach (ItemIndex i in ItemCatalog.allItems)
            {
                ItemDef def = ItemCatalog.GetItemDef(i);
                if (def == null) continue;
                if (def.name.Equals(identifier, StringComparison.OrdinalIgnoreCase) ||
                    Language.GetString(def.nameToken).Equals(identifier, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return ItemIndex.None;
        }

        private ItemIndex PickRandomItem(string keyword)
        {
            List<PickupIndex>? dropList = keyword switch
            {
                "AnyWhite" => Run.instance.availableTier1DropList,
                "AnyGreen" => Run.instance.availableTier2DropList,
                "AnyRed" => Run.instance.availableTier3DropList,
                "AnyVoid" => Run.instance.availableVoidTier1DropList,
                _ => null
            };

            if (dropList == null || dropList.Count == 0) return ItemIndex.None;

            var valid = dropList.Where(p => {
                ItemIndex idx = PickupCatalog.GetPickupDef(p)?.itemIndex ?? ItemIndex.None;
                return idx != ItemIndex.None && !BannedItemIndices.Contains(idx);
            }).ToList();

            if (valid.Count == 0) return ItemIndex.None;
            return PickupCatalog.GetPickupDef(Run.instance.treasureRng.NextElementUniform(valid))?.itemIndex ?? ItemIndex.None;
        }
    }
}