using LAS.Utility;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace LAS
{
    public class JobDriver_Lockpick : JobDriver
    {
		public bool checkLocks = false;
		public int giveUpTime = 5000000;
		public override bool TryMakePreToilReservations(bool errorOnFailed)
			=> pawn.Reserve(job.targetA.Thing, job, errorOnFailed: errorOnFailed);
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			if (!(job.targetA.Thing is Building_Door door) || !door.HasDoorLock(out var doorLock))
			{
				JobFailReason.Is($"Invalid door [{job.targetA.Thing}]");
				yield break;
			}
			if (!pawn.HasKeyHolderComp(out var keyHolder))
			{
				JobFailReason.Is($"Invalid pawn [{pawn}]");
				yield break;
			}
			if (job.targetQueueB.NullOrEmpty())
				foreach (var lc in doorLock.LockComps.Where(t => t.State.IsLocked()))
					job.AddQueuedTarget(TargetIndex.B, lc.parent);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			var GetLocks = new Toil
			{
				initAction = delegate
				{
					//job.targetQueueB = new List<LocalTargetInfo>(doorLock.LockComps.Where(t => t.State.IsLocked()).Select(t => t.parent).Cast<LocalTargetInfo>());
					var queue = job.GetTargetQueue(TargetIndex.B);
					queue.RemoveAll(t => !(t.Thing is ThingWithComps twc) || !twc.IsLock(out var tlc) || tlc.InstalledThing != door);
					if (queue.NullOrEmpty() && checkLocks)
						foreach (var lc in doorLock.LockComps.Where(t => t.State.IsLocked()))
							job.AddQueuedTarget(TargetIndex.B, lc.parent);
				},
			};
			yield return GetLocks;
			var UnlockDoor = Toils_JobTransforms.SucceedOnNoTargetInQueue(TargetIndex.B);
			UnlockDoor.AddFinishAction(delegate
			{
				if (doorLock.State.IsLocked())
					return;
				door.StartManualOpenBy(pawn);
				pawn.stances.SetStance(new Stance_Cooldown(door.TicksTillFullyOpened, door, null) { neverAimWeapon = true });
			});
			yield return UnlockDoor;
			yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B, false);
			int ticksLeftToLockpick = 100;
			int ticksToLockpick = 1;
			LockComp lockComp = null;
			var lockpick = new Toil
			{
				initAction = delegate
				{
					if (!(TargetThingB is ThingWithComps twc) || !twc.IsLock(out lockComp))
						return;
					var num = 100f;
					num /= lockComp.InstalledThing.GetStatValue(StatDefOf.LockpickingSpeed);
					num /= pawn.GetStatValue(StatDefOf.LockpickingSpeed);
					ticksToLockpick = Mathf.Clamp(Mathf.FloorToInt(num), Settings.minLockpickTime, Settings.maxLockpickTime);
					ticksLeftToLockpick = ticksToLockpick;
				},
				tickAction = delegate
				{
					giveUpTime--;
					if (lockComp == null || !lockComp.State.IsLocked())
					{
						ReadyForNextToil();
						return;
					}
					if (pawn.skills != null)
					{
						if (Rand.Chance(0.3f))
							pawn.skills.Learn(SkillDefOf.Crafting, 0.01f);
						else
							pawn.skills.Learn(SkillDefOf.Intellectual, 0.01f);
					}
					if (--ticksLeftToLockpick <= 0)
					{
						var num = pawn.GetStatValue(StatDefOf.LockpickingSuccess);
						num += lockComp.InstalledThing.GetStatValue(StatDefOf.LockpickingSuccess);
						num = Mathf.Clamp(num, Settings.minLockpickSuccess, Settings.maxLockpickSuccess);
						if (!Rand.Chance(num))
						{
							ticksLeftToLockpick = ticksToLockpick;
							return;
						}
						lockComp.SetPin();
						if (!lockComp.State.IsLocked())
							ReadyForNextToil();
						else
							ticksLeftToLockpick = ticksToLockpick;
					}
				},
				defaultCompleteMode = ToilCompleteMode.Never,
			};
			lockpick.AddFailCondition(delegate
			{
				return giveUpTime < 0;
			});
			lockpick.WithProgressBar(TargetIndex.A, delegate
			{
				if (lockComp == null)
					return 0f;
				return (lockComp.BindingPin - (float)ticksLeftToLockpick / ticksToLockpick) / lockComp.Pins;
			});
			yield return lockpick;
			yield return Toils_Jump.Jump(GetLocks);
		}
	}
}
