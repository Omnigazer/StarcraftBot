using SWIG.BWAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StarcraftBotLib
{
    class EarlyStage : AIState
    {
        public EarlyStage(GameState state, EconomicAI eco_ai) : base(state, eco_ai) { }        
        
        public bool haltProduction = false;        
        public override AIState Run()
        {
            Scout();
            if (!haltProduction)
            {
                produceProbes();
            }
            // Nominal supply should account for unstarted pylons
            if (State.freeSupply() <= ProductionCapacity && State.freeMinerals() >= 100)
            {
                //waitingForPylon = !buildPylon();
                var chokepoint = SWIG.BWTA.bwta.getNearestChokepoint(State.getUnit("Protoss Nexus").theUnit.getPosition());
                if (chokepoint != null)
                {
                    bwapi.Broodwar.printf("Chokepoint found");
                    Position position = chokepoint.getCenter();
                    State.pushItem("Protoss Pylon", position);
                    buildPylon(position);        
                }
                else
                {
                    bwapi.Broodwar.printf("Chokepoint not found");
                    State.pushItem("Protoss Pylon");
                    buildPylon();
                }

            }
            foreach (var gate in State.myUnits.Where(x => !x.theUnit.isTraining() && x.Type == "Protoss Gateway"))
            {
                if (State.freeMinerals() >= 100)
                    gate.theUnit.train(bwapi.getUnitType("Protoss Zealot"));
            }
            if (State.usedSupply() >= 10)
            {
                State.gatewaysCount = 1;
            }
            if (State.usedSupply() >= 12)
            {
                State.gatewaysCount = 2;
            }
            if (State.getBuildingCount("Protoss Gateway") < State.gatewaysCount)
            {
                if (buildGateway())
                    State.pushItem("Protoss Gateway");
            }            

            if (State.buildingQueue.Any())
            {
                if (getBuildingProbe() != null && getBuildingProbe().theUnit.getOrder().getName() != "PlaceBuilding" && getBuildingProbe().theUnit.getOrder().getName() != "Move")
                {
                    var item = State.buildingQueue.First();
                    switch (item.Type)
                    {
                        case "Protoss Pylon":
                            {
                                buildPylon(item.Position);                                    
                                break;
                            }
                        case "Protoss Gateway":
                            {
                                buildGateway();
                                break;
                            }
                    }
                }
            }            
            if (State.workerSupply() >= 20)
            {
                return new MassZealots(State, EcoAI);
            }
            base.Run();
            return this;
        }

        public override void onMyUnitCreated(BW.Unit unit)
        {
            if (unit.theUnit.getType().isBuilding())
            {
                State.popItem(unit.theUnit.getType(), unit.theUnit.getPosition());
                transferUnitTo(getBuildingProbe(), EcoAI);
                buildingProbeId = 0;
            }
        }
    }
}
