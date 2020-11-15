using LAS.Utility;
using RimWorld;
using Verse;

/*namespace LAS
{
    public class StatWorker_Secuirty : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req)
            => req.Def is ThingDef tDef && (tDef.HasDoorLock(out _) || tDef.IsLock(out _));
        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            if (req.Thing is ThingWithComps twc && twc.IsLock(out var lockComp))
            {
                if (lockComp.State.IsBroken())
                    return base.GetValueUnfinalized(req, applyPostProcess) * lockComp.CompProp.brokenSecurityFactor;
                return base.GetValueUnfinalized(req, applyPostProcess);
            }
            else if (req.Thing.HasDoorLock(out var doorLock))
            {
                foreach (var )
                return base.GetValueUnfinalized(req, applyPostProcess);
            }
            return 0f;
        }
    }
}*/
