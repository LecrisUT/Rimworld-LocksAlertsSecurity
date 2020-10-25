using HarmonyLib;
using LAS.Utility;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace LAS.Harmony
{
    [HarmonyPatch(typeof(Building_Door))]
    public static class Patch_Building_Door
    {
        [HarmonyPatch(nameof(Building_Door.PawnCanOpen))]
        [HarmonyPrefix]
        public static bool Prefix_PawnCanOpen(ref bool __result, Building_Door __instance, Pawn p)
        {
            if (__instance.FreePassage || !__instance.HasDoorLock(out var doorLockComp))
                return true;
            if (doorLockComp.State.IsLocked())
                LockedDoor(ref __result, p, doorLockComp);
            else
                UnlockedDoor(ref __result, p);
            return false;
        }
        private static void UnlockedDoor(ref bool result, Pawn pawn)
        {
            result = true;
        }
        private static void LockedDoor(ref bool result, Pawn pawn, DoorLockComp doorLockComp)
        {
            if (!pawn.HostileTo(doorLockComp.Door) && pawn.HasKeyHolderComp(out var keyHolder) && keyHolder.CanInteract(doorLockComp))
                result = true;
            else
                result = false;
        }
        [HarmonyPatch("DoorTryClose")]
        [HarmonyPostfix]
        public static void Postfix_DoorTryClose(bool __result, Building_Door __instance)
        {
            if (__result == true && __instance.HasDoorLock(out var lockComp))
                lockComp.TryAutomaticLock();
        }
        [HarmonyPatch(nameof(Building_Door.StartManualOpenBy))]
        [HarmonyPrefix]
        public static bool Prefix_StartManualOpenBy(Building_Door __instance, Pawn opener)
        {
            if (!__instance.HasDoorLock(out var doorLockComp))
                return true;
            if (!opener.HasKeyHolderComp(out var keyComp))
                return !doorLockComp.State.IsLocked();
            if (!doorLockComp.State.IsLocked())
            {
                if (doorLockComp.Toggling)
                    doorLockComp.InterruptToggle();
                return true;
            }
            if (doorLockComp.Toggling)
                return false;
            doorLockComp.StartUnlock(keyComp, true);
            return false;
        }
        [HarmonyPatch(nameof(Building_Door.StartManualCloseBy))]
        [HarmonyPostfix]
        public static void Postfix_StartManualCloseBy(Building_Door __instance, Pawn closer)
        {
            if (!__instance.HasDoorLock(out var doorLockComp) || !closer.HasKeyHolderComp(out var keyComp) || !doorLockComp.lockWhenAway)
                return;
            var map = __instance.MapHeld;
            var mapTracker = map.GetComponent<MapTracker>();
            foreach (var keyHolder in mapTracker.keyHolders.Where(t=> t.Pawn != closer && !t.Pawn.HostileTo(__instance) && t.CanInteract(doorLockComp)))
            {
                var path = keyHolder.Pawn.pather.curPath;
                if (path == null)
                    continue;
                for (int i = 0; i < path.NodesLeftCount; i++)
                    if (path.Peek(i).GetDoor(map) == __instance)
                        return;
            }
            doorLockComp.StartLock(keyComp, true);
        }
        /*[HarmonyPatch(nameof(Building_Door.TicksToOpenNow), MethodType.Getter)]
        [HarmonyPostfix]
        public static void Postfix_TicksToOpenNow(Building_Door __instance, ref int __result)
        {
            if (!__instance.HasDoorLock(out var lockComp))
                return;
            __result += lockComp.ToggleTime;
        }*/
        [HarmonyPatch(nameof(Building_Door.SlowsPawns), MethodType.Getter)]
        [HarmonyPrefix]
        public static bool Prefix_SlowsPawns(Building_Door __instance, ref bool __result)
        {
            if (!__instance.HasDoorLock(out var doorLockComp))
                return true;
            var state = doorLockComp.State;
            if (state.IsAutomatic() || !state.IsLocked())
                return true;
            __result = true;
            return false;
        }
        [HarmonyPatch(nameof(Building_Door.Notify_PawnApproaching))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler_Notify_PawnApproaching(IEnumerable<CodeInstruction> instructions)
        {
            var instructionList = instructions.ToList();
            var DoorOpen = AccessTools.Method(typeof(Building_Door), "DoorOpen");
            for (int i = 0; i < instructionList.Count; i++)
            {
                var instruction = instructionList[i];
                if (instruction.Calls(DoorOpen))
                {
                    instructionList.InsertRange(i + 1, new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Ldarg_0, null),
                        new CodeInstruction(OpCodes.Ldarg_1, null),
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_Building_Door), nameof(Notify_PawnApproaching))),
                    });
                    return instructionList;
                }
            }
            throw new Exception();
        }
        public static void Notify_PawnApproaching(Building_Door door, Pawn pawn)
        {
            if (!door.HasDoorLock(out var doorLockComp) || !pawn.HasKeyHolderComp(out var keyHolder))
                return;
            doorLockComp.StartUnlock(withProgressBar: true);
        }
    }
}
