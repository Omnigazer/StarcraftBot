using SWIG.BWAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace StarcraftBotLib
{
    class Scouter : IAIModule
    {
        BW.Unit ScoutingProbe { get; set; }
        GameState State { get; set; }
        TaskCompletionSource<bool> current_task;
        Position current_position;

        public void Run()
        {
            if (bwapi.Broodwar.isVisible(new TilePosition(current_position)) && !current_task.Task.IsCompleted)
            {               
                Dispatcher.CurrentDispatcher.BeginInvoke((Action)(() => { current_task.SetResult(true); }));
                // current_task.SetResult(true);
            }                
            else if (ScoutingProbe.IsIdle())
            {
                ScoutingProbe.theUnit.move(current_position);
            }
        }

        public Scouter(GameState state)
        {
            State = state;
            State.myUnitDestroyed += delegate(object sender, UnitArgs args) { if (args.Unit.Id == ScoutingProbe.Id) onScoutDestroyed(); }; 
        }

        public void onScoutDestroyed()
        {
            current_task.SetResult(false);
        }

        public async Task Scout()
        {
            foreach (var Base in State.BaseLocations)
            {
                if (!(await ScoutPosition(Base.getPosition())))
                {
                    bwapi.Broodwar.printf("Scout Destroyed");
                    break;
                }
            }            
        }

        async Task<bool> ScoutPosition(Position position) 
        {
            current_position = position;
            ScoutingProbe.theUnit.move(position);
            current_task = new TaskCompletionSource<bool>();
            return await current_task.Task;
        }

        public void ReceiveUnit(BW.Unit unit)
        {            
            ScoutingProbe = unit;
        }

        public BW.Unit YieldUnit()
        {
            var probe = ScoutingProbe;
            ScoutingProbe = null;
            return probe;
        }
    }
}
