using LAS.Utility;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace LAS
{
    public class JobDriver_UpgradeLock : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, errorOnFailed: errorOnFailed);
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            return null;
            /*var lockThing = TargetThingA as ThingWithComps;
            var door = TargetThingB as ThingWithComps;
            if (!door.HasDoorLock(out var doorLock) || !lockThing.IsLock(out var lockComp))
            {
                JobFailReason.Is($"Invalid lock [{lockThing}] and/or door [{door}]");
                yield break;
            }
            var des = pawn.Map.designationManager;
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            var carry = Toils_Haul.StartCarryThing(TargetIndex.A);
            carry.AddPreInitAction(delegate
            {
                des.TryRemoveDesignationOn(lockThing, DesignationDefOf.InstallLock);
            });
            yield return carry;
            yield return Toils_Haul.CarryHauledThingToContainer();
            yield return Toils_General.WaitWith(TargetIndex.A, 1000, true);
            yield return Toils_General.Do(delegate
            {
                doorLock.InstallLock(lockComp);
                des.TryRemoveDesignationOn(door, DesignationDefOf.InstallLock);
            });*/
        }
    }
}
