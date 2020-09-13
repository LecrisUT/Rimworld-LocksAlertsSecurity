using LAS.Utility;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace LAS
{
    public class DoorLockComp : LockComp
    {
        public LockComp this[ThingWithComps thing] => installedLocks.Contains(thing) ? thing.GetComp<LockComp>() : null;
        private Building_Door Door => parent as Building_Door;
        public Petdoor petdoor;
        public Lock.LockState LockState
        {
            get
            {
                var state = Lock.State;
                if (installedLocks.NullOrEmpty() || state == Lock.LockState.Locked)
                    return state;
                foreach (var lockComp in LockComps)
                {
                    switch (lockComp.Lock.State)
                    {
                        case Lock.LockState.Locked:
                            return Lock.LockState.Locked;
                        case Lock.LockState.Broken:
                            state = Lock.LockState.Broken;
                            break;
                    }
                }
                return state;
            }
        }
        public override void TryAutomaticLock()
        {
            base.TryAutomaticLock();
            foreach (var lockComp in LockComps)
                lockComp.TryAutomaticLock();
        }
        public bool cached_installedLocks = false;
        private List<ThingWithComps> installedLocks = new List<ThingWithComps>();
        public List<ThingWithComps> InstalledLocksList
        {
            get
            {
                cached_installedLocks = false;
                return installedLocks;
            }
        }
        public IEnumerable<ThingWithComps> InstalledLocks
        {
            get
            {
                if (!cached_installedLocks)
                    UpdateLocks();
                return installedLocks;
            }
        }
        private List<LockComp> lockComps;
        public IEnumerable<LockComp> LockComps
        {
            get
            {
                if (!cached_installedLocks)
                    UpdateLocks();
                return lockComps;
            }
        }
        public void UpdateLocks()
        {
            lockComps = new List<LockComp>();
            foreach (var thing in installedLocks.ToList())
                if (thing.IsLock(out LockComp lockComp))
                    lockComps.Add(lockComp);
                else
                    installedLocks.Remove(thing);
            cached_security = false;
            cached_installedLocks = true;
        }
        public IEnumerable<LockExtension> LockExtensions { get => LockComps.Select(t => t.Extension); }
        public override int ToggleTime
        {
            get
            {
                var val = base.ToggleTime;
                foreach (var lockComp in LockComps)
                    val += lockComp.ToggleTime;
                return val;
            }
        }
        public override float Security
        {
            get
            {
                var val = base.Security;
                foreach (var lockComp in lockComps)
                    val += lockComp.Security;
                return val;
            }
        }
        public override bool ToggleLock()
        {
            var result = base.ToggleLock();
            foreach (var lockComp in LockComps)
                result &= lockComp.ToggleLock();
            var map = Door.Map;
            map.pathGrid.RecalculatePerceivedPathCostUnderThing(Door);
            map.reachability.ClearCache();
            map.regionDirtyer.DirtyCells.Add(Door.Position);
            return result;
        }
        public override void AssignLockState(Lock.LockState lockState)
        {
            base.AssignLockState(lockState);
            foreach (var lockComp in LockComps)
                lockComp.AssignLockState(lockState);
        }
        public override string CompInspectStringExtra()
        {
            return base.CompInspectStringExtra();
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new DoorLockGizmo(Door, this);
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref petdoor, "petdoor");
            Scribe_Collections.Look(ref installedLocks, "installedLocks", LookMode.Reference);
        }
    }
}
