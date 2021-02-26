using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using System.Collections.Generic;

namespace StaffGenerator
{
    public record Settings
    {
        public string StaffNamePrefix = "";

        public string StaffNameSuffix = " Staff";

        public HashSet<FormLink<IBookGetter>> SpellBooksToIgnore = new();

        public HashSet<FormLink<ISpellGetter>> SpellsToIgnore = new();

        public HashSet<FormLink<IWeaponGetter>> StavesToIgnore = new();
    }
}
