using SWIG.BWAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StarcraftBotLib
{
    public class GameState
    {
        public List<SWIG.BWTA.BaseLocation> BaseLocations { get; set; }
        public List<BW.Unit> myUnits = new List<BW.Unit>();
        public List<BuildingQueueItem> buildingQueue = new List<BuildingQueueItem>();
        // Move to AI
        // public int gatewaysCount = 0;
        // public int forgeCount = 0;
        // public int cannonCount = 0;
        //
        public bool analysis_complete = false;
        // shared info for all ai states, forwarded here temporarily
        public Position enemy_position;

        //  Events
        public event EventHandler<UnitArgs> myUnitCreated = delegate { };
        public event EventHandler<UnitArgs> myUnitDestroyed = delegate { bwapi.Broodwar.printf("MY UNIT DESTROYED"); };
        public event EventHandler<UnitArgs> enemyShown = delegate { };

        // Callbacks
        public void onMyUnitCreated(BW.Unit myunit)
        {
            myUnits.Add(myunit);
            myUnitCreated(this, new UnitArgs(myunit));
        }

        public void onEnemyShown(Unit unit)
        {
            var myunit = new BW.Unit(unit);
            enemyShown(this, new UnitArgs(myunit));
        }

        public void onMyUnitDestroyed(BW.Unit unit)
        {            
            myUnitDestroyed(this, new UnitArgs(unit));
        }

        //
        public int getMinerals()
        {
            return bwapi.Broodwar.self().minerals();
        }

        public virtual int freeMinerals()
        {
            int sum = 0;
            foreach (var item in buildingQueue)
            {
                sum += bwapi.getUnitType(item.Type).mineralPrice();
            }
            return getMinerals() - sum;
        }

        public virtual int effectiveSupply()
        {
            int sum = bwapi.Broodwar.self().supplyTotal();
            sum += myUnits.Where(x => !x.theUnit.isCompleted()).Where(x => x.Type == "Protoss Pylon").Count() * 8;
            sum += buildingQueue.Count(x => x.Type == "Protoss Pylon") * 8;
            return sum / 2;
        }

        public int workerSupply()
        {
            return myUnits.Where(x => x.Alive && x.Type == "Protoss Probe").Count();
        }

        public int usedSupply()
        {
            return bwapi.Broodwar.self().supplyUsed() / 2;
        }

        public int freeSupply()
        {
            return effectiveSupply() - usedSupply();
        }       

        public BW.Unit getUnit(string type)
        {
            return myUnits.Find(x => x.Type == type);
        }

        public IEnumerable<BW.Unit> getUnits(string type)
        {
            return myUnits.Where(x => x.Type == type);
        }

        public BW.Unit getPylonClosestTo(Position position)
        {
            return myUnits.Where(x => x.Alive && x.Type == "Protoss Pylon").OrderBy(x => x.theUnit.getDistance(position)).First();
        }

        public int getBuildingCount(string type, bool with_enqueued = true, bool completed = false)
        {
            return myUnits.Where(unit => (unit.Type == type) && (completed ? unit.theUnit.isCompleted() : true)).Count() + (with_enqueued ? buildingQueue.Where(item => item.Type == type).Count() : 0);
        }

        public void pushItem(string type_name)
        {
            buildingQueue.Add(new BuildingQueueItem(type_name, getUnit("Protoss Nexus").theUnit.getTilePosition()));
        }

        public BuildingQueueItem pushItem(string type_name, TilePosition tile_position)
        {
            var item = new BuildingQueueItem(type_name, tile_position);
            buildingQueue.Add(item);
            return item;
        }

        public void pushItem(UnitType type)
        {
            string type_name = type.getName();
            pushItem(type_name);
        }

        // public void popItem(UnitType type, TilePosition position)
        public void popItem(BuildingQueueItem item)
        {           
            buildingQueue.Remove(item);
        }        
    }
}
