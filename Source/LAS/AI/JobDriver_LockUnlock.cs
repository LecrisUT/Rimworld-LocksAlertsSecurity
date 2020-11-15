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
            var door = TargetThingA as Building_Door;
            door.HasDoorLock(out var doorLock);
            pawn.HasKeyHolderComp(out var keyHolder);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_Utility.LockUnlockDoor(keyHolder, door, doorLock);
            yield return Toils_General.RemoveDesignationsOnThing(TargetIndex.A, DesignationDefOf.ToggleDoorLock);
        }
    }
}
