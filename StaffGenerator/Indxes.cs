using Mutagen.Bethesda;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Noggog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;


namespace StaffGenerator
{
    public partial class Program
    {
        private static void IndexLeveledLists(
            IEnumerable<ILeveledItemGetter> leveledLists,
            out ImmutableDictionary<IFormLinkGetter<ILeveledItemGetter>, ILeveledItemGetter> leveledListsByFormKey,
            out ImmutableDictionary<string, ILeveledItemGetter> leveledListsByEditorID)
        {
            var leveledListsByFormKeyBuilder = ImmutableDictionary.CreateBuilder<IFormLinkGetter<ILeveledItemGetter>, ILeveledItemGetter>();
            var leveledListsByEditorIDBuilder = ImmutableDictionary.CreateBuilder<string, ILeveledItemGetter>();

            foreach (var leveledList in leveledLists)
            {
                if (leveledList.EditorID is null) continue;
                leveledListsByFormKeyBuilder[leveledList.ToLink()] = leveledList;
                leveledListsByEditorIDBuilder[leveledList.EditorID] = leveledList;
            }

            leveledListsByFormKey = leveledListsByFormKeyBuilder.ToImmutable();
            leveledListsByEditorID = leveledListsByEditorIDBuilder.ToImmutable();
        }

        private static ImmutableDictionary<IFormLinkGetter<IWeaponGetter>, IConstructibleObjectGetter> IndexRecipies(
            IEnumerable<IConstructibleObjectGetter> recipes,
            ImmutableHashSet<IFormLinkGetter<IWeaponGetter>> staves)
        {
            ImmutableDictionary<IFormLinkGetter<IWeaponGetter>, IConstructibleObjectGetter> recipesByCreatedStaff;
            {
                var recipesByCreatedStaffBuilder = ImmutableDictionary.CreateBuilder<IFormLinkGetter<IWeaponGetter>, IConstructibleObjectGetter>();

                foreach (var recipe in recipes)
                {
                    if (recipe.CreatedObjectCount.HasValue)
                        if (!(recipe.CreatedObjectCount.Value == 1)) continue;
                    if (recipe.CreatedObject.IsNull) continue;
                    if (!staves.Contains(recipe.CreatedObject.Cast<IWeaponGetter>())) continue;
                    recipesByCreatedStaffBuilder[new FormLink<IWeaponGetter>(recipe.CreatedObject.FormKey)] = recipe;
                }

                recipesByCreatedStaff = recipesByCreatedStaffBuilder.ToImmutable();
            }

            return recipesByCreatedStaff;
        }

        private static void IndexWeapons(
            IEnumerable<IWeaponGetter> weapons,
            out ImmutableHashSet<IFormLinkGetter<IWeaponGetter>> staves,
            out ImmutableDictionary<string, IWeaponGetter> stavesByEditorID,
            out ImmutableDictionary<MagicSchool, IWeaponGetter> unenchantedStavesByMagicSchool,
            out ImmutableDictionary<IFormLinkGetter<IEffectRecordGetter>, ImmutableHashSet<IWeaponGetter>> enchantedStavesByEnchantment)
        {
            var stavesBuilder = ImmutableHashSet.CreateBuilder<IFormLinkGetter<IWeaponGetter>>();
            var stavesByEditorIDBuilder = ImmutableDictionary.CreateBuilder<string, IWeaponGetter>();
            var unenchantedStavesByMagicSchoolBuilder = ImmutableDictionary.CreateBuilder<MagicSchool, IWeaponGetter>();
            var enchantedStavesByEnchantmentTemp = new Dictionary<IFormLinkGetter<IEffectRecordGetter>, ImmutableHashSet<IWeaponGetter>.Builder>();

            foreach (var staff in weapons)
            {
                if (staff.Keywords?.Contains(Skyrim.Keyword.WeapTypeStaff) != true) continue;
                if (staff.EditorID is null) continue;
                if (UnenchantedStaffIDByMagicSchool.TryGetValue(staff.ToLink(), out var magicSchool))
                    unenchantedStavesByMagicSchoolBuilder.Add(magicSchool, staff);
                if (!staff.ObjectEffect.IsNull)
                {
                    if (staff.Template.IsNull) continue;
                    stavesBuilder.Add(staff.ToLink());
                    stavesByEditorIDBuilder[staff.EditorID] = staff;
                    enchantedStavesByEnchantmentTemp.Autovivify(staff.ObjectEffect).Add(staff);
                }
            }

            var enchantedStavesByEnchantmentBuilder = ImmutableDictionary.CreateBuilder<IFormLinkGetter<IEffectRecordGetter>, ImmutableHashSet<IWeaponGetter>>();
            foreach (var item in enchantedStavesByEnchantmentTemp)
            {
                enchantedStavesByEnchantmentBuilder[item.Key] = item.Value.ToImmutable();
            }

            staves = stavesBuilder.ToImmutable();
            stavesByEditorID = stavesByEditorIDBuilder.ToImmutable();
            unenchantedStavesByMagicSchool = unenchantedStavesByMagicSchoolBuilder.ToImmutable();
            enchantedStavesByEnchantment = enchantedStavesByEnchantmentBuilder.ToImmutable();
        }

        public static void IndexStaffEnchantments(
            IEnumerable<IObjectEffectGetter> staffEnchantments,
            out ImmutableDictionary<string, IObjectEffectGetter> staffEnchantmentsByEditorID,
            out ImmutableDictionary<IFormLinkGetter<IMagicEffectGetter>, ImmutableHashSet<IObjectEffectGetter>> staffEnchantmentsByMagicEffect)
        {
            var staffEnchantmentsByEditorIDBuilder = ImmutableDictionary.CreateBuilder<string, IObjectEffectGetter>();
            var staffEnchantmentsByMagicEffectTemp = new Dictionary<IFormLinkGetter<IMagicEffectGetter>, ImmutableHashSet<IObjectEffectGetter>.Builder>();

            foreach (var staffEnchantment in staffEnchantments)
            {
                if (staffEnchantment.EnchantType != ObjectEffect.EnchantTypeEnum.StaffEnchantment) continue;
                if (staffEnchantment.EditorID is null) continue;
                foreach (var magicEffect in staffEnchantment.Effects)
                {
                    var baseEffect = magicEffect.BaseEffect;
                    if (baseEffect.IsNull) continue;
                    staffEnchantmentsByMagicEffectTemp.Autovivify(baseEffect).Add(staffEnchantment);
                }
                staffEnchantmentsByEditorIDBuilder[staffEnchantment.EditorID] = staffEnchantment;
            }

            var staffEnchantmentsByMagicEffectBuilder = ImmutableDictionary.CreateBuilder<IFormLinkGetter<IMagicEffectGetter>, ImmutableHashSet<IObjectEffectGetter>>();
            foreach (var item in staffEnchantmentsByMagicEffectTemp)
            {
                staffEnchantmentsByMagicEffectBuilder[item.Key] = item.Value.ToImmutable();
            }

            staffEnchantmentsByEditorID = staffEnchantmentsByEditorIDBuilder.ToImmutable();
            staffEnchantmentsByMagicEffect = staffEnchantmentsByMagicEffectBuilder.ToImmutable();
        }


    }
}
