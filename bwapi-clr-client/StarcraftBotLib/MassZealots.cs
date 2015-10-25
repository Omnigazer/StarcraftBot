using SWIG.BWAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StarcraftBotLib
{
    public class MassZealots : AIState
    {        
        public bool haltProduction;

        public MassZealots(GameState state, EconomicAI eco_ai) : base(state, eco_ai) { }        

        public override AIState Run()
        {
            if (State.getBuildingCount("Protoss Gateway") < 4 && State.freeMinerals() >= 150)
                if (buildGateway())
                    State.pushItem("Protoss Gateway");
            if (State.freeSupply() <= ProductionCapacity && State.freeMinerals() >= 100)
            {
                //waitingForPylon = !buildPylon();                
                State.pushItem("Protoss Pylon");
                buildPylon();
            }
            foreach (var gate in State.myUnits.Where(x => !x.theUnit.isTraining() && x.Type == "Protoss Gateway" && !haltProduction))
            {
                if (State.freeMinerals() >= 100)
                    gate.theUnit.train(bwapi.getUnitType("Protoss Zealot"));
            }

            foreach (var zealot in State.myUnits.Where(x => x.Type == "Protoss Zealot"))
            {
                string order = zealot.theUnit.getOrder().getName();
                if (enemy_index != -1 && order != "AttackMove" && order != "AttackUnit")
                    zealot.theUnit.attack(State.BaseLocations[enemy_index].getRegion().getCenter());
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
            EcoAI.Run();
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
            else if (unit.Type == "Protoss Zealot")
            {
                myArmy.Add(unit);
            }
        }
    }
}
