using SWIG.BWAPI;
using SWIG.BWTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarcraftBotLib.CustomTypes;
#pragma warning disable 4014
namespace StarcraftBotLib
{
    public abstract class AIState
    {
        public AIState(GameState state, EconomicAI eco_ai)
        {
            State = state;
            EcoAI = eco_ai;
            running_modules.Add(EcoAI);
            FreeUnits = new List<BW.Unit>();                
        }

        public GameState State { get; set; }
        public EconomicAI EcoAI;
        public List<BW.Unit> FreeUnits { get; set; }
        List<IAIModule> running_modules = new List<IAIModule>();
        protected NullableDictionary requiredBuildings = new NullableDictionary();
        // Estimation of required supply ahead of time
        public int ProductionCapacity
        {
            get 
            {
                // sample formula
                int sum = 0;
                sum += 2 * State.getBuildingCount("Protoss Gateway");
                sum += State.getBuildingCount("Protoss Nexus");
                // safety offset
                // sum += 1;
                return sum;
            } 
        }


        // !!!        
        public int buildingProbeId;
        public bool is_scouting = false;        

        // shared data, temporarily stored in gamestate
        public Position enemy_position
        {
            get { return State.enemy_position; }
            set { State.enemy_position = value; }
        }
        //

        public List<BW.Unit> myArmy = new List<BW.Unit>();

        // Tasks
        Dictionary<TilePosition, TaskCompletionSource<bool>> RevelationTasks = new Dictionary<TilePosition, TaskCompletionSource<bool>>();        
        
        //

        #region ModuleInteraction
        protected virtual void transferUnitTo(BW.Unit unit, IAIModule module)
        {            
            module.ReceiveUnit(unit);
            bwapi.Broodwar.printf("Unit transferred to : " + module.ToString());
        }
        // Probably with type
        protected virtual BW.Unit requestUnit(IAIModule module)
        {
            var unit = module.YieldUnit();
            bwapi.Broodwar.printf("Unit transferred from : " + module.ToString());            
            return unit;
        }
        #endregion

        public virtual AIState Run()
        {
            foreach (var choke in bwta.getChokepoints())
            {
                var pos = choke.getCenter();                
                bwapi.Broodwar.drawCircleMap(pos.xConst(), pos.yConst(), Convert.ToInt32(choke.getSides().first.getDistance(choke.getSides().second)), new Color(255, 0, 0));
                bwapi.Broodwar.drawLineMap(choke.getSides().first.xConst(), choke.getSides().first.yConst(), choke.getSides().second.xConst(), choke.getSides().second.yConst(), new Color(0, 255, 0));
            }

            foreach (var item in State.buildingQueue)
            {
                int tile_width = bwapi.getUnitType(item.Type).tileWidth();
                int tile_height = bwapi.getUnitType(item.Type).tileHeight();

                var pos = new Position(32 * item.TilePosition.xConst() + (tile_width * 32) / 2, 32 * item.TilePosition.yConst() + (tile_height * 32) / 2);
                bwapi.Broodwar.drawCircleMap(pos.xConst(), pos.yConst(), Math.Max(tile_width, tile_height) * 16, new Color(0, 255, 255));

                pos = new Position(32 * item.ActualPosition.xConst() + (tile_width * 32) / 2, 32 * item.ActualPosition.yConst() + (tile_height * 32) / 2);
                bwapi.Broodwar.drawCircleMap(pos.xConst(), pos.yConst(), Math.Max(tile_width, tile_height) * 16, new Color(0, 255, 0));
            }

            // Old format
            var tmp = new Dictionary<TilePosition, TaskCompletionSource<bool>>();
            foreach (var rev_task in RevelationTasks)
            {
                if (bwapi.Broodwar.isVisible(rev_task.Key) && bwapi.Broodwar.isVisible(new TilePosition(rev_task.Key.xConst() + 3, rev_task.Key.yConst() + 3)))
                {
                    var task = rev_task.Value;
                    task.SetResult(true);
                }
                else
                {
                    tmp.Add(rev_task.Key, rev_task.Value);
                }
            }
            RevelationTasks = tmp;
            //

            foreach (var module in running_modules)
            {
                module.Run();
            }
            processBuildingQueue();
            return this;
        }

        #region Callbacks
        public virtual void onFoundMinerals(BW.Unit unit)
        {

        }

        public virtual void onMyUnitCreated(BW.Unit unit)
        {
            if (unit.theUnit.getType().isBuilding())
            {
                var item = State.buildingQueue.Where(x => x.Type == unit.Type).OrderBy(x => x.TilePosition.getDistance(unit.theUnit.getTilePosition())).First();
                item.buildingTask.SetResult(unit);
                // State.popItem(unit.theUnit.getType(), unit.theUnit.getTilePosition());
                State.popItem(item);
                transferUnitTo(getBuildingProbe(), EcoAI);
                buildingProbeId = 0;
            }

            if (unit.Type == "Protoss Probe")
            {
                EcoAI.Probes.Add(unit);
            }                        
        }

        public virtual void onMyUnitMorph(BW.Unit unit)
        {
            if (unit.theUnit.getType().getName() == "Protoss Assimilator")
            {
                var item = State.buildingQueue.Where(x => x.Type == unit.Type).OrderBy(x => x.TilePosition.getDistance(unit.theUnit.getTilePosition())).First();
                item.buildingTask.SetResult(unit);
                // State.popItem(unit.theUnit.getType(), unit.theUnit.getTilePosition());
                State.popItem(item);
                transferUnitTo(getBuildingProbe(), EcoAI);
                buildingProbeId = 0;
            }
        }

        public virtual void onEnemyShown(Unit unit)
        {
            
        }

        public virtual void onAnalysisComplete()
        {
            // Position position = SWIG.BWTA.bwta.getNearestChokepoint(State.getUnit("Protoss Nexus").theUnit.getPosition()).getCenter();
            /*
            foreach (var patch in SWIG.BWTA.bwta.getNearestBaseLocation(position).getStaticMinerals())
            {
                EcoAI.NatMinerals.Add(new BW.Unit(patch));
            }
            */ 
        }
        #endregion

        public virtual BW.Unit getFreeProbe()
        {
            // temporary
            var probe = FreeUnits.Where(x => x.Type == "Protoss Probe").FirstOrDefault();
            if (probe != null)
            {
                FreeUnits.Remove(probe);
                return probe;                
            }
            return requestUnit(EcoAI);
        }        

        // !!!
        public virtual BW.Unit getBuildingProbe()
        {
            if (buildingProbeId == 0)
            {
                buildingProbeId = getFreeProbe().Id;
            }
            return State.myUnits.First(x => x.Id == buildingProbeId);
        }        
        //

        public async virtual void Scout()
        {
            if (State.analysis_complete && !is_scouting)
            {                
                var probe = requestUnit(EcoAI);
                using (Scouter scouter = new Scouter(State))
                {
                    transferUnitTo(probe, scouter);
                    running_modules.Add(scouter);
                    is_scouting = true;
                    enemy_position = await scouter.Scout();
                    scouter.YieldUnit();
                    // is_scouting = false;
                    running_modules.Remove(scouter);
                    if (probe.Alive)
                    {
                        transferUnitTo(probe, EcoAI);
                    }
                }               
            }
        }
        
        public async Task<bool> buildBuilding(BuildingQueueItem item)
        {
            var probe = getBuildingProbe();
            UnitType building_type = bwapi.getUnitType(item.Type);
            // TilePosition building_position = central_position;
            TilePosition central_position = item.TilePosition;
            TilePosition building_position = item.TilePosition;

            if (probe != null)
            {                
                for (int i = 0; i < 20; i++)
                    for (int j = 0; j < 20; j++)
                    {                        
                        int xoffset = j / 2 * (j % 2 == 0 ? -1 : 1);
                        int yoffset = i / 2 * (i % 2 == 0 ? -1 : 1);                        
                        building_position = new TilePosition(central_position.xConst() + xoffset, central_position.yConst() + yoffset);
                        if (bwapi.Broodwar.canBuildHere(probe.theUnit, building_position, building_type))
                        {
                            item.ActualPosition = building_position;
                            if (!bwapi.Broodwar.isVisible(building_position))
                                await RevealPosition(probe, building_position);
                            return probe.theUnit.build(building_position, building_type);
                        }
                    }
            }
            return false;
        }

        public void processBuildingQueue()
        {
            if (State.buildingQueue.Any())
            {                
                if (getBuildingProbe() != null && getBuildingProbe().theUnit.getOrder().getName() != "PlaceBuilding" && getBuildingProbe().theUnit.getOrder().getName() != "Move")
                {
                    var item = State.buildingQueue.First();
                    switch (item.Type)
                    {
                        default:
                            {
                                buildBuilding(item);
                                break;
                            }
                    }
                }
            }
        }

        public async Task RevealPosition(BW.Unit unit, TilePosition position)
        {
            var tcs = new TaskCompletionSource<bool>();
            unit.theUnit.move(new Position(position));
            RevelationTasks.Add(position, tcs);
            await (Task)tcs.Task;
        }        
    }
}
