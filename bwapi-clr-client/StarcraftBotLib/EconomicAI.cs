using SWIG.BWAPI;
using SWIG.BWTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StarcraftBotLib
{
    public class EconomicAI : IAIModule
    {
        public EconomicAI(GameState state)
        {
            State = state;
            BaseModules = new List<EcoBaseAI>();
            Probes = new List<BW.Unit>();            
        }
        // own units
        public List<BW.Unit> Probes { get; set; }
        //

        public GameState State { get; set; }
        public List<EcoBaseAI> BaseModules { get; set; }
        // config
        public int bases = 1;
        public int gases = 0;
        //

        //int required_probes()
        //{
        //    return bases * 20;
        //}

        public bool buildProbe(BW.Unit nexus)
        {
            UnitType probe_type = bwapi.getUnitType("Protoss Probe");            
            if (nexus != null)
                return nexus.theUnit.train(probe_type);
            return false;
        }

        public async void Run()
        {
            int gases_count = BaseModules.Count(x => x.with_gas);
            int bases_w_o_gas = BaseModules.Count(x => !x.with_gas);            
            if (gases_count < gases && bases_w_o_gas > 0)
            {
                int needed_gas = gases - gases_count;
                foreach (var module in BaseModules.Where(x => !x.with_gas).Take(needed_gas))
                {
                    module.BuildGas();                    
                }
            }
            else if (gases_count > gases)
            {
                int excess_gas = gases_count - gases;
                foreach (var module in BaseModules.Where(x => x.with_gas).Take(excess_gas))
                {
                    module.with_gas = false;
                }
            }

            foreach (var module in BaseModules)
            {
                module.Run();
            }            

            if (BaseModules.Any())
            {                
                while (Probes.Where(x => x.theUnit.isCompleted()).Any())
                {
                    var probe = YieldUnit();                    
                    BaseModules.OrderBy(x => x.Probes.Count()).First().ReceiveUnit(probe);                    
                }                
            }
            
            if (State.getBuildingCount("Protoss Nexus") < bases)
            {                
                TilePosition pos = State.getUnit("Protoss Nexus").theUnit.getTilePosition();
                // currently only natural
                Position position = SWIG.BWTA.bwta.getNearestChokepoint(State.getUnit("Protoss Nexus").theUnit.getPosition()).getCenter();
                BaseLocation expand_location = SWIG.BWTA.bwta.getNearestBaseLocation(position);
                TilePosition tile_position = expand_location.getTilePosition();
                var item = State.pushItem("Protoss Nexus", tile_position);
                BW.Unit nexus = await item.buildingTask.Task;
                var module = new EcoBaseAI(State, expand_location, nexus);
                BaseModules.Add(module);
            }            
        }

        public IEnumerable<BW.Unit> getIdleProbes()
        {
            return Probes.Where(x => x.theUnit.getOrder().getName() == "Stop" || x.theUnit.getOrder().getName() == "PlayerGuard");
        }        

        public BW.Unit YieldUnit()
        {
            if (Probes.Where(x => x.theUnit.isCompleted()).Any())
            {
                var probe = Probes.Where(x => x.theUnit.isCompleted()).Last();
                Probes.Remove(probe);
                return probe;
            }            
            else
            {
                if (BaseModules.Any())
                {
                    BW.Unit unit = null;
                    foreach (var module in BaseModules)
                    {
                        unit = module.YieldUnit();
                        if (unit != null) break;
                    }
                    return unit;
                }                                
            }
            return null;
        }       

        public void ReceiveUnit(BW.Unit unit)
        {
            if (unit.theUnit.getType().getName() == "Protoss Probe")
            {
                Probes.Add(unit);
            }
            else
            {
                throw new NotImplementedException("AI cannot receive non-Probe units");
            }
        }

        public override string ToString()
        {
            return "Economic AI";
        }

    }
}
