using RimWorld;
using Verse;

namespace LAS.Utility
{
    public static class LockpickUtility
    {
        public static bool IsGoodLockpicker(this Pawn p)
        {
            if (p.RaceProps.Humanlike && !p.skills.GetSkill(SkillDefOf.Crafting).TotallyDisabled && !StatDefOf.LockpickingSpeed.Worker.IsDisabledFor(p))
            {
                return p.skills.GetSkill(SkillDefOf.Crafting).Level >= 4;
            }
            return false;
        }
    }
}
