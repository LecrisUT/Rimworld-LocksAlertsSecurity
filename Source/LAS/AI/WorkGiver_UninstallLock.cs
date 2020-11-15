using HugsLib.Utils;
using LAS.Utility;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace LAS
{
    public class WorkGiver_UninstallLock : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;
        public override Danger MaxPathDanger(Pawn pawn) => Danger.Deadly;

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            foreach (var des in pawn.Map.designationManager.allDesignations.Where(t => t.def == DesignationDefOf.UninstallLock))
                if (des.target.Thing is Building_Door door)
                    yield return door;
        }
        public override bool ShouldSkip(Pawn pawn, bool forced = false)
            => !pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.UninstallLock);
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.UninstallLock) == null || !(t is Building_Door))
                return false;
            if (!pawn.CanReserve(t, 1, -1, null, forced))
                return false;
            return true;
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!((ThingWithComps)t).HasDoorLock(out var doorLock))
                return null;
            var manager = pawn.Map.designationManager;
            var locks = doorLock.LockComps.Select(tt => tt.parent).Where(tt => manager.DesignationOn(tt, DesignationDefOf.UninstallLock) != null);
            if (locks.EnumerableNullOrEmpty())
            {
                manager.TryRemoveDesignationOn(t, DesignationDefOf.UninstallLock);
                return null;
            }
            var job = JobMaker.MakeJob(JobDefOf.UninstallLock, t);
            foreach (var tLock in locks)
                job.AddQueuedTarget(TargetIndex.B, tLock);
            return job;
        }
    }
}
