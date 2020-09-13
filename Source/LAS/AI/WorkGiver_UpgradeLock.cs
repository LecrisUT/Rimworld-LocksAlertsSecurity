using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace LAS
{
    /*public class WorkGiver_UpgradeLock : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;
        public override Danger MaxPathDanger(Pawn pawn) => Danger.Deadly;

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            foreach (var des in pawn.Map.designationManager.allDesignations.Where(t => t.def == DesignationDefOf.UpgradeDoorLock))
                yield return des.target.Thing;
        }
        public override bool ShouldSkip(Pawn pawn, bool forced = false)
            => !pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.UpgradeDoorLock);
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.UpgradeDoorLock) == null)
                return false;
            return true;
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
            => JobMaker.MakeJob(JobDefOf.UpgradeLock, t);
    }*/
}
