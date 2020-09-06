using UnityEngine;
using Verse;

namespace LAS
{
    public class Settings : ModSettings
    {
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
