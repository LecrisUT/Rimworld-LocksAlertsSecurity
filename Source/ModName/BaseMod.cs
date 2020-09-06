using UnityEngine;
using Verse;

namespace ModName
{
    public class ModName : Mod
    {
        public static ModName thisMod;
        public Settings settings;

        public ModName(ModContentPack content) : base(content)
        {
            thisMod = this;
        }

        public override string SettingsCategory() => "ModName".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            GetSettings<Settings>().DoWindowContents(inRect);
        }
    }
}
