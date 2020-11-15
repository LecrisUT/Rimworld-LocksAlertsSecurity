using RimWorld;
using Verse;
using Verse.AI.Group;

namespace LAS.Storyteller
{
    public class LordJob_AssaultColony : RimWorld.LordJob_AssaultColony
    {
        protected bool sappers = false;
        protected bool lockpicker = false;
        public LordJob_AssaultColony(Faction assaulterFaction, bool lockpicker = false, bool sappers = false) : base(assaulterFaction, sappers: sappers, useAvoidGridSmart: true)
        {
            this.sappers = sappers;
            this.lockpicker = lockpicker;
        }
        public override StateGraph CreateGraph()
        {
            var stateGraph = new StateGraph();
            var baseGraph = base.CreateGraph();
            var locpickerToil = new LordToil_AssaultColonyLockpicker
            {
                useAvoidGrid = true,
            };
            if (lockpicker)
            {
                stateGraph.AddToil(locpickerToil);
                var assaultToil = sappers ? baseGraph.lordToils[1] : baseGraph.lordToils[0];
                var lockpickToLockpick = new Transition(locpickerToil, locpickerToil, canMoveToSameState: true);
                lockpickToLockpick.AddTrigger(new Trigger_PawnLost());
                stateGraph.AddTransition(lockpickToLockpick);
                var lockpickToAssault = new Transition(locpickerToil, assaultToil);
                lockpickToAssault.AddTrigger(new Trigger_NoFightingLockpickers());
                stateGraph.AddTransition(lockpickToAssault);
                //foreach (var trans in baseGraph.transitions)
                //     trans.AddSource(locpickerToil);
            }
            stateGraph.AttachSubgraph(baseGraph);
            return stateGraph;
        }
    }
}
