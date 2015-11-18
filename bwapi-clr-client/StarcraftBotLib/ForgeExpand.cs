using SWIG.BWAPI;
using SWIG.BWTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#pragma warning disable 4014
namespace StarcraftBotLib
{
    class ForgeExpand : AIState
    {
        public bool haltProduction = false;

        public ForgeExpand(GameState state, EconomicAI eco_ai) : base(state, eco_ai) { }

        public Chokepoint getNatChoke()
        {
            Position position = MapHelper.getMainChokePoint().getCenter();
            return SWIG.BWTA.bwta.getChokepoints().OrderBy(x => x.getCenter().getDistance(position)).ElementAt(1);
        }

        public BaseLocation getNatural()
        {
            Position position = MapHelper.getMainChokePoint().getCenter();
            return SWIG.BWTA.bwta.getNearestBaseLocation(position);
        }

        public override AIState Run()
        {
            if (State.usedSupply() == 8 && State.freeSupply() <= 1 && !FreeUnits.Any())
            {
                var probe = EcoAI.YieldUnit();
                FreeUnits.Add(probe);
                var choke = getNatChoke();
                var position = choke.getCenter();
                State.pushItem("Protoss Pylon", new TilePosition(position));
                probe.theUnit.move(position);
            }

            if (State.freeSupply() <= ProductionCapacity && State.freeMinerals() >= 100)
            {
                var choke = getNatChoke();
                var position = choke.getCenter();
                if (State.analysis_complete && position != null)
                {
                    bwapi.Broodwar.printf("Chokepoint found");
                    State.pushItem("Protoss Pylon", new TilePosition(position));
                }
                else
                {
                    bwapi.Broodwar.printf("Chokepoint not found");
                    State.pushItem("Protoss Pylon");
                }
            }

            if (State.usedSupply() >= 10)
            {
                requiredBuildings["Protoss Forge"] = 1;
            }

            if (State.getBuildingCount("Protoss Forge") < requiredBuildings["Protoss Forge"])
            {
                TilePosition pylon_position = State.getUnit("Protoss Pylon").theUnit.getTilePosition();
                TilePosition forge_position;
                var pair = getNatChoke().getSides();
                var dx = Math.Abs(pair.first.xConst() - pair.second.xConst());
                var dy = Math.Abs(pair.first.yConst() - pair.second.yConst());
                // horizontal
                if (dx > dy)
                {
                    forge_position = new TilePosition(pylon_position.xConst() - bwapi.getUnitType("Protoss Forge").tileWidth(), pylon_position.yConst());
                    bwapi.Broodwar.printf("Building forge horizontally");
                }
                else
                {
                    forge_position = new TilePosition(pylon_position.xConst(), pylon_position.yConst() - bwapi.getUnitType("Protoss Forge").tileHeight());
                    bwapi.Broodwar.printf("Building forge vertically");
                }
                State.pushItem("Protoss Forge", forge_position);
            }

            if (State.getBuildingCount("Protoss Forge", false, true) > 0)
            {
                requiredBuildings["Protoss Photon Cannon"] = 2;
                
                if (State.getBuildingCount("Protoss Photon Cannon") < requiredBuildings["Protoss Photon Cannon"])
                {
                    TilePosition pylon_position = State.getUnit("Protoss Pylon").theUnit.getTilePosition();
                    TilePosition cannon_position;
                    var pair = getNatChoke().getSides();
                    var dx = Math.Abs(pair.first.xConst() - pair.second.xConst());
                    var dy = Math.Abs(pair.first.yConst() - pair.second.yConst());
                    // vertical
                    var nat_position = getNatural().getTilePosition();
                    if (dx < dy)
                    {
                        int x_offset = nat_position.xConst() > pylon_position.xConst() ? bwapi.getUnitType("Protoss Pylon").tileWidth() : -bwapi.getUnitType("Protoss Photon Cannon").tileWidth();
                        cannon_position = new TilePosition(pylon_position.xConst() + x_offset, pylon_position.yConst());
                        bwapi.Broodwar.printf("Building cannon vertically");
                    }
                    else
                    {
                        int y_offset = nat_position.yConst() > pylon_position.yConst() ? bwapi.getUnitType("Protoss Pylon").tileHeight() : -bwapi.getUnitType("Protoss Photon Cannon").tileHeight();
                        cannon_position = new TilePosition(pylon_position.xConst(), pylon_position.yConst() + y_offset);
                        bwapi.Broodwar.printf("Building cannon horizontally");
                    }

                    State.pushItem("Protoss Photon Cannon", cannon_position);
                }
                if (State.freeMinerals() >= 400)
                {
                    EcoAI.bases = 2;                    
                }
            }
            // should be exactly at intended natural position
            if (State.getBuildingCount("Protoss Nexus") > 1)
            {
                return new MassZealots(State, EcoAI);
            }
            
            base.Run();
            return this;
        }

        public override void onMyUnitCreated(BW.Unit unit)
        {            
            base.onMyUnitCreated(unit);
        }
    }
}
