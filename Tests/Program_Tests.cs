using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using StaffGenerator;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Order;

namespace Tests
{
    public class Program_Tests
    {
        public static readonly ModKey masterModKey = ModKey.FromNameAndExtension("Master.esm");

        public static readonly ModKey patchModKey = ModKey.FromNameAndExtension("Patch.esp");
        private readonly SkyrimMod masterMod;
        private readonly SkyrimMod patchMod;
        private readonly LoadOrder<IModListing<ISkyrimModGetter>> loadOrder;

        public Program_Tests()
        {
            masterMod = new SkyrimMod(masterModKey, SkyrimRelease.SkyrimSE);
            patchMod = new SkyrimMod(patchModKey, SkyrimRelease.SkyrimSE);

            loadOrder = new LoadOrder<IModListing<ISkyrimModGetter>>
            {
                new ModListing<ISkyrimModGetter>(masterMod, true),
                new ModListing<ISkyrimModGetter>(patchMod, true)
            };
        }

        [Fact]
        public void TestDoesNothing()
        {
            var linkCache = loadOrder.ToImmutableLinkCache();

            Program program = new(loadOrder, linkCache, patchMod, new(new Settings()));

            program.RunPatch();

            Assert.Empty(patchMod.EnumerateMajorRecords());
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void Test2(bool createStaff, bool createStaffEnchantment)
        {
            AddQAStaffContainer();
            AddLItemStaffDestruction00();
            AddStaffTemplateDestruction();

            var halfCostPerkLink = Skyrim.Perk.DestructionNovice00;

            var spellLink = Skyrim.Spell.Flames;
            var spell = new Spell(spellLink.FormKey, masterMod.SkyrimRelease)
            {
                EditorID = "Flames",
                Name = "Flames",
                Type = SpellType.Spell,
                TargetType = TargetType.Aimed,
                CastType = CastType.Concentration,
                EquipmentType = Skyrim.EquipType.EitherHand.AsNullable(),
                HalfCostPerk = halfCostPerkLink
            };
            masterMod.Spells.Add(spell);

            var spellBookLink = Skyrim.Book.SpellTomeFlames;
            var spellBook = new Book(spellBookLink.FormKey, masterMod.SkyrimRelease)
            {
                EditorID = "SpellTomeFlames",
                Teaches = new BookSpell()
                {
                    Spell = spellLink,
                }
            };
            masterMod.Books.Add(spellBook);

            var staffLink = Skyrim.Weapon.StaffFlames;
            var staffEditorID = nameof(Skyrim.Weapon.StaffFlames);
            var enchantmentLink = Skyrim.ObjectEffect.StaffEnchFlames;
            var enchantmentEditorId = nameof(Skyrim.ObjectEffect.StaffEnchFlames);
            if (createStaffEnchantment)
            {
                var enchantment = new ObjectEffect(enchantmentLink.FormKey, masterMod.SkyrimRelease)
                {
                    EditorID = enchantmentEditorId,
                    EnchantType = ObjectEffect.EnchantTypeEnum.StaffEnchantment,
                };
                masterMod.ObjectEffects.Add(enchantment);

                if (createStaff)
                {
                    var originalStaff = new Weapon(staffLink.FormKey, masterMod.SkyrimRelease)
                    {
                        EditorID = staffEditorID,
                        Template = Skyrim.Weapon.StaffTemplateDestruction.AsNullable(),
                        ObjectEffect = enchantmentLink.AsNullable(),
                        Keywords = new()
                        {
                            Skyrim.Keyword.WeapTypeStaff
                        }
                    };
                    masterMod.Weapons.Add(originalStaff);
                }
            }

            var linkCache = loadOrder.ToImmutableLinkCache();

            Program program = new(loadOrder, linkCache, patchMod, new(new Settings()));


            program.RunPatch();


            linkCache = loadOrder.ToImmutableLinkCache();

            Assert.True(linkCache.TryResolve<ObjectEffect>(enchantmentEditorId, out var staffEnchantment));
            Assert.NotNull(staffEnchantment);
            if (createStaffEnchantment)
                Assert.Equal(enchantmentLink.AsGetter(), staffEnchantment?.ToLinkGetter());

            Assert.True(linkCache.TryResolve<IWeaponGetter>(staffEditorID, out var staff));
            Assert.NotNull(staff);
            if (createStaff)
                Assert.Equal(staffLink.AsGetter(), staff?.ToLinkGetter());

            Assert.Equal(staffEnchantment?.ToNullableLink(), staff?.ObjectEffect);
        }

        private void AddStaffTemplateDestruction()
        {
            var templateStaffLink = Skyrim.Weapon.StaffTemplateDestruction;
            var templateStaff = new Weapon(templateStaffLink.FormKey, masterMod.SkyrimRelease)
            {
                EditorID = "StaffTemplateDestruction",
                Keywords = new()
                {
                    Skyrim.Keyword.WeapTypeStaff
                }
            };
            masterMod.Weapons.Add(templateStaff);
        }

        private void AddLItemStaffDestruction00()
        {
            var leveledListLink = Skyrim.LeveledItem.LItemStaffDestruction00;
            var leveledListEditorId = nameof(Skyrim.LeveledItem.LItemStaffDestruction00);
            var leveledList = new LeveledItem(leveledListLink.FormKey, masterMod.SkyrimRelease)
            {
                EditorID = leveledListEditorId
            };
            masterMod.LeveledItems.Add(leveledList);
        }

        private void AddQAStaffContainer()
        {
            var qAStaffContainerLink = Skyrim.Container.QAStaffContainer;
            var qAStaffContainer = new Container(qAStaffContainerLink.FormKey, masterMod.SkyrimRelease);
            masterMod.Containers.Add(qAStaffContainer);
        }
    }
}
