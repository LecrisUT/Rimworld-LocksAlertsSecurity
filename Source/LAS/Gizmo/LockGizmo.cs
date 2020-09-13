using LAS.Utility;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace LAS
{
    public class LockGizmo : Command_Target
    {
        private ThingWithComps lockThing;
        private LockComp Lock;
        public LockGizmo(ThingWithComps thing, LockComp Lock)
        {
            lockThing = thing;
            this.Lock = Lock;
            targetingParams = new TargetingParameters
            {
                thingCategory = ThingCategory.Building,
                canTargetPawns = false,
                canTargetBuildings = true,
                canTargetItems = false,
                onlyTargetFactions = new List<Faction>(1) { Find.FactionManager.OfPlayer},
                validator = delegate(TargetInfo target)
                {
                    var t = target.Thing as ThingWithComps;
                    if (t != null && t.IsDoorLock(out _))
                        return true;
                    return false;
                }
            };
            action = delegate (Thing door)
            {
                var manager = door.Map.designationManager;
                var desig = manager.DesignationOn(door, DesignationDefOf.ModifyDoorLock);
                if (desig == null || !(desig is MultipleThingDesignation mdesig))
                    door.Map.designationManager.AddDesignation(new MultipleThingDesignation(door, DesignationDefOf.ModifyDoorLock, new List<Thing> { lockThing }));
                else
                    mdesig.things.Add(lockThing);
            };
        }
    }
}
