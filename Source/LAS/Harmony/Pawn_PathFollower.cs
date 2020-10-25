using HarmonyLib;
using LAS.Utility;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

namespace LAS.Harmony
{
    [HarmonyPatch(typeof(Pawn_PathFollower), "TryEnterNextPathCell")]
    public static class Patch_Pawn_PathFollower_TryEnterNextPathCell
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var instructionList = instructions.ToList();
            var get_FreePassage = AccessTools.PropertyGetter(typeof(Building_Door), nameof(Building_Door.FreePassage));
            var pawn = AccessTools.Field(typeof(Pawn_PathFollower), "pawn");
            var Stance_Cooldown = AccessTools.Constructor(typeof(Stance_Cooldown), new Type[] { typeof(int), typeof(LocalTargetInfo), typeof(Verb) });
            var Stance_ToggleDoor = AccessTools.Constructor(typeof(Stance_ToggleDoor), new Type[] { typeof(int), typeof(LocalTargetInfo), typeof(Verb) });
            var SlowsPawn = AccessTools.PropertyGetter(typeof(Building_Door), nameof(Building_Door.SlowsPawns));
            var BlockedOpenMomentary = AccessTools.PropertyGetter(typeof(Building_Door), nameof(Building_Door.BlockedOpenMomentary));
            for (int i = 0; i < instructionList.Count; i++)
            {
                var instruction = instructionList[i];
                if (instruction.Calls(get_FreePassage) && instructionList[i - 1].IsLdloc())
                {
                    var BuildingDoor1 = instructionList[i - 1];
                    if (instructionList[i + 1].opcode == OpCodes.Brtrue || instructionList[i + 1].opcode == OpCodes.Brtrue_S)
                    {
                        var label = generator.DefineLabel();
                        var doorLockComp = generator.DeclareLocal(typeof(DoorLockComp));
                        instructionList[i + 2].labels.Add(label);
                        instructionList.InsertRange(i + 2, new List<CodeInstruction>
                        {
                            new CodeInstruction(OpCodes.Ldarg_0, null),
                            new CodeInstruction(OpCodes.Ldfld, pawn),
                            BuildingDoor1,
                            new CodeInstruction(OpCodes.Ldloca, doorLockComp),
                            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_Pawn_PathFollower_TryEnterNextPathCell), nameof(CanLockpick))),
                            new CodeInstruction(OpCodes.Brfalse, label),
                            new CodeInstruction(OpCodes.Ldarg_0, null),
                            new CodeInstruction(OpCodes.Ldfld, pawn),
                            new CodeInstruction(OpCodes.Ldloc, doorLockComp),
                            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_Pawn_PathFollower_TryEnterNextPathCell), nameof(TryLockpick))),
                            new CodeInstruction(OpCodes.Ret),
                        });
                        i += 10;
                        continue;
                    }
                    else
                        throw new NotImplementedException("Temp error  46873546");
                }
                if (instruction.opcode == OpCodes.Newobj && instruction.OperandIs(Stance_Cooldown))
                {
                    var unlockLabel = generator.DefineLabel();
                    var continueLabel = generator.DefineLabel();
                    instructionList[i].labels.Add(unlockLabel);
                    instructionList[i + 1].labels.Add(continueLabel);
                    instructionList.InsertRange(i, new List<CodeInstruction>
                    {
                        instructionList[i - 3],
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Extensions), nameof(Extensions.IsLocked), new Type[] { typeof(Building_Door) })),
                        new CodeInstruction(OpCodes.Brfalse, unlockLabel),
                        new CodeInstruction(OpCodes.Newobj, Stance_ToggleDoor),
                        new CodeInstruction(OpCodes.Br, continueLabel),
                    });
                    i += 5;
                    continue;
                }
                if (instruction.Calls(BlockedOpenMomentary))
                {
                    instructionList[i].operand = AccessTools.Method(typeof(Patch_Pawn_PathFollower_TryEnterNextPathCell), nameof(BlockedOpen));
                    continue;
                }
                if (instruction.Calls(SlowsPawn))
                {
                    instructionList.RemoveRange(i - 1, 3);
                    return instructionList;
                }
                /*if (instruction.Calls(NextCellDoorToWaitForOrManuallyOpen))
                {
                    var label = generator.DefineLabel();
                    var prevBuildingDoor = generator.DeclareLocal(typeof(Building_Door));
                    instructionList[i - 1].labels.Add(label);
                    instructionList.InsertRange(i - 1, new List<CodeInstruction>
                    {

                    });
                    break;
                }*/
                /*if (instruction.Calls(set_Position))
                {
                    instructionList.InsertRange(i+1, new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Ldarg_0, null),
                        new CodeInstruction(OpCodes.Ldfld, lastCell),
                        new CodeInstruction(OpCodes.Ldarg_0, null),
                        new CodeInstruction(OpCodes.Ldfld, pawn),
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_Pawn_PathFollower_TryEnterNextPathCell), nameof(PrevDoorToBeClosed))),
                    })
                }*/
            }
            return instructionList;
        }
        /*private static Building_Door PrevDoorToBeClosed(IntVec3 lastCell, Pawn pawn)
        {
            var door = pawn.Map.thingGrid.ThingAt<Building_Door>(lastCell);
            if (door != null && door.HasDoorLock(out var doorLockComp) && door.PawnCanLock(pawn) && doorLockComp.StillUnlocked(true))
                return door;
            return null;
        }*/
        private static bool BlockedOpen(Building_Door door)
        {
            if (!door.HasDoorLock(out var doorLockComp))
                return door.BlockedOpenMomentary;
            var thingList = door.Position.GetThingList(door.Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                Thing thing = thingList[i];
                if (thing.def.category == ThingCategory.Item)
                    return true;
                else if (thing is Pawn pawn)
                {
                    if (pawn.Downed)
                        return true;
                    // if (pawn.HasKeyHolderComp(out var keyHolder, false) && keyHolder.CanInteract(doorLockComp))
                    //     return true;
                }
            }
            return false;
        }
        public static bool CanLockpick(this Pawn pawn, Building_Door door, out DoorLockComp lockComp)
        {
            if (!door.HasDoorLock(out lockComp) || !pawn.HostileTo(door))
                return false;
            // if (pawn.GetStatValue(StatDefOf.LockPickSkill) > lockComp.MaxSecurity)
            //     return true;
            return false;
        }
        public static void TryLockpick(this Pawn pawn, DoorLockComp lockComp)
        {
        }
    }
}
