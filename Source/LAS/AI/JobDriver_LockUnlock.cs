using LAS.Utility;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace LAS
{
    public class JobDriver_LockUnlock : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            var door = TargetThingA as ThingWithComps;
            door.IsDoorLock(out var doorLockComp);
            var des = door.Map.designationManager;
            //this.FailOn(delegate
            //{
            //    return des.DesignationOn(door, DesignationDefOf.ToggleDoorLock) == null;
            //});
            var time = doorLockComp.UnlockTime;
            // yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.WaitWith(TargetIndex.A, time, true);
            var toilLockUnlock = Toils_General.Do(delegate
            {
                doorLockComp.ToggleLock();
                SoundDefOf.FlickSwitch.PlayOneShot(door);
                des.TryRemoveDesignationOn(door, DesignationDefOf.ToggleDoorLock);
            });
            toilLockUnlock.defaultCompleteMode = ToilCompleteMode.FinishedBusy;
            yield return toilLockUnlock;
        }
    }
}
