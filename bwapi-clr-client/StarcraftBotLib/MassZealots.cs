using SWIG.BWAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#pragma warning disable 4014
namespace StarcraftBotLib
{
    public class MassZealots : AIState
    {        
        public bool haltProduction;

        public MassZealots(GameState state, EconomicAI eco_ai) : base(state, eco_ai) { }        

        public override AIState Run()
        {
            requiredBuildings["Protoss Gateway"] = 2;
            requiredBuildings["Protoss Photon Cannon"] = 4;
            Scout();

            if (State.workerSupply() >= 20 && !bwapi.Broodwar.self().isUpgrading(bwapi.UpgradeTypes_Leg_Enhancements) && bwapi.Broodwar.self().getUpgradeLevel(bwapi.UpgradeTypes_Leg_Enhancements) == 0)
                EcoAI.gases = 1;
            // if (State.workerSupply() >= 33)
            //     EcoAI.gases = 2;

            // for extra defense
            AddCannons();
            // if forge is present
            UpgradeWeapons();
            // if a gate is present
            BuildCore();
            // if a core is present, sets gateway count to 9
            BuildAdun();
            // if adun is present
            UpgradeLegs();
            BuildMoreGates(9);
            // when needed
            BuildPylons();
            // when possible
            BuildGates();           
            // nonstop, when possible
            ProduceZealots();           

            // basic condition
            if (myArmy.Count() >= 15)
            {
                foreach (var zealot in myArmy)
                {
                    string order = zealot.theUnit.getOrder().getName();
                    if (enemy_position != null && order != "AttackMove" && order != "AttackUnit")
                        zealot.theUnit.attack(enemy_position);
                }
            }
            else
            {
                var forge = State.getUnit("Protoss Forge");
                foreach (var zealot in myArmy.Where(x => x.theUnit.getDistance(forge.theUnit.getPosition()) > 200))
                {
                    string order = zealot.theUnit.getOrder().getName();
                    if (enemy_position != null && order != "PlayerGuard" && order != "Stop")
                        zealot.theUnit.move(forge.theUnit.getPosition());
                }
            }       
                        
            base.Run();            
            return this;
        }

        public void UpgradeWeapons()
        {
            var forge = State.getUnit("Protoss Forge");
            var weapons = bwapi.getUpgradeType("Protoss Ground Weapons");
            if (bwapi.Broodwar.canUpgrade(forge.theUnit, weapons))
                forge.theUnit.upgrade(weapons);
        }

        public void UpgradeLegs()
        {
            if (State.getBuildingCount("Protoss Citadel of Adun", false, true) > 0)
            {                
                var citadel = State.getUnit("Protoss Citadel of Adun");
                var legs = bwapi.UpgradeTypes_Leg_Enhancements;
                if (bwapi.Broodwar.canUpgrade(citadel.theUnit, legs))
                    if (citadel.theUnit.upgrade(legs))
                        EcoAI.gases = 0;
            }
        }

        public void AddCannons()
        {
            if (State.getBuildingCount("Protoss Photon Cannon") < requiredBuildings["Protoss Photon Cannon"] && State.freeMinerals() >= 150)
            {
                var pylon = State.getPylonClosestTo(State.getUnit("Protoss Forge").theUnit.getPosition());
                State.pushItem("Protoss Photon Cannon", pylon.theUnit.getTilePosition());
            }
        }

        public void BuildCore()
        {
            if (State.getBuildingCount("Protoss Gateway", false, true) > 0 && requiredBuildings["Protoss Cybernetics Core"] <= 1)
                requiredBuildings["Protoss Cybernetics Core"] = 1;
            if (State.getBuildingCount("Protoss Cybernetics Core") < requiredBuildings["Protoss Cybernetics Core"] && State.freeMinerals() >= 200)
            {
                var pylon = State.getPylonClosestTo(MapHelper.getMainToChokePosition());
                State.pushItem("Protoss Cybernetics Core", pylon.theUnit.getTilePosition());
            }
        }

        public void BuildAdun()
        {
            if (State.getBuildingCount("Protoss Cybernetics Core", false, true) > 0)
            {
                requiredBuildings["Protoss Citadel of Adun"] = 1;                
            }
            if (State.getBuildingCount("Protoss Citadel of Adun") < requiredBuildings["Protoss Citadel of Adun"] && State.freeMinerals() >= 150)
            {
                var pylon = State.getPylonClosestTo(MapHelper.getMainToChokePosition());
                State.pushItem("Protoss Citadel of Adun", pylon.theUnit.getTilePosition());                
            }
        }

        public void BuildMoreGates(int count)
        {
            if (State.getBuildingCount("Protoss Citadel of Adun") > 0)
                requiredBuildings["Protoss Gateway"] = count;
        }

        public void BuildGates()
        {
            if (State.getBuildingCount("Protoss Gateway") < requiredBuildings["Protoss Gateway"] && State.freeMinerals() >= 150)
            {
                var pylon = State.getPylonClosestTo(MapHelper.getMainToChokePosition());
                State.pushItem("Protoss Gateway", pylon.theUnit.getTilePosition());
            }
        }

        public void BuildPylons()
        {
            if (State.freeSupply() <= ProductionCapacity && State.freeMinerals() >= 100)
            {                            
                State.pushItem("Protoss Pylon", new TilePosition(MapHelper.getMainToChokePosition()));
            }
        }

        public void ProduceZealots()
        {
            foreach (var gate in State.myUnits.Where(x => !x.theUnit.isTraining() && x.Type == "Protoss Gateway" && !haltProduction))
            {
                if (State.freeMinerals() >= 100)
                    gate.theUnit.train(bwapi.getUnitType("Protoss Zealot"));
            }
        }

        public override void onMyUnitCreated(BW.Unit unit)
        {            
            if (unit.Type == "Protoss Zealot")
            {
                myArmy.Add(unit);
            }
            base.onMyUnitCreated(unit);
        }
    }
}
