using SWIG.BWAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#pragma warning disable 4014
namespace StarcraftBotLib
{
    class EarlyStage : AIState
    {
        public EarlyStage(GameState state, EconomicAI eco_ai) : base(state, eco_ai) { }

        public bool haltProduction = false;
        public override AIState Run()
        {
            if (State.freeSupply() <= ProductionCapacity && State.freeMinerals() >= 100)
            {
                // var chokepoint = SWIG.BWTA.bwta.getNearestChokepoint(State.getUnit("Protoss Nexus").theUnit.getPosition());
                var position = MapHelper.getMainToChokePosition();
                if (State.analysis_complete && position != null)
                {
                    bwapi.Broodwar.printf("Chokepoint found");
                    // Position position = chokepoint.getCenter();
                    State.pushItem("Protoss Pylon", new TilePosition(position));
                }
                else
                {
                    bwapi.Broodwar.printf("Chokepoint not found");
                    State.pushItem("Protoss Pylon");
                }

            }
            foreach (var gate in State.myUnits.Where(x => !x.theUnit.isTraining() && x.Type == "Protoss Gateway"))
            {
                if (State.freeMinerals() >= 100)
                    gate.theUnit.train(bwapi.getUnitType("Protoss Zealot"));
            }
            if (State.usedSupply() >= 10)
            {
                Scout();
                // State.gatewaysCount = 1;
                requiredBuildings["Protoss Gateway"] = 1;
            }
            if (State.usedSupply() >= 12)
            {
                // State.gatewaysCount = 2;
                requiredBuildings["Protoss Gateway"] = 2;
            }
            if (State.getBuildingCount("Protoss Gateway") < requiredBuildings["Protoss Gateway"])
            {
                State.pushItem("Protoss Gateway");
            }

            base.Run();
            if (State.workerSupply() >= 20)
            {
                return new MassZealots(State, EcoAI);
            }
            return this;
        }

        public override void onMyUnitCreated(BW.Unit unit)
        {            
            base.onMyUnitCreated(unit);
        }
    }
}
