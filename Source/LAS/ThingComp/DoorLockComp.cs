using LAS.Utility;
using Verse;

namespace LAS
{
    public class DoorLockComp : ThingComp
    {
        public Petdoor petdoor;
        public ThingWithComps installedLock;
        private LockComp lockComp;
        public LockComp LockComp
        {
            get
            {
                if (lockComp == null)
                    installedLock.IsLock(out lockComp);
                return lockComp;
            }
        }
        private LockExtension lockExtension;
        public LockExtension LockExtension
        {
            get
            {
                if (lockExtension == null)
                    installedLock.IsLock(out lockExtension);
                return lockExtension;
            }
        }
        public float Security => DoorSecurity + LockSecurity;
        private float doorSecurity = 0f;
        private float lockSecurity = 0f;
        public bool securityCached = false;
        public float DoorSecurity
        {
            get
            {
                if (!securityCached)
                    UpdateSecurity();
                return doorSecurity;
            }
        }
        public float LockSecurity
        {
            get
            {
                if (!securityCached)
                    UpdateSecurity();
                return lockSecurity;
            }
        }
        public void UpdateSecurity()
        {
            var ext = parent.def.GetModExtension<LockExtension>();
            if (ext != null)
                doorSecurity = ext.securityLevel;
            ext = installedLock?.def.GetModExtension<LockExtension>();
            if (ext != null)
            {
                lockSecurity = ext.securityLevel;
                if (lockComp.Lock.state == Lock.LockState.Broken)
                    lockSecurity *= ext.brokenSecurityFactor;
            }
            securityCached = true;
        }
        public void ToggleLock()
        {

        }
        public override string CompInspectStringExtra()
        {
            return base.CompInspectStringExtra();
        }
        public override void PostExposeData()
        {
            Scribe_Deep.Look(ref petdoor, "petdoor");
            Scribe_Deep.Look(ref installedLock, "installedLock");
        }
    }
}
