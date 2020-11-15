using LAS.Utility;
using RimWorld;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace LAS
{
    public class JobGiver_AILockpicker : ThinkNode_JobGiver
    {
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!pawn.HasLockpicks())
				return null;
			var intVec = pawn.mindState.duty.focus.Cell;
			if (intVec.IsValid && intVec.DistanceToSquared(pawn.Position) < 100f && intVec.GetRoom(pawn.Map) == pawn.GetRoom() && intVec.WithinRegions(pawn.Position, pawn.Map, 9, TraverseMode.NoPassClosedDoors))
			{
				pawn.GetLord().Notify_ReachedDutyLocation(pawn);
				return null;
			}
			if (!intVec.IsValid)
			{
				if (pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn)
					.Where(t => !t.ThreatDisabled(pawn) && t.Thing.Faction == Faction.OfPlayer && pawn.CanReach(t.Thing, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.PassDoors))
					.TryRandomElement(out var result))
					return null;
				intVec = result.Thing.Position;
			}
			if (!pawn.CanReach(intVec, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.PassDoors))
				return null;
			using (var path = pawn.Map.pathFinder.FindPath(pawn.Position, intVec, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors, false)))
			{
				if (path.FirstBlockingBuilding(out var cellBefore, pawn) is Building_Door door && door.HasDoorLock(out var doorLock))
				{
					if (!pawn.CanReserve(door))
						return WaitNearJob(pawn, cellBefore);
					var job = JobMaker.MakeJob(JobDefOf.Lockpick, door);
					foreach (var lockComp in doorLock.LockComps.Where(t => t.State.IsLocked()))
						job.AddQueuedTarget(TargetIndex.B, lockComp.parent);
					job.expiryInterval = JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange;
					job.checkOverrideOnExpire = true;
					return job;
				}
			}
			return JobMaker.MakeJob(RimWorld.JobDefOf.Goto, intVec, 500, checkOverrideOnExpiry: true);
		}
		private static Job WaitNearJob(Pawn pawn, IntVec3 cellBeforeBlocker)
		{
			var intVec = CellFinder.RandomClosewalkCellNear(cellBeforeBlocker, pawn.Map, 10);
			if (intVec == pawn.Position)
			{
				return JobMaker.MakeJob(RimWorld.JobDefOf.Wait, 20, checkOverrideOnExpiry: true);
			}
			return JobMaker.MakeJob(RimWorld.JobDefOf.Goto, intVec, 500, checkOverrideOnExpiry: true);
		}
	}
}
