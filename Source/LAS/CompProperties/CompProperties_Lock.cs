using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace LAS
{
    public class CompProperties_Lock : CompProperties
    {
        public float baseSecurityLevel = 0.0f;
        public float brokenSecurityFactor = 0.5f;
        public int unlockTime = 100;
        public bool automatic = false;
        public float progressBarOffset = -0.5f;
        public int basePins = 4;
        public int maxPins = 4;
        public CompProperties_Lock() { compClass = typeof(LockComp); }
        public CompProperties_Lock(Type compClass) : base(compClass) { }
        public override void PostLoadSpecial(ThingDef parent)
        {
            if (typeof(Building_Door).IsAssignableFrom(parent.thingClass))
                compClass = typeof(DoorLockComp);
        }
        /*public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            if (req.HasThing)
                return null;
            return null;
        }*/
    }
}
