using HugsLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace LAS
{
    internal class Controller : ModBase
    {
        public override void DefsLoaded()
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefs.Where(t => t.race?.Humanlike ?? false))
            {
                if (def.comps == null)
                    def.comps = new List<CompProperties>();
                def.comps.Add(new CompProperties(typeof(KeyHolderComp)));
            }
            foreach (var def in DefDatabase<ThingDef>.AllDefs.Where(t => typeof(Building_Door).IsAssignableFrom(t.thingClass)))
            {
                if (!def.HasComp(typeof(DoorLockComp)))
                {
                    if (def.comps == null)
                        def.comps = new List<CompProperties>();
                    def.comps.Add(new CompProperties_DoorLock());
                }
                /*if (def.thingClass == typeof(Building_Door))
                    def.thingClass = typeof(Door);*/
            }
        }
    }
}
