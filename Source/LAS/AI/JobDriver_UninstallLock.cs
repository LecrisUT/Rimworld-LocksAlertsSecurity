using LAS.Utility;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace LAS
{
    public class JobDriver_UninstallLock : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, errorOnFailed: errorOnFailed) && pawn.Reserve(job.targetB, job, errorOnFailed: errorOnFailed);
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            var lockThing = TargetThingA as ThingWithComps;
            var door = TargetThingB as ThingWithComps;
            if (!door.HasDoorLock(out var doorLock) || !lockThing.IsLock(out var lockComp) || lockComp.installedThing != door)
            {
                JobFailReason.Is($"Invalid lock [{lockThing}] and/or door [{door}]");
                yield break;
            }
            var des = pawn.Map.designationManager;
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
            yield return Toils_General.WaitWith(TargetIndex.A, 1000, true);
            yield return Toils_General.Do(delegate
            {
                doorLock.RemoveLock(lockComp);
                des.TryRemoveDesignationOn(door, DesignationDefOf.UninstallDoorLock);
            });
        }
    }
}
