using HarmonyLib;
using LAS.Utility;
using RimWorld;
using Verse;

namespace LAS.Harmony
{
    [HarmonyPatch(typeof(Building_Door), nameof(Building_Door.PawnCanOpen))]
    public class Patch_Building_Door_PawnCanOpen
    {
        public static bool Prefix(ref bool __result, Building_Door __instance, Pawn p)
        {
            if (!__instance.IsDoorLock(out var doorLockComp))
                return true;
            switch (doorLockComp.LockState)
            {
                case Lock.LockState.Unlocked:
                    UnlockedDoor(ref __result, p);
                    return false;
                case Lock.LockState.Locked:
                    LockedDoor(ref __result, p, doorLockComp);
                    return false;
                case Lock.LockState.Broken:
                    BrokenDoor(ref __result, p, doorLockComp);
                    return false;
            }
            return true;
        }
        private static void UnlockedDoor(ref bool result, Pawn pawn)
        {
            result = true;
        }
        private static void LockedDoor(ref bool result, Pawn pawn, DoorLockComp doorLockComp)
        {
            result = false;
        }
        private static void BrokenDoor(ref bool result, Pawn pawn, DoorLockComp doorLockComp)
        {
            result = true;
        }
    }
    [HarmonyPatch(typeof(Building_Door), "DoorTryClose")]
    public class Patch_Building_Door_DoorTryClose
    {
        public static void Postfix(bool __result, Building_Door __instance)
        {
            if (__result == true && __instance.IsDoorLock(out var doorLockComp))
                doorLockComp.TryAutomaticLock();
        }
    }
}
