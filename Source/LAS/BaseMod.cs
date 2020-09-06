using UnityEngine;
using Verse;

namespace LAS
{
    public class LAS : Mod
    {
        public static LAS thisMod;
        public Settings settings;

        public LAS(ModContentPack content) : base(content)
        {
            thisMod = this;
        }

        public override string SettingsCategory() => "LockpickLock".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            GetSettings<Settings>().DoWindowContents(inRect);
        }
    }
}
