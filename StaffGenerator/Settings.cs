using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using System.Collections.Generic;

namespace StaffGenerator
{
    public record Settings
    {
        public string StaffNamePrefix = "";

        public string StaffNameSuffix = " Staff";

        public bool CopySpellEffectsToExistingStaffEnchantments = true;

        public bool OverrideNamesOfExistingStaves = false;

        public bool SetStaffPriceToSpellBookPrice = false;

        public HashSet<FormLink<IBookGetter>> SpellBooksNotToCreateStavesFor = new();

        public HashSet<FormLink<ISpellGetter>> SpellsNotToCreateStavesFor = new();

        public HashSet<FormLink<IObjectEffectGetter>> StaffEnchantmentsNotToRefresh = new();

        public HashSet<FormLink<IWeaponGetter>> StavesToNotRefreshRecipesFor = new();
    }
}
