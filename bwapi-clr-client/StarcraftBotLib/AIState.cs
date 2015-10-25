using SWIG.BWAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int scoutingProbeId;
        public int buildingProbeId;
        public bool is_scouting = false;
        // !!!
        public int enemy_index = -1;
        public List<BW.Unit> myArmy = new List<BW.Unit>();

        // Tasks
        Dictionary<Position, TaskCompletionSource<bool>> RevelationTasks = new Dictionary<Position, TaskCompletionSource<bool>>();        
        
        //

        #region ModuleInteraction
        protected virtual void transferUnitTo(BW.Unit unit, IAIModule module)
        {
            /// FreeUnits.Remove(unit);
            module.ReceiveUnit(unit);
        }
        // Probably with type
        protected virtual BW.Unit requestUnit(IAIModule module)
        {
            var unit = module.YieldUnit();
            // FreeUnits.Add(unit);
            return unit;
        }
        #endregion

        public virtual AIState Run()
        {
            // Old format
            var tmp = new Dictionary<Position, TaskCompletionSource<bool>>();
            foreach (var rev_task in RevelationTasks)
            {
                if (bwapi.Broodwar.isVisible(new TilePosition(rev_task.Key)))
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
            return this;
        }

        public virtual void onFoundMinerals(BW.Unit unit)
        {

        }

        public virtual void onMyUnitCreated(BW.Unit unit)
        {            
            FreeUnits.Add(unit);
        }

        public virtual void onEnemyShown(Unit unit)
        {
            var type = unit.getType();
            if (type.isBuilding())
            {
                foreach (var location in State.BaseLocations)
                {                    
                    if (enemy_index == -1 && location.getRegion().getPolygon().isInside(unit.getPosition()))
                    {
                        enemy_index = State.BaseLocations.IndexOf(location);
                        bwapi.Broodwar.printf(String.Format("Enemy base spotted"));
                    }
                }
            }
        }

        public virtual BW.Unit getFreeProbe()
        {
            return requestUnit(EcoAI);
        }        

        public virtual BW.Unit getBuildingProbe()
        {
            if (buildingProbeId == 0)
            {
                buildingProbeId = getFreeProbe().Id;
            }
            return State.myUnits.First(x => x.Id == buildingProbeId);
        }

        public virtual BW.Unit getScoutingProbe()
        {
            if (scoutingProbeId == 0)
            {
                scoutingProbeId = getFreeProbe().Id;
            }
            return State.myUnits.First(x => x.Id == scoutingProbeId);
        }

        public virtual void produceProbes()
        {
            if (!State.getUnit("Protoss Nexus").theUnit.isTraining() && State.freeMinerals() >= 50)
            {
                buildProbe();
            }
        }

        public async virtual void Scout()
        {
            if (State.analysis_complete && !is_scouting)
            {
                // var probe = getScoutingProbe();                
                var probe = EcoAI.YieldUnit();
                Scouter scouter = new Scouter(State);
                scouter.ReceiveUnit(probe);
                running_modules.Add(scouter);
                is_scouting = true;
                await scouter.Scout();
                // is_scouting = false;
                running_modules.Remove(scouter);
                transferUnitTo(probe, EcoAI);
                
                /*
                if (!probe.isScouting)
                {
                    var location = SWIG.BWTA.bwta.getStartLocations().Last().getRegion().getCenter();
                    probe.theUnit.move(location);
                    probe.isScouting = true;
                }
                */
            }
        }

        public bool buildProbe()
        {
            UnitType probe_type = bwapi.getUnitType("Protoss Probe");
            var nexus = State.getUnit("Protoss Nexus");
            if (nexus != null)
                return nexus.theUnit.train(probe_type);
            return false;
        }

        public async Task<bool> buildPylon()
        {
            Position nexus_position = State.getUnit("Protoss Nexus").theUnit.getPosition();
            return await buildPylon(nexus_position);            
        }

        public async Task<bool> buildPylon(Position central_position)
        {
            var probe = getBuildingProbe();
            UnitType pylon_type = bwapi.getUnitType("Protoss Pylon");
            Position pylon_position = new Position(central_position.xConst() - 200, central_position.yConst() + 200);

            if (probe != null)
            {
                for (int i = -10; i < 10; i++)
                    for (int j = -10; j < 10; j++)
                    {
                        pylon_position = new Position(central_position.xConst() + 20 * i, central_position.yConst() + 20 * j);
                        if (bwapi.Broodwar.canBuildHere(probe.theUnit, new TilePosition(pylon_position), pylon_type))
                        {
                            if (!bwapi.Broodwar.isVisible(new TilePosition(pylon_position)))
                                await RevealPosition(probe, pylon_position);
                            return probe.theUnit.build(new TilePosition(pylon_position), pylon_type);
                        }                            
                    }
            }
            //return probe.theUnit.move(new Position(nexus_position.xConst() - 300, nexus_position.yConst() + 200));                
            return false;
        }

        public async Task RevealPosition(BW.Unit unit, Position position)
        {
            var tcs = new TaskCompletionSource<bool>();
            unit.theUnit.move(position);
            RevelationTasks.Add(position, tcs);
            await (Task)tcs.Task;
        }

        public bool buildGateway()
        {
            var probe = getBuildingProbe();
            UnitType gateway_type = bwapi.getUnitType("Protoss Gateway");
            Position pylon_position = State.getUnit("Protoss Pylon").theUnit.getPosition();
            Position gateway_position;

            if (probe != null)
            {
                for (int i = -10; i < 10; i++)
                    for (int j = -10; j < 10; j++)
                    {
                        gateway_position = new Position(pylon_position.xConst() + 20 * i, pylon_position.yConst() + 20 * j);
                        if (bwapi.Broodwar.canBuildHere(probe.theUnit, new TilePosition(gateway_position), gateway_type))
                            return probe.theUnit.build(new TilePosition(gateway_position), gateway_type);
                    }
            }
            //return probe.theUnit.move(new Position(nexus_position.xConst() - 300, nexus_position.yConst() + 200));                            
            return false;
        }
    }
}
