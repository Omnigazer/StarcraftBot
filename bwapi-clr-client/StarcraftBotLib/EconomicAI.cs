using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StarcraftBotLib
{
    public class EconomicAI : IAIModule
    {
        public EconomicAI()
        {
            Probes = new List<BW.Unit>();
            InitialMinerals = new List<BW.Unit>();   
        }
        public List<BW.Unit> Probes { get; set; }

        // one base
        public List<BW.Unit> InitialMinerals { get; set; }
        public int currentPatchIndex { get; set; }        

        public void Run()
        {
            if (InitialMinerals.Any())
            {                
                foreach (var probe in getIdleProbes())
                {
                    GatherNextMineral(probe);
                }
            }
        }

        public BW.Unit YieldUnit()
        {
            return RequestProbe();
        }

        public BW.Unit RequestProbe()
        {
            if (Probes.Any())
            {
                var probe = Probes.Where(x => x.theUnit.isCompleted()).Last();
                Probes.Remove(probe);
                return probe;
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<BW.Unit> getIdleProbes()
        {
            return Probes.Where(x => x.theUnit.getOrder().getName() == "Stop" || x.theUnit.getOrder().getName() == "PlayerGuard");
        }

        private void GatherNextMineral(BW.Unit unit)
        {
            unit.Gather(InitialMinerals[currentPatchIndex].theUnit);
            currentPatchIndex = (currentPatchIndex + 1) % InitialMinerals.Count;
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
    }
}
