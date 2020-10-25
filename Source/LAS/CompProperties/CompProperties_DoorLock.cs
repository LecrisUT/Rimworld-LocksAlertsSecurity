using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace LAS
{
    public class CompProperties_DoorLock : CompProperties
    {
        public float progressBarOffset = -0.5f - 0.12f;
        public List<ThingDef> defaultLocks = new List<ThingDef>();
        public CompProperties_DoorLock() { compClass = typeof(DoorLockComp); }
        public CompProperties_DoorLock(Type compClass) : base(compClass) { }
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            if (req.HasThing)
                return null;
            return null;
        }
    }
}
