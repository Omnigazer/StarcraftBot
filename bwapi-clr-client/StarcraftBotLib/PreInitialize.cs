using SWIG.BWTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StarcraftBotLib
{
    public class PreInitialize : AIState
    {
        public PreInitialize(GameState state, EconomicAI eco_ai) : base(state, eco_ai) { }

        public override AIState Run()
        {
            if (State.getBuildingCount("Protoss Nexus") == 1 && State.myUnits.Where(x => x.Type == "Protoss Probe").Count() == 4)
            {
                // return new EarlyStage(State, EcoAI);
                EcoBaseAI new_base_module = new EcoBaseAI(State, MapHelper.getMainBase(), State.getUnit("Protoss Nexus"));
                EcoAI.BaseModules.Add(new_base_module);
                return new ForgeExpand(State, EcoAI);
            }
            return this;
        }

        public override void onMyUnitCreated(BW.Unit unit)
        {
            if (unit.Type == "Protoss Probe")
            {
                EcoAI.Probes.Add(unit);
            }            
        }

        public override void onFoundMinerals(BW.Unit unit)
        {
            // EcoAI.InitialMinerals.Add(unit);
        }

        public override string ToString()
        {
            return "PreInitialize";
        }
    }
}
