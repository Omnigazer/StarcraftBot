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
                return new EarlyStage(State, EcoAI);
            }
            return this;
        }

        public override void onFoundMinerals(BW.Unit unit)
        {
            EcoAI.InitialMinerals.Add(unit);
        }

        public override string ToString()
        {
            return "PreInitialize";
        }
    }
}
