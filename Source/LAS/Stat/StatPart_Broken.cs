using LAS.Utility;
using RimWorld;
using Verse;

namespace LAS
{
    public class StatPart_Broken : StatPart
    {
        float effect = 1f;
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!(req.Thing is ThingWithComps twc) || !twc.IsLock(out var lockComp))
                return;
            if (lockComp.State.IsBroken())
                val *= lockComp.CompProp.brokenSecurityFactor * effect;
        }
        public override string ExplanationPart(StatRequest req)
        {
            if (!(req.Thing is ThingWithComps twc) || !twc.IsLock(out var lockComp) || !lockComp.State.IsBroken())
                return null;
            return $"Broken: x{lockComp.CompProp.brokenSecurityFactor * effect}";
        }
    }
}
