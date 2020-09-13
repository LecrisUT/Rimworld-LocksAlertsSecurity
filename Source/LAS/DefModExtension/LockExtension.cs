using Verse;

namespace LAS
{
    public class LockExtension : DefModExtension
    {
        public float securityLevel = 1f;
        public float brokenSecurityFactor = 0.5f;
        public int unlockTime = 100;
        public bool automatic = false;
    }
}
