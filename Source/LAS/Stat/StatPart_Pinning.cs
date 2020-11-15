using LAS.Utility;
using RimWorld;
using Verse;

namespace LAS
{
    public class StatPart_Pinning : StatPart
    {
        float effect = 1f;
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!(req.Thing is ThingWithComps twc) || !twc.IsLock(out var lockComp))
                return;
            val *= lockComp.Security * effect;
        }
        public override string ExplanationPart(StatRequest req)
        {
            if (!(req.Thing is ThingWithComps twc) || !twc.IsLock(out var lockComp))
                return null;
            return $"Custom pinning: x{lockComp.Security * effect}";
        }
    }
}
