using SWIG.BWAPI;
using SWIG.BWTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarcraftBotLib
{
    public class EcoBaseAI : IAIModule
    {
        public EcoBaseAI(GameState state, BaseLocation location, BW.Unit nexus)
        {
            State = state;
            this.nexus = nexus;
            Probes = new List<BW.Unit>();
            BaseMinerals = new List<BW.Unit>();
            Location = location;
            foreach (var patch in location.getStaticMinerals())
            {
                BaseMinerals.Add(new BW.Unit(patch));
            }
        }
        // own units
        public List<BW.Unit> Probes { get; set; }
        // nexus ?
        public BW.Unit nexus;
        public BW.Unit assimilator;
        public BaseLocation Location { get; set; }
        //
        public GameState State { get; set; }
        // one base
        public List<BW.Unit> BaseMinerals { get; set; }
        public int currentPatchIndex { get; set; }

        // config        
        public bool with_gas = false;        
        //

        int required_probes()
        {
            return 23;
        }

        public bool buildProbe(BW.Unit nexus)
        {
            UnitType probe_type = bwapi.getUnitType("Protoss Probe");
            if (nexus != null)
                return nexus.theUnit.train(probe_type);
            return false;
        }

        public void produceProbes()
        {
            if (!nexus.theUnit.isTraining() && State.freeMinerals() >= 50)
            {
                buildProbe(nexus);
            }
        }

        public void Run()
        {
            if (Probes.Where(x => x.Alive).Count() < required_probes())
            {
                produceProbes();
            }

            if (BaseMinerals.Any())
            {
                foreach (var probe in getIdleProbes())
                {
                    GatherNextMineral(probe);
                }
            }            
            ManageGas();
        }

        public BW.Unit YieldUnit()
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

        public IEnumerable<BW.Unit> getMiningProbes()
        {
            return Probes.Where(x => x.Alive && x.theUnit.isGatheringMinerals());
        }

        public IEnumerable<BW.Unit> getGasProbes()
        {
            return Probes.Where(x => x.Alive && x.theUnit.isGatheringGas());
        }

        private void GatherNextMineral(BW.Unit unit)
        {
            if (BaseMinerals.Any())
            {
                var patch = BaseMinerals[currentPatchIndex].theUnit;
                if (patch.isVisible(bwapi.Broodwar.self()))
                    unit.Gather(patch);
                else
                    unit.theUnit.move(patch.getInitialPosition());
                currentPatchIndex = (currentPatchIndex + 1) % BaseMinerals.Count;
            }
        }

        public async Task BuildGas()
        {
            with_gas = true;
            TilePosition tile_position = Location.getGeysers().First().getTilePosition();
            var item = State.pushItem("Protoss Assimilator", tile_position);            
            assimilator = await item.buildingTask.Task;
            if (assimilator == null)
                with_gas = false;
        }

        public void ManageGas()
        {
            if (assimilator != null)
            {
                if (assimilator.Alive)
                {
                    if (assimilator.theUnit.isCompleted())
                    {
                        if (with_gas)
                        {
                            if (getGasProbes().Count() < 3)
                            {
                                var mining_probes = getMiningProbes();
                                if (mining_probes.Any())
                                {
                                    var probe = mining_probes.First();
                                    probe.theUnit.gather(assimilator.theUnit);
                                }
                            }
                        }
                        else
                        {
                            foreach (var probe in getGasProbes())
                            {
                                GatherNextMineral(probe);
                            }
                        }
                    }
                }
                else
                {
                    with_gas = false;
                }
            }            
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
            return "Economic Base AI";
        }
    }
}
