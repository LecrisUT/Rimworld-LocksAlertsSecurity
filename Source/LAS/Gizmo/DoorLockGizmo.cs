using LAS.Utility;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace LAS
{
    public class DoorLockGizmo : Command_Toggle
    {
        private static readonly float subIcon_yOffset = 5f;
        private static readonly float subIcon_xOffset = 2f;

        private static readonly Color subIcon_BaseColor = new Color(1.0f, 0.2f, 0.0f, 0.5f);
        private static readonly Color subIcon_MouseOverColor = new Color(1.0f, 0.2f, 0.0f, 0.8f);
        private static readonly Color subIcon_SelectColor = new Color(0.8f, 0.4f, 0.0f, 0.9f);
        private static bool subIcon_Selected = false;

        private enum interaction
        {
            None,
            MainButton,
            SubButton,
        }
        private static interaction Interaction;
        private Action subAction;
        private Action<LockComp> selectLockAction;
        public DoorLockGizmo(Building_Door door, DoorLockComp doorLock)
        {
            // icon = ContentFinder<Texture2D>.Get(DesignationDefOf.ToggleDoorLock.texturePath);
            isActive = delegate { return doorLock.State.IsLocked(); };
            toggleAction = delegate
            {
                var des = door.Map.designationManager;
                var togdes = des.DesignationOn(door, DesignationDefOf.ToggleDoorLock);
                if (togdes != null)
                    des.RemoveDesignation(togdes);
                else
                {
                    des.AddDesignation(new Designation(door, DesignationDefOf.ToggleDoorLock));
                    doorLock.assignedState = isActive() ? LockComp.LockState.Default : LockComp.LockState.Locked;
                }
            };
            subAction = delegate
            {
                var options = new List<FloatMenuOption>();
                if (doorLock.LockComps.EnumerableNullOrEmpty())
                    options.Add(new FloatMenuOption("No locks installed", () => { }));
                else
                    options.AddRange(LocksFloatMenu(doorLock.LockComps));
                Find.WindowStack.Add(new FloatMenu(options, null, false)
                {
                    givesColonistOrders = false,
                    vanishIfMouseDistant = true,
                    closeOnClickedOutside = false,
                });
            };
            selectLockAction = delegate (LockComp lockComp)
            {
                var options = new List<FloatMenuOption>(LockCompFloatMenu(lockComp));
                Find.WindowStack.Add(new FloatMenu(options));
            };
        }
        public override Color IconDrawColor => isActive() ? Color.red : Color.green;
        protected override void DrawIcon(Rect rect, Material buttonMat = null)
        {
            icon = isActive() ? ContentFinder<Texture2D>.Get("Icons/Locked") : ContentFinder<Texture2D>.Get("Icons/Unlocked");
            base.DrawIcon(rect, buttonMat);
        }
        protected override GizmoResult GizmoOnGUIInt(Rect butRect, bool shrunk = false)
        {
            // Invisible button has to be drawn on background to be clickable
            GizmoResult result;
            var height = butRect.height / 4;
            var width = butRect.width / 4;
            var LocksButton = new Rect(butRect.x + subIcon_xOffset, butRect.y + subIcon_yOffset, width, height);
            if (Widgets.ButtonInvisible(LocksButton))
            {
                result = new GizmoResult(GizmoState.Interacted, Event.current);
                Interaction = interaction.SubButton;
                base.GizmoOnGUIInt(butRect, shrunk);
                Log.Message($"Test 1.0: {Interaction} : {result.State} : {result.InteractEvent}");
            }
            else
            {
                result = base.GizmoOnGUIInt(butRect, shrunk);
                if (result.State == GizmoState.Interacted)
                    Interaction = interaction.MainButton;
                else
                    Interaction = interaction.None;
            }
            GUI.color = subIcon_Selected ? subIcon_SelectColor : Mouse.IsOver(LocksButton) ? subIcon_MouseOverColor : subIcon_BaseColor;
            GUI.DrawTexture(LocksButton, ContentFinder<Texture2D>.Get("Icons/Locked"));
            return result;
        }
        public override void ProcessInput(Event ev)
        {
            if (ev.button < 0)
                return;
            else
            {
                switch (Interaction)
                {
                    case interaction.MainButton:
                        base.ProcessInput(ev);
                        break;
                    case interaction.SubButton:
                        activateSound?.PlayOneShotOnCamera();
                        subAction();
                        break;
                }
            }

        }
        private IEnumerable<FloatMenuOptionNoClose> LocksFloatMenu(IEnumerable<LockComp> lockComps)
        {
            foreach (var lockComp in lockComps)
                yield return new FloatMenuOptionNoClose(lockComp.parent.LabelCapNoCount, delegate { selectLockAction(lockComp); }, lockComp.parent.def);
        }
        private static IEnumerable<FloatMenuOption> LockCompFloatMenu(LockComp lockComp)
        {
            yield return new FloatMenuOption("Uninstall", delegate
            {
                var door = lockComp.InstalledThing;
                var tLock = lockComp.parent;
                var manager = door.Map.designationManager;
                if (manager.DesignationOn(tLock, DesignationDefOf.UninstallLock) != null)
                    return;
                if (manager.DesignationOn(door, DesignationDefOf.UninstallLock) == null)
                    manager.AddDesignation(new Designation(door, DesignationDefOf.UninstallLock));
                manager.AddDesignation(new Designation(tLock, DesignationDefOf.UninstallLock));
            });
        }
        // Expanded from Brainz's formulation
        private class FloatMenuOptionNoClose : FloatMenuOption
        {
            public FloatMenuOptionNoClose(string label, Action action, ThingDef shownItemForIcon)
                : base(label, action, shownItemForIcon, MenuOptionPriority.Default, null, null, 0, null, null) { }
            public override bool DoGUI(Rect rect, bool colonistOrdering, FloatMenu floatMenu)
            {
                base.DoGUI(rect, colonistOrdering, floatMenu);
                return false; // don't close after an item is selected
            }
        }
    }
}
