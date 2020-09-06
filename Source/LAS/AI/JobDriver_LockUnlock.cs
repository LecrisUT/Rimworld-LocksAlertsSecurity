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
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            var door = TargetThingA as ThingWithComps;
            door.IsDoorLock(out var doorLockComp);
            var time = doorLockComp.LockExtension.unlockTime;
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            var toilLockUnlock = Toils_General.WaitWith(TargetIndex.A, time);
            toilLockUnlock.AddFinishAction(delegate
            {
                doorLockComp.ToggleLock();
                SoundDefOf.FlickSwitch.PlayOneShot(door);
            });
        }
    }
}
