using RimWorld;
using Verse;

namespace LAS.Utility
{
    public static class Extensions
    {
        public static bool IsLock(this ThingWithComps thing, out LockComp lockComp)
            => (lockComp = thing.TryGetComp<LockComp>()) != null;
        public static bool IsLock(this ThingDef tdef, out CompProperties_Lock compProp)
            => (compProp = tdef.GetCompProperties<CompProperties_Lock>()) != null;
        // Door might have a lot of Comps => cache DoorLockComp if necessary
        public static bool HasDoorLock(this ThingWithComps thing, bool AddToDictionary = true)
            => thing is Building_Door door && door.HasDoorLock(AddToDictionary);
        public static bool HasDoorLock(this ThingWithComps thing, out DoorLockComp doorLockComp, bool AddToDictionary = true)
        {
            if (!(thing is Building_Door door))
            {
                doorLockComp = null;
                return false;
            }
            return door.HasDoorLock(out doorLockComp, AddToDictionary);
        }
        public static bool HasDoorLock(this Building_Door door, bool AddToDictionary = true)
            => door is IDoor || (!Settings.AllIDoor && door.HasDoorLock(out _, AddToDictionary));
        public static bool HasDoorLock(this Building_Door door, out DoorLockComp doorLockComp, bool AddToDictionary = true)
        {
            if (door is IDoor iDoor)
            {
                doorLockComp = iDoor.LockComp;
                return true;
            }
            if (!Dictionaries.DoorLocks.TryGetValue(door, out doorLockComp))
            {
                if (Dictionaries.DoorLocks.Count > Settings.nCachedDoorLocks)
                    Dictionaries.DoorLocks.Clear();
                doorLockComp = door.GetComp<DoorLockComp>();
                Dictionaries.DoorLocks.Add(door, doorLockComp);
            }
            return doorLockComp != null;
        }
        public static bool HasKeyHolderComp(this Pawn pawn, out KeyHolderComp comp, bool AddToDictionary = true)
        {
            if (!pawn.RaceProps.Humanlike)
            {
                comp = null;
                return false;
            }
            comp = pawn.GetComp<KeyHolderComp>();
            return comp != null;
        }
        // TO DO: write it up
        public static bool CanInteract(this KeyHolderComp keyHolder, DoorLockComp doorLockComp)
        {
            return true;
        }
        public static bool IsLocked(this LockComp.LockState state)
            => (state & LockComp.LockState.Locked) == LockComp.LockState.Locked;
        public static bool IsBroken(this LockComp.LockState state)
            => (state & LockComp.LockState.Broken) == LockComp.LockState.Broken;
        public static bool IsAutomatic(this LockComp.LockState state)
            => (state & LockComp.LockState.Automatic) == LockComp.LockState.Automatic;
        public static bool IsState(this LockComp.LockState state, LockComp.LockState otherState, LockComp.LockState mask = LockComp.LockState.Locked)
            => (state & mask) == (otherState & mask);
        public static void SetState(this ref LockComp.LockState state, LockComp.LockState newState, LockComp.LockState mask = LockComp.LockState.Locked)
        {
            // Keep unmasked states and set the rest
            state = (state & ~mask) | newState;
        }
        public static LockComp.LockState Toggle(this LockComp.LockState state, LockComp.LockState mask = LockComp.LockState.Locked)
            => state ^ mask;
        public static bool IsLocked(this Building_Door door)
        {
            if (!door.HasDoorLock(out var doorLock))
                return false;
            return doorLock.State.IsLocked();
        }
    }
}
