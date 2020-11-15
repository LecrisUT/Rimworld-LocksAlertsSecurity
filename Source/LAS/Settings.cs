using UnityEngine;
using Verse;

namespace LAS
{
    public class Settings : ModSettings
    {
        public static bool AllIDoor = false;
        public static int nCachedDoorLocks = 500;
        public static int minLockpickTime = 20;
        public static int maxLockpickTime = 500;
        public static float minLockpickSuccess = 0.01f;
        public static float maxLockpickSuccess = 1f;
        public void DoWindowContents(Rect wrect)
        {
            Listing_Standard options = new Listing_Standard();
            Color defaultColor = GUI.color;
            options.Begin(wrect);

            GUI.color = defaultColor;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public override void ExposeData()
        {
        }
    }
}
