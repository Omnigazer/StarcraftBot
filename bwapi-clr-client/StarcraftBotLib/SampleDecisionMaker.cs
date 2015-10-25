using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SWIG.BWAPI;

namespace StarcraftBotLib
{
    class SampleDecisionMaker : DecisionMaker
    {
        // public string State { get; set; }
        public GameState State { get; set; }
        public AIState AIState { get; set; }        
        // int gatewaysCount = 0;
        // int scoutingProbeId;
        // int buildingProbeId;

        // UnitTypeId to enqueued count
        // Dictionary<string, int> buildingQueue = new Dictionary<string, int>();
        EconomicAI EcoAI { get; set; }
        
        bool haltProduction = false;                       
        

        public SampleDecisionMaker()
        {            
            State = new GameState();
            EcoAI = new EconomicAI();
            AIState = new PreInitialize(State, EcoAI);            
        }        

        #region Callbacks        

        public void onAnalysisComplete()
        {
            State.BaseLocations = SWIG.BWTA.bwta.getStartLocations().ToList();
            State.analysis_complete = true;
        }

        public void onfoundMinerals(BW.Unit unit)
        {
            AIState.onFoundMinerals(unit);            
        }

        public void onEnemyShown(Unit unit)
        {
            State.onEnemyShown(unit);
            AIState.onEnemyShown(unit);
            var type = unit.getType();
            bwapi.Broodwar.printf(String.Format("Enemy spotted: {0}", type.getName()));            
        }

        public void onMyUnitCreated(Unit unit)
        {            
            var myunit = new BW.Unit(unit);
            State.onMyUnitCreated(myunit);
            if (myunit.Type == "Protoss Probe")
            {
                EcoAI.Probes.Add(myunit);
            }
            AIState.onMyUnitCreated(myunit);            
        }

        public void onMyUnitDestroyed(Unit unit)
        {
            var myunit = new BW.Unit(unit);
            State.onMyUnitDestroyed(myunit);
        }

        public void onMyUnitCompleted(Unit unit)
        {
                
        }
        #endregion                

        public void newFrame()
        {
            AIState = AIState.Run();                        
        }        
        
    }
}
