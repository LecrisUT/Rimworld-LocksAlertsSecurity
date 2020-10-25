using LAS.Utility;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace LAS
{
    public class LockComp : ThingComp, ILoadReferenceable
    {
        public class LockThingPair : IEquatable<LockThingPair>
        {
            public ThingWithComps thing;
            public LockComp comp;
            public LockThingPair() { }
            public LockThingPair(ThingWithComps thing, bool getInfo = true)
            {
                this.thing = thing;
                if (getInfo)
                    thing.IsLock(out comp);
            }
            public LockThingPair(ThingWithComps thing, LockComp comp)
            {
                this.thing = thing;
                this.comp = comp;
            }
            public override bool Equals(object obj)
            {
                if (!(obj is LockThingPair pair))
                    return false;
                return comp == pair.comp;
            }
            public override int GetHashCode()
                => comp.GetHashCode();
            public bool Equals(LockThingPair other)
                => comp == other?.comp;

            public static explicit operator LockThingPair((ThingWithComps, LockComp) info) => new LockThingPair(info.Item1, info.Item2);
            public static explicit operator (ThingWithComps tool, LockComp comp)(LockThingPair info) => (info.thing, info.comp);
            public static explicit operator ThingWithComps(LockThingPair info) => info.thing;
            public static explicit operator LockComp(LockThingPair info) => info.comp;
        }
        public enum LockState
        {
            Default = 0b000,
            Locked = 0b001,
            Broken = 0b010,
            Automatic = 0b100,
        }
        private Effecter progressBar;
        public Thing installedThing;
        private LockState state;
        public LockState assignedState;
        public bool lockWhenAway = true;
        private float security;
        public Action postToggle;
        public KeyHolderComp keyHolder;
        private bool toggling = false;
        private int nextTickToToggle;
        private int ticksToToggle;
        public CompProperties_Lock CompProp => (CompProperties_Lock)props;
        public virtual LockState State => state;
        public bool Toggling => toggling;
        public bool NeedsToggling => state != assignedState;
        public virtual int ToggleTime => (NeedsToggling && !state.IsAutomatic()) ? CompProp.unlockTime : 0;
        public virtual float Security
        {
            get
            {
                if (!state.IsLocked())
                    return 0f;
                return MaxSecurity;
            }
        }
        public virtual float MaxSecurity
        {
            get
            {
                if (state.IsBroken())
                    return security * CompProp.brokenSecurityFactor;
                return security;
            }
        }
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            state.SetState(CompProp.automatic ? LockState.Automatic : LockState.Default, LockState.Automatic);
            security = CompProp.baseSecurityLevel;
            assignedState = state;
        }
        public virtual void AssignState(LockState state, LockState mask = LockState.Locked)
        {
            assignedState = this.state;
            assignedState.SetState(state, mask);
        }
        public virtual bool TrySetState(LockState state, LockState mask = LockState.Locked)
        {
            if (this.state.IsBroken())
                return false;
            this.state.SetState(state, mask);
            return true;
        }
        public virtual bool StartToggle(KeyHolderComp keyComp = null, bool withProgressBar = true)
            => StartToggle(!state.IsLocked(), keyComp, withProgressBar);
        public bool StartToggle(bool locked, KeyHolderComp keyComp = null, bool withProgressBar = true)
        {
            if (state.IsBroken())
                return false;
            if (state.IsLocked() == locked)
                return true;
            assignedState = state;
            assignedState.SetState(locked ? LockState.Locked : LockState.Default);
            keyHolder = keyComp;
            ticksToToggle = CompProp.unlockTime;
            if (withProgressBar)
            {
                progressBar = EffecterDefOf.ProgressBar.Spawn();
                progressBar.ticksLeft = ticksToToggle;
            }
            nextTickToToggle = Find.TickManager.TicksGame + ticksToToggle;
            toggling = true;
            return true;
        }
        public bool StartLock(KeyHolderComp keyComp = null, bool withProgressBar = true)
            => StartToggle(true, keyComp, withProgressBar);
        public bool StartUnlock(KeyHolderComp keyComp = null, bool withProgressBar = true)
            => StartToggle(false, keyComp, withProgressBar);
        public virtual void TryAutomaticLock()
        {
            if (state.IsBroken() || !state.IsAutomatic() || !lockWhenAway)
                return;
            StartToggle();
        }
        public void InterruptToggle()
        {
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
            var progress = Find.TickManager.TicksGame - nextTickToToggle;
            if (progressBar != null)
            {
                progressBar.EffectTick(installedThing ?? parent, keyHolder?.Pawn ?? TargetInfo.Invalid);
                if (progress < 0)
                {
                    var mote = ((SubEffecter_ProgressBar)progressBar.children[0]).mote;
                    if (mote != null)
                    {
                        mote.progress = 1 - Mathf.Clamp01((float)-progress / ticksToToggle);
                        mote.offsetZ = CompProp.progressBarOffset;
                    }
                }
            }
            if (progress > 0)
            {
                state = assignedState;
                postToggle?.Invoke();
                progressBar?.Cleanup();
                progressBar = null;
                toggling = false;
            }
        }
        public override void CompTickRare()
        {
            CompTick();
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new LockGizmo(parent, this);
        }
        public override void PostExposeData()
        {
            Scribe_References.Look(ref keyHolder, "keyHolder");
            Scribe_Values.Look(ref state, "state", LockState.Locked);
            Scribe_Values.Look(ref assignedState, "assignedState", state);
            Scribe_References.Look(ref installedThing, "installedThings");
            Scribe_Deep.Look(ref progressBar, "progressBar");
            Scribe_Values.Look(ref lockWhenAway, "lockWhenAway", false);
            Scribe_Values.Look(ref security, "security", CompProp.baseSecurityLevel);
            Scribe_Values.Look(ref toggling, "toggling", false);
            Scribe_Values.Look(ref nextTickToToggle, "nextTickToToggle", Find.TickManager.TicksGame - 1);
            Scribe_Values.Look(ref ticksToToggle, "ticksToToggle", ToggleTime);
        }
        public string GetUniqueLoadID() => parent.GetUniqueLoadID() + "_LockComp";
    }
}
