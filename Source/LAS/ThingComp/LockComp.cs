using System.Collections.Generic;
using Verse;

namespace LAS
{
    public class LockComp : ThingComp
    {
        public Thing installedThing;
        public Lock Lock = new Lock();
        public Lock.LockState assignedState;
        private LockExtension extension;
        private Pawn pawn;
        public LockExtension Extension
        {
            get
            {
                if (extension == null)
                    extension = parent.def.GetModExtension<LockExtension>();
                return extension;
            }
        }
        public virtual void TryAutomaticLock()
        {

        }
        private int unlockTime = -1;
        public int UnlockTime
        {
            get
            {
                if (unlockTime < 0)
                    if (Extension != null)
                        unlockTime = Extension.unlockTime;
                    else
                        unlockTime = 10;
                return unlockTime;
            }
        }
        public bool NeedsToggling => Lock.State != assignedState;
        private float security = 0f;
        public bool cached_security = false;
        public virtual float Security
        {
            get
            {
                if (!cached_security)
                    UpdateSecurity();
                return security;
            }
        }
        public virtual void UpdateSecurity()
        {
            if (Extension != null)
                security = Extension.securityLevel;
            cached_security = true;
        }
        public virtual int ToggleTime => (NeedsToggling && !Lock.automatic) ? Extension.unlockTime : 0;
        public virtual bool ToggleLock() => Lock.TrySetState(assignedState);
        public virtual void AssignLockState(Lock.LockState lockState) => assignedState = lockState;
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new LockGizmo(parent, this);
        }
        public override void PostExposeData()
        {
            Scribe_References.Look(ref installedThing, "installedThings");
            Scribe_Deep.Look(ref Lock, "Lock");
            Scribe_Values.Look(ref assignedState, "assignedState");
        }
    }
}
