using Mutagen.Bethesda;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Noggog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace StaffGenerator
{
    public class Program
    {
        static Lazy<Settings> Settings = null!;

        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetAutogeneratedSettings(
                    nickname: "Settings",
                    path: "settings.json",
                    out Settings)
                .Run(args, new RunPreferences()
                {
                    ActionsForEmptyArgs = new RunDefaultPatcher()
                    {
                        IdentifyingModKey = "YourPatcher.esp",
                        TargetRelease = GameRelease.SkyrimSE,
                    }
                });
        }

        enum MagicSchool
        {
            Alteration,
            Conjuration,
            Destruction,
            Illusion,
            Restoration
        };

        enum MagicLevel
        {
            Novice = 0,
            Apprentice = 25,
            Adept = 50,
            Expert = 75,
            Master = 100
        };

        private static readonly Dictionary<FormKey, HashSet<FormKey>> HalfCostPerkIDToLevelledListIDs = new()
        {
            { Skyrim.Perk.AlterationApprentice25, new() { Skyrim.LeveledItem.LItemStaffAlteration25 } },
            { Skyrim.Perk.AlterationExpert75, new() { Skyrim.LeveledItem.LItemStaffAlteration75 } },
            { Skyrim.Perk.ConjurationNovice00, new() { Skyrim.LeveledItem.LItemStaffConjuration00, Skyrim.LeveledItem.LItemStaffConjuration00NPC } },
            { Skyrim.Perk.ConjurationApprentice25, new() { Skyrim.LeveledItem.LItemStaffConjuration25, Skyrim.LeveledItem.LItemStaffConjuration25NPC } },
            { Skyrim.Perk.ConjurationAdept50, new() { Skyrim.LeveledItem.LItemStaffConjuration50, Skyrim.LeveledItem.LItemStaffConjuration50NPC } },
            { Skyrim.Perk.ConjurationExpert75, new() { Skyrim.LeveledItem.LItemStaffConjuration75, Skyrim.LeveledItem.LItemStaffConjuration75NPC } },
            { Skyrim.Perk.DestructionNovice00, new() { Skyrim.LeveledItem.LItemStaffDestruction00 } },
            { Skyrim.Perk.DestructionApprentice25, new() { Skyrim.LeveledItem.LItemStaffDestruction25, Skyrim.LeveledItem.LItemStaffDestruction25NPC50 } },
            { Skyrim.Perk.DestructionAdept50, new() { Skyrim.LeveledItem.LItemStaffDestruction50, Skyrim.LeveledItem.LItemStaffDestruction50NPC50 } },
            { Skyrim.Perk.DestructionExpert75, new() { Skyrim.LeveledItem.LItemStaffDestruction75 } },
            { Skyrim.Perk.IllusionNovice00, new() { Skyrim.LeveledItem.LItemStaffIllusion00 } },
            { Skyrim.Perk.IllusionApprentice25, new() { Skyrim.LeveledItem.LItemStaffIllusion25 } },
            { Skyrim.Perk.IllusionAdept50, new() { Skyrim.LeveledItem.LItemStaffIllusion50 } },
            { Skyrim.Perk.IllusionExpert75, new() { Skyrim.LeveledItem.LItemStaffIllusion75 } },
            { Skyrim.Perk.RestorationApprentice25, new() { Skyrim.LeveledItem.LItemStaffRestoration25 } },
            { Skyrim.Perk.RestorationAdept50, new() { Skyrim.LeveledItem.LItemStaffRestoration50 } },
            { Skyrim.Perk.RestorationExpert75, new() { Skyrim.LeveledItem.LItemStaffRestoration75 } },
        };

        private static readonly Dictionary<FormKey, Tuple<MagicSchool, MagicLevel>> HalfCostPerkIDToLeveledListData = new()
        {
            { Skyrim.Perk.AlterationNovice00, new(MagicSchool.Alteration, MagicLevel.Novice) },
            { Skyrim.Perk.AlterationApprentice25, new(MagicSchool.Alteration, MagicLevel.Apprentice) },
            { Skyrim.Perk.AlterationAdept50, new(MagicSchool.Alteration, MagicLevel.Adept) },
            { Skyrim.Perk.AlterationExpert75, new(MagicSchool.Alteration, MagicLevel.Expert) },
            { Skyrim.Perk.ConjurationNovice00, new(MagicSchool.Conjuration, MagicLevel.Novice) },
            { Skyrim.Perk.ConjurationApprentice25, new(MagicSchool.Conjuration, MagicLevel.Apprentice) },
            { Skyrim.Perk.ConjurationAdept50, new(MagicSchool.Conjuration, MagicLevel.Adept) },
            { Skyrim.Perk.ConjurationExpert75, new(MagicSchool.Conjuration, MagicLevel.Expert) },
            { Skyrim.Perk.DestructionNovice00, new(MagicSchool.Destruction, MagicLevel.Novice) },
            { Skyrim.Perk.DestructionApprentice25, new(MagicSchool.Destruction, MagicLevel.Apprentice) },
            { Skyrim.Perk.DestructionAdept50, new(MagicSchool.Destruction, MagicLevel.Adept) },
            { Skyrim.Perk.DestructionExpert75, new(MagicSchool.Destruction, MagicLevel.Expert) },
            { Skyrim.Perk.IllusionNovice00, new(MagicSchool.Illusion, MagicLevel.Novice) },
            { Skyrim.Perk.IllusionApprentice25, new(MagicSchool.Illusion, MagicLevel.Apprentice) },
            { Skyrim.Perk.IllusionAdept50, new(MagicSchool.Illusion, MagicLevel.Adept) },
            { Skyrim.Perk.IllusionExpert75, new(MagicSchool.Illusion, MagicLevel.Expert) },
            { Skyrim.Perk.RestorationNovice00, new(MagicSchool.Restoration, MagicLevel.Novice) },
            { Skyrim.Perk.RestorationApprentice25, new(MagicSchool.Restoration, MagicLevel.Apprentice) },
            { Skyrim.Perk.RestorationAdept50, new(MagicSchool.Restoration, MagicLevel.Adept) },
            { Skyrim.Perk.RestorationExpert75, new(MagicSchool.Restoration, MagicLevel.Expert) },
        };

        private static readonly HashSet<CastType> AllowedCastTypes = new()
        {
            CastType.Concentration,
            CastType.FireAndForget
        };

        private static readonly HashSet<FormKey> AllowedEquipmentTypes = new()
        {
            Skyrim.EquipType.EitherHand,
            Skyrim.EquipType.LeftHand,
            Skyrim.EquipType.RightHand
        };

        private static readonly HashSet<ConditionData.Function> conditionFunctionsAlwaysFalseUsingAStaff = new()
        {
            ConditionData.Function.EffectWasDualCast,
            ConditionData.Function.IsDualCasting,
        };

        private static readonly HashSet<ConditionData.Function> playerOnlyConditionFunctions = new()
        {
            ConditionData.Function.IsPCAMurderer,
            ConditionData.Function.GetPCExpelled,
            ConditionData.Function.GetPCFactionMurder,
            ConditionData.Function.GetPCEnemyofFaction,
            ConditionData.Function.GetPCFactionAttack,
            ConditionData.Function.GetVATSMode,
            ConditionData.Function.GetPCMiscStat,
        };

        private static readonly Dictionary<ConditionData.Function, ConditionData.Function> playerSpecificToCasterConditionFunctions = new()
        {
            { ConditionData.Function.GetPCIsClass, ConditionData.Function.GetIsClass },
            { ConditionData.Function.GetPCIsRace, ConditionData.Function.GetIsRace },
            { ConditionData.Function.GetPCIsSex, ConditionData.Function.GetIsSex },
            { ConditionData.Function.GetPCInFaction, ConditionData.Function.GetInFaction },
            { ConditionData.Function.SameFactionAsPC, ConditionData.Function.SameFaction },
            { ConditionData.Function.SameRaceAsPC, ConditionData.Function.SameRace },
            { ConditionData.Function.SameSexAsPC, ConditionData.Function.SameSex },
        };

        private static readonly Dictionary<FormKey, MagicSchool> UnenchantedStaffIDByMagicSchool = new()
        {
            { Skyrim.Weapon.StaffTemplateAlteration, MagicSchool.Alteration },
            { Skyrim.Weapon.StaffTemplateConjuration, MagicSchool.Conjuration },
            { Skyrim.Weapon.StaffTemplateDestruction, MagicSchool.Destruction },
            { Skyrim.Weapon.StaffTemplateIIllusion, MagicSchool.Illusion },
            { Skyrim.Weapon.StaffTemplateRestoration, MagicSchool.Restoration },
        };

        private static FormLinkNullable<T> NewFormLinkNullable<T>(T majorRecord) where T : class, IMajorRecordGetter
        {
            return new FormLinkNullable<T>(majorRecord.FormKey);
        }

        private static FormLink<T> NewFormLink<T>(FormLinkNullable<T> majorRecordLink) where T : class, IMajorRecordCommonGetter
        {
            return new FormLink<T>(majorRecordLink.FormKey);
        }

        private static FormLink<T> NewFormLink<T>(T majorRecord) where T : class, IMajorRecordGetter
        {
            return new FormLink<T>(majorRecord.FormKey);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            IContainer? modifiedQAStaffContainer = null;

            void AddToQAStaffContainer(Weapon newStaff)
            {
                if (modifiedQAStaffContainer == null)
                    modifiedQAStaffContainer = state.PatchMod.Containers.GetOrAddAsOverride(state.LinkCache.Resolve<IContainerGetter>(Skyrim.Container.QAStaffContainer));
                modifiedQAStaffContainer.Items?.Add(new()
                {
                    Item = new()
                    {
                        Item = newStaff.AsLink<IItemGetter>(),
                        Count = 1
                    }
                });
            }

            ImmutableDictionary<string, IObjectEffectGetter> staffEnchantmentsByEditorID;
            ImmutableDictionary<FormLink<IMagicEffectGetter>, ImmutableHashSet<IObjectEffectGetter>> staffEnchantmentsByMagicEffect;

            {
                var staffEnchantmentsByEditorIDBuilder = ImmutableDictionary.CreateBuilder<string, IObjectEffectGetter>();
                var staffEnchantmentsByMagicEffectTemp = new Dictionary<FormLink<IMagicEffectGetter>, ImmutableHashSet<IObjectEffectGetter>.Builder>();

                foreach (var staffEnchantment in state.LoadOrder.PriorityOrder.ObjectEffect().WinningOverrides())
                {
                    if (staffEnchantment.EnchantType != ObjectEffect.EnchantTypeEnum.StaffEnchantment) continue;
                    if (staffEnchantment.EditorID is null) continue;
                    foreach (var magicEffect in staffEnchantment.Effects)
                    {
                        var baseEffect = magicEffect.BaseEffect;
                        if (baseEffect.IsNull) continue;
                        Autovivify(staffEnchantmentsByMagicEffectTemp, NewFormLink(baseEffect)).Add(staffEnchantment);
                    }
                    staffEnchantmentsByEditorIDBuilder.Add(staffEnchantment.EditorID, staffEnchantment);
                }

                var staffEnchantmentsByMagicEffectBuilder = ImmutableDictionary.CreateBuilder<FormLink<IMagicEffectGetter>, ImmutableHashSet<IObjectEffectGetter>>();
                foreach (var item in staffEnchantmentsByMagicEffectTemp)
                {
                    staffEnchantmentsByMagicEffectBuilder.Add(item.Key, item.Value.ToImmutable());
                }

                staffEnchantmentsByEditorID = staffEnchantmentsByEditorIDBuilder.ToImmutable();
                staffEnchantmentsByMagicEffect = staffEnchantmentsByMagicEffectBuilder.ToImmutable();
            }

            ImmutableHashSet<FormLink<IWeaponGetter>> staves;
            ImmutableDictionary<string, IWeaponGetter> stavesByEditorID;
            ImmutableDictionary<MagicSchool, IWeaponGetter> unenchantedStavesByMagicSchool;
            ImmutableDictionary<FormLink<IEffectRecordGetter>, ImmutableHashSet<IWeaponGetter>> enchantedStavesByEnchantment;

            {
                var stavesBuilder = ImmutableHashSet.CreateBuilder<FormLink<IWeaponGetter>>();
                var stavesByEditorIDBuilder = ImmutableDictionary.CreateBuilder<string, IWeaponGetter>();
                var unenchantedStavesByMagicSchoolBuilder = ImmutableDictionary.CreateBuilder<MagicSchool, IWeaponGetter>();
                var enchantedStavesByEnchantmentTemp = new Dictionary<FormLink<IEffectRecordGetter>, ImmutableHashSet<IWeaponGetter>.Builder>();

                foreach (var staff in state.LoadOrder.PriorityOrder.Weapon().WinningOverrides())
                {
                    if (staff.Keywords?.Contains(Skyrim.Keyword.WeapTypeStaff) != true) continue;
                    if (staff.EditorID is null) continue;
                    if (UnenchantedStaffIDByMagicSchool.TryGetValue(staff.FormKey, out var magicSchool))
                        unenchantedStavesByMagicSchoolBuilder.Add(magicSchool, staff);
                    if (!staff.ObjectEffect.IsNull)
                    {
                        if (staff.Template.IsNull) continue;
                        stavesBuilder.Add(staff.AsLink());
                        stavesByEditorIDBuilder.Add(staff.EditorID, staff);
                        Autovivify(enchantedStavesByEnchantmentTemp, NewFormLink(staff.ObjectEffect)).Add(staff);
                    }
                }

                var enchantedStavesByEnchantmentBuilder = ImmutableDictionary.CreateBuilder<FormLink<IEffectRecordGetter>, ImmutableHashSet<IWeaponGetter>>();
                foreach (var item in enchantedStavesByEnchantmentTemp)
                {
                    enchantedStavesByEnchantmentBuilder.Add(item.Key, item.Value.ToImmutable());
                }

                staves = stavesBuilder.ToImmutable();
                stavesByEditorID = stavesByEditorIDBuilder.ToImmutable();
                unenchantedStavesByMagicSchool = unenchantedStavesByMagicSchoolBuilder.ToImmutable();
                enchantedStavesByEnchantment = enchantedStavesByEnchantmentBuilder.ToImmutable();
            }

            ImmutableDictionary<FormLink<IWeaponGetter>, IConstructibleObjectGetter> recipesByCreatedStaff;

            {
                var recipesByCreatedStaffBuilder = ImmutableDictionary.CreateBuilder<FormLink<IWeaponGetter>, IConstructibleObjectGetter>();

                foreach (var recipe in state.LoadOrder.PriorityOrder.ConstructibleObject().WinningOverrides())
                {
                    if (recipe.CreatedObjectCount.HasValue)
                        if (!(recipe.CreatedObjectCount.Value == 1)) continue;
                    if (recipe.CreatedObject.IsNull) continue;
                    if (!staves.Contains(recipe.CreatedObject.FormKey)) continue;
                    recipesByCreatedStaffBuilder.Add(new FormLink<IWeaponGetter>(recipe.CreatedObject.FormKey), recipe);
                }

                recipesByCreatedStaff = recipesByCreatedStaffBuilder.ToImmutable();
            }

            ImmutableDictionary<FormKey, ILeveledItemGetter> leveledListsByFormKey;
            ImmutableDictionary<string, ILeveledItemGetter> leveledListsByEditorID;

            {
                var leveledListsByFormKeyBuilder = ImmutableDictionary.CreateBuilder<FormKey, ILeveledItemGetter>();
                var leveledListsByEditorIDBuilder = ImmutableDictionary.CreateBuilder<string, ILeveledItemGetter>();

                foreach (var leveledList in state.LoadOrder.PriorityOrder.LeveledItem().WinningOverrides())
                {
                    if (leveledList.EditorID is null) continue;
                    leveledListsByFormKeyBuilder.Add(leveledList.FormKey, leveledList);
                    leveledListsByEditorIDBuilder.Add(leveledList.EditorID, leveledList);
                }

                leveledListsByFormKey = leveledListsByFormKeyBuilder.ToImmutable();
                leveledListsByEditorID = leveledListsByEditorIDBuilder.ToImmutable();
            }

            var newStavesByHalfCostPerk = new Dictionary<FormLink<IPerkGetter>, List<IWeapon>>();
            var recipeIngredientsByUnenchantedStaffID = new Dictionary<FormKey, ExtendedList<ContainerEntry>>();

            foreach (var book in state.LoadOrder.PriorityOrder.Book().WinningOverrides())
            {
                if (Settings.Value.SpellBooksToIgnore.Contains(book)) continue;
                if (book.Teaches is not IBookSpellGetter spellBook) continue;

                if (!spellBook.Spell.TryResolve(state.LinkCache, out var spell)) continue;
                if (Settings.Value.SpellsToIgnore.Contains(spell)) continue;

                if (spell.Name is null) continue;
                var spellName = spell.Name.String;
                if (spell.EditorID is null) continue;
                var spellEditorID = spell.EditorID;
                if (spell.Type != SpellType.Spell) continue;
                if (spell.TargetType == TargetType.Self) continue;
                if (!AllowedCastTypes.Contains(spell.CastType)) continue;
                if (!AllowedEquipmentTypes.Contains(spell.EquipmentType.FormKey)) continue;

                var halfCostPerkID = spell.HalfCostPerk;

                if (!HalfCostPerkIDToLeveledListData.TryGetValue(halfCostPerkID.FormKey, out var leveledListData)) continue;
                if (!unenchantedStavesByMagicSchool.TryGetValue(leveledListData.Item1, out var unenchantedStaff)) continue;

                var enchantmentEditorID = "StaffEnch" + spell.EditorID;

                IObjectEffectGetter? enchantment = null;

                var foo =
                    from magicEffect in spell.Effects
                    let baseEffect = magicEffect.BaseEffect
                    where !baseEffect.IsNull
                    join ench in staffEnchantmentsByMagicEffect
                      on NewFormLink(baseEffect) equals ench.Key
                    group 1 by ench.Value
                    ;

                var candidateEnchantments = new Dictionary<IObjectEffectGetter, int>();

                foreach (var magicEffect in spell.Effects)
                {
                    var baseEffect = magicEffect.BaseEffect;
                    if (baseEffect.IsNull) continue;
                    if (staffEnchantmentsByMagicEffect.TryGetValue(NewFormLink(baseEffect), out var candidateEnchantments1))
                    {
                        foreach (var candidateEnchantment in candidateEnchantments1)
                        {
                            candidateEnchantments.TryGetValue(candidateEnchantment, out var count);
                            candidateEnchantments[candidateEnchantment] = count + 1;
                        }
                    }
                }

                foreach (var candidateEnchantment in candidateEnchantments.OrderBy(x => x.Value))
                {
                    // TODO how do we pick the right one?
                    enchantment = candidateEnchantment.Key;
                    break;
                }

                if (!staffEnchantmentsByEditorID.TryGetValue(enchantmentEditorID, out enchantment))
                {
                    var newEnchantment = state.PatchMod.ObjectEffects.AddNew(enchantmentEditorID);
                    newEnchantment.Name = spellName;
                    newEnchantment.CastType = spell.CastType;
                    newEnchantment.TargetType = spell.TargetType;
                    newEnchantment.EnchantType = ObjectEffect.EnchantTypeEnum.StaffEnchantment;
                    newEnchantment.Effects.AddRange(spell.Effects.Select(x => x.DeepCopy()));

                    enchantment = newEnchantment;
                }

                IWeaponGetter? staff = null;

                if (enchantedStavesByEnchantment.TryGetValue(NewFormLink<IEffectRecordGetter>(enchantment), out var candidateStaves))
                {
                    foreach (var candidateStaff in candidateStaves)
                    {
                        if (candidateStaff.Template != unenchantedStaff.AsLink()) continue;
                        // TODO add more criteria?
                        staff = candidateStaff;
                        break;
                    }
                }

                var staffEditorID = "Staff" + spell.EditorID;
                if (!stavesByEditorID.TryGetValue(staffEditorID, out staff))
                {
                    var newStaff = state.PatchMod.Weapons.AddNew(staffEditorID);
                    // TODO settings for staff prefix + suffix.
                    newStaff.Name = "Staff of " + spellName;
                    newStaff.Template = NewFormLinkNullable(unenchantedStaff);
                    newStaff.ObjectEffect = NewFormLinkNullable<IEffectRecordGetter>(enchantment);
                    // TODO other stuff?

                    AddToQAStaffContainer(newStaff);

                    Autovivify(newStavesByHalfCostPerk, halfCostPerkID).Add(newStaff);

                    staff = newStaff;
                }

                if (Settings.Value.StavesToIgnore.Contains(staff)) continue;

                if (!recipeIngredientsByUnenchantedStaffID.TryGetValue(unenchantedStaff.FormKey, out var newRecipeIngredients))
                    newRecipeIngredients = recipeIngredientsByUnenchantedStaffID[unenchantedStaff.FormKey] = new()
                    {
                        new()
                        {
                            Item = new()
                            {
                                Item = unenchantedStaff.AsLink<IItemGetter>(),
                                Count = 1
                            }
                        },
                        new()
                        {
                            Item = new()
                            {
                                Item = Dragonborn.MiscItem.DLC2HeartStone,
                                Count = 3
                            }
                        }
                    };

                IConstructibleObject newRecipe;
                if (recipesByCreatedStaff.TryGetValue(staff.FormKey, out var oldRrecipe))
                    newRecipe = state.PatchMod.ConstructibleObjects.GetOrAddAsOverride(oldRrecipe);
                else
                    newRecipe = state.PatchMod.ConstructibleObjects.AddNew("DLC2Recipe" + staff.EditorID);

                newRecipe.Items = newRecipeIngredients;
                newRecipe.Conditions.Clear();
                newRecipe.Conditions.Add(new ConditionFloat()
                {
                    CompareOperator = CompareOperator.EqualTo,
                    ComparisonValue = 1,
                    Data = new FunctionConditionData()
                    {
                        Function = (ushort)ConditionData.Function.HasSpell,
                        ParameterOneRecord = spell.AsLink<ISkyrimMajorRecordGetter>(),
                        Unknown2 = 0,
                        Unknown3 = (int)Condition.RunOnType.Reference,
                        Unknown4 = 0x00000014, // PlayerRef [PLYR:000014]
                        Unknown5 = 0
                    }
                });
                newRecipe.CreatedObject = NewFormLinkNullable<IConstructibleGetter>(staff);
                newRecipe.WorkbenchKeyword = Dragonborn.Keyword.DLC2StaffEnchanter;
                newRecipe.CreatedObjectCount = 1;
            }

            var leveledListByMagicLevel = new Dictionary<MagicLevel, ILeveledItem>();
            var modifiedLeveledListByFormKey = new Dictionary<FormKey, ILeveledItem>();

            foreach (var (halfCostPerkID, newStaves) in newStavesByHalfCostPerk)
            {
                var leveledLists = new HashSet<ILeveledItemGetter>();
                var modifiedLeveledLists = new List<ILeveledItem>();

                if (HalfCostPerkIDToLevelledListIDs.TryGetValue(halfCostPerkID.FormKey, out var leveledListIDs))
                    foreach (var leveledListFormKey in leveledListIDs)
                        if (leveledListsByFormKey.TryGetValue(leveledListFormKey, out var leveledList))
                            leveledLists.Add(leveledList);

                if (HalfCostPerkIDToLeveledListData.TryGetValue(halfCostPerkID.FormKey, out var editorIdFoo))
                {
                    var (magicSchool, magicLevel) = editorIdFoo;

                    var magicLevelString = String.Format("{0:D2}", (int)magicLevel);

                    var leveledListEditorID = $"LItemStaff{magicSchool:g}{magicLevelString}";
                    if (leveledListsByEditorID.TryGetValue(leveledListEditorID, out var leveledList))
                        leveledLists.Add(leveledList);
                    else
                    {
                        var newLeveledList = state.PatchMod.LeveledItems.AddNew(leveledListEditorID);
                        newLeveledList.ChanceNone = 0;
                        newLeveledList.Flags = LeveledItem.Flag.CalculateForEachItemInCount | LeveledItem.Flag.CalculateFromAllLevelsLessThanOrEqualPlayer;

                        if (!leveledListByMagicLevel.TryGetValue(magicLevel, out var leveledListForMagicLevel))
                        {
                            var leveledListForMagicLevelEditorID = $"LItemStaff{magicLevelString}";
                            if (!leveledListsByEditorID.TryGetValue(leveledListForMagicLevelEditorID, out var leveledListForMagicLevelGetter))
                                throw new KeyNotFoundException($"LeveledList defined in Skyrim.esm \"{leveledListForMagicLevelEditorID}\" couldn't be found?");
                            leveledListForMagicLevel = leveledListByMagicLevel[magicLevel] = state.PatchMod.LeveledItems.GetOrAddAsOverride(leveledListForMagicLevelGetter);
                        }
                        leveledListForMagicLevel.Entries ??= new();
                        leveledListForMagicLevel.Entries.Add(new()
                        {
                            Data = new()
                            {
                                Level = 1,
                                Reference = newLeveledList.AsLink<IItemGetter>(),
                                Count = 1,
                            }
                        });

                        modifiedLeveledLists.Add(newLeveledList);
                    }
                }

                foreach (var leveledList in leveledLists)
                    modifiedLeveledLists.Add(Autovivify(modifiedLeveledListByFormKey, leveledList.FormKey, () => state.PatchMod.LeveledItems.GetOrAddAsOverride(leveledList)));

                var leveledItems = newStaves.Select(staff => new LeveledItemEntry()
                {
                    Data = new()
                    {
                        Level = 1,
                        Reference = staff.AsLink<IItemGetter>(),
                        Count = 1,
                    }
                }).ToList();

                foreach (var leveledList in modifiedLeveledLists)
                    (leveledList.Entries ??= new()).AddRange(leveledItems);
            }
        }

        private static V Autovivify<K, V>(IDictionary<K, V> dict, K key) where K : notnull where V : new() => Autovivify(dict, key, () => new());

        private static ImmutableHashSet<V>.Builder Autovivify<K, V>(IDictionary<K, ImmutableHashSet<V>.Builder> dict, K key) where K : notnull => Autovivify(dict, key, () => ImmutableHashSet.CreateBuilder<V>());

        private static V Autovivify<K, V>(IDictionary<K, V> dict, K key, Func<V> newThing) where K : notnull
        {
            if (!dict.TryGetValue(key, out var value))
                value = dict[key] = newThing();
            return value;
        }
    }
}
