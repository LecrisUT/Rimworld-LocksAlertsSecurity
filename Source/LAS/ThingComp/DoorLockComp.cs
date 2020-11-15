using LAS.Utility;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace LAS
{
    public class DoorLockComp : ThingComp, ILoadReferenceable, IThingHolder
    {
        public class Petdoor : IExposable
        {
            public enum Size
            {
                Small,
                Medium,
                Large,
            }
            public Size size;
            public void ExposeData()
            {
                Scribe_Values.Look(ref size, "size");
            }
        }
        public Petdoor petdoor;
        public KeyHolderComp keyHolder;
        public LockComp.LockState assignedState;
        private Effecter progressBar;
        private LockComp currLock;
        public bool lockWhenAway = true;
        private bool toggling = false;
        private List<LockComp> locksToToggle = new List<LockComp>();
        public Building_Door Door => parent as Building_Door;
        public CompProperties_DoorLock CompProp => (CompProperties_DoorLock)props;
        public bool Toggling => toggling;
        private ThingOwner<ThingWithComps> lockOwner;
        private List<LockComp.LockThingPair> locks = new List<LockComp.LockThingPair>();
        public IEnumerable<LockComp.LockThingPair> Locks => locks;
        public IEnumerable<LockComp> LockComps
        {
            get
            {
                foreach (var pair in locks)
                    yield return pair.comp;
            }
        }
        public LockComp FirstLocked => LockComps.FirstOrDefault(t => t.State.IsLocked());
        public LockComp.LockState State 
        {
            get
            {
                if (locks.NullOrEmpty())
                    return LockComp.LockState.Default;
                var state = locks.First().comp.State;
                foreach (var pair in locks)
                {
                    var lockState = pair.comp.State;
                    // If any lock is locked, door is locked
                    state |= lockState & (LockComp.LockState)0b001;
                    // If all locks are broken, door lock is broken
                    // If all locks are automatic, door is automatic
                    state &= lockState | (LockComp.LockState)0b001;
                }
                return state;
            }
        }
        public int ToggleTime
        {
            get
            {
                var val = 0;
                foreach (var pair in locks)
                    val += pair.comp.ToggleTime;
                return val;
            }
        }
        public float Security
        {
            get
            {
                var val = 0f;
                foreach (var pair in locks)
                    val += pair.comp.Security;
                return val;
            }
        }
        public float MaxSecurity
        {
            get
            {
                var val = 0f;
                foreach (var pair in locks)
                    val += pair.comp.MaxSecurity;
                return val;
            }
        }
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            lockOwner = new ThingOwner<ThingWithComps>(this);
            var tickManager = Find.TickManager;
            foreach (var tLockDef in CompProp.defaultLocks)
            {
                var tLock = (ThingWithComps)ThingMaker.MakeThing(tLockDef, GenStuff.DefaultStuffFor(tLockDef));
                InstallLock(tLock);
                tickManager.RegisterAllTickabilityFor(tLock);
            }
        }
        public void InstallLock(LockComp.LockThingPair pair)
        {
            pair.comp.installedThing = parent;
            pair.comp.postToggle = delegate
            {
                var map = parent.MapHeld;
                map.pathGrid.RecalculatePerceivedPathCostUnderThing(parent);
                map.reachability.ClearCache();
                map.regionDirtyer.DirtyCells.Add(parent.Position);
            };
            if (!locks.Contains(pair))
                locks.Add(pair);
            if (!lockOwner.Contains(pair.thing))
                lockOwner.TryAddOrTransfer(pair.thing);
        }
        public void InstallLock(ThingWithComps lockThing)
        {
            if (!lockThing.IsLock(out var lockComp))
                if (lockOwner.Contains(lockThing))
                {
                    lockOwner.TryDrop(lockThing, ThingPlaceMode.Near, out _);
                    return;
                }
            var pair = new LockComp.LockThingPair(lockComp.parent, lockComp);
            InstallLock(pair);
        }
        public void InstallLock(LockComp lockComp)
        {
            var pair = new LockComp.LockThingPair(lockComp.parent, lockComp);
            InstallLock(pair);
        }
        public void RemoveLock(ThingWithComps lockThing, ThingOwner outOwner = null)
        {
            if (!lockThing.IsLock(out var lockComp))
            {
                if (lockOwner.Contains(lockThing))
                    lockOwner.TryDrop(lockThing, ThingPlaceMode.Near, out _);
                return;
            }
            if (lockComp.installedThing != parent)
                return;
            lockComp.installedThing = null;
            lockComp.postToggle = null;
            locks.RemoveAll(t => t.thing == lockThing);
            if (outOwner == null)
                lockOwner.TryDrop(lockThing, ThingPlaceMode.Near, out _);
            else
                lockOwner.TryTransferToContainer(lockThing, outOwner, false);
        }
        public void RemoveLock(LockComp lockComp, ThingOwner outOwner = null, Map map = null, IntVec3 pos = default)
        {
            if (lockComp.installedThing != parent)
                return;
            lockComp.installedThing = null;
            lockComp.postToggle = null;
            locks.RemoveAll(t => t.comp == lockComp);
            if (outOwner == null)
                lockOwner.TryDrop(lockComp.parent, map != null ? pos : parent.PositionHeld, map != null ? map : parent.MapHeld, ThingPlaceMode.Near, out _);
            else
                lockOwner.TryTransferToContainer(lockComp.parent, outOwner, false);
        }
        public virtual void StartToggle(KeyHolderComp keyComp = null, bool withProgressBar = true)
            => StartToggle(!State.IsLocked(), keyComp, withProgressBar);
        public void StartToggle(bool locked, KeyHolderComp keyComp = null, bool withProgressBar = true)
        {
            var ticksToToggle = 0;
            var waitTime = 0;
            locksToToggle.Clear();
            foreach (var lockComp in LockComps)
            {
                var lockState = lockComp.State;
                if (lockState.IsLocked() == locked || lockState.IsBroken())
                    continue;
                locksToToggle.Add(lockComp);
                if (lockState.IsAutomatic())
                {
                    lockComp.StartToggle(withProgressBar: withProgressBar);
                    waitTime = Math.Max(waitTime, lockComp.CompProp.unlockTime);
                }
                else
                    ticksToToggle += lockComp.CompProp.unlockTime;
            }
            waitTime = Math.Max(waitTime, ticksToToggle);
            if (keyHolder != null && keyHolder.Pawn.stances.curStance is Stance_ToggleDoor stance)
                stance.Interrupt();
            if (waitTime == 0)
                return;
            assignedState = locked ? LockComp.LockState.Locked : LockComp.LockState.Default;
            keyHolder = keyComp;
            if (keyHolder != null)
                keyHolder.Pawn.stances.SetStance(new Stance_ToggleDoor(waitTime, parent, null));
            if (withProgressBar)
            {
                progressBar = EffecterDefOf.ProgressBar.Spawn();
                progressBar.ticksLeft = waitTime;
            }
            toggling = true;
        }
        public void StartLock(KeyHolderComp keyComp = null, bool withProgressBar = true)
            => StartToggle(true, keyComp, withProgressBar);
        public void StartUnlock(KeyHolderComp keyComp = null, bool withProgressBar = true)
            => StartToggle(false, keyComp, withProgressBar);
        public void TryAutomaticLock()
        {
            foreach (var lockComp in LockComps)
                lockComp.TryAutomaticLock();
        }
        public LockComp GetNextLockToToggle(bool includeAutomatic = false)
            => LockComps.FirstOrFallback(t => !t.Toggling && !t.State.IsBroken() && !t.State.IsState(assignedState) && (includeAutomatic || !t.State.IsAutomatic()));
        public void InterruptToggle()
        {
            foreach (var lockComp in LockComps)
                lockComp.InterruptToggle();
            if (progressBar != null)
            {
                progressBar.Cleanup();
                progressBar = null;
            }
            toggling = false;
        }
        public override void CompTick()
        {
            if (!toggling)
                return;
            if (progressBar != null)
            {
                if (progressBar.ticksLeft <= 0)
                    progressBar.ticksLeft = 1;
                var progress = locksToToggle.Where(t => t.State.IsState(assignedState)).Count();
                progressBar.EffectTick(parent, keyHolder?.Pawn ?? TargetInfo.Invalid);
                var mote = ((SubEffecter_ProgressBar)progressBar.children[0]).mote;
                if (mote != null)
                {
                    mote.progress = Mathf.Clamp01((float)progress / locksToToggle.Count);
                    mote.offsetZ = CompProp.progressBarOffset;
                }
            }
            if (assignedState.IsLocked() && Door.Open)
                return;
            if (currLock == null)
                currLock = GetNextLockToToggle();
            if (currLock != null)
            {
                if (currLock.Toggling)
                    return;
                currLock = GetNextLockToToggle();
                currLock?.StartToggle(keyHolder, progressBar != null);
                return;
            }
            else if (!LockComps.Any(t => t.Toggling))
            {
                if (progressBar != null)
                {
                    progressBar.Cleanup();
                    progressBar = null;
                }
                toggling = false;
                locksToToggle.Clear();
            }
        }
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (mode == DestroyMode.Deconstruct)
                foreach (var tLock in lockOwner)
                    RemoveLock(tLock);
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new DoorLockGizmo(Door, this);
            if (Prefs.DevMode)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "Debug: Lock/Unlock door";
                command_Action.action = delegate
                {
                    foreach (var lockComp in LockComps)
                        lockComp.TrySetState(State.IsLocked() ? LockComp.LockState.Default : LockComp.LockState.Locked, (LockComp.LockState)0b001);
                };
                yield return command_Action;
            }
        }
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            yield return new FloatMenuOption("Lockpick", delegate
            {
                var job = JobMaker.MakeJob(JobDefOf.Lockpick, parent);
                foreach (var lockComp in LockComps.Where(t => t.State.IsLocked()))
                    job.AddQueuedTarget(TargetIndex.B, lockComp.parent);
                if (job.GetTargetQueue(TargetIndex.B).NullOrEmpty())
                    return;
                selPawn.jobs.StartJob(job, JobCondition.InterruptForced);
            });
        }
        public override void PostExposeData()
        {
            Scribe_References.Look(ref keyHolder, "keyHolder");
            Scribe_Values.Look(ref assignedState, "assignedState");
            Scribe_Deep.Look(ref progressBar, "progressBar");
            Scribe_References.Look(ref currLock, "currLock");
            Scribe_Values.Look(ref toggling, "toggling", false);
            Scribe_Collections.Look(ref locksToToggle, "locksToToggle", LookMode.Reference);
            Scribe_Deep.Look(ref petdoor, "petdoor");
            Scribe_Deep.Look(ref lockOwner, "lockOwner", this);
            Scribe_Values.Look(ref lockWhenAway, "lockWhenAway", true);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
                foreach (var tLock in lockOwner)
                    InstallLock(tLock);
        }
        public string GetUniqueLoadID() => parent.GetUniqueLoadID() + "_DoorLockComp";
        public void GetChildHolders(List<IThingHolder> outChildren) { }
        public ThingOwner GetDirectlyHeldThings()
            => lockOwner;
    }
}
