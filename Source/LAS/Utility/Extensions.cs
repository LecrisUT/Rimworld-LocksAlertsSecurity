using Verse;

namespace LAS.Utility
{
    public static class Extensions
    {
        public static bool IsLock(this ThingWithComps thing, out LockComp lockComp)
            => (lockComp = thing.TryGetComp<LockComp>()) != null;
        public static bool IsLock(this ThingWithComps thing, out LockExtension lockExtension)
            => (lockExtension = thing?.def.GetModExtension<LockExtension>()) != null;
        public static bool IsLock(this ThingWithComps thing, out LockComp lockComp, out LockExtension lockExtension)
        {
            lockComp = thing.TryGetComp<LockComp>();
            lockExtension = thing?.def.GetModExtension<LockExtension>();
            return lockComp != null && lockExtension != null;
        }
        public static bool IsDoorLock(this ThingWithComps thing, out DoorLockComp doorLockComp)
            => (doorLockComp = thing.TryGetComp<DoorLockComp>()) != null;
    }
}
