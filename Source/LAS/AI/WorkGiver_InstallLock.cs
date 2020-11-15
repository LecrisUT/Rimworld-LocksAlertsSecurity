using LAS.Utility;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace LAS
{
    public class WorkGiver_InstallLock : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;
        public override Danger MaxPathDanger(Pawn pawn) => Danger.Deadly;

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            foreach (var des in pawn.Map.designationManager.allDesignations.Where(t => t.def == DesignationDefOf.InstallLock))
            {
                var thing = des.target.Thing as ThingWithComps;
                if (thing.IsLock(out _))
                    yield return thing;
            }
        }
        public override bool ShouldSkip(Pawn pawn, bool forced = false)
            => !pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.InstallLock);
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.InstallLock) == null || !((ThingWithComps)t).IsLock(out _))
                return false;
            if (!pawn.CanReserve(t, 1, -1, null, forced))
                return false;
            return true;
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!((ThingWithComps)t).IsLock(out var lockComp))
                return null;
            var job = JobMaker.MakeJob(JobDefOf.InstallLock, t, lockComp.AssignedThing);
            job.count = 1;
            return job;
        }
    }
}
