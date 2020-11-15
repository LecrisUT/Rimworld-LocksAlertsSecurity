using LAS.Utility;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace LAS
{
    public static class Toils_Utility
    {
        public static Toil LockUnlockDoor(KeyHolderComp keyHolder, Building_Door door, DoorLockComp doorLock)
            => Toils_General.Do(delegate
            {
                doorLock.StartToggle(doorLock.assignedState.IsLocked(), keyHolder, true);
                SoundDefOf.FlickSwitch.PlayOneShot(door);
            });
        public static Toil LockUnlockLock(KeyHolderComp keyHolder, LockComp lockComp)
            => Toils_General.Do(delegate
            {
                lockComp.StartToggle(false, keyHolder, true);
                keyHolder.Pawn.stances.SetStance(new Stance_ToggleLock(lockComp.CompProp.unlockTime, lockComp.parent, null));
            });
    }
}
