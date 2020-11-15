using System.Collections.Generic;
using Verse;

namespace LAS
{
    public class KeyHolderComp : ThingComp, ILoadReferenceable
    {
        public Pawn Pawn => parent as Pawn;
        public static List<KeyHolderComp> allKeyHolders;
        public string GetUniqueLoadID() => parent.GetUniqueLoadID() + "_KeyHolderComp";
        public bool CanUnlock(LockComp lockComp)
        {
            return true;
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            var map = parent.MapHeld;
            var mapTracker = map.GetComponent<MapTracker>();
            mapTracker.keyHolders.AddDistinct(this);
        }
        public override void PostDeSpawn(Map map)
        {
            var mapTracker = map.GetComponent<MapTracker>();
            if (mapTracker.keyHolders.Contains(this))
                mapTracker.keyHolders.Remove(this);
        }
    }
    public class MapTracker : MapComponent
    {
        public MapTracker(Map map) : base(map) { }
        public List<KeyHolderComp> keyHolders = new List<KeyHolderComp>();
    }
}
