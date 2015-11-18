using SWIG.BWAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace StarcraftBotLib
{
    class Scouter : IAIModule, IDisposable
    {
        BW.Unit ScoutingProbe { get; set; }
        GameState State { get; set; }
        TaskCompletionSource<bool> current_task;
        Position enemy_position;
        Position current_position;

        public void Run()
        {
            if (bwapi.Broodwar.isVisible(new TilePosition(current_position)) && !current_task.Task.IsCompleted)
            {               
                // Dispatcher.CurrentDispatcher.BeginInvoke((Action)(() => { current_task.SetResult(true); }));
                Task.Run(() => current_task.SetResult(true));
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
            State.myUnitDestroyed += onMyUnitDestroyed;
            State.enemyShown += onEnemyShown;            
        }

        public void onMyUnitDestroyed(object sender, UnitArgs args)
        {
            if (args.Unit.Id == ScoutingProbe.Id)
            {
                ScoutingProbe.Alive = false;
                current_task.SetResult(false);
            }
        }

        public void onEnemyShown(object sender, UnitArgs args)
        {
            Unit unit = args.Unit.theUnit;
            foreach (var location in State.BaseLocations)
            {
                if (enemy_position == null && location.getRegion().getPolygon().isInside(unit.getPosition()))
                {
                    enemy_position = location.getPosition();
                    bwapi.Broodwar.printf(String.Format("Enemy base spotted"));
                }
            }
        }

        public async Task<Position> Scout()
        {
            foreach (var Base in State.BaseLocations)
            {
                if (!(await ScoutPosition(Base.getPosition())))
                {
                    bwapi.Broodwar.printf("Scout Destroyed");
                    break;
                }
            }
            return enemy_position;
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

        public override string ToString()
        {
            return "Scouter";
        }

        public void Dispose()
        {
            State.myUnitDestroyed -= onMyUnitDestroyed;
            State.enemyShown -= onEnemyShown;
        }
    }
}
