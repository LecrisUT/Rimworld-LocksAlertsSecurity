using LAS.Utility;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace LAS
{
    public class JobDriver_UninstallLock : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, errorOnFailed: errorOnFailed);
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            var door = TargetThingA as Building_Door;
            if (!door.HasDoorLock(out var doorLock))
            {
                JobFailReason.Is($"Invalid door [{door}]");
                yield break;
            }
            var des = pawn.Map.designationManager;
            if (job.targetQueueB.NullOrEmpty())
                foreach (var tLock in doorLock.LockComps.Select(t => t.parent))
                    if (des.DesignationOn(tLock, DesignationDefOf.UninstallLock) != null)
                        job.AddQueuedTarget(TargetIndex.B, tLock);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            var GetLocks = new Toil
            {
                initAction = delegate
                {
                    var queue = job.GetTargetQueue(TargetIndex.B);
                    queue.RemoveAll(t => !(t.Thing is ThingWithComps twc) || !twc.IsLock(out var tlc) || tlc.InstalledThing != door);
                    if (queue.EnumerableNullOrEmpty())
                        des.TryRemoveDesignationOn(door, DesignationDefOf.UninstallLock);
                },
            };
            yield return GetLocks;
            yield return Toils_JobTransforms.SucceedOnNoTargetInQueue(TargetIndex.B);
            yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B, false);
            yield return Toils_General.WaitWith(TargetIndex.A, 1000, true);
            yield return Toils_General.Do(delegate
            {
                if (!((ThingWithComps)TargetThingB).IsLock(out var lockComp))
                    return;
                doorLock.RemoveLock(lockComp);
                des.TryRemoveDesignationOn(TargetThingB, DesignationDefOf.UninstallLock);
            });
            yield return Toils_Jump.Jump(GetLocks);
        }
    }
}
